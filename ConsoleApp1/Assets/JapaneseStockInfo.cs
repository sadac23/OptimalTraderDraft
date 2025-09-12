using ConsoleApp1.Assets;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using System.Text;

public class JapaneseStockInfo : AssetInfo
{
    public JapaneseStockInfo(WatchList.WatchStock watchStock)
        : base(watchStock, new JapaneseStockUpdater(), new JapaneseStockFormatter()) { }

    // 必要に応じて日本株固有のプロパティやメソッドを追加可能
}

public class JapaneseStockUpdater : IExternalSourceUpdatable
{
    public async Task UpdateFromExternalSourceAsync(AssetInfo stockInfo)
    {
        // 日本株用の外部情報取得処理をここに実装
        // 例: 各種スクレイパーやAPIを呼び出してstockInfoのプロパティを更新
        List<Task> tasks = new List<Task>();

        var yahooScraper = new YahooScraper();
        var kabutanScraper = new KabutanScraper();
        var minkabuScraper = new MinkabuScraper();

        Task kabutanFinance = Task.Run(async () =>
        {
            await kabutanScraper.ScrapeFinance(stockInfo);
        });
        tasks.Add(kabutanFinance);

        Task minkabuDividend = Task.Run(async () =>
        {
            await minkabuScraper.ScrapeDividend(stockInfo);
        });
        tasks.Add(minkabuDividend);

        Task minkabuYutai = Task.Run(async () =>
        {
            await minkabuScraper.ScrapeYutai(stockInfo);
        });
        tasks.Add(minkabuYutai);

        Task yahooTop = Task.Run(async () =>
        {
            await yahooScraper.ScrapeTop(stockInfo);
        });
        tasks.Add(yahooTop);

        Task yahooProfile = Task.Run(async () =>
        {
            await yahooScraper.ScrapeProfile(stockInfo);
        });
        tasks.Add(yahooProfile);

        //Task yahooDisclosure = Task.Run(async () =>
        //{
        //    await yahooScraper.ScrapeDisclosure(this);
        //});
        //tasks.Add(yahooDisclosure);

        Task yahooHistory = Task.Run(async () =>
        {
            // 履歴更新の最終日を取得（なければ基準開始日を取得）
            var lastUpdateDay = stockInfo.GetLastHistoryUpdateDay();

            // 最終更新後に直近営業日がある場合は履歴取得
            if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
            {
                await yahooScraper.ScrapeHistory(stockInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate);

                // 株式分割がある場合は履歴をクリアして再取得
                if (stockInfo.HasRecentStockSplitOccurred() && lastUpdateDay != CommonUtils.Instance.MasterStartDate)
                {
                    stockInfo.DeleteHistory(CommonUtils.Instance.ExecusionDate);
                    stockInfo.ScrapedPrices.Clear();
                    await yahooScraper.ScrapeHistory(stockInfo, CommonUtils.Instance.MasterStartDate, CommonUtils.Instance.ExecusionDate);
                }
            }
        });
        tasks.Add(yahooHistory);

        // タスクの実行待ち
        await Task.WhenAll(tasks);

        await Task.CompletedTask;
    }
}

