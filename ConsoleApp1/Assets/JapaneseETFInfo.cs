// See https://aka.ms/new-console-template for more information
using ConsoleApp1.Assets;
using System.Text;

internal class JapaneseETFInfo : AssetInfo
{
    public JapaneseETFInfo(WatchList.WatchStock watchStock) : base(watchStock)
    {
    }
    internal override string ToOutputString()
    {
        StringBuilder sb = new StringBuilder();

        var mark = CommonUtils.Instance.BadgeString.ShouldWatch;
        var count = 0;
        var s = string.Empty;

        sb.AppendLine($"{this.Code}：{this.Name}");

        sb.AppendLine($"株価：{this.LatestPrice.Price.ToString("N1")}" +
            $"（{this.LatestPrice.Date.ToString("yy/MM/dd")}" +
            $"：S{this.LatestPrice.RSIS.ToString("N2")}" +
            $",L{this.LatestPrice.RSIL.ToString("N2")}" +
            $"）{(this.LatestPrice.OversoldIndicator() || (this.IsOwnedNow() && this.LatestPrice.OverboughtIndicator()) ? mark : string.Empty)}");

        sb.AppendLine($"運用会社：{this.FundManagementCompany}");
        sb.AppendLine($"信託報酬：{CommonUtils.Instance.ConvertToPercentage(this.TrustFeeRate, false, "F3")}");

        sb.AppendLine($"信用残：{this.MarginBuyBalance}/{this.MarginSellBalance}（{this.MarginBalanceDate}）");

        sb.AppendLine($"出来高：{this.LatestTradingVolume}");

        sb.AppendLine($"決算：{this.EarningsPeriod}");

        s = string.Empty;
        if (!string.IsNullOrEmpty(this.PressReleaseDate))
        {
            s += this.PressReleaseDate;
            s += this.ExtractAndValidateDateWithinOneMonth() ? mark : string.Empty;
            sb.AppendLine($"{s}");
        }

        count = 0;
        s = string.Empty;
        foreach (ExecutionList.Execution e in this.Executions)
        {
            if (count == 0) sb.AppendLine($"約定履歴：");

            sb.AppendLine($"{e.BuyOrSell}" +
                $"：{e.Date.ToString("yy/MM/dd")}" +
                $"：{e.Price.ToString("N1")}*{e.Quantity}" +
                $"：{CommonUtils.Instance.ConvertToPercentage((this.LatestPrice.Price / e.Price) - 1, true)}" +
                $"{(this.ShouldAverageDown(e) ? mark : string.Empty)}");

            count++;
        }

        //チャート：
        sb.AppendLine($"チャート（RSI）：");
        foreach (var p in this.ChartPrices)
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

        if (!string.IsNullOrEmpty(this.Memo))
        {
            //メモ：
            sb.AppendLine($"メモ：");
            sb.AppendLine(this.Memo);
        }

        return sb.ToString();
    }

    internal override async Task UpdateFromExternalSource()
    {
        List<Task> tasks = new List<Task>();

        var yahooScraper = new YahooScraper();

        Task yahooTop = Task.Run(async () =>
        {
            await yahooScraper.ScrapeTop(this);
        });
        tasks.Add(yahooTop);

        Task yahooHistory = Task.Run(async () =>
        {
            // 履歴更新の最終日を取得（なければ基準開始日を取得）
            var lastUpdateDay = this.GetLastHistoryUpdateDay();

            // 最終更新後に直近営業日がある場合は履歴取得
            if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
            {
                await yahooScraper.ScrapeHistory(this, lastUpdateDay, CommonUtils.Instance.ExecusionDate);

                // 株式分割がある場合は履歴をクリアして再取得
                if (this.HasRecentStockSplitOccurred() && lastUpdateDay != CommonUtils.Instance.MasterStartDate)
                {
                    this.DeleteHistory(CommonUtils.Instance.ExecusionDate);
                    this.ScrapedPrices.Clear();
                    await yahooScraper.ScrapeHistory(this, CommonUtils.Instance.MasterStartDate, CommonUtils.Instance.ExecusionDate);
                }
            }
        });
        tasks.Add(yahooHistory);

        // タスクの実行待ち
        await Task.WhenAll(tasks);
    }
}