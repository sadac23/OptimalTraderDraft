// See https://aka.ms/new-console-template for more information
using ConsoleApp1.Assets;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ConsoleApp1.Assets;

public class JapaneseETFInfo : AssetInfo
{
    // Factory以外からの直接生成を禁止
    internal JapaneseETFInfo(
        WatchList.WatchStock watchStock,
        AssetInfoDependencies deps)
        : base(watchStock, deps)
    {
    }

    // 必要に応じてETF固有のプロパティやメソッドを追加可能
}

internal class JapaneseETFUpdater : IExternalSourceUpdatable
{
    public async Task UpdateFromExternalSourceAsync(AssetInfo assetInfo)
    {
        List<Task> tasks = new List<Task>();
        var yahooScraper = new YahooScraper();

        // Top情報取得（リトライなし、例外はログ＋伝播）
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                await yahooScraper.ScrapeTop(assetInfo);
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
                var lastUpdateDay = assetInfo.GetLastHistoryUpdateDay();
                if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
                {
                    // 履歴取得（最大3回リトライ、1秒待機）
                    await yahooScraper.ScrapeHistory(assetInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate, 3, 1000);

                    if (assetInfo.HasRecentStockSplitOccurred() && lastUpdateDay != CommonUtils.Instance.MasterStartDate)
                    {
                        if (assetInfo.Repository != null)
                            await assetInfo.DeleteHistoryAsync(CommonUtils.Instance.ExecusionDate);
                        assetInfo.ScrapedPrices.Clear();
                        await yahooScraper.ScrapeHistory(assetInfo, CommonUtils.Instance.MasterStartDate, CommonUtils.Instance.ExecusionDate, 3, 1000);
                    }
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