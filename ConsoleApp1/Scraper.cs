// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using Microsoft.SqlServer.Server;
using System.Globalization;

internal class Scraper
{
    int _pageCountMax = 100;

    public Scraper()
    {
    }
    public Scraper(int pageCountMax)
    {
        _pageCountMax = pageCountMax;
    }

    internal async Task<StockInfo> GetStockInfo(WatchList.WatchStock watchStock, DateTime from, DateTime to)
    {
        var stockInfo = new StockInfo(watchStock.Code);
        var httpClient = new HttpClient();
        var htmlDocument = new HtmlDocument();

        var urlBaseYahooFinance = $"https://finance.yahoo.co.jp/quote/{watchStock.Code}.T/history?styl=stock&from={from.ToString("yyyyMMdd")}&to={to.ToString("yyyyMMdd")}&timeFrame=d";
        var urlBaseKabutanTop = $"https://kabutan.jp/stock/?code={watchStock.Code}";
        var urlBaseKabutanFinance = $"https://kabutan.jp/stock/finance?code={watchStock.Code}";

        for (int i = 1; i < _pageCountMax; i++)
        {

            var url = urlBaseYahooFinance + $"&page={i}";
            var html = await httpClient.GetStringAsync(url);
            Console.WriteLine(url);

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

            var rows = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'StocksEtfReitPriceHistory')]/tbody/tr");

            if (rows != null && rows.Count != 0)
            {
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
                }
            }

            url = urlBaseKabutanFinance;
            html = await httpClient.GetStringAsync(url);
            htmlDocument.LoadHtml(html);
            Console.WriteLine(url);

            rows = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'fin_year_t0_d fin_year_profit_d dispnone')]/table/tbody/tr");

            if (rows != null && rows.Count != 0)
            {
                foreach (var row in rows)
                {
                    var columns = row.SelectNodes("td|th");

                    if (columns != null && columns.Count > 6)
                    {
                        var roe = this.GetDouble(columns[4].InnerText.Trim());

                        stockInfo.Roe = roe;
                    }
                }
            }
        }

        return stockInfo;
    }

    internal double GetDouble(string v)
    {
        double.TryParse(v, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double result);

        return result;

    }

    internal DateTime GetDate(string v)
    {
        string[] formats = { "yyyy年M月d日", "yyyy年MM月dd日", "yyyy年MM月d日", "yyyy年M月dd日" };
        DateTime.TryParseExact(v, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);

        return date;
    }
}