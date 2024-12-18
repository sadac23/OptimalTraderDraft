// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using static StockInfo;
using System.Net.Http;
using System.Security.Policy;
using static WatchList;
using System.Globalization;
using System.Data;

internal class KabutanScraper
{
    public KabutanScraper()
    {
    }

    internal async Task ScrapeFinance(StockInfo stockInfo)
    {
        try
        {
            var urlBaseKabutanFinance = $"https://kabutan.jp/stock/finance?code={stockInfo.Code}";

            var httpClient = new HttpClient();
            var htmlDocument = new HtmlDocument();

            var url = string.Empty;
            var html = string.Empty;
            HtmlNodeCollection rows = null;

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

                    if (columns != null && columns.Count >= 8)
                    {

                        var fiscalPeriod = columns[0].InnerText.Trim();
                        var revenue = columns[1].InnerText.Trim();
                        var operatingIncome = columns[2].InnerText.Trim();
                        var operatingMargin = columns[3].InnerText.Trim();
                        var roe = columns[4].InnerText.Trim();
                        var roa = columns[5].InnerText.Trim();
                        var totalAssetTurnover = columns[6].InnerText.Trim();
                        var adjustedEarningsPerShare = columns[7].InnerText.Trim();

                        FullYearProfit p = new FullYearProfit()
                        {
                            FiscalPeriod = fiscalPeriod,
                            Revenue = revenue,
                            OperatingIncome = operatingIncome,
                            OperatingMargin = operatingMargin,
                            Roe = GetDouble(roe),
                            Roa = GetDouble(roa),
                            TotalAssetTurnover = totalAssetTurnover,
                            AdjustedEarningsPerShare = adjustedEarningsPerShare,
                        };

                        stockInfo.FullYearProfits.Add(p);
                        stockInfo.Roe = this.GetDouble(roe);
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
                        if (columns != null && columns.Count >= 4)
                        {
                            var per = columns[0].InnerText.Trim();
                            var pbr = columns[1].InnerText.Trim();
                            var dividendYield = columns[2].InnerText.Trim();
                            var marginBalanceRatio = columns[3].InnerText.Trim();
                            stockInfo.Per = ConvertToDoubleForPerPbr(per);
                            stockInfo.Pbr = ConvertToDoubleForPerPbr(pbr);
                            stockInfo.DividendYield = ConvertToDoubleForYield(dividendYield);
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
                            AdjustedEarningsPerShare = adjustedEarningsPerShare,
                            AdjustedDividendPerShare = adjustedDividendPerShare,
                            AnnouncementDate = announcementDate,
                        };

                        stockInfo.FullYearPerformances.Add(p);
                    }
                }
                stockInfo.UpdateFullYearPerformanceForcastSummary();
                stockInfo.UpdateDividendPayoutRatio();
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

        }
        catch (Exception e)
        {
            Console.WriteLine("リクエストエラー: " + e.Message);
        }
    }
    internal double GetDouble(string v)
    {
        if (!double.TryParse(v, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double result))
        {
            Console.WriteLine("GetDoubleエラー: " + v);
        }
        return result;
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

    private double ConvertToDoubleForYield(string percentString)
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

}