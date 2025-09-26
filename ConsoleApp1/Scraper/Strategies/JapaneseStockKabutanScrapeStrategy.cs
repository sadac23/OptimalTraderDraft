using System;
using System.Collections.Generic;
using System.Globalization;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Scraper.Contracts;
using HtmlAgilityPack;

namespace ConsoleApp1.Scraper.Strategies
{
    public class JapaneseStockKabutanScrapeStrategy : IAssetScrapeStrategy
    {
        public string GetUrl(AssetInfo assetInfo, ScrapeTarget target)
        {
            return target switch
            {
                ScrapeTarget.Finance => $"https://kabutan.jp/stock/finance?code={assetInfo.Code}",
                _ => throw new NotSupportedException($"Target {target} is not supported by Kabutan strategy.")
            };
        }

        public Dictionary<string, string> GetXPaths(ScrapeTarget target)
        {
            return target switch
            {
                ScrapeTarget.Finance => new Dictionary<string, string>
                {
                    { "Section", "//*[@id='stockinfo_i1']/div[1]/div/span" },
                    { "Industry", "//*[@id='stockinfo_i2']/div/a" },
                    { "ROE", "//div[contains(@class, 'fin_year_t0_d fin_year_profit_d dispnone')]/table/tbody/tr" },
                    { "FinanceRows", "//div[contains(@id, 'stockinfo_i3')]/table/tbody/tr" },
                    { "FullYearPerformances", "//div[contains(@class, 'fin_year_t0_d fin_year_result_d')]/table/tbody/tr" },
                    { "FullYearPerformancesForcasts", "//*[@id='finance_box']/div[6]/table/tbody/tr" },
                    { "LastQuarterPeriod1", "//*[@id='finance_box']/div[17]/div[1]/h3" },
                    { "LastQuarterPeriod2", "//*[@id='finance_box']/div[18]/div[1]/h3" },
                    { "QuarterlyPerformances", "//*[@id='finance_box']/table[3]/tbody/tr" },
                    { "EquityRatioTables", "//table" }
                },
                _ => new Dictionary<string, string>()
            };
        }

