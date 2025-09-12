// See https://aka.ms/new-console-template for more information
using ConsoleApp1.Assets;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using System.Text;

internal class JapaneseETFInfo : AssetInfo
{
    public JapaneseETFInfo(
        WatchList.WatchStock watchStock,
        IExternalSourceUpdatable updater,
        IOutputFormattable formatter)
        : base(watchStock, updater, formatter)
    {
    }

    // 必要に応じてETF固有のプロパティやメソッドを追加可能
}

internal class JapaneseETFUpdater : IExternalSourceUpdatable
{
    public async Task UpdateFromExternalSourceAsync(AssetInfo assetInfo)
    {
        // ETF用の外部情報取得処理をここに実装
        // 例: 各種スクレイパーやAPIを呼び出してassetInfoのプロパティを更新
        List<Task> tasks = new List<Task>();

        var yahooScraper = new YahooScraper();

        Task yahooTop = Task.Run(async () =>
        {
            await yahooScraper.ScrapeTop(assetInfo);
        });
        tasks.Add(yahooTop);

        Task yahooHistory = Task.Run(async () =>
        {
            // 履歴更新の最終日を取得（なければ基準開始日を取得）
            var lastUpdateDay = assetInfo.GetLastHistoryUpdateDay();

            // 最終更新後に直近営業日がある場合は履歴取得
            if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
            {
                await yahooScraper.ScrapeHistory(assetInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate);

                // 株式分割がある場合は履歴をクリアして再取得
                if (assetInfo.HasRecentStockSplitOccurred() && lastUpdateDay != CommonUtils.Instance.MasterStartDate)
                {
                    assetInfo.DeleteHistory(CommonUtils.Instance.ExecusionDate);
                    assetInfo.ScrapedPrices.Clear();
                    await yahooScraper.ScrapeHistory(assetInfo, CommonUtils.Instance.MasterStartDate, CommonUtils.Instance.ExecusionDate);
                }
            }
        });
        tasks.Add(yahooHistory);

        // タスクの実行待ち
        await Task.WhenAll(tasks);

        await Task.CompletedTask;
    }
}

internal class JapaneseETFFormatter : IOutputFormattable
{
    public string ToOutputString(AssetInfo assetInfo)
    {
        // ETF用の出力処理をここに実装
        // 必要に応じてassetInfoのプロパティを参照し、出力フォーマットを整形
        StringBuilder sb = new StringBuilder();

        var mark = CommonUtils.Instance.BadgeString.ShouldWatch;
        var count = 0;
        var s = string.Empty;

        sb.AppendLine($"{assetInfo.Code}：{assetInfo.Name}");

        sb.AppendLine($"株価：{assetInfo.LatestPrice.Price.ToString("N1")}" +
            $"（{assetInfo.LatestPrice.Date.ToString("yy/MM/dd")}" +
            $"：S{assetInfo.LatestPrice.RSIS.ToString("N2")}" +
            $",L{assetInfo.LatestPrice.RSIL.ToString("N2")}" +
            $"）{(assetInfo.LatestPrice.OversoldIndicator() || (assetInfo.IsOwnedNow() && assetInfo.LatestPrice.OverboughtIndicator()) ? mark : string.Empty)}");

        sb.AppendLine($"運用会社：{assetInfo.FundManagementCompany}");
        sb.AppendLine($"信託報酬：{CommonUtils.Instance.ConvertToPercentage(assetInfo.TrustFeeRate, false, "F3")}");

        sb.AppendLine($"信用残：{assetInfo.MarginBuyBalance}/{assetInfo.MarginSellBalance}（{assetInfo.MarginBalanceDate}）");

        sb.AppendLine($"出来高：{assetInfo.LatestTradingVolume}");

        sb.AppendLine($"決算：{assetInfo.EarningsPeriod}");

        s = string.Empty;
        if (!string.IsNullOrEmpty(assetInfo.PressReleaseDate))
        {
            s += assetInfo.PressReleaseDate;
            s += assetInfo.ExtractAndValidateDateWithinOneMonth() ? mark : string.Empty;
            sb.AppendLine($"{s}");
        }

        count = 0;
        s = string.Empty;
        foreach (ExecutionList.Execution e in assetInfo.Executions)
        {
            if (count == 0) sb.AppendLine($"約定履歴：");

            sb.AppendLine($"{e.BuyOrSell}" +
                $"：{e.Date.ToString("yy/MM/dd")}" +
                $"：{e.Price.ToString("N1")}*{e.Quantity}" +
                $"：{CommonUtils.Instance.ConvertToPercentage((assetInfo.LatestPrice.Price / e.Price) - 1, true)}" +
                $"{(assetInfo.ShouldAverageDown(e) ? mark : string.Empty)}");

            count++;
        }

        //チャート：
        sb.AppendLine($"チャート（RSI）：");
        foreach (var p in assetInfo.ChartPrices)
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

        if (!string.IsNullOrEmpty(assetInfo.Memo))
        {
            //メモ：
            sb.AppendLine($"メモ：");
            sb.AppendLine(assetInfo.Memo);
        }

        return sb.ToString();
    }
}