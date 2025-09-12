using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Assets.Repositories;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using System.Text;

public class JapaneseStockInfo : AssetInfo
{
    // �V����Repository�Ή��̃R���X�g���N�^�i�����j
    public JapaneseStockInfo(
        WatchList.WatchStock watchStock,
        IExternalSourceUpdatable updater,
        IOutputFormattable formatter,
        IAssetRepository repository)
        : base(watchStock, updater, formatter, repository)
    {
    }

    // �K�v�ɉ����ē��{���ŗL�̃v���p�e�B�⃁�\�b�h��ǉ��\

}

public class JapaneseStockUpdater : IExternalSourceUpdatable
{
    public async Task UpdateFromExternalSourceAsync(AssetInfo stockInfo)
    {
        // ���{���p�̊O�����擾�����������Ɏ���
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

        Task yahooHistory = Task.Run(async () =>
        {
            var lastUpdateDay = stockInfo.GetLastHistoryUpdateDay();

            if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
            {
                await yahooScraper.ScrapeHistory(stockInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate);

                if (stockInfo.HasRecentStockSplitOccurred() && lastUpdateDay != CommonUtils.Instance.MasterStartDate)
                {
                    // Repository�p�^�[���Ή��FDB�폜�͔񓯊����\�b�h�𗘗p
                    if (stockInfo.Repository != null)
                        await stockInfo.DeleteHistoryAsync(CommonUtils.Instance.ExecusionDate);
                    stockInfo.ScrapedPrices.Clear();
                    await yahooScraper.ScrapeHistory(stockInfo, CommonUtils.Instance.MasterStartDate, CommonUtils.Instance.ExecusionDate);
                }
            }
        });
        tasks.Add(yahooHistory);

        await Task.WhenAll(tasks);
    }
}

public class JapaneseStockFormatter : IOutputFormattable
{
    public string ToOutputString(AssetInfo stockInfo)
    {
        // �����̏o�͏��������̂܂܈ێ�
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