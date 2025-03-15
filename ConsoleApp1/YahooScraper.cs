// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.Policy;
using static WatchList;

internal class YahooScraper
{
    internal async Task ScrapeHistory(StockInfo stockInfo, DateTime from, DateTime to)
    {
        int _pageCountMax = CommonUtils.Instance.MaxPageCountToScrapeYahooHistory;

        try
        {
            var httpClient = new HttpClient();
            var htmlDocument = new HtmlDocument();
            var urlBaseYahooFinance = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T/history?styl=stock&from={from.ToString("yyyyMMdd")}&to={to.ToString("yyyyMMdd")}&timeFrame=d";

            /** Yahooファイナンス */
            for (int i = 1; i < _pageCountMax; i++)
            {
                var url = urlBaseYahooFinance + $"&page={i}";
                var html = await httpClient.GetStringAsync(url);

                CommonUtils.Instance.Logger.LogInformation(url);

                // ページ内行カウント
                short rowCount = 0;

                // 取得できない場合は終了
                if (html.Contains("時系列情報がありません")) break;

                // 他の処理
                htmlDocument.LoadHtml(html);

                var rows = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'StocksEtfReitPriceHistory')]/tbody/tr");

                if (rows != null && rows.Count != 0)
                {
                    rowCount = 0;

                    foreach (var row in rows)
                    {
                        var columns = row.SelectNodes("td|th");

                        if (columns != null && columns.Count > 6)
                        {
                            var date = this.GetDate(columns[0].InnerText.Trim());
                            var open = CommonUtils.Instance.GetDouble(columns[1].InnerText.Trim());
                            var high = CommonUtils.Instance.GetDouble(columns[2].InnerText.Trim());
                            var low = CommonUtils.Instance.GetDouble(columns[3].InnerText.Trim());
                            var close = CommonUtils.Instance.GetDouble(columns[4].InnerText.Trim());
                            var volume = CommonUtils.Instance.GetDouble(columns[5].InnerText.Trim());
                            var adjustedClose = CommonUtils.Instance.GetDouble(columns[6].InnerText.Trim());

                            stockInfo.ScrapedPrices.Add(new StockInfo.ScrapedPrice
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
                        rowCount++;
                    }
                }

                // ページ内行カウントが19件以下の時は終了
                if (rowCount <= 19) break;
            }
        }
        catch (Exception e) {
            Console.WriteLine("ScrapeHistory: " + e.Message);
        }
    }

    internal async Task ScrapeProfile(StockInfo stockInfo)
    {
        var url = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T/profile";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string pageContent = await response.Content.ReadAsStringAsync();

                CommonUtils.Instance.Logger.LogInformation(url);

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageContent);

                // 市場名を含むノードをXPathで選択
                //var marketNode = document.DocumentNode.SelectSingleNode("//th[text()='市場名']/following-sibling::td");
                //if (marketNode != null)
                //{
                //    stockInfo.Section = marketNode.InnerText.Trim();
                //}
                // 業種分類を含むノードをXPathで選択
                //var industryNode = document.DocumentNode.SelectSingleNode("//th[text()='業種分類']/following-sibling::td");
                //if (industryNode != null)
                //{
                //    stockInfo.Industry = industryNode.InnerText.Trim();
                //}
                // 決算を含むノードをXPathで選択
                var earningsPeriod = document.DocumentNode.SelectSingleNode("//th[text()='決算']/following-sibling::td");
                if (earningsPeriod != null)
                {
                    stockInfo.EarningsPeriod = earningsPeriod.InnerText.Trim();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ScrapeProfile: " + e.Message);
            }
        }
    }
    internal async Task ScrapeTop(StockInfo stockInfo)
    {
        var url = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string pageContent = await response.Content.ReadAsStringAsync();

                CommonUtils.Instance.Logger.LogInformation(url);

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageContent);

                // 名称
                var title = document.DocumentNode.SelectNodes("//title");
                string[] parts = title[0].InnerText.Trim().Split('【');
                stockInfo.Name = parts.Length > 0 ? parts[0] : stockInfo.Name;

                // 決算発表
                var node = document.DocumentNode.SelectSingleNode("//*[@id=\"summary\"]/div/section[1]/p");
                if (node != null)
                {
                    stockInfo.PressReleaseDate = node.InnerText.Trim();
                }

                // 出来高
                node = document.DocumentNode.SelectSingleNode("//dt/span[text()='出来高']/ancestor::dt/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']");
                if (node != null)
                {
                    stockInfo.LatestTradingVolume = node.InnerText.Trim();
                }

                // 信用買残
                // <section id="margin" class="MarginTransactionInformation__1kka">
//                node = document.DocumentNode.SelectSingleNode("//dt[text()='信用買残']/following-sibling::dd/span[@class='StyledNumber__value__3rXW']");
                node = document.DocumentNode.SelectSingleNode("//*[@id=\"margin\"]/div/ul/li[1]/dl/dd/span[1]/span/span[1]");
                if (node != null)
                {
                    stockInfo.MarginBuyBalance = node.InnerText.Trim();
                }

                // 信用売残
                node = document.DocumentNode.SelectSingleNode("//*[@id=\"margin\"]/div/ul/li[4]/dl/dd/span[1]/span/span[1]");
                if (node != null)
                {
                    stockInfo.MarginSellBalance = node.InnerText.Trim();
                }

                // 信用残更新日付
                node = document.DocumentNode.SelectSingleNode("//*[@id=\"margin\"]/div/ul/li[1]/dl/dd/span[2]/text()[2]");
                if (node != null)
                {
                    stockInfo.MarginBalanceDate = node.InnerText.Trim();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("ScrapeTop: " + e.Message);
            }
        }
    }
    private DateTime GetDate(string v)
    {
        string[] formats = { "yyyy年M月d日", "yyyy年MM月dd日", "yyyy年MM月d日", "yyyy年M月dd日" };
        DateTime.TryParseExact(v, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);

        return date;
    }
}