        public void Parse(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target)
        {
            if (target != ScrapeTarget.Finance) return;

            var xpaths = GetXPaths(target);

            // ésèÍ
            var sectionNode = doc.DocumentNode.SelectSingleNode(xpaths["Section"]);
            if (sectionNode != null)
                assetInfo.Section = sectionNode.InnerText.Trim();

            // ã∆éÌ
            var industryNode = doc.DocumentNode.SelectSingleNode(xpaths["Industry"]);
            if (industryNode != null)
                assetInfo.Industry = industryNode.InnerText.Trim();

            // ROE, í ä˙é˚âvóöó
            var roeRows = doc.DocumentNode.SelectNodes(xpaths["ROE"]);
            if (roeRows != null && roeRows.Count != 0)
            {
                foreach (var row in roeRows)
                {
                    var columns = row.SelectNodes("td|th");
                    if (columns != null && columns.Count >= 8)
                    {
                        var p = new FullYearProfit
                        {
                            FiscalPeriod = columns[0].InnerText.Trim(),
                            Revenue = columns[1].InnerText.Trim(),
                            OperatingIncome = columns[2].InnerText.Trim(),
                            OperatingMargin = columns[3].InnerText.Trim(),
                            Roe = ParseDouble(columns[4].InnerText.Trim()),
                            Roa = ParseDouble(columns[5].InnerText.Trim()),
                            TotalAssetTurnover = columns[6].InnerText.Trim(),
                            AdjustedEarningsPerShare = columns[7].InnerText.Trim(),
                        };
                        assetInfo.FullYearProfits.Add(p);
                        assetInfo.Roe = p.Roe;
                    }
                }
            }

            // PER/PBR/óòâÒÇË/êMópî{ó¶/éûâøëçäz
            var financeRows = doc.DocumentNode.SelectNodes(xpaths["FinanceRows"]);
            if (financeRows != null && financeRows.Count != 0)
            {
                short s = 0;
                foreach (var row in financeRows)
                {
                    var columns = row.SelectNodes("td|th");
                    if (s == 0 && columns != null && columns.Count >= 4)
                    {
                        assetInfo.Per = ParseDouble(columns[0].InnerText.Trim().Replace("î{", ""));
                        assetInfo.Pbr = ParseDouble(columns[1].InnerText.Trim().Replace("î{", ""));
                        assetInfo.DividendYield = ParsePercent(columns[2].InnerText.Trim());
                        assetInfo.MarginBalanceRatio = columns[3].InnerText.Trim();
                    }
                    if (s == 1 && columns != null && columns.Count > 1)
                    {
                        assetInfo.MarketCap = ParseMarketCap(columns[1].InnerText.Trim());
                    }
                    s++;
                }
            }

            // í ä˙ã∆ê—óöó
            var perfRows = doc.DocumentNode.SelectNodes(xpaths["FullYearPerformances"]);
            if (perfRows != null && perfRows.Count != 0)
            {
                foreach (var row in perfRows)
                {
                    var columns = row.SelectNodes("td|th");
                    if (columns != null && columns.Count >= 8)
                    {
                        var p = new FullYearPerformance
                        {
                            FiscalPeriod = columns[0].InnerText.Trim(),
                            Revenue = columns[1].InnerText.Trim(),
                            OperatingProfit = columns[2].InnerText.Trim(),
                            OrdinaryProfit = columns[3].InnerText.Trim(),
                            NetProft = columns[4].InnerText.Trim(),
                            AdjustedEarningsPerShare = columns[5].InnerText.Trim(),
                            AdjustedDividendPerShare = columns[6].InnerText.Trim(),
                            AnnouncementDate = columns[7].InnerText.Trim(),
                        };
                        assetInfo.FullYearPerformances.Add(p);
                    }
                }
            }

            // í ä˙ã∆ê—ó\ëzóöó
            var forcastRows = doc.DocumentNode.SelectNodes(xpaths["FullYearPerformancesForcasts"]);
            if (forcastRows != null && forcastRows.Count != 0)
            {
                int countHeader = 0;
                FullYearPerformanceForcast cloneP = null;
                string tempFiscalPeriod = string.Empty;

                foreach (var row in forcastRows)
                {
                    var columns = row.SelectNodes("td|th");
                    string fiscalPeriod = string.Empty, revisionDate = string.Empty, category = string.Empty, revisionDirection = string.Empty;
                    string revenue = string.Empty, operatingProfit = string.Empty, ordinaryProfit = string.Empty, netProfit = string.Empty, revisedDividend = string.Empty;

                    if (columns != null && columns.Count >= 10)
                    {
                        countHeader++;
                        fiscalPeriod = columns[1].InnerText.Trim();
                        revisionDate = columns[2].InnerText.Trim();
                        category = columns[3].InnerText.Trim();
                        revenue = columns[5].InnerText.Trim();
                        operatingProfit = columns[6].InnerText.Trim();
                        ordinaryProfit = columns[7].InnerText.Trim();
                        netProfit = columns[8].InnerText.Trim();
                        revisedDividend = columns[9].InnerText.Trim();
                        tempFiscalPeriod = fiscalPeriod;
                    }
                    else if (columns != null && columns.Count >= 9)
                    {
                        fiscalPeriod = tempFiscalPeriod;
                        revisionDate = columns[1].InnerText.Trim();
                        category = columns[2].InnerText.Trim();
                        revenue = columns[4].InnerText.Trim();
                        operatingProfit = columns[5].InnerText.Trim();
                        ordinaryProfit = columns[6].InnerText.Trim();
                        netProfit = columns[7].InnerText.Trim();
                        revisedDividend = columns[8].InnerText.Trim();
                    }
                    else
                    {
                        continue;
                    }

                    if (countHeader < 2) continue;

                    revisedDividend = revisedDividend.Replace("*", "").Replace("#", "");

                    var p = new FullYearPerformanceForcast
                    {
                        FiscalPeriod = fiscalPeriod,
                        RevisionDate = ConvertToDateTime(revisionDate),
                        Category = category,
                        RevisionDirection = revisionDirection,
                        Revenue = revenue,
                        OperatingProfit = operatingProfit,
                        OrdinaryProfit = ordinaryProfit,
                        NetProfit = netProfit,
                        RevisedDividend = revisedDividend,
                        PreviousForcast = cloneP
                    };
                    cloneP = (FullYearPerformanceForcast)p.Clone();
                    assetInfo.FullYearPerformancesForcasts.Add(p);
                }
            }

            // élîºä˙åàéZä˙ä‘
            var node = doc.DocumentNode.SelectSingleNode(xpaths["LastQuarterPeriod1"]);
            var period = string.Empty;
            if (node != null)
            {
                period = node.InnerText.Trim();
            }
            else
            {
                node = doc.DocumentNode.SelectSingleNode(xpaths["LastQuarterPeriod2"]);
                if (node != null)
                {
                    period = node.InnerText.Trim();
                }
            }
            assetInfo.LastQuarterPeriod = period switch
            {
                string s when s.Contains("ëÊÇP") => "1Q",
                string s when s.Contains("ëÊÇQ") => "2Q",
                string s when s.Contains("ëÊÇR") => "3Q",
                string s when s.Contains("ëOä˙") => "4Q",
                _ => period
            };

            // élîºä˙é¿ê—óöó
            var qRows = doc.DocumentNode.SelectNodes(xpaths["QuarterlyPerformances"]);
            if (qRows != null && qRows.Count != 0)
            {
                foreach (var row in qRows)
                {
                    var columns = row.SelectNodes("td|th");
                    if (columns != null && columns.Count >= 8)
                    {
                        var p = new QuarterlyPerformance
                        {
                            FiscalPeriod = columns[0].InnerText.Trim(),
                            Revenue = columns[1].InnerText.Trim(),
                            OperatingProfit = columns[2].InnerText.Trim(),
                            OrdinaryProfit = ParseDouble(columns[3].InnerText.Trim()),
                            NetProfit = columns[4].InnerText.Trim(),
                            AdjustedEarningsPerShare = columns[5].InnerText.Trim(),
                            AdjustedDividendPerShare = columns[6].InnerText.Trim(),
                            ReleaseDate = columns[7].InnerText.Trim(),
                        };
                        assetInfo.QuarterlyPerformances.Add(p);
                    }
                }
            }

            // é©å»éëñ{î‰ó¶
            string[] requiredHeaders = new string[]
            {
                "åàéZä˙", "ÇPäîèÉéëéY", "é©å»éëñ{î‰ó¶", "ëçéëéY", "é©å»éëñ{", "èËó]ã‡", "óLóòéqïâç¬î{ó¶", "î≠ï\ì˙"
            };
            var tables = doc.DocumentNode.SelectNodes(xpaths["EquityRatioTables"]);
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    var headerRow = table.SelectSingleNode(".//tr[1]");
                    if (headerRow != null)
                    {
                        var headerNodes = headerRow.SelectNodes("th");
                        if (headerNodes == null) continue;
                        var headers = new List<string>();
                        foreach (var th in headerNodes)
                            headers.Add(th.InnerText.Trim());
                        bool allHeadersPresent = true;
                        foreach (var requiredHeader in requiredHeaders)
                        {
                            if (!headers.Contains(requiredHeader))
                            {
                                allHeadersPresent = false;
                                break;
                            }
                        }
                        if (allHeadersPresent)
                        {
                            var targetRows = table.SelectNodes(".//tr[position() > 1]");
                            if (targetRows != null)
                            {
                                foreach (var row in targetRows)
                                {
                                    var columns = row.SelectNodes("td|th");
                                    if (columns != null && columns.Count >= 8)
                                    {
                                        assetInfo.EquityRatio = columns[2].InnerText.Trim();
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private double ParsePercent(string value)
        {
            value = value.Replace("%", "").Replace("Åì", "");
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d / 100.0;
            return 0;
        }

        private double ParseDouble(string value)
        {
            value = value.Replace(",", "").Replace("î{", "");
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d;
            return 0;
        }

        private double ParseMarketCap(string input)
        {
            double result = 0;
            var units = new (string Unit, double Multiplier)[]
            {
                ("íõ", 1e12),
                ("â≠", 1e8),
                ("ñú", 1e4)
            };
            foreach (var (unit, multiplier) in units)
            {
                int unitIndex = input.IndexOf(unit);
                if (unitIndex != -1)
                {
                    string numberPart = input.Substring(0, unitIndex).Replace(",", "");
                    if (double.TryParse(numberPart, out double number))
                    {
                        result += number * multiplier;
                    }
                    input = input.Substring(unitIndex + unit.Length);
                }
            }
            input = input.Replace(",", "").Replace("â~", "");
            if (double.TryParse(input, out double remainingNumber))
            {
                result += remainingNumber;
            }
            return result;
        }

        private DateTime ConvertToDateTime(string dateString, string format = "yy/MM/dd")
        {
            try
            {
                return DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}