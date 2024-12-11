// See https://aka.ms/new-console-template for more information

using DocumentFormat.OpenXml.Bibliography;
using HtmlAgilityPack;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Policy;
using System.Transactions;
using static StockInfo;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        var stockInfo = new StockInfo(watchStock);
        var httpClient = new HttpClient();
        var htmlDocument = new HtmlDocument();

        var url = string.Empty;
        var html = string.Empty;
        HtmlNodeCollection rows = null;

        var urlBaseYahooFinance = $"https://finance.yahoo.co.jp/quote/{watchStock.Code}.T/history?styl=stock&from={from.ToString("yyyyMMdd")}&to={to.ToString("yyyyMMdd")}&timeFrame=d";
        var urlBaseKabutanTop = $"https://kabutan.jp/stock/?code={watchStock.Code}";
        var urlBaseKabutanFinance = $"https://kabutan.jp/stock/finance?code={watchStock.Code}";
        var urlBaseMinkabuDividend = $"https://minkabu.jp/stock/{watchStock.Code}/dividend";
        var urlBaseMinkabuYutai = $"https://minkabu.jp/stock/{watchStock.Code}/yutai";

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

        /** 株探（かぶたん） */
        url = urlBaseKabutanFinance;
        html = await httpClient.GetStringAsync(url);
        htmlDocument.LoadHtml(html);
        Console.WriteLine(url);

        // ROE
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

        // PER/PBR/利回り/信用倍率/時価総額
        rows = htmlDocument.DocumentNode.SelectNodes("//div[contains(@id, 'stockinfo_i3')]/table/tbody/tr");

        if (rows != null && rows.Count != 0)
        {
            short s = 0;
            foreach (var row in rows)
            {
                var columns = row.SelectNodes("td|th");

                // 1行目
                if (s == 0)
                {
                    if (columns != null && columns.Count > 3)
                    {
                        var per = columns[0].InnerText.Trim();
                        var pbr = columns[1].InnerText.Trim();
                        var dividendYield = columns[2].InnerText.Trim();
                        var marginBalanceRatio = columns[3].InnerText.Trim();
                        stockInfo.Per = ConvertToDoubleForPerPbr(per);
                        stockInfo.Pbr = ConvertToDoubleForPerPbr(pbr);
                        stockInfo.DividendYield = ConvertToDoubleForDividendYield(dividendYield);
                        stockInfo.MarginBalanceRatio = marginBalanceRatio;
                    }
                }
                // 2行目
                if (s == 1)
                {
                    if (columns != null && columns.Count > 1)
                    {
                        var marketCap = columns[1].InnerText.Trim();
                        stockInfo.MarketCap = ConvertToDoubleMarketCap(marketCap);
                    }

                }
                s++;
            }
        }

        // 通期業績
        rows = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'fin_year_t0_d fin_year_result_d')]/table/tbody/tr");

        if (rows != null && rows.Count != 0)
        {
            foreach (var row in rows)
            {
                var columns = row.SelectNodes("td|th");

                if (columns != null && columns.Count >= 8)
                {
                    var fiscalPeriod = columns[0].InnerText.Trim();
                    var revenue = columns[1].InnerText.Trim();
                    var operatingIncome = columns[2].InnerText.Trim();
                    var ordinaryIncome = columns[3].InnerText.Trim();
                    var netIncome = columns[4].InnerText.Trim();
                    var adjustedEarningsPerShare = columns[5].InnerText.Trim();
                    var adjustedDividendPerShare = columns[6].InnerText.Trim();
                    var announcementDate = columns[7].InnerText.Trim();

                    FullYearPerformance p = new FullYearPerformance()
                    {
                        FiscalPeriod = fiscalPeriod,
                        Revenue = revenue,
                        OperatingIncome = operatingIncome,
                        OrdinaryIncome = ordinaryIncome,
                        NetIncome = netIncome,
                        AdjustedEarningsPerShare= adjustedEarningsPerShare,
                        AdjustedDividendPerShare= adjustedDividendPerShare,
                        AnnouncementDate=announcementDate,
                    };

                    stockInfo.FullYearPerformances.Add( p );
                }
            }
            stockInfo.UpdateFullYearPerformanceForcastSummary();
        }

        // 自己資本比率

        // 指定されたヘッダータイトル
        string[] requiredHeaders = new string[]
        {
            "決算期",
            "１株純資産",
            "自己資本比率",
            "総資産",
            "自己資本",
            "剰余金",
            "有利子負債倍率",
            "発表日"
        };

        // すべてのテーブルを取得
        var tables = htmlDocument.DocumentNode.SelectNodes("//table");

        HtmlNodeCollection targetRows = null;

        foreach (var table in tables)
        {
            var headerRow = table.SelectSingleNode(".//tr[1]");
            if (headerRow != null)
            {
                var headerNodes = headerRow.SelectNodes("th");
                List<string> headers = new List<string>();

                // thが取得できないとき
                if (headerNodes == null) continue; 

                foreach (var th in headerNodes)
                {
                    headers.Add(th.InnerText.Trim());
                }

                bool allHeadersPresent = true;
                foreach (var requiredHeader in requiredHeaders)
                {
                    if (!headers.Contains(requiredHeader))
                    {
                        allHeadersPresent = false;
                        break;
                    }
                }

                if (allHeadersPresent)
                {
                    // ヘッダー行以外のすべての行を取得
                    targetRows = table.SelectNodes(".//tr[position() > 1]");
                    break;
                }
            }
        }

        if (targetRows != null)
        {
            foreach (var row in targetRows)
            {
                var columns = row.SelectNodes("td|th");

                if (columns != null && columns.Count >= 8)
                {
                    var equityRatio = columns[2].InnerText.Trim();
                    stockInfo.EquityRatio = equityRatio;
                }
            }
        }

        /** みんかぶ（配当） */
        url = urlBaseMinkabuDividend;
        html = await httpClient.GetStringAsync(url);
        htmlDocument.LoadHtml(html);
        Console.WriteLine(url);

        // 配当利回り/配当性向/配当権利確定月
        rows = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'ly_col ly_colsize_6 pt10')]/table/tr");

        if (rows != null && rows.Count != 0)
        {
            short s = 0;
            foreach (var row in rows)
            {
                var columns = row.SelectNodes("td|th");

                // 配当利回り
                if (s == 0)
                {
                    if (columns != null && columns.Count >= 2)
                    {
                        var dividendYield = columns[1].InnerText.Trim();
                        // みんかぶの利回りを利用しても良い
                        //stockInfo.DividendYield = dividendYield;
                    }
                }
                // 配当性向
                if (s == 1)
                {
                    if (columns != null && columns.Count >= 2)
                    {
                        var dividendPayoutRatio = columns[1].InnerText.Trim();
                        stockInfo.DividendPayoutRatio = dividendPayoutRatio;
                    }
                }
                // 配当権利確定月
                if (s == 2)
                {
                    if (columns != null && columns.Count >= 2)
                    {
                        var dividendRecordDateMonth = columns[1].InnerText.Trim();
                        stockInfo.DividendRecordDateMonth = dividendRecordDateMonth.Replace(" ", "");
                    }
                }
                s++;
            }
        }

        /** みんかぶ（優待） */
        url = urlBaseMinkabuYutai;
        html = await httpClient.GetStringAsync(url);
        htmlDocument.LoadHtml(html);
        Console.WriteLine(url);

        // 優待内容
        rows = htmlDocument.DocumentNode.SelectNodes("//h3[contains(@class, 'category fwb fsl')]");

        if (rows != null && rows.Count != 0)
        {
            string shareholderBenefitsDetails = rows[0].InnerText.Trim();
            stockInfo.ShareholderBenefitsDetails = shareholderBenefitsDetails;
        }

        // 優待利回り/優待発生株数/優待権利確定月
        rows = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'md_table simple md_table_vertical')]/tbody/tr");

        if (rows != null && rows.Count != 0)
        {
            short s = 0;
            foreach (var row in rows)
            {
                var columns = row.SelectNodes("td|th");

                // 優待利回り
                if (s == 0)
                {
                    if (columns != null && columns.Count >= 4)
                    {
                        var shareholderBenefitYield = columns[3].InnerText.Trim();
                        stockInfo.ShareholderBenefitYield = shareholderBenefitYield;
                    }
                }
                // 優待発生株数
                if (s == 1)
                {
                    if (columns != null && columns.Count >= 4)
                    {
                        var numberOfSharesRequiredForBenefits = columns[3].InnerText.Trim();
                        stockInfo.NumberOfSharesRequiredForBenefits = numberOfSharesRequiredForBenefits;
                    }
                }
                // 優待権利確定月
                if (s == 2)
                {
                    if (columns != null && columns.Count >= 4)
                    {
                        var shareholderBenefitRecordDateMonth = columns[1].InnerText.Trim();
                        stockInfo.ShareholderBenefitRecordDateMonth = shareholderBenefitRecordDateMonth;
                    }
                }
                s++;
            }
        }

        return stockInfo;
    }

    private double ConvertToDoubleForPerPbr(string multiplierString)
    {
        // "倍"を除去
        multiplierString = multiplierString.Replace("倍", "");

        // 文字列をdoubleに変換
        if (double.TryParse(multiplierString, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }

    private double ConvertToDoubleForDividendYield(string percentString)
    {
        // パーセント記号を除去
        percentString = percentString.Replace("％", "");

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

    private double ConvertToDoubleMarketCap(string input)
    {
        // 兆、億の値を取得する変数
        double trillion = 0;
        double billion = 0;

        // "兆" の位置を探して数値を抽出
        int trillionIndex = input.IndexOf("兆");
        if (trillionIndex != -1)
        {
            string trillionPart = input.Substring(0, trillionIndex).Replace(",", "");
            if (double.TryParse(trillionPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double tValue))
            {
                trillion = tValue * 1_000_000_000_000;
            }
            input = input.Substring(trillionIndex + 1);
        }

        // "億" の位置を探して数値を抽出
        int billionIndex = input.IndexOf("億");
        if (billionIndex != -1)
        {
            string billionPart = input.Substring(0, billionIndex).Replace(",", "");
            if (double.TryParse(billionPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double bValue))
            {
                billion = bValue * 100_000_000;
            }
        }

        // 合計を計算
        return trillion + billion;
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