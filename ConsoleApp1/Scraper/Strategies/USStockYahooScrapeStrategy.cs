using System;
using System.Collections.Generic;
using ConsoleApp1.Assets;
using ConsoleApp1.Scraper.Contracts;
using HtmlAgilityPack;
using System.Globalization;

namespace ConsoleApp1.Scraper.Strategies
{
    /// <summary>
    /// �č����pYahoo�t�@�C�i���X�X�N���C�s���O�헪
    /// </summary>
    public class USStockYahooScrapeStrategy : IAssetScrapeStrategy
    {
        public string GetUrl(AssetInfo assetInfo, ScrapeTarget target)
        {
            // �č�����Yahoo�t�@�C�i���X�ŁuAAPL�v���uAAPL�v�ȂǁA�T�t�B�b�N�X�Ȃ�
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
                    { "MarketCap", "//th[contains(text(),'�������z')]/following-sibling::td" }
                },
                ScrapeTarget.Profile => new Dictionary<string, string>
                {
                    { "Industry", "//th[contains(text(),'�Ǝ�')]/following-sibling::td" },
                    { "Section", "//th[contains(text(),'�s��')]/following-sibling::td" }
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
                        // ���t�A�n�l�A���l�A���l�A�I�l�A�o����
                        // �K�v�ɉ�����ScrapedPrice���Ɋi�[
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
                        // �K�v�ɉ�����Disclosure���Ɋi�[
                    }
                }
            }
        }

        private double ParseMarketCap(string value)
        {
            value = value.Replace(",", "");
            if (value.EndsWith("B")) // Billion�h��
            {
                value = value.Replace("B", "");
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d * 1_0000_0000 * 100; // 1B = 10���h���A�~���Z�͕ʓr
            }
            if (value.EndsWith("M")) // Million�h��
            {
                value = value.Replace("M", "");
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d * 1_0000_00 * 100; // 1M = 100���h���A�~���Z�͕ʓr
            }
            // ���{��\�L�̏ꍇ
            if (value.EndsWith("���~"))
            {
                value = value.Replace("���~", "");
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d * 1_0000_0000;
            }
            if (value.EndsWith("���~"))
            {
                value = value.Replace("���~", "");
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d * 1_0000_0000_0000;
            }
            return 0;
        }
    }
}