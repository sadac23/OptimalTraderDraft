// See https://aka.ms/new-console-template for more information
using ConsoleApp1.Assets.Models;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using Microsoft.Extensions.Logging;
using System.Text;
using ConsoleApp1.Scraper.Contracts;
using ConsoleApp1.Scraper.Strategies;

namespace ConsoleApp1.Assets;

/// <summary>
/// �č��ʊ��p�̎��Y���N���X
/// </summary>
public sealed class USStockInfo : AssetInfo
{
    // Factory�ȊO����̒��ڐ������֎~
    internal USStockInfo(
        WatchList.WatchStock watchStock,
        AssetInfoDependencies deps)
        : base(watchStock, deps)
    {
    }

    // �K�v�ɉ����ĕč����ŗL�̃v���p�e�B�⃁�\�b�h��ǉ��\
}

// �č��ʊ��p�̊O�����擾����
internal class USStockUpdater : IExternalSourceUpdatable
{
    public async Task UpdateFromExternalSourceAsync(AssetInfo stockInfo)
    {
        List<Task> tasks = new List<Task>();

        // Strategy�𒍓�����YahooScraper�𗘗p
        var yahooStrategy = new USStockYahooScrapeStrategy();
        var yahooScraper = new YahooScraper(yahooStrategy);

        // Top���擾
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                await yahooScraper.ScrapeAsync(stockInfo, ScrapeTarget.Top);
            }
            catch (Exception ex)
            {
                CommonUtils.Instance.Logger.LogError($"YahooTop���s: {ex.Message}", ex);
                throw;
            }
        }));

        // �������擾
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                var lastUpdateDay = stockInfo.GetLastHistoryUpdateDay();
                if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
                {
                    await yahooScraper.ScrapeAsync(stockInfo, ScrapeTarget.History);
                }
            }
            catch (Exception ex)
            {
                CommonUtils.Instance.Logger.LogError($"YahooHistory���s: {ex.Message}", ex);
                throw;
            }
        }));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            CommonUtils.Instance.Logger.LogError($"UpdateFromExternalSourceAsync�ŗ�O: {ex.Message}", ex);
            throw;
        }
    }
}

