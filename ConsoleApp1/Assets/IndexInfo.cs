// See https://aka.ms/new-console-template for more information
using System.Text;
using ConsoleApp1.Assets;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using Microsoft.Extensions.Logging;

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

        // Top情報取得（例外はログ＋伝播）
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                await yahooScraper.ScrapeTop(stockInfo);
            }
            catch (Exception ex)
            {
                CommonUtils.Instance.Logger.LogError($"YahooTop失敗: {ex.Message}", ex);
                throw;
            }
        }));

        // 履歴情報取得（リトライあり、例外はログ＋伝播）
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                var lastUpdateDay = stockInfo.GetLastHistoryUpdateDay();
                if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
                {
                    // 最大3回リトライ、1秒待機
                    await yahooScraper.ScrapeHistory(stockInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate, 3, 1000);
                }
            }
            catch (Exception ex)
            {
                CommonUtils.Instance.Logger.LogError($"YahooHistory失敗: {ex.Message}", ex);
                throw;
            }
        }));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            CommonUtils.Instance.Logger.LogError($"UpdateFromExternalSourceAsyncで例外: {ex.Message}", ex);
            throw;
        }
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
