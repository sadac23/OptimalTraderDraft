using ConsoleApp1.Assets.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ConsoleApp1.Assets.Calculators
{
    public class DefaultAssetCalculator : IAssetCalculator
    {
        public void UpdateFullYearPerformanceForcastSummary(AssetInfo asset)
        {
            foreach (var p in asset.FullYearPerformancesForcasts)
            {
                var secondLast = asset.FullYearPerformances[asset.FullYearPerformances.Count - 3];
                if (p.Category == CommonUtils.Instance.ForecastCategoryString.Final)
                {
                    secondLast = asset.FullYearPerformances[asset.FullYearPerformances.Count - 4];
                }

                p.Summary += GetRevenueIncreasedSummary(p.Revenue, secondLast.Revenue);
                p.Summary += GetOrdinaryIncomeIncreasedSummary(p.OrdinaryProfit, secondLast.OrdinaryProfit);
                p.Summary += GetDividendPerShareIncreasedSummary(p.RevisedDividend, secondLast.AdjustedDividendPerShare);
                p.Summary += $"（{GetIncreasedRate(p.Revenue, secondLast.Revenue)}";
                p.Summary += $",{GetIncreasedRate(p.OrdinaryProfit, secondLast.OrdinaryProfit)}";
                p.Summary += $",{GetDividendPerShareIncreased(p.RevisedDividend, secondLast.AdjustedDividendPerShare)}）";
            }
        }

        public void UpdateDividendPayoutRatio(AssetInfo asset)
        {
            asset.DividendPayoutRatio = 0;
            if (asset.FullYearPerformances.Count >= 2)
            {
                var lastValue = asset.FullYearPerformances[asset.FullYearPerformances.Count - 2];
                asset.DividendPayoutRatio = GetDividendPayoutRatio(lastValue.AdjustedDividendPerShare, lastValue.AdjustedEarningsPerShare);
                asset.Doe = GetDOE(asset.DividendPayoutRatio, asset.FullYearProfits.Last().Roe);
            }
        }

        public void UpdateProgress(AssetInfo asset)
        {
            double fullYearOrdinaryIncome = 0;
            double latestOrdinaryIncome = 0;
            double previousOrdinaryIncome = 0;

            var quarterlyPerformance = GetQuarterlyPerformance(asset, CommonUtils.Instance.PeriodString.Current);
            var fullYearPerformance = GetFullYearForecast(asset, CommonUtils.Instance.PeriodString.Current);

            asset.QuarterlyPerformanceReleaseDate = ConvertToDateTime(quarterlyPerformance.ReleaseDate);

            latestOrdinaryIncome = quarterlyPerformance.OrdinaryProfit;
            fullYearOrdinaryIncome = CommonUtils.Instance.GetDouble(fullYearPerformance.OrdinaryProfit);

            if (fullYearOrdinaryIncome > 0)
            {
                asset.QuarterlyFullyearProgressRate = latestOrdinaryIncome / fullYearOrdinaryIncome;
            }

            var revenue = CommonUtils.Instance.GetDouble(quarterlyPerformance.Revenue);
            var operatingProfit = CommonUtils.Instance.GetDouble(quarterlyPerformance.OperatingProfit);
            asset.OperatingProfitMargin = operatingProfit / revenue;

            var previousQuarterlyPerformance = GetQuarterlyPerformance(asset, CommonUtils.Instance.PeriodString.Previous);
            var previousFullYearPerformance = GetFullYearForecast(asset, CommonUtils.Instance.PeriodString.Previous);

            asset.PreviousPerformanceReleaseDate = ConvertToDateTime(previousQuarterlyPerformance.ReleaseDate);

            previousOrdinaryIncome = previousQuarterlyPerformance.OrdinaryProfit;
            fullYearOrdinaryIncome = CommonUtils.Instance.GetDouble(previousFullYearPerformance.OrdinaryProfit);

            if (fullYearOrdinaryIncome > 0)
            {
                asset.PreviousFullyearProgressRate = previousOrdinaryIncome / fullYearOrdinaryIncome;
            }

            asset.QuarterlyOperatingProfitMarginYoY = CommonUtils.Instance.CalculateYearOverYearGrowth(previousOrdinaryIncome, latestOrdinaryIncome);
        }

        public void SetupChartPrices(AssetInfo asset)
        {
            var analizer = new Analyzer();
            var rows = asset.Repository.GetChartPriceRows(asset.Code, CommonUtils.Instance.ChartDays + 1);

            if (asset.LatestScrapedPrice != null &&
                !rows.Any(r => ((DateTime)r["date"]) == asset.LatestScrapedPrice.Date))
            {
                var latestRow = new Dictionary<string, object>
                {
                    { "code", asset.Code },
                    { "date", asset.LatestScrapedPrice.Date },
                    { "open", asset.LatestScrapedPrice.Open },
                    { "high", asset.LatestScrapedPrice.High },
                    { "low", asset.LatestScrapedPrice.Low },
                    { "close", asset.LatestScrapedPrice.Close },
                    { "volume", asset.LatestScrapedPrice.Volume }
                };
                rows.Add(latestRow);
            }

            rows.Sort((a, b) => ((DateTime)a["date"]).CompareTo((DateTime)b["date"]));

            ChartPrice previousPrice = null;
            List<ChartPrice> prices = new List<ChartPrice>();

            foreach (var row in rows)
            {
                string code = row["code"].ToString();
                DateTime date = (DateTime)row["date"];
                double close = Convert.ToDouble(row["close"]);
                double sma25 = Analyzer.GetSMA(25, date, asset.Code);
                double sma75 = Analyzer.GetSMA(75, date, asset.Code);

                ChartPrice price = new ChartPrice()
                {
                    Date = date,
                    Price = close,
                    Volatility = previousPrice != null ? (close / previousPrice.Price) - 1 : 0,
                    RSIL = analizer.GetCutlerRSI(CommonUtils.Instance.RSILongPeriodDays, date, asset),
                    RSIS = analizer.GetCutlerRSI(CommonUtils.Instance.RSIShortPeriodDays, date, asset),
                    SMA25 = sma25,
                    SMA75 = sma75,
                    SMAdev = sma25 - sma75,
                    MADS = (close - sma25) / sma25,
                    MADL = (close - sma75) / sma75,
                };

                prices.Add(price);
                previousPrice = (ChartPrice)price.Clone();
            }

            asset.ChartPrices = prices.OrderByDescending(p => p.Date).Take(CommonUtils.Instance.ChartDays).ToList();
        }

        // --- 以下はprivateメソッドの移植 ---

        public double GetDividendPayoutRatio(string adjustedDividendPerShare, string adjustedEarningsPerShare)
        {
            if (double.TryParse(adjustedDividendPerShare, out double result1) && double.TryParse(adjustedEarningsPerShare, out double result2))
            {
                return result1 / result2;
            }
            return 0;
        }

        public double GetDOE(double dividendPayoutRatio, double roe)
        {
            return dividendPayoutRatio * (roe / 100);
        }

        public string GetDividendPerShareIncreased(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
        {
            if (double.TryParse(adjustedDividendPerShare1, out double result1) && double.TryParse(adjustedDividendPerShare2, out double result2))
            {
                return ConvertToStringWithSign(Math.Floor(result1 - result2));
            }
            return "？";
        }

        public string GetIncreasedRate(string lastValue, string secondLastValue)
        {
            if (double.TryParse(lastValue, out double result1) && double.TryParse(secondLastValue, out double result2))
            {
                return ConvertToPercentageStringWithSign(CommonUtils.Instance.CalculateYearOverYearGrowth(result2, result1));
            }
            return "？";
        }

        public string ConvertToPercentageStringWithSign(double v)
        {
            double percentage = v * 100;
            if (percentage >= 0)
            {
                return "+" + percentage.ToString("0.0") + "%";
            }
            else
            {
                return percentage.ToString("0.0") + "%";
            }
        }

        public string ConvertToStringWithSign(double number)
        {
            if (number >= 0)
            {
                return "+" + number.ToString();
            }
            else
            {
                return number.ToString();
            }
        }

        public string GetDividendPerShareIncreasedSummary(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
        {
            if (double.TryParse(adjustedDividendPerShare1, out double result1) && double.TryParse(adjustedDividendPerShare2, out double result2))
            {
                if (result1 > result2) return "↑";
                if (result1 == result2) return "→";
                return "↓";
            }
            return "？";
        }

        public string GetOrdinaryIncomeIncreasedSummary(string ordinaryIncome1, string ordinaryIncome2)
        {
            if (double.TryParse(ordinaryIncome1, out double result1) && double.TryParse(ordinaryIncome2, out double result2))
            {
                return result1 > result2 ? "↑" : "↓";
            }
            return "？";
        }

        public string GetRevenueIncreasedSummary(string revenue1, string revenue2)
        {
            if (double.TryParse(revenue1, out double result1) && double.TryParse(revenue2, out double result2))
            {
                return result1 > result2 ? "↑" : "↓";
            }
            return "？";
        }

        // --- AssetInfoのprivateメソッドの一部をstatic/privateで移植 ---
        private QuarterlyPerformance GetQuarterlyPerformance(AssetInfo asset, string period)
        {
            QuarterlyPerformance result = new QuarterlyPerformance();
            var refIndex = period == CommonUtils.Instance.PeriodString.Current ? 2 : 3;
            if (asset.QuarterlyPerformances.Count >= refIndex)
            {
                result = asset.QuarterlyPerformances[asset.QuarterlyPerformances.Count - refIndex];
            }
            return result;
        }

        private FullYearPerformance GetFullYearForecast(AssetInfo asset, string periodString)
        {
            FullYearPerformance result = new FullYearPerformance();
            var refIndex = periodString == CommonUtils.Instance.PeriodString.Current ? 2 : 3;
            if (asset.FullYearPerformances.Count >= refIndex)
            {
                if (asset.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter4)
                {
                    // AddYears(-1)の前にCurrentFiscalMonthの値をチェック
                    if (asset.CurrentFiscalMonth > DateTime.MinValue.AddYears(1))
                    {
                        var prevFiscalPeriod = asset.CurrentFiscalMonth.AddYears(-1).ToString("yyyy.MM");
                        var previousFinalForcast = asset.FullYearPerformancesForcasts
                            .Where(forcast =>
                                forcast.FiscalPeriod == prevFiscalPeriod &&
                                (forcast.Category == CommonUtils.Instance.ForecastCategoryString.Initial ||
                                 forcast.Category == CommonUtils.Instance.ForecastCategoryString.Revised))
                            .LastOrDefault();

                        if (previousFinalForcast != null)
                        {
                            result.FiscalPeriod = previousFinalForcast.FiscalPeriod;
                            result.Revenue = previousFinalForcast.Revenue;
                            result.OperatingProfit = previousFinalForcast.OperatingProfit;
                            result.OrdinaryProfit = previousFinalForcast.OrdinaryProfit;
                            result.NetProft = previousFinalForcast.NetProfit;
                            result.AdjustedEarningsPerShare = string.Empty;
                            result.AdjustedDividendPerShare = previousFinalForcast.RevisedDividend;
                            result.AnnouncementDate = previousFinalForcast.RevisionDate.ToString();
                        }
                        else
                        {
                            result = asset.FullYearPerformances[asset.FullYearPerformances.Count - (refIndex + 1)];
                        }
                    }
                    else
                    {
                        // 不正なCurrentFiscalMonthの場合はログ出力し、直近の履歴を返す
                        CommonUtils.Instance.Logger.LogWarning(
                            $"CurrentFiscalMonthが不正なため、AddYears(-1)をスキップします。Code:{asset.Code}, Value:{asset.CurrentFiscalMonth:yyyy/MM/dd}");
                        result = asset.FullYearPerformances[asset.FullYearPerformances.Count - refIndex];
                    }
                }
                else
                {
                    result = asset.FullYearPerformances[asset.FullYearPerformances.Count - refIndex];
                }
            }
            return result;
        }

        private DateTime ConvertToDateTime(string releaseDate)
        {
            try
            {
                return DateTime.ParseExact(releaseDate, "yy/MM/dd", CultureInfo.InvariantCulture);
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }
}