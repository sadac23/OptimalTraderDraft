// See https://aka.ms/new-console-template for more information
using System.Text;
using ConsoleApp1.Assets;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

public class IndexInfo : AssetInfo
{
    // Factory以外からの直接生成を禁止
    internal IndexInfo(
        WatchList.WatchStock watchStock, 
        AssetInfoDependencies deps)
        : base(watchStock, deps)
    {
    }

}

// インデックス種別用の外部情報取得処理
internal class IndexUpdater : IExternalSourceUpdatable
{
    public async Task UpdateFromExternalSourceAsync(AssetInfo stockInfo)
    {
        List<Task> tasks = new List<Task>();

        var yahooScraper = new YahooScraper();

        Task yahooTop = Task.Run(async () =>
        {
            await yahooScraper.ScrapeTop(stockInfo);
        });
        tasks.Add(yahooTop);

        Task yahooHistory = Task.Run(async () =>
        {
            var lastUpdateDay = stockInfo.GetLastHistoryUpdateDay();

            if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
            {
                await yahooScraper.ScrapeHistory(stockInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate);
            }
        });
        tasks.Add(yahooHistory);

        await Task.WhenAll(tasks);
    }
}

// インデックス種別用の出力処理
public class IndexFormatter : IOutputFormattable
{
    public string ToOutputString(AssetInfo stockInfo)
    {
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
            sb.AppendLine($"メモ：");
            sb.AppendLine(stockInfo.Memo);
        }

        return sb.ToString();
    }
}
