// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using static StockInfo;
using System.Net.Http;
using System.Security.Policy;
using static WatchList;
using System.Globalization;
using System.Data;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.Extensions.Logging;

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

            CommonUtils.Instance.Logger.LogInformation(url);

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
                            Roe = CommonUtils.Instance.GetDouble(roe),
                            Roa = CommonUtils.Instance.GetDouble(roa),
                            TotalAssetTurnover = totalAssetTurnover,
                            AdjustedEarningsPerShare = adjustedEarningsPerShare,
                        };

                        stockInfo.FullYearProfits.Add(p);
                        stockInfo.Roe = CommonUtils.Instance.GetDouble(roe);
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

                stockInfo.UpdateDividendPayoutRatio();
            }

            // 修正履歴
            rows = htmlDocument.DocumentNode.SelectNodes("//*[@id=\"finance_box\"]/div[6]/table/tbody/tr");

            if (rows != null && rows.Count != 0)
            {
                var countHeader = 0;
                FullYearPerformanceForcast cloneP = null;

                foreach (var row in rows)
                {
                    var columns = row.SelectNodes("td|th");

                    var fiscalPeriod = string.Empty;
                    var revisionDate = string.Empty;
                    var category = string.Empty;
                    var revisionDirection = string.Empty;
                    var revenue = string.Empty;
                    var operatingProfit = string.Empty;
                    var ordinaryProfit = string.Empty;
                    var netProfit = string.Empty;
                    var revisedDividend = string.Empty;

                    // ヘッダ行
                    if (columns != null && columns.Count >= 10)
                    {
                        countHeader++;

                        fiscalPeriod = columns[1].InnerText.Trim();
                        revisionDate = columns[2].InnerText.Trim();
                        category = columns[3].InnerText.Trim();
                        revisionDirection = string.Empty;
                        revenue = columns[5].InnerText.Trim();
                        operatingProfit = columns[6].InnerText.Trim();
                        ordinaryProfit = columns[7].InnerText.Trim();
                        netProfit = columns[8].InnerText.Trim();
                        revisedDividend = columns[9].InnerText.Trim();
                    }
                    // 明細行
                    else if (columns != null && columns.Count >= 9)
                    {
                        fiscalPeriod = string.Empty;
                        revisionDate = columns[1].InnerText.Trim();
                        category = columns[2].InnerText.Trim();
                        revisionDirection = string.Empty;
                        revenue = columns[4].InnerText.Trim();
                        operatingProfit = columns[5].InnerText.Trim();
                        ordinaryProfit = columns[6].InnerText.Trim();
                        netProfit = columns[7].InnerText.Trim();
                        revisedDividend = columns[8].InnerText.Trim();
                    }
                    else
                    {
                        // ヘッダと明細行以外はマスク行と判断してスキップ
                        continue;
                    }

                    // ヘッダ取得回数が2件未満は過去履歴なのでスキップ
                    if (countHeader < 2) continue;

                    // 修正配当値の不要文字列を除去
                    revisedDividend = revisedDividend.Replace("*", string.Empty);
                    revisedDividend = revisedDividend.Replace("#", string.Empty);

                    FullYearPerformanceForcast p = new FullYearPerformanceForcast()
                    {
                        FiscalPeriod = fiscalPeriod,
                        RevisionDate = ConvertToDateTime(revisionDate),
                        Category = category,
                        RevisionDirection = revisionDirection,
                        Revenue = revenue,
                        OperatingProfit = operatingProfit,
                        OrdinaryProfit = ordinaryProfit,
                        NetProfit = netProfit,
                        RevisedDividend = revisedDividend,
                        PreviousForcast = cloneP
                    };

                    // 前回分の保持
                    cloneP = (FullYearPerformanceForcast)p.Clone();

                    stockInfo.FullYearPerformancesForcasts.Add(p);
                }
            }

            // 四半期決算期間
            var node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"finance_box\"]/div[17]/div[1]/h3");
            var period = string.Empty;
            if (node != null)
            {
                period = node.InnerText.Trim();
            }
            else {
                node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"finance_box\"]/div[18]/div[1]/h3");
                if (node != null)
                {
                    period = node.InnerText.Trim();
                }
            }
            stockInfo.LastQuarterPeriod = period switch
            {
                string s when s.Contains("第１") => CommonUtils.Instance.QuarterString.Quarter1,
                string s when s.Contains("第２") => CommonUtils.Instance.QuarterString.Quarter2,
                string s when s.Contains("第３") => CommonUtils.Instance.QuarterString.Quarter3,
                string s when s.Contains("前期") => CommonUtils.Instance.QuarterString.Quarter4,
                _ => period // デフォルト値（変更しない場合）
            };

            // 実績履歴
            rows = htmlDocument.DocumentNode.SelectNodes("//*[@id=\"finance_box\"]/table[3]/tbody/tr");

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
                        var ordinaryProfit = columns[3].InnerText.Trim();
                        var netIncome = columns[4].InnerText.Trim();
                        var adjustedEarningsPerShare = columns[5].InnerText.Trim();
                        var progressRate = columns[6].InnerText.Trim();
                        var releaseDate = columns[7].InnerText.Trim();

                        var p = new QuarterlyPerformance()
                        {
                            FiscalPeriod = fiscalPeriod,
                            Revenue = revenue,
                            OperatingProfit = operatingIncome,
                            OrdinaryProfit = CommonUtils.Instance.GetDouble(ordinaryProfit),
                            NetProfit = netIncome,
                            AdjustedEarningsPerShare = adjustedEarningsPerShare,
                            AdjustedDividendPerShare = progressRate,
                            ReleaseDate = releaseDate,
                        };

                        stockInfo.QuarterlyPerformances.Add(p);
                    }
                }
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

    private DateTime ConvertToDateTime(string dateString)
    {
        // 指定された形式に従って文字列を解析します
        string format = "yy/MM/dd";
        DateTime dateTime = DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
        return dateTime;
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