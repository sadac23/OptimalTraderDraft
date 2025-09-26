using System;
using System.Collections.Generic;
using ConsoleApp1.Assets;
using ConsoleApp1.Scraper.Contracts;
using HtmlAgilityPack;
using System.Globalization;

namespace ConsoleApp1.Scraper.Strategies
{
    public class JapaneseStockMinkabuScrapeStrategy : IAssetScrapeStrategy
    {
        public string GetUrl(AssetInfo assetInfo, ScrapeTarget target)
        {
            return target switch
            {
                ScrapeTarget.Dividend => $"https://minkabu.jp/stock/{assetInfo.Code}/dividend",
                ScrapeTarget.Yutai => $"https://minkabu.jp/stock/{assetInfo.Code}/yutai",
                _ => throw new NotSupportedException($"Target {target} is not supported by Minkabu strategy.")
            };
        }

        public Dictionary<string, string> GetXPaths(ScrapeTarget target)
        {
            return target switch
            {
                ScrapeTarget.Dividend => new Dictionary<string, string>
                {
                    { "Rows", "//div[contains(@class, 'ly_col ly_colsize_6 pt10')]/table/tr" }
                },
                ScrapeTarget.Yutai => new Dictionary<string, string>
                {
                    { "BenefitDetails", "//*[@id=\"yutai_info\"]/div[4]/div[1]/div[2]/div/div[1]/h3" },
                    { "Rows", "//table[contains(@class, 'md_table simple md_table_vertical')]/tbody/tr" }
                },
                _ => new Dictionary<string, string>()
            };
        }

        public void Parse(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target)
        {
            if (target == ScrapeTarget.Dividend)
            {
                var xpaths = GetXPaths(target);
                var rows = doc.DocumentNode.SelectNodes(xpaths["Rows"]);
                if (rows != null && rows.Count != 0)
                {
                    short s = 0;
                    foreach (var row in rows)
                    {
                        var columns = row.SelectNodes("td|th");
                        if (s == 0 && columns != null && columns.Count >= 2)
                        {
                            var dividendYield = columns[1].InnerText.Trim();
                            if (double.TryParse(dividendYield.Replace("%", "").Replace("Åì", ""), out double yield))
                            {
                                assetInfo.DividendYield = yield / 100.0;
                            }
                        }
                        if (s == 1 && columns != null && columns.Count >= 2)
                        {
                            var dividendPayoutRatio = columns[1].InnerText.Trim();
                            if (double.TryParse(dividendPayoutRatio.Replace("%", "").Replace("Åì", ""), out double payout))
                            {
                                assetInfo.DividendPayoutRatio = payout / 100.0;
                            }
                        }
                        if (s == 2 && columns != null && columns.Count >= 2)
                        {
                            var dividendRecordDateMonth = columns[1].InnerText.Trim();
                            assetInfo.DividendRecordDateMonth = dividendRecordDateMonth.Replace(" ", "");
                        }
                        s++;
                    }
                }
            }
            else if (target == ScrapeTarget.Yutai)
            {
                var xpaths = GetXPaths(target);
                var benefitNodes = doc.DocumentNode.SelectNodes(xpaths["BenefitDetails"]);
                if (benefitNodes != null && benefitNodes.Count != 0)
                {
                    assetInfo.ShareholderBenefitsDetails = benefitNodes[0].InnerText.Trim();
                }
                var rows = doc.DocumentNode.SelectNodes(xpaths["Rows"]);
                if (rows != null && rows.Count != 0)
                {
                    short s = 0;
                    foreach (var row in rows)
                    {
                        var columns = row.SelectNodes("td|th");
                        if (s == 0 && columns != null && columns.Count >= 4)
                        {
                            var shareholderBenefitYield = columns[3].InnerText.Trim();
                            assetInfo.ShareholderBenefitYield = ConvertToDoubleForYield(shareholderBenefitYield);
                        }
                        if (s == 1 && columns != null && columns.Count >= 4)
                        {
                            var numberOfSharesRequiredForBenefits = columns[3].InnerText.Trim();
                            assetInfo.NumberOfSharesRequiredForBenefits = numberOfSharesRequiredForBenefits;
                        }
                        if (s == 2 && columns != null && columns.Count >= 4)
                        {
                            var shareholderBenefitRecordMonth = columns[1].InnerText.Trim();
                            var shareholderBenefitRecordDay = columns[3].InnerText.Trim();
                            assetInfo.ShareholderBenefitRecordMonth = shareholderBenefitRecordMonth;
                            assetInfo.ShareholderBenefitRecordDay = shareholderBenefitRecordDay;
                        }
                        s++;
                    }
                }
            }
        }

        private double ConvertToDoubleForYield(string percentString)
        {
            percentString = percentString.Replace("Åì", "").Replace("%", "");
            if (double.TryParse(percentString, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                return value / 100.0;
            }
            else
            {
                return 0;
            }
        }
    }
}