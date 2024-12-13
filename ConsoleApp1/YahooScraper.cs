// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using System.Globalization;
using static WatchList;

internal class YahooScraper
{
    int _pageCountMax = 100;

    internal async Task ScrapeHistory(StockInfo stockInfo, DateTime from, DateTime to)
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
}