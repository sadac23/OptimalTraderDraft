using System;
using System.Collections.Generic;
using ConsoleApp1.Assets;
using ConsoleApp1.Scraper.Contracts;
using HtmlAgilityPack;
using System.Globalization;
using ConsoleApp1.Assets.Models;

namespace ConsoleApp1.Scraper.Strategies
{
    /// <summary>
    /// �w���i�C���f�b�N�X�j�p��Yahoo�X�N���C�s���O�헪�i�������̕���EXPath�E�p�[�X���W�b�N�𔽉f�j
    /// </summary>
    public class IndexYahooScrapeStrategy : IAssetScrapeStrategy
    {
        public string GetUrl(AssetInfo assetInfo, ScrapeTarget target)
        {
            // �w���̏ꍇ�� .O�A����ȊO�� .T
            string suffix = assetInfo.Classification == CommonUtils.Instance.Classification.Indexs ? ".O" : ".T";
            return target switch
            {
                ScrapeTarget.Top => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}",
                ScrapeTarget.Profile => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}/profile",
                ScrapeTarget.History => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}/history",
                ScrapeTarget.Disclosure => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}/disclosure",
                _ => throw new NotSupportedException($"Target {target} is not supported by IndexYahooScrapeStrategy.")
            };
        }

        public Dictionary<string, string> GetXPaths(ScrapeTarget target)
        {
            // ��������XPath�𔽉f
            return target switch
            {
                ScrapeTarget.Top => new Dictionary<string, string>
                {
                    { "Title", "//title" },
                    { "PressReleaseDate", "//*[@id=\"summary\"]/div/section[1]/p" },
                    { "LatestTradingVolume", "//dt/span[text()='�o����']/ancestor::dt/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']" },
                    { "MarginBuyBalance", "//*[@id=\"margin\"]/div/ul/li[1]/dl/dd/span[1]/span/span[1]" },
                    { "MarginSellBalance", "//*[@id=\"margin\"]/div/ul/li[4]/dl/dd/span[1]/span/span[1]" },
                    { "MarginBalanceDate", "//*[@id=\"margin\"]/div/ul/li[1]/dl/dd/span[2]/text()[2]" },
                    { "Open", "//*[@id=\"detail\"]/div/div/dl[2]/dd/span[1]" },
                    { "High", "//*[@id=\"detail\"]/div/div/dl[3]/dd/span[1]" },
                    { "Low", "//*[@id=\"detail\"]/div/div/dl[4]/dd/span[1]" },
                    { "Close", "//*[@id=\"root\"]/main/div/section/div[2]/div[2]/div[1]/span/span/span" }
                },
                ScrapeTarget.Profile => new Dictionary<string, string>
                {
                    { "EarningsPeriod", "//th[text()='���Z']/following-sibling::td" }
                },
                ScrapeTarget.History => new Dictionary<string, string>
                {
                    { "Rows", "//table[contains(@class, 'table__CO3B')]/tbody/tr" }
                },
                ScrapeTarget.Disclosure => new Dictionary<string, string>
                {
                    { "Rows", "//*[@id=\"disclist\"]/li" }
                },
                _ => new Dictionary<string, string>()
            };
        }

        public void Parse(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target)
        {
            var xpaths = GetXPaths(target);

            if (target == ScrapeTarget.Top)
            {
                // ����
                var titleNode = doc.DocumentNode.SelectSingleNode(xpaths["Title"]);
                if (titleNode != null)
                {
                    var titleText = titleNode.InnerText.Trim();
                    // �w���̏ꍇ�́u�F�v�܂��́u:�v�ŕ������A�ŏ��̕������擾
                    var name = titleText.Split(new[] { '�F', ':' }, 2)[0].Trim();
                    assetInfo.Name = name;
                }
                // ���Z���\
                var node = doc.DocumentNode.SelectSingleNode(xpaths["PressReleaseDate"]);
                if (node != null)
                {
                    assetInfo.PressReleaseDate = node.InnerText.Trim();
                }
                // �o����
                node = doc.DocumentNode.SelectSingleNode(xpaths["LatestTradingVolume"]);
                if (node != null)
                {
                    assetInfo.LatestTradingVolume = node.InnerText.Trim();
                }
                // �M�p���c
                node = doc.DocumentNode.SelectSingleNode(xpaths["MarginBuyBalance"]);
                if (node != null)
                {
                    assetInfo.MarginBuyBalance = node.InnerText.Trim();
                }
                // �M�p���c
                node = doc.DocumentNode.SelectSingleNode(xpaths["MarginSellBalance"]);
                if (node != null)
                {
                    assetInfo.MarginSellBalance = node.InnerText.Trim();
                }
                // �M�p�c�X�V���t
                node = doc.DocumentNode.SelectSingleNode(xpaths["MarginBalanceDate"]);
                if (node != null)
                {
                    assetInfo.MarginBalanceDate = node.InnerText.Trim();
                }
                // �ŐV�̊������
                var date = CommonUtils.Instance.LastTradingDate;
                var open = doc.DocumentNode.SelectSingleNode(xpaths["Open"]);
                var high = doc.DocumentNode.SelectSingleNode(xpaths["High"]);
                var low = doc.DocumentNode.SelectSingleNode(xpaths["Low"]);
                var close = doc.DocumentNode.SelectSingleNode(xpaths["Close"]);
                assetInfo.LatestScrapedPrice = new ScrapedPrice()
                {
                    Date = date,
                    DateYYYYMMDD = date.ToString("yyyyMMdd"),
                    Open = open != null ? CommonUtils.Instance.GetDouble(open.InnerText.Trim()) : 0,
                    High = high != null ? CommonUtils.Instance.GetDouble(high.InnerText.Trim()) : 0,
                    Low = low != null ? CommonUtils.Instance.GetDouble(low.InnerText.Trim()) : 0,
                    Close = close != null ? CommonUtils.Instance.GetDouble(close.InnerText.Trim()) : 0,
                    Volume = 0,
                    AdjustedClose = close != null ? CommonUtils.Instance.GetDouble(close.InnerText.Trim()) : 0
                };
            }
            else if (target == ScrapeTarget.Profile)
            {
                var earningsPeriod = doc.DocumentNode.SelectSingleNode(xpaths["EarningsPeriod"]);
                if (earningsPeriod != null)
                {
                    assetInfo.EarningsPeriod = earningsPeriod.InnerText.Trim();
                    assetInfo.CurrentFiscalMonth = ParseNextClosingDate(CommonUtils.Instance.ExecusionDate, assetInfo.EarningsPeriod);
                }
            }
            else if (target == ScrapeTarget.History)
            {
                var rows = doc.DocumentNode.SelectNodes(xpaths["Rows"]);
                if (rows != null && rows.Count != 0)
                {
                    int startIndex = rows.Count > 1 ? 1 : 0;
                    for (int r = startIndex; r < rows.Count; r++)
                    {
                        var row = rows[r];
                        var columns = row.SelectNodes("td|th");
                        if (columns != null && columns.Count >= 5)
                        {
                            var date = ParseDate(columns[0].InnerText.Trim());
                            var open = CommonUtils.Instance.GetDouble(columns[1].InnerText.Trim());
                            var high = CommonUtils.Instance.GetDouble(columns[2].InnerText.Trim());
                            var low = CommonUtils.Instance.GetDouble(columns[3].InnerText.Trim());
                            var close = CommonUtils.Instance.GetDouble(columns[4].InnerText.Trim());
                            assetInfo.ScrapedPrices.Add(new ScrapedPrice
                            {
                                Date = date,
                                DateYYYYMMDD = date.ToString("yyyyMMdd"),
                                Open = open,
                                High = high,
                                Low = low,
                                Close = close,
                                Volume = 0,
                                AdjustedClose = close
                            });
                        }
                    }
                }
            }
            else if (target == ScrapeTarget.Disclosure)
            {
                var rows = doc.DocumentNode.SelectNodes(xpaths["Rows"]);
                if (rows != null && rows.Count != 0)
                {
                    foreach (var item in rows)
                    {
                        var header = string.Empty;
                        DateTime datetime = DateTime.MinValue;
                        var nodeHeader = item.SelectSingleNode(".//article/a/h3");
                        if (nodeHeader != null)
                        {
                            header = nodeHeader.InnerText.Trim();
                        }
                        var nodeDate = item.SelectSingleNode(".//article/a/ul/li[1]/time");
                        if (nodeDate != null)
                        {
                            datetime = ParseDatetime(nodeDate.InnerText.Trim());
                        }
                        assetInfo.Disclosures.Add(new Disclosure
                        {
                            Header = header,
                            Datetime = datetime,
                        });
                    }
                }
            }
        }

        // ��������GetDate����
        private DateTime ParseDate(string v)
        {
            string[] formats = { "yyyy�NM��d��", "yyyy�NMM��dd��", "yyyy�NMM��d��", "yyyy�NM��dd��" };
            DateTime.TryParseExact(v, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
            return date;
        }

        // ��������ConvertToDatetime����
        private DateTime ParseDatetime(string v)
        {
            DateTime result;
            int currentYear = DateTime.Now.Year;
            string fullDateString = $"{currentYear}/{v}";
            if (DateTime.TryParseExact(fullDateString, "yyyy/M/d", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            if (DateTime.TryParseExact(v, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                DateTime currentDate = DateTime.Now.Date;
                result = currentDate.Add(result.TimeOfDay);
                return result;
            }
            throw new FormatException("���͕�����̌`��������������܂���B");
        }

        // ��������GetNextClosingDate����
        private DateTime ParseNextClosingDate(DateTime currentDate, string closingDateStr)
        {
            int month = ParseMonth(closingDateStr);
            if (month == -1)
            {
                throw new ArgumentException("�����ȏ����ł��B");
            }
            DateTime closingDate = new DateTime(currentDate.Year, month, DateTime.DaysInMonth(currentDate.Year, month));
            if (currentDate <= closingDate)
            {
                return closingDate;
            }
            else
            {
                return new DateTime(currentDate.Year + 1, month, DateTime.DaysInMonth(currentDate.Year + 1, month));
            }
        }

        // ��������ParseMonth����
        private int ParseMonth(string closingDateStr)
        {
            if (closingDateStr.EndsWith("������"))
            {
                string monthStr = closingDateStr.Replace("������", "");
                if (int.TryParse(monthStr, out int month) && month >= 1 && month <= 12)
                {
                    return month;
                }
            }
            return -1;
        }

        private double ParseMarketCap(string value)
        {
            value = value.Replace(",", "");
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