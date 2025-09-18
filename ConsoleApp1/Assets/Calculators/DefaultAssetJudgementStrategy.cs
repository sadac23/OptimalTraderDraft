using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Assets.Calculators
{
    /// <summary>
    /// デフォルトの判定系ストラテジー実装
    /// </summary>
    public class DefaultAssetJudgementStrategy : IAssetJudgementStrategy
    {
        public bool IsPERUndervalued(AssetInfo asset, bool isLenient = false)
            => asset.Per > 0 && asset.Per < (isLenient ? asset.AveragePer * CommonUtils.Instance.LenientFactor : asset.AveragePer);

        public bool IsPBRUndervalued(AssetInfo asset, bool isLenient = false)
            => asset.Pbr > 0 && asset.Pbr < (isLenient ? asset.AveragePbr * CommonUtils.Instance.LenientFactor : asset.AveragePbr);

        public bool IsROEAboveThreshold(AssetInfo asset)
            => asset.FullYearProfits.Count > 0 && asset.FullYearProfits.Last().Roe >= CommonUtils.Instance.ThresholdOfROE;

        public bool IsAnnualProgressOnTrack(AssetInfo asset)
        {
            if (asset.QuarterlyFullyearProgressRate >= asset.PreviousFullyearProgressRate)
            {
                var q = asset.LastQuarterPeriod;
                if (q == CommonUtils.Instance.QuarterString.Quarter1)
                    return asset.QuarterlyFullyearProgressRate >= CommonUtils.Instance.ThresholdOfProgressSuccess.Q1;
                if (q == CommonUtils.Instance.QuarterString.Quarter2)
                    return asset.QuarterlyFullyearProgressRate >= CommonUtils.Instance.ThresholdOfProgressSuccess.Q2;
                if (q == CommonUtils.Instance.QuarterString.Quarter3)
                    return asset.QuarterlyFullyearProgressRate >= CommonUtils.Instance.ThresholdOfProgressSuccess.Q3;
                if (q == CommonUtils.Instance.QuarterString.Quarter4)
                    return asset.QuarterlyFullyearProgressRate >= CommonUtils.Instance.ThresholdOfProgressSuccess.Q4;
            }
            return false;
        }

        public bool IsHighYield(AssetInfo asset)
            => (asset.DividendYield + asset.ShareholderBenefitYield) > CommonUtils.Instance.ThresholdOfYield;

        public bool IsHighMarketCap(AssetInfo asset)
            => asset.MarketCap > CommonUtils.Instance.ThresholdOfMarketCap;

        public bool IsCloseToDividendRecordDate(AssetInfo asset)
            => AssetInfo.IsWithinMonths(asset.DividendRecordDateMonth, 0);

        public bool IsCloseToShareholderBenefitRecordDate(AssetInfo asset)
            => AssetInfo.IsWithinMonths(asset.ShareholderBenefitRecordMonth, 0);

        public bool IsCloseToQuarterEnd(AssetInfo asset)
        {
            if (string.IsNullOrEmpty(asset.PressReleaseDate)) return false;
            var datePattern = @"\d{4}年\d{1,2}月\d{1,2}日";
            var match = System.Text.RegularExpressions.Regex.Match(asset.PressReleaseDate, datePattern);
            if (match.Success)
            {
                if (DateTime.TryParseExact(match.Value, "yyyy年M月d日", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime extractedDate))
                {
                    var oneMonthBefore = CommonUtils.Instance.ExecusionDate;
                    var oneMonthAfter = CommonUtils.Instance.ExecusionDate.AddDays(CommonUtils.Instance.ThresholdOfDaysToQuarterEnd);
                    return extractedDate > oneMonthBefore && extractedDate <= oneMonthAfter;
                }
            }
            return false;
        }

        public bool IsAfterQuarterEnd(AssetInfo asset)
        {
            if (string.IsNullOrEmpty(asset.PressReleaseDate)) return false;
            var datePattern = @"\d{4}年\d{1,2}月\d{1,2}日";
            var match = System.Text.RegularExpressions.Regex.Match(asset.PressReleaseDate, datePattern);
            if (match.Success)
            {
                if (DateTime.TryParseExact(match.Value, "yyyy年M月d日", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime extractedDate))
                {
                    var oneMonthBefore = CommonUtils.Instance.ExecusionDate.AddDays(CommonUtils.Instance.ThresholdOfDaysFromQuarterEnd * -1);
                    var oneMonthAfter = CommonUtils.Instance.ExecusionDate;
                    return extractedDate >= oneMonthBefore && extractedDate < oneMonthAfter;
                }
            }
            return false;
        }

        public bool IsQuarterEnd(AssetInfo asset)
        {
            if (string.IsNullOrEmpty(asset.PressReleaseDate)) return false;
            var datePattern = @"\d{4}年\d{1,2}月\d{1,2}日";
            var match = System.Text.RegularExpressions.Regex.Match(asset.PressReleaseDate, datePattern);
            if (match.Success)
            {
                if (DateTime.TryParseExact(match.Value, "yyyy年M月d日", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime extractedDate))
                {
                    return CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd") == extractedDate.ToString("yyyyMMdd");
                }
            }
            return false;
        }

        public bool IsJustSold(AssetInfo asset)
        {
            foreach (var execution in asset.Executions)
            {
                if (execution.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Sell
                    && execution.Date >= CommonUtils.Instance.ExecusionDate.AddDays(-1 * CommonUtils.Instance.ThresholdOfDaysJustSold))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOwnedNow(AssetInfo asset)
        {
            foreach (var e in asset.Executions)
            {
                if (e.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Buy && !e.HasSellExecuted) return true;
            }
            return false;
        }

        public bool IsGoldenCrossPossible(AssetInfo asset)
        {
            bool result = true;
            double tempSMAdev = 0;
            short count = 0;
            foreach (var item in asset.ChartPrices)
            {
                if (count > 7) break;
                if (item.SMAdev > 0) result = false;
                if (count > 0)
                {
                    if (item.SMAdev >= tempSMAdev) result = false;
                }
                tempSMAdev = item.SMAdev;
                count++;
            }
            return result;
        }

        public bool HasRecentStockSplitOccurred(AssetInfo asset)
        {
            foreach (var item in asset.ScrapedPrices)
            {
                if (item.Close != item.AdjustedClose)
                {
                    CommonUtils.Instance.Logger.LogInformation($"" +
                        $"Code:{asset.Code}, " +
                        $"Message:株式分割あり（" +
                        $"日付：{item.Date.ToString("yyyyMMdd")}, " +
                        $"終値：{item.Close}, " +
                        $"調整後終値：{item.AdjustedClose}）");
                    return true;
                }
            }
            return false;
        }

        public bool ShouldAverageDown(AssetInfo asset, ExecutionList.Execution e)
        {
            if (e.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Buy && !e.HasSellExecuted)
            {
                if (((asset.LatestPrice.Price / e.Price) - 1) <= CommonUtils.Instance.ThresholdOfAverageDown)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsGranvilleCase1Matched(AssetInfo asset)
        {
            if (asset.ChartPrices.Count == 0) return false;
            var start = asset.ChartPrices.First().Price - asset.ChartPrices.First().SMA25;
            var end = asset.ChartPrices.Last().Price - asset.ChartPrices.Last().SMA25;
            if (start < 0 && end > 0) return true;
            return false;
        }

        public bool IsGranvilleCase2Matched(AssetInfo asset)
        {
            if (asset.ChartPrices.Count == 0) return false;
            var start = asset.ChartPrices.First().Price - asset.ChartPrices.First().SMA25;
            var end = asset.ChartPrices.Last().Price - asset.ChartPrices.Last().SMA25;
            if (start > 0 && end > 0)
            {
                foreach (var item in asset.ChartPrices)
                {
                    var dev = item.Price - item.SMA25;
                    if (dev < 0) return true;
                }
            }
            return false;
        }

        public bool HasDisclosure(AssetInfo asset)
        {
            foreach (var item in asset.Disclosures)
            {
                if (item.Datetime.ToString("yyyyMMdd") == CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsRecordDate(AssetInfo asset)
        {
            return asset.IsDividendRecordDate() || asset.IsShareholderBenefitRecordDate();
        }

        public bool IsAfterRecordDate(AssetInfo asset)
        {
            return asset.IsAfterDividendRecordDate() || asset.IsAfterShareholderBenefitRecordDate();
        }

        public bool IsCloseToRecordDate(AssetInfo asset)
        {
            return asset.IsCloseToDividendRecordDate() || asset.IsCloseToShareholderBenefitRecordDate();
        }

        public bool ExtractAndValidateDateWithinOneMonth(AssetInfo asset)
        {
            if (string.IsNullOrEmpty(asset.PressReleaseDate)) return false;
            var datePattern = @"\d{4}年\d{1,2}月\d{1,2}日";
            var match = System.Text.RegularExpressions.Regex.Match(asset.PressReleaseDate, datePattern);
            if (match.Success)
            {
                if (DateTime.TryParseExact(match.Value, "yyyy年M月d日", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime extractedDate))
                {
                    var oneMonthBefore = CommonUtils.Instance.ExecusionDate.AddMonths(-1);
                    var oneMonthAfter = CommonUtils.Instance.ExecusionDate.AddMonths(1);
                    return extractedDate >= oneMonthBefore && extractedDate <= oneMonthAfter;
                }
            }
            return false;
        }
    }
}