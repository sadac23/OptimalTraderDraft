using System;
using System.Collections.Generic;
using ConsoleApp1.Assets;
using ConsoleApp1.Scraper.Contracts;
using HtmlAgilityPack;
using System.Globalization;

namespace ConsoleApp1.Scraper.Strategies
{
    /// <summary>
    /// 米国株用Yahooファイナンススクレイピング戦略
    /// </summary>
    public class USStockYahooScrapeStrategy : IAssetScrapeStrategy
    {
        public string GetUrl(AssetInfo assetInfo, ScrapeTarget target)
        {
            // 米国株はYahooファイナンスで「AAPL」→「AAPL」など、サフィックスなし
            return target switch
            {
                ScrapeTarget.Top => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}",
                ScrapeTarget.Profile => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}/profile",
                ScrapeTarget.History => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}/history",
                ScrapeTarget.Disclosure => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}/disclosure",
                _ => throw new NotSupportedException($"Target {target} is not supported by USStockYahooScrapeStrategy.")
            };
        }

        public Dictionary<string, string> GetXPaths(ScrapeTarget target)
        {
            return target switch
            {
                ScrapeTarget.Top => new Dictionary<string, string>
                {
                    { "LatestPrice", "//span[contains(@class, 'stoksPrice')]" },
                    { "MarketCap", "//th[contains(text(),'時価総額')]/following-sibling::td" }
                },
                ScrapeTarget.Profile => new Dictionary<string, string>
                {
                    { "Industry", "//th[contains(text(),'業種')]/following-sibling::td" },
                    { "Section", "//th[contains(text(),'市場')]/following-sibling::td" }
                },
                ScrapeTarget.History => new Dictionary<string, string>
                {
                    { "Rows", "//table[contains(@class, 'historyTable')]/tbody/tr" }
                },
                ScrapeTarget.Disclosure => new Dictionary<string, string>
                {
                    { "Rows", "//table[contains(@class, 'disclosureTable')]/tbody/tr" }
                },
                _ => new Dictionary<string, string>()
            };
        }

        public void Parse(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target)
        {
            var xpaths = GetXPaths(target);

            if (target == ScrapeTarget.Top)
            {
                var priceNode = doc.DocumentNode.SelectSingleNode(xpaths["LatestPrice"]);
                if (priceNode != null && double.TryParse(priceNode.InnerText.Replace(",", ""), out var price))
                {
                    if (assetInfo.LatestPrice != null)
                        assetInfo.LatestPrice.Price = price;
                }

                var marketCapNode = doc.DocumentNode.SelectSingleNode(xpaths["MarketCap"]);
                if (marketCapNode != null)
                {
                    assetInfo.MarketCap = ParseMarketCap(marketCapNode.InnerText);
                }
            }
            else if (target == ScrapeTarget.Profile)
            {
                var industryNode = doc.DocumentNode.SelectSingleNode(xpaths["Industry"]);
                if (industryNode != null)
                {
                    assetInfo.Industry = industryNode.InnerText.Trim();
                }
                var sectionNode = doc.DocumentNode.SelectSingleNode(xpaths["Section"]);
                if (sectionNode != null)
                {
                    assetInfo.Section = sectionNode.InnerText.Trim();
                }
            }
            else if (target == ScrapeTarget.History)
            {
                var rows = doc.DocumentNode.SelectNodes(xpaths["Rows"]);
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells == null || cells.Count < 6) continue;
                        // 日付、始値、高値、安値、終値、出来高
                        // 必要に応じてScrapedPrice等に格納
                    }
                }
            }
            else if (target == ScrapeTarget.Disclosure)
            {
                var rows = doc.DocumentNode.SelectNodes(xpaths["Rows"]);
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells == null || cells.Count < 2) continue;
                        // 必要に応じてDisclosure等に格納
                    }
                }
            }
        }

        private double ParseMarketCap(string value)
        {
            value = value.Replace(",", "");
            if (value.EndsWith("B")) // Billionドル
            {
                value = value.Replace("B", "");
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d * 1_0000_0000 * 100; // 1B = 10億ドル、円換算は別途
            }
            if (value.EndsWith("M")) // Millionドル
            {
                value = value.Replace("M", "");
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d * 1_0000_00 * 100; // 1M = 100万ドル、円換算は別途
            }
            // 日本語表記の場合
            if (value.EndsWith("億円"))
            {
                value = value.Replace("億円", "");
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d * 1_0000_0000;
            }
            if (value.EndsWith("兆円"))
            {
                value = value.Replace("兆円", "");
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d * 1_0000_0000_0000;
            }
            return 0;
        }
    }
}