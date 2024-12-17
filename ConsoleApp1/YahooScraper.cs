// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using System.Globalization;
using System.Security.Policy;
using static WatchList;

internal class YahooScraper
{
    int _pageCountMax = 100;

    internal async Task ScrapeHistory(StockInfo stockInfo, DateTime from, DateTime to)
    {
        try
        {
            var httpClient = new HttpClient();
            var htmlDocument = new HtmlDocument();

            var url = string.Empty;
            var html = string.Empty;
            HtmlNodeCollection rows = null;

            var urlBaseYahooFinance = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T/history?styl=stock&from={from.ToString("yyyyMMdd")}&to={to.ToString("yyyyMMdd")}&timeFrame=d";

            /** Yahooファイナンス */
            for (int i = 1; i < _pageCountMax; i++)
            {
                url = urlBaseYahooFinance + $"&page={i}";
                html = await httpClient.GetStringAsync(url);

                Console.WriteLine(url);

                // ページ内行カウント
                short rowCount = 0;

                // 取得できない場合は終了
                if (html.Contains("時系列情報がありません")) break;

                // 他の処理
                htmlDocument.LoadHtml(html);

                if (string.IsNullOrEmpty(stockInfo.Name))
                {
                    var title = htmlDocument.DocumentNode.SelectNodes("//title");
                    string[] parts = title[0].InnerText.Trim().Split('【');
                    stockInfo.Name = parts.Length > 0 ? parts[0] : stockInfo.Name;
                }

                rows = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'StocksEtfReitPriceHistory')]/tbody/tr");

                if (rows != null && rows.Count != 0)
                {
                    rowCount = 0;

                    foreach (var row in rows)
                    {
                        var columns = row.SelectNodes("td|th");

                        if (columns != null && columns.Count > 6)
                        {
                            var date = this.GetDate(columns[0].InnerText.Trim());
                            var open = this.GetDouble(columns[1].InnerText.Trim());
                            var high = this.GetDouble(columns[2].InnerText.Trim());
                            var low = this.GetDouble(columns[3].InnerText.Trim());
                            var close = this.GetDouble(columns[4].InnerText.Trim());
                            var volume = this.GetDouble(columns[5].InnerText.Trim());

                            stockInfo.Prices.Add(new StockInfo.Price
                            {
                                Date = date,
                                DateYYYYMMDD = date.ToString("yyyyMMdd"),
                                Open = open,
                                High = high,
                                Low = low,
                                Close = close,
                                Volume = volume
                            });
                        }
                        rowCount++;
                    }
                }

                // ページ内行カウントが19件以下の時は終了
                if (rowCount <= 19) break;
            }
        }
        catch (Exception ex) { 
            // 無視
        }
    }
    internal DateTime GetDate(string v)
    {
        string[] formats = { "yyyy年M月d日", "yyyy年MM月dd日", "yyyy年MM月d日", "yyyy年M月dd日" };
        DateTime.TryParseExact(v, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);

        return date;
    }
    internal double GetDouble(string v)
    {
        double.TryParse(v, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double result);

        return result;

    }

    internal async Task ScrapeProfile(StockInfo stockInfo)
    {
        var url = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T/profile";
        Console.WriteLine(url);

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string pageContent = await response.Content.ReadAsStringAsync();

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageContent);

                // 市場名を含むノードをXPathで選択
                var marketNode = document.DocumentNode.SelectSingleNode("//th[text()='市場名']/following-sibling::td");
                if (marketNode != null)
                {
                    stockInfo.Section = marketNode.InnerText.Trim();
                }
                // 業種分類を含むノードをXPathで選択
                var industryNode = document.DocumentNode.SelectSingleNode("//th[text()='業種分類']/following-sibling::td");
                if (industryNode != null)
                {
                    stockInfo.Industry = industryNode.InnerText.Trim();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("リクエストエラー: " + e.Message);
            }
        }
    }
    internal async Task ScrapeTop(StockInfo stockInfo)
    {
        var url = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T";
        Console.WriteLine(url);

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string pageContent = await response.Content.ReadAsStringAsync();

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageContent);

                // ノードをXPathで選択
                var node = document.DocumentNode.SelectSingleNode("//p[contains(@class, 'PressReleaseDate__message__3kiO')]");
                if (node != null)
                {
                    stockInfo.PressReleaseDate = node.InnerText.Trim();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("リクエストエラー: " + e.Message);
            }
        }
    }
}