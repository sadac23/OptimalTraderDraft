﻿// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;

internal class Scraper
{
    //string url = $"https://finance.yahoo.co.jp/quote/{stockCode}.T/history";
    //string url = $"https://finance.yahoo.co.jp/quote/7203.T/history?styl=stock&from=20241106&to=20241107&timeFrame=d&page=1";
    //var httpClient = new HttpClient();
    //var html = await httpClient.GetStringAsync(url);
    //var htmlDocument = new HtmlDocument();

    public Scraper()
    {
    }

    internal async Task<StockInfo> GetStockInfo(string code, DateTime from, DateTime to)
    {
        var stockInfo = new StockInfo(code);
        var httpClient = new HttpClient();
        var htmlDocument = new HtmlDocument();
        var urlBase = $"https://finance.yahoo.co.jp/quote/{code}.T/history?styl=stock&from={from.ToString("yyyyMMdd")}&to={to.ToString("yyyyMMdd")}&timeFrame=d";

        for (int i = 1; i < 10; i++)
        {

            var url = urlBase + $"&page={i}";
            var html = await httpClient.GetStringAsync(url);
            Console.WriteLine(url);

            // 取得できない場合は終了
            if (html.Contains("時系列情報がありません")) break;

            // 他の処理
            htmlDocument.LoadHtml(html);

            if (stockInfo.Name == string.Empty)
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
                        var date = columns[0].InnerText.Trim();
                        var open = columns[1].InnerText.Trim();
                        var high = columns[2].InnerText.Trim();
                        var low = columns[3].InnerText.Trim();
                        var close = columns[4].InnerText.Trim();
                        var volume = columns[5].InnerText.Trim();

                        stockInfo.Prices.Add(new StockInfo.Price
                        {
                            Date = date,
                            Open = open,
                            High = high,
                            Low = low,
                            Close = close,
                            Volume = volume
                        });
                    }
                }
            }
        }
        return stockInfo;
    }
}