public class JapaneseStockFormatter : IOutputFormattable
{
    public string ToOutputString(AssetInfo stockInfo)
    {
        // 日本株用の出力処理をここに実装
        // 必要に応じてstockInfoのプロパティを参照し、出力フォーマットを整形
        StringBuilder sb = new StringBuilder();

        var mark = CommonUtils.Instance.BadgeString.ShouldWatch;
        var count = 0;
        var s = string.Empty;

        sb.AppendLine($"{stockInfo.Code}：{stockInfo.Name}");

        sb.AppendLine($"株価：{stockInfo.LatestPrice.Price.ToString("N1")}" +
            $"（{stockInfo.LatestPrice.Date.ToString("yy/MM/dd")}" +
            $"：S{stockInfo.LatestPrice.RSIS.ToString("N2")}" +
            $",L{stockInfo.LatestPrice.RSIL.ToString("N2")}" +
            $"）{(stockInfo.LatestPrice.OversoldIndicator() || (stockInfo.IsOwnedNow() && stockInfo.LatestPrice.OverboughtIndicator()) ? mark : string.Empty)}");

        sb.AppendLine($"市場/業種：{stockInfo.Section}{(!string.IsNullOrEmpty(stockInfo.Industry) ? $"/{stockInfo.Industry}" : string.Empty)}");

        sb.AppendLine($"配当利回り：{CommonUtils.Instance.ConvertToPercentage(stockInfo.DividendYield)}" +
            $"（{CommonUtils.Instance.ConvertToPercentage(stockInfo.DividendPayoutRatio)}" +
            $",{CommonUtils.Instance.ConvertToPercentage(stockInfo.Doe)}" +
            $",{stockInfo.DividendRecordDateMonth}" +
            $"）{(stockInfo.IsCloseToDividendRecordDate() ? mark : string.Empty)}");

        if (!string.IsNullOrEmpty(stockInfo.ShareholderBenefitsDetails))
            sb.AppendLine($"優待利回り：{CommonUtils.Instance.ConvertToPercentage(stockInfo.ShareholderBenefitYield)}" +
                $"（{stockInfo.ShareholderBenefitsDetails}" +
                $",{stockInfo.NumberOfSharesRequiredForBenefits}" +
                $",{stockInfo.ShareholderBenefitRecordMonth}" +
                $",{stockInfo.ShareholderBenefitRecordDay}" +
                $"）{(stockInfo.IsCloseToShareholderBenefitRecordDate() ? mark : string.Empty)}");

        // 通期予想履歴
        foreach (var p in stockInfo.FullYearPerformancesForcasts)
        {
            if (count == 0) sb.AppendLine($"通期予想（前期比）：");
            sb.AppendLine($"{p.Category}" +
                $"：{p.RevisionDate.ToString("yy/MM/dd")}" +
                $"：{p.Summary}{(p.HasUpwardRevision() ? mark : string.Empty)}");
            count++;
        }

        sb.AppendLine($"通期進捗：{stockInfo.LastQuarterPeriod}" +
            $"：{CommonUtils.Instance.ConvertToPercentage(stockInfo.QuarterlyFullyearProgressRate)}" +
            $"（{stockInfo.QuarterlyPerformanceReleaseDate.ToString("yy/MM/dd")}" +
            $"：{CommonUtils.Instance.ConvertToPercentage(stockInfo.QuarterlyOperatingProfitMarginYoY, true)}）" +
            $"{(stockInfo.IsAnnualProgressOnTrack() ? mark : string.Empty)}");

        sb.AppendLine($"前期進捗：{stockInfo.LastQuarterPeriod}" +
            $"：{CommonUtils.Instance.ConvertToPercentage(stockInfo.PreviousFullyearProgressRate)}" +
            $"（{stockInfo.PreviousPerformanceReleaseDate.ToString("yy/MM/dd")}）");

        sb.AppendLine($"時価総額：{CommonUtils.Instance.ConvertToYenNotation(stockInfo.MarketCap)}");

        count = 0;
        s = string.Empty;
        foreach (AssetInfo.FullYearProfit p in stockInfo.FullYearProfits)
        {
            if (count > 0) s += "→";
            s += p.Roe;
            count++;
        }
        if (!string.IsNullOrEmpty(s))
            sb.AppendLine($"ROE：{s}{(stockInfo.IsROEAboveThreshold() ? mark : string.Empty)}");

        sb.AppendLine($"PER：{CommonUtils.Instance.ConvertToMultiplierString(stockInfo.Per)}" +
            $"（{stockInfo.AveragePer.ToString("N1")}）{(stockInfo.IsPERUndervalued() ? mark : string.Empty)}");

        sb.AppendLine($"PBR：{CommonUtils.Instance.ConvertToMultiplierString(stockInfo.Pbr)}" +
            $"（{stockInfo.AveragePbr.ToString("N1")}）{(stockInfo.IsPBRUndervalued() ? mark : string.Empty)}");

        sb.AppendLine($"営業利益率：{CommonUtils.Instance.ConvertToPercentage(stockInfo.OperatingProfitMargin)}");

        sb.AppendLine($"信用倍率：{stockInfo.MarginBalanceRatio}");

        sb.AppendLine($"信用残：{stockInfo.MarginBuyBalance}/{stockInfo.MarginSellBalance}（{stockInfo.MarginBalanceDate}）");

        sb.AppendLine($"出来高：{stockInfo.LatestTradingVolume}");

        sb.AppendLine($"自己資本比率：{stockInfo.EquityRatio}");

        sb.AppendLine($"決算：{stockInfo.EarningsPeriod}");

        s = string.Empty;
        if (!string.IsNullOrEmpty(stockInfo.PressReleaseDate))
        {
            s += stockInfo.PressReleaseDate;
            s += stockInfo.ExtractAndValidateDateWithinOneMonth() ? mark : string.Empty;
            sb.AppendLine($"{s}");
        }

        count = 0;
        s = string.Empty;
        foreach (ExecutionList.Execution e in stockInfo.Executions)
        {
            if (count == 0) sb.AppendLine($"約定履歴：");

            sb.AppendLine($"{e.BuyOrSell}" +
                $"：{e.Date.ToString("yy/MM/dd")}" +
                $"：{e.Price.ToString("N1")}*{e.Quantity}" +
                $"：{CommonUtils.Instance.ConvertToPercentage((stockInfo.LatestPrice.Price / e.Price) - 1, true)}" +
                $"{(stockInfo.ShouldAverageDown(e) ? mark : string.Empty)}");

            count++;
        }

        //チャート：
        sb.AppendLine($"チャート（RSI）：");
        foreach (var p in stockInfo.ChartPrices)
        {
            sb.AppendLine(
                $"{p.Date.ToString("MM/dd")}" +
                $"：{p.Price.ToString("N1")}" +
                $"：{CommonUtils.Instance.ConvertToPercentage(p.Volatility, true)}" +
                $"（S{p.RSIS.ToString("N2")}" +
                $",L{p.RSIL.ToString("N2")}）" +
                $"{(p.OversoldIndicator() ? mark : string.Empty)}" +
                $"");
        }

        if (!string.IsNullOrEmpty(stockInfo.Memo))
        {
            //メモ：
            sb.AppendLine($"メモ：");
            sb.AppendLine(stockInfo.Memo);
        }

        return sb.ToString();
    }
}