using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using ConsoleApp1.Scraper.Contracts;
using ConsoleApp1.Scraper.Strategies;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ConsoleApp1.Assets;

public sealed class JapaneseStockInfo : AssetInfo
{
    // Factory以外からの直接生成を禁止
    internal JapaneseStockInfo(WatchList.WatchStock watchStock, AssetInfoDependencies deps)
        : base(watchStock, deps)
    {
    }

    // 必要に応じて日本株固有のプロパティやメソッドを追加可能

}

public class JapaneseStockUpdater : IExternalSourceUpdatable
{
    public async Task UpdateFromExternalSourceAsync(AssetInfo stockInfo)
    {
        List<Task> tasks = new List<Task>();

        var kabutanStrategy = new ConsoleApp1.Scraper.Strategies.JapaneseStockKabutanScrapeStrategy();
        var kabutanScraper = new KabutanScraper(kabutanStrategy);

        var minkabuStrategy = new ConsoleApp1.Scraper.Strategies.JapaneseStockMinkabuScrapeStrategy();
        var minkabuScraper = new MinkabuScraper(minkabuStrategy);

        var yahooStrategy = new ConsoleApp1.Scraper.Strategies.JapaneseStockYahooScrapeStrategy();
        var yahooScraper = new YahooScraper(yahooStrategy);

        tasks.Add(Task.Run(async () =>
        {
            try { await kabutanScraper.ScrapeAsync(stockInfo, ScrapeTarget.Finance); }
            catch (Exception ex) { CommonUtils.Instance.Logger.LogError($"KabutanFinance失敗: {ex.Message}", ex); throw; }
        }));

        tasks.Add(Task.Run(async () =>
        {
            try { await minkabuScraper.ScrapeAsync(stockInfo, ScrapeTarget.Dividend); }
            catch (Exception ex) { CommonUtils.Instance.Logger.LogError($"MinkabuDividend失敗: {ex.Message}", ex); throw; }
        }));

        tasks.Add(Task.Run(async () =>
        {
            try { await minkabuScraper.ScrapeAsync(stockInfo, ScrapeTarget.Yutai); }
            catch (Exception ex) { CommonUtils.Instance.Logger.LogError($"MinkabuYutai失敗: {ex.Message}", ex); throw; }
        }));

        tasks.Add(Task.Run(async () =>
        {
            try { await yahooScraper.ScrapeAsync(stockInfo, ScrapeTarget.Top); }
            catch (Exception ex) { CommonUtils.Instance.Logger.LogError($"YahooTop失敗: {ex.Message}", ex); throw; }
        }));

        tasks.Add(Task.Run(async () =>
        {
            try { await yahooScraper.ScrapeAsync(stockInfo, ScrapeTarget.Profile); }
            catch (Exception ex) { CommonUtils.Instance.Logger.LogError($"YahooProfile失敗: {ex.Message}", ex); throw; }
        }));

        tasks.Add(Task.Run(async () =>
        {
            try
            {
                var lastUpdateDay = stockInfo.GetLastHistoryUpdateDay();
                if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
                {
                    await yahooScraper.ScrapeAsync(stockInfo, ScrapeTarget.History);
                    if (stockInfo.HasRecentStockSplitOccurred() && lastUpdateDay != CommonUtils.Instance.MasterStartDate)
                    {
                        if (stockInfo.Repository != null)
                            await stockInfo.DeleteHistoryAsync(CommonUtils.Instance.ExecusionDate);
                        stockInfo.ScrapedPrices.Clear();
                        await yahooScraper.ScrapeAsync(stockInfo, ScrapeTarget.History);
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

public class JapaneseStockFormatter : IOutputFormattable
{
    public string ToOutputString(AssetInfo stockInfo)
    {
        // 既存の出力処理をそのまま維持
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
        foreach (FullYearProfit p in stockInfo.FullYearProfits)
        {
            if (count > 0) s += "→";
            s += p.Roe;
            count++;
        }
        if (!string.IsNullOrEmpty(s))
            sb.AppendLine($"ROE：{s}{(stockInfo.IsROEAboveThreshold() ? mark : string.Empty)}");

        sb.AppendLine($"PER：{CommonUtils.Instance.ConvertToMultiplierString(stockInfo.Per)}" +
            $"（{stockInfo.AveragePer.ToString("N1")}）{(stockInfo.IsPERUndervalued() ? mark : string.Empty)}");

        sb.AppendLine($"PEGレシオ：{stockInfo.PEGRatio.ToString("N2")}");

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