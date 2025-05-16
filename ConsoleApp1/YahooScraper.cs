// See https://aka.ms/new-console-template for more information

using DocumentFormat.OpenXml.Bibliography;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.Policy;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static WatchList;

internal class YahooScraper
{
    internal async Task ScrapeHistory(StockInfo stockInfo, DateTime from, DateTime to)
    {
        int _pageCountMax = CommonUtils.Instance.MaxPageCountToScrapeYahooHistory;

        try
        {
            var htmlDocument = new HtmlDocument();
            var urlBaseYahooFinance = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T/history?styl=stock&from={from.ToString("yyyyMMdd")}&to={to.ToString("yyyyMMdd")}&timeFrame=d";

            /** Yahooファイナンス */
            for (int i = 1; i < _pageCountMax; i++)
            {
                var url = urlBaseYahooFinance + $"&page={i}";
                var html = await CommonUtils.Instance.HttpClient.GetStringAsync(url);

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

        try
        {
            HttpResponseMessage response = await CommonUtils.Instance.HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string pageContent = await response.Content.ReadAsStringAsync();

            CommonUtils.Instance.Logger.LogInformation(url);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(pageContent);

            // 決算を含むノードをXPathで選択
            var earningsPeriod = document.DocumentNode.SelectSingleNode("//th[text()='決算']/following-sibling::td");
            if (earningsPeriod != null)
            {
                stockInfo.EarningsPeriod = earningsPeriod.InnerText.Trim();

                // 直近の4Q決算月
                stockInfo.CurrentFiscalMonth = GetNextClosingDate(CommonUtils.Instance.ExecusionDate, stockInfo.EarningsPeriod);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("ScrapeProfile: " + e.Message);
        }
    }
    internal async Task ScrapeTop(StockInfo stockInfo)
    {
        try
        {
            var url = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T";
            CommonUtils.Instance.Logger.LogInformation(url);

            HttpResponseMessage response = await CommonUtils.Instance.HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string pageContent = await response.Content.ReadAsStringAsync();

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

            // ** ETFは追加情報取得
            if (stockInfo.Classification == CommonUtils.Instance.Classification.JapaneseETFs)
            {
                // 決算月
                var dividendNode = document.DocumentNode.SelectSingleNode("//*[@id=\"referenc\"]/div/ul/li[11]/dl/dd/span/span/span");
                if (dividendNode != null)
                {
                    stockInfo.EarningsPeriod = dividendNode.InnerText.Trim();
                }
                // 運用会社
                var companyNode = document.DocumentNode.SelectSingleNode("//*[@id=\"referenc\"]/div/ul/li[6]/dl/dd/span/span/span");
                if (companyNode != null)
                {
                    stockInfo.FundManagementCompany = companyNode.InnerText.Trim();
                }
                // 信託報酬率
                var trustFeeRateNode = document.DocumentNode.SelectSingleNode("//*[@id=\"referenc\"]/div/ul/li[13]/dl/dd/span/span/span");
                if (trustFeeRateNode != null)
                {
                    stockInfo.TrustFeeRate = ConvertToDouble(trustFeeRateNode.InnerText.Trim());
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("ScrapeTop: " + e.Message);
        }
    }
    private DateTime GetDate(string v)
    {
        string[] formats = { "yyyy年M月d日", "yyyy年MM月dd日", "yyyy年MM月d日", "yyyy年M月dd日" };
        DateTime.TryParseExact(v, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);

        return date;
    }
    private double ConvertToDouble(string percentString)
    {
        // パーセント記号を除去
        percentString = percentString.Replace("％", "");
        percentString = percentString.Replace("%", "");

        // 文字列をdoubleに変換
        if (double.TryParse(percentString, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
        {
            // パーセントから小数に変換
            return value / 100.0;
        }
        else
        {
            return 0;
        }
    }

    internal async Task ScrapeDisclosure(StockInfo stockInfo)
    {
        var url = $"https://finance.yahoo.co.jp/quote/{stockInfo.Code}.T/disclosure";

        try
        {
            HttpResponseMessage response = await CommonUtils.Instance.HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string pageContent = await response.Content.ReadAsStringAsync();

            CommonUtils.Instance.Logger.LogInformation(url);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(pageContent);

            var nodes = document.DocumentNode.SelectNodes("//*[@id=\"disclist\"]/li");
            if (nodes != null && nodes.Count != 0)
            {
                foreach (var item in nodes)
                {
                    var header = string.Empty;
                    DateTime datetime = DateTime.MinValue;

                    var nodeHeader = item.SelectSingleNode("//article/a/h3");
                    if (nodeHeader != null)
                    {
                        header = nodeHeader.InnerText.Trim();
                    }
                    var nodeDate = item.SelectSingleNode("//article/a/ul/li[1]/time");
                    if (nodeDate != null)
                    {
                        datetime = this.ConvertToDatetime(nodeDate.InnerText.Trim());
                    }
                    stockInfo.Disclosures.Add(new StockInfo.Disclosure
                    {
                        Header = header,
                        Datetime = datetime,
                    });
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("ScrapeDisclosure: " + e.Message);
        }
    }

    private DateTime ConvertToDatetime(string v)
    {
        DateTime result;

        // 現在の年を取得
        int currentYear = DateTime.Now.Year;

        // 日付形式を試す
        string fullDateString = $"{currentYear}/{v}";
        if (DateTime.TryParseExact(fullDateString, "yyyy/M/d", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return result;
        }

        // 時刻形式を試す
        if (DateTime.TryParseExact(v, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            // 現在の日付に時刻を設定
            DateTime currentDate = DateTime.Now.Date;
            result = currentDate.Add(result.TimeOfDay);
            return result;
        }

        throw new FormatException("入力文字列の形式が正しくありません。");
    }
    private DateTime GetNextClosingDate(DateTime currentDate, string closingDateStr)
    {
        // 月を解析
        int month = ParseMonth(closingDateStr);
        if (month == -1)
        {
            throw new ArgumentException("無効な書式です。");
        }

        // 指定された月の末日を取得
        DateTime closingDate = new DateTime(currentDate.Year, month, DateTime.DaysInMonth(currentDate.Year, month));

        // 現在の日付と比較して次回の決算日を判定
        if (currentDate <= closingDate)
        {
            return closingDate;
        }
        else
        {
            // 現在の日付が指定された月の末日を過ぎている場合、次の年の同じ月の末日を返す
            return new DateTime(currentDate.Year + 1, month, DateTime.DaysInMonth(currentDate.Year + 1, month));
        }
    }

    private int ParseMonth(string closingDateStr)
    {
        // 月の名前を解析
        if (closingDateStr.EndsWith("月末日"))
        {
            string monthStr = closingDateStr.Replace("月末日", "");
            if (int.TryParse(monthStr, out int month) && month >= 1 && month <= 12)
            {
                return month;
            }
        }
        return -1; // 無効な書式
    }
}