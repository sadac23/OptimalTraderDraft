using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Scraper.Contracts;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Scraper.Strategies
{
    public class JapaneseStockYahooScrapeStrategy : IAssetScrapeStrategy
    {
        public string GetUrl(AssetInfo assetInfo, ScrapeTarget target)
        {
            string suffix = assetInfo.Classification == CommonUtils.Instance.Classification.Indexs ? ".O" : ".T";
            return target switch
            {
                ScrapeTarget.Top => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}",
                ScrapeTarget.Profile => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}/profile",
                ScrapeTarget.History => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}/history",
                ScrapeTarget.Disclosure => $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}/disclosure",
                _ => throw new NotSupportedException($"Target {target} is not supported by Yahoo strategy.")
            };
        }

        public Dictionary<string, string> GetXPaths(ScrapeTarget target)
        {
            // 必要に応じて拡張
            return new Dictionary<string, string>();
        }

        // 追加：IAssetScrapeStrategyのParse実装
        public void Parse(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target)
        {
            // 非同期でないParseAsyncのラッパーとして同期的に呼び出す
            // Historyの場合はfrom/toは未指定でOK
            ParseAsync(doc, assetInfo, target).GetAwaiter().GetResult();
        }

        // ページネーション・分岐・補助メソッド・詳細情報取得をすべて反映
        public async Task ParseAsync(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target, DateTime? from = null, DateTime? to = null)
        {
            if (target == ScrapeTarget.History)
            {
                await ScrapeHistoryAsync(assetInfo, from, to);
            }
            else if (target == ScrapeTarget.Top)
            {
                ScrapeTop(doc, assetInfo);
            }
            else if (target == ScrapeTarget.Profile)
            {
                ScrapeProfile(doc, assetInfo);
            }
            // Disclosure等も必要に応じて追加
        }

        private async Task ScrapeHistoryAsync(AssetInfo assetInfo, DateTime? from, DateTime? to)
        {
            int _pageCountMax = CommonUtils.Instance.MaxPageCountToScrapeYahooHistory;
            int retryCount = 0, maxRetry = 3, retryDelayMs = 1000;
            assetInfo.ScrapedPrices = new List<ScrapedPrice>();

            string suffix = assetInfo.Classification == CommonUtils.Instance.Classification.Indexs ? ".O" : ".T";
            string urlBaseYahooFinance = $"https://finance.yahoo.co.jp/quote/{assetInfo.Code}{suffix}/history?styl=stock";
            if (from.HasValue && to.HasValue)
            {
                urlBaseYahooFinance += $"&from={from.Value:yyyyMMdd}&to={to.Value:yyyyMMdd}&timeFrame=d";
            }

            while (retryCount < maxRetry)
            {
                try
                {
                    for (int i = 1; i < _pageCountMax; i++)
                    {
                        var url = urlBaseYahooFinance + $"&page={i}";
                        string html = await CommonUtils.Instance.HttpClient.GetStringAsync(url);
                        CommonUtils.Instance.Logger.LogInformation(url);

                        if (html.Contains("時系列情報がありません")) break;

                        var htmlDocument = new HtmlDocument();
                        htmlDocument.LoadHtml(html);

                        string xpath = assetInfo.Classification == CommonUtils.Instance.Classification.Indexs
                            ? "//table[contains(@class, 'table__CO3B')]/tbody/tr"
                            : "//table[contains(@class, 'StocksEtfReitPriceHistory')]/tbody/tr";
                        var rows = htmlDocument.DocumentNode.SelectNodes(xpath);

                        if (rows != null && rows.Count != 0)
                        {
                            int startIndex = (assetInfo.Classification == CommonUtils.Instance.Classification.Indexs && rows.Count > 1) ? 1 : 0;
                            for (int r = startIndex; r < rows.Count; r++)
                            {
                                var row = rows[r];
                                var columns = row.SelectNodes("td|th");
                                if (assetInfo.Classification == CommonUtils.Instance.Classification.Indexs)
                                {
                                    if (columns != null && columns.Count >= 5)
                                    {
                                        var date = GetDate(columns[0].InnerText.Trim());
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
                                else
                                {
                                    if (columns != null && columns.Count > 6)
                                    {
                                        var date = GetDate(columns[0].InnerText.Trim());
                                        var open = CommonUtils.Instance.GetDouble(columns[1].InnerText.Trim());
                                        var high = CommonUtils.Instance.GetDouble(columns[2].InnerText.Trim());
                                        var low = CommonUtils.Instance.GetDouble(columns[3].InnerText.Trim());
                                        var close = CommonUtils.Instance.GetDouble(columns[4].InnerText.Trim());
                                        var volume = CommonUtils.Instance.GetDouble(columns[5].InnerText.Trim());
                                        var adjustedClose = CommonUtils.Instance.GetDouble(columns[6].InnerText.Trim());

                                        assetInfo.ScrapedPrices.Add(new ScrapedPrice
                                        {
                                            Date = date,
                                            DateYYYYMMDD = date.ToString("yyyyMMdd"),
                                            Open = open,
                                            High = high,
                                            Low = low,
                                            Close = close,
                                            Volume = volume,
                                            AdjustedClose = adjustedClose
                                        });
                                    }
                                }
                            }
                        }
                        // 1ページ20件未満なら終了
                        if (rows == null || rows.Count <= 19) break;
                    }
                    break; // 成功したらループを抜ける
                }
                catch (Exception e)
                {
                    CommonUtils.Instance.Logger.LogError($"ScrapeHistory失敗（{retryCount + 1}回目）: {assetInfo.Code} {e.Message}", e);
                    retryCount++;
                    if (retryCount >= maxRetry) return;
                    await Task.Delay(retryDelayMs);
                }
            }
        }

        private void ScrapeTop(HtmlDocument doc, AssetInfo assetInfo)
        {
            // 名称
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            if (titleNode != null)
            {
                var titleText = titleNode.InnerText.Trim();
                if (assetInfo.Classification == CommonUtils.Instance.Classification.Indexs)
                {
                    var name = titleText.Split(new[] { '：', ':' }, 2)[0].Trim();
                    assetInfo.Name = name;
                }
                else
                {
                    string[] parts = titleText.Split('【');
                    assetInfo.Name = parts.Length > 0 ? parts[0] : assetInfo.Name;
                }
            }

            // 決算発表
            var node = doc.DocumentNode.SelectSingleNode("//*[@id=\"summary\"]/div/section[1]/p");
            if (node != null)
            {
                assetInfo.PressReleaseDate = node.InnerText.Trim();
            }

            // 出来高
            node = doc.DocumentNode.SelectSingleNode("//dt/span[text()='出来高']/ancestor::dt/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']");
            if (node != null)
            {
                assetInfo.LatestTradingVolume = node.InnerText.Trim();
            }

            // 信用買残
            node = doc.DocumentNode.SelectSingleNode("//*[@id=\"margin\"]/div/ul/li[1]/dl/dd/span[1]/span/span[1]");
            if (node != null)
            {
                assetInfo.MarginBuyBalance = node.InnerText.Trim();
            }

            // 信用売残
            node = doc.DocumentNode.SelectSingleNode("//*[@id=\"margin\"]/div/ul/li[4]/dl/dd/span[1]/span/span[1]");
            if (node != null)
            {
                assetInfo.MarginSellBalance = node.InnerText.Trim();
            }

            // 信用残更新日付
            node = doc.DocumentNode.SelectSingleNode("//*[@id=\"margin\"]/div/ul/li[1]/dl/dd/span[2]/text()[2]");
            if (node != null)
            {
                assetInfo.MarginBalanceDate = node.InnerText.Trim();
            }

            // ETF固有情報
            if (assetInfo.Classification == CommonUtils.Instance.Classification.JapaneseETFs)
            {
                var dividendNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"referenc\"]/div/ul/li[11]/dl/dd/span/span/span");
                if (dividendNode != null)
                {
                    assetInfo.EarningsPeriod = dividendNode.InnerText.Trim();
                }
                var companyNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"referenc\"]/div/ul/li[6]/dl/dd/span/span/span");
                if (companyNode != null)
                {
                    assetInfo.FundManagementCompany = companyNode.InnerText.Trim();
                }
                var trustFeeRateNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"referenc\"]/div/ul/li[13]/dl/dd/span/span/span");
                if (trustFeeRateNode != null)
                {
                    assetInfo.TrustFeeRate = ConvertToDouble(trustFeeRateNode.InnerText.Trim());
                }
            }

            // 最新株価
            string xpath_open, xpath_high, xpath_low, xpath_close;
            if (assetInfo.Classification == CommonUtils.Instance.Classification.Indexs)
            {
                xpath_open = "//*[@id=\"detail\"]/div/div/dl[2]/dd/span[1]";
                xpath_high = "//*[@id=\"detail\"]/div/div/dl[3]/dd/span[1]";
                xpath_low = "//*[@id=\"detail\"]/div/div/dl[4]/dd/span[1]";
                xpath_close = "//*[@id=\"root\"]/main/div/section/div[2]/div[2]/div[1]/span/span/span";
            }
            else
            {
                xpath_open = "//*[@id=\"detail\"]/section[1]/div/ul/li[2]/dl/dd/span[1]/span/span";
                xpath_high = "//*[@id=\"detail\"]/section[1]/div/ul/li[3]/dl/dd/span[1]/span/span";
                xpath_low = "//*[@id=\"detail\"]/section[1]/div/ul/li[4]/dl/dd/span[1]/span/span";
                xpath_close = "//*[@id=\"root\"]/main/div/section/div[2]/div[2]/div[1]/span/span/span";
            }
            var date = CommonUtils.Instance.LastTradingDate;
            var open = doc.DocumentNode.SelectSingleNode(xpath_open);
            var high = doc.DocumentNode.SelectSingleNode(xpath_high);
            var low = doc.DocumentNode.SelectSingleNode(xpath_low);
            var close = doc.DocumentNode.SelectSingleNode(xpath_close);
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

        private void ScrapeProfile(HtmlDocument doc, AssetInfo assetInfo)
        {
            // 決算期情報
            var earningsPeriod = doc.DocumentNode.SelectSingleNode("//th[text()='決算']/following-sibling::td");
            if (earningsPeriod != null)
            {
                assetInfo.EarningsPeriod = earningsPeriod.InnerText.Trim();
                assetInfo.CurrentFiscalMonth = GetNextClosingDate(CommonUtils.Instance.ExecusionDate, assetInfo.EarningsPeriod);
            }
            // 業種・市場
            var industryNode = doc.DocumentNode.SelectSingleNode("//th[contains(text(),'業種')]/following-sibling::td");
            if (industryNode != null)
            {
                assetInfo.Industry = industryNode.InnerText.Trim();
            }
            var sectionNode = doc.DocumentNode.SelectSingleNode("//th[contains(text(),'市場')]/following-sibling::td");
            if (sectionNode != null)
            {
                assetInfo.Section = sectionNode.InnerText.Trim();
            }
        }

        // --- 補助メソッド ---
        private DateTime GetDate(string v)
        {
            string[] formats = { "yyyy年M月d日", "yyyy年MM月dd日", "yyyy年MM月d日", "yyyy年M月dd日" };
            DateTime.TryParseExact(v, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
            return date;
        }
        private double ConvertToDouble(string percentString)
        {
            percentString = percentString.Replace("％", "").Replace("%", "");
            if (double.TryParse(percentString, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                return value / 100.0;
            else
                return 0;
        }
        private DateTime GetNextClosingDate(DateTime currentDate, string closingDateStr)
        {
            int month = ParseMonth(closingDateStr);
            if (month == -1) throw new ArgumentException("無効な書式です。");
            DateTime closingDate = new DateTime(currentDate.Year, month, DateTime.DaysInMonth(currentDate.Year, month));
            if (currentDate <= closingDate)
                return closingDate;
            else
                return new DateTime(currentDate.Year + 1, month, DateTime.DaysInMonth(currentDate.Year + 1, month));
        }
        private int ParseMonth(string closingDateStr)
        {
            if (closingDateStr.EndsWith("月末日"))
            {
                string monthStr = closingDateStr.Replace("月末日", "");
                if (int.TryParse(monthStr, out int month) && month >= 1 && month <= 12)
                    return month;
            }
            return -1;
        }
    }
}