// �č��ʊ��p�̏o�͏���
public class USStockFormatter : IOutputFormattable
{
    public string ToOutputString(AssetInfo stockInfo)
    {
        // JapaneseStockFormatter�̏o�͏������x�[�X�ɈڐA
        StringBuilder sb = new StringBuilder();

        var mark = CommonUtils.Instance.BadgeString.ShouldWatch;
        var count = 0;
        var s = string.Empty;

        sb.AppendLine($"{stockInfo.Code}�F{stockInfo.Name}");

        sb.AppendLine($"�����F{stockInfo.LatestPrice.Price.ToString("N1")}" +
            $"�i{stockInfo.LatestPrice.Date.ToString("yy/MM/dd")}" +
            $"�FS{stockInfo.LatestPrice.RSIS.ToString("N2")}" +
            $",L{stockInfo.LatestPrice.RSIL.ToString("N2")}" +
            $"�j{(stockInfo.LatestPrice.OversoldIndicator() || (stockInfo.IsOwnedNow() && stockInfo.LatestPrice.OverboughtIndicator()) ? mark : string.Empty)}");

        sb.AppendLine($"�s��/�Ǝ�F{stockInfo.Section}{(!string.IsNullOrEmpty(stockInfo.Industry) ? $"/{stockInfo.Industry}" : string.Empty)}");

        sb.AppendLine($"�z�������F{CommonUtils.Instance.ConvertToPercentage(stockInfo.DividendYield)}" +
            $"�i{CommonUtils.Instance.ConvertToPercentage(stockInfo.DividendPayoutRatio)}" +
            $",{CommonUtils.Instance.ConvertToPercentage(stockInfo.Doe)}" +
            $",{stockInfo.DividendRecordDateMonth}" +
            $"�j{(stockInfo.IsCloseToDividendRecordDate() ? mark : string.Empty)}");

        if (!string.IsNullOrEmpty(stockInfo.ShareholderBenefitsDetails))
            sb.AppendLine($"�D�җ����F{CommonUtils.Instance.ConvertToPercentage(stockInfo.ShareholderBenefitYield)}" +
                $"�i{stockInfo.ShareholderBenefitsDetails}" +
                $",{stockInfo.NumberOfSharesRequiredForBenefits}" +
                $",{stockInfo.ShareholderBenefitRecordMonth}" +
                $",{stockInfo.ShareholderBenefitRecordDay}" +
                $"�j{(stockInfo.IsCloseToShareholderBenefitRecordDate() ? mark : string.Empty)}");

        foreach (var p in stockInfo.FullYearPerformancesForcasts)
        {
            if (count == 0) sb.AppendLine($"�ʊ��\�z�i�O����j�F");
            sb.AppendLine($"{p.Category}" +
                $"�F{p.RevisionDate.ToString("yy/MM/dd")}" +
                $"�F{p.Summary}{(p.HasUpwardRevision() ? mark : string.Empty)}");
            count++;
        }

        sb.AppendLine($"�ʊ��i���F{stockInfo.LastQuarterPeriod}" +
            $"�F{CommonUtils.Instance.ConvertToPercentage(stockInfo.QuarterlyFullyearProgressRate)}" +
            $"�i{stockInfo.QuarterlyPerformanceReleaseDate.ToString("yy/MM/dd")}" +
            $"�F{CommonUtils.Instance.ConvertToPercentage(stockInfo.QuarterlyOperatingProfitMarginYoY, true)}�j" +
            $"{(stockInfo.IsAnnualProgressOnTrack() ? mark : string.Empty)}");

        sb.AppendLine($"�O���i���F{stockInfo.LastQuarterPeriod}" +
            $"�F{CommonUtils.Instance.ConvertToPercentage(stockInfo.PreviousFullyearProgressRate)}" +
            $"�i{stockInfo.PreviousPerformanceReleaseDate.ToString("yy/MM/dd")}�j");

        sb.AppendLine($"�������z�F{CommonUtils.Instance.ConvertToYenNotation(stockInfo.MarketCap)}");

        count = 0;
        s = string.Empty;
        foreach (FullYearProfit p in stockInfo.FullYearProfits)
        {
            if (count > 0) s += "��";
            s += p.Roe;
            count++;
        }
        if (!string.IsNullOrEmpty(s))
            sb.AppendLine($"ROE�F{s}{(stockInfo.IsROEAboveThreshold() ? mark : string.Empty)}");

        sb.AppendLine($"PER�F{CommonUtils.Instance.ConvertToMultiplierString(stockInfo.Per)}" +
            $"�i{stockInfo.AveragePer.ToString("N1")}�j{(stockInfo.IsPERUndervalued() ? mark : string.Empty)}");

        sb.AppendLine($"PEG���V�I�F{stockInfo.PEGRatio.ToString("N2")}");

        sb.AppendLine($"PBR�F{CommonUtils.Instance.ConvertToMultiplierString(stockInfo.Pbr)}" +
            $"�i{stockInfo.AveragePbr.ToString("N1")}�j{(stockInfo.IsPBRUndervalued() ? mark : string.Empty)}");

        sb.AppendLine($"�c�Ɨ��v���F{CommonUtils.Instance.ConvertToPercentage(stockInfo.OperatingProfitMargin)}");

        sb.AppendLine($"�M�p�{���F{stockInfo.MarginBalanceRatio}");

        sb.AppendLine($"�M�p�c�F{stockInfo.MarginBuyBalance}/{stockInfo.MarginSellBalance}�i{stockInfo.MarginBalanceDate}�j");

        sb.AppendLine($"�o�����F{stockInfo.LatestTradingVolume}");

        sb.AppendLine($"���Ȏ��{�䗦�F{stockInfo.EquityRatio}");

        sb.AppendLine($"���Z�F{stockInfo.EarningsPeriod}");

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
            if (count == 0) sb.AppendLine($"��藚���F");

            sb.AppendLine($"{e.BuyOrSell}" +
                $"�F{e.Date.ToString("yy/MM/dd")}" +
                $"�F{e.Price.ToString("N1")}*{e.Quantity}" +
                $"�F{CommonUtils.Instance.ConvertToPercentage((stockInfo.LatestPrice.Price / e.Price) - 1, true)}" +
                $"{(stockInfo.ShouldAverageDown(e) ? mark : string.Empty)}");

            count++;
        }

        sb.AppendLine($"�`���[�g�iRSI�j�F");
        foreach (var p in stockInfo.ChartPrices)
        {
            sb.AppendLine(
                $"{p.Date.ToString("MM/dd")}" +
                $"�F{p.Price.ToString("N1")}" +
                $"�F{CommonUtils.Instance.ConvertToPercentage(p.Volatility, true)}" +
                $"�iS{p.RSIS.ToString("N2")}" +
                $",L{p.RSIL.ToString("N2")}�j" +
                $"{(p.OversoldIndicator() ? mark : string.Empty)}" +
                $"");
        }

        if (!string.IsNullOrEmpty(stockInfo.Memo))
        {
            sb.AppendLine($"�����F");
            sb.AppendLine(stockInfo.Memo);
        }

        return sb.ToString();
    }
}