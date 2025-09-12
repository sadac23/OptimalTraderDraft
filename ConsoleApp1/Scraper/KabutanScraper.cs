// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using System.Globalization;
using Microsoft.Extensions.Logging;
using ConsoleApp1.Assets;
using static ConsoleApp1.Assets.AssetInfo;
using ConsoleApp1.Assets.Models;

internal class KabutanScraper
{
    public KabutanScraper()
    {
    }

    internal async Task ScrapeFinance(AssetInfo stockInfo)
    {
        if (stockInfo.Classification == CommonUtils.Instance.Classification.JapaneseStocks) 
        {
            await this.ScrapeFinanceJP(stockInfo);
        }
        else if (stockInfo.Classification == CommonUtils.Instance.Classification.USStocks)
        {
            await this.ScrapeFinanceUS(stockInfo);
        }
    }

    private async Task ScrapeFinanceUS(AssetInfo stockInfo)
    {
        try
        {
            var url = $"https://us.kabutan.jp/stocks/{stockInfo.Code}/finance";

            var htmlDocument = new HtmlDocument();

            var html = string.Empty;

            /** 株探（かぶたん） */
            html = await CommonUtils.Instance.HttpClient.GetStringAsync(url);
            htmlDocument.LoadHtml(html);

            CommonUtils.Instance.Logger.LogInformation(url);

            // 市場
            var sectionNode = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div/div[2]/div[1]/main/div[1]/div[1]/div[1]/span");
            if (sectionNode != null)
            {
                stockInfo.Section = sectionNode.InnerText.Trim();
            }
            // 業種
            var industryNode = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"stockinfo_i2\"]/div/a");
            if (industryNode != null)
            {
                stockInfo.Industry = industryNode.InnerText.Trim();
            }
            // ROE
            var fullYearProfitRows = htmlDocument.DocumentNode.SelectNodes("/html/body/div/div[2]/div[1]/main/div[7]/table/tbody/tr");
            if (fullYearProfitRows != null && fullYearProfitRows.Count >= 3)
            {
                // 直近の3件のみ取得
                for (int i = fullYearProfitRows.Count - 3; i < fullYearProfitRows.Count; i++)
                {
                    var columns = fullYearProfitRows[i].SelectNodes("td|th");

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

            // PER
            var perNode = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div/div[2]/div[1]/main/div[1]/div[2]/div[2]/div[2]/div/div[1]/div[2]/text()");
            if (perNode != null)
            {
                var per = perNode.InnerText;
                stockInfo.Per = ConvertToDoubleForPerPbr(per);
            }
            // 利回り
            var dividendYieldNode = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div/div[2]/div[1]/main/div[1]/div[2]/div[2]/div[2]/div/div[3]/div[2]/text()");
            if (dividendYieldNode != null)
            {
                var dividendYield = dividendYieldNode.InnerText;
                stockInfo.DividendYield = ConvertToDoubleForYield(dividendYield);
            }
            // 時価総額
            var marketCapNode = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div/div[2]/div[1]/main/div[1]/div[2]/div[3]/div[2]/span[2]");
            if (marketCapNode != null)
            {
                var marketCap = marketCapNode.InnerText;
                stockInfo.MarketCap = ConvertJapaneseNumberToDouble(marketCap);
            }

            // 通期業績
            var fullYearPerformanceRows = htmlDocument.DocumentNode.SelectNodes("/html/body/div/div[2]/div[1]/main/div[5]/table/tbody/tr");
            if (fullYearPerformanceRows != null && fullYearPerformanceRows.Count >= 5)
            {
                // 直近の5件のみ取得
                for (int i = fullYearPerformanceRows.Count - 5; i < fullYearPerformanceRows.Count; i++)
                {
                    var columns = fullYearPerformanceRows[i].SelectNodes("td|th");

                    if (columns != null && columns.Count >= 8)
                    {
                        var fiscalPeriod = columns[0].InnerText.Trim();
                        var revenue = columns[1].InnerText.Trim();
                        var operatingProfit = columns[2].InnerText.Trim();
                        var ordinaryProfit = columns[3].InnerText.Trim();
                        var netProfit = columns[4].InnerText.Trim();
                        var adjustedEarningsPerShare = columns[5].InnerText.Trim();
                        var adjustedDividendPerShare = columns[6].InnerText.Trim();
                        var announcementDate = columns[7].InnerText.Trim();

                        FullYearPerformance p = new FullYearPerformance()
                        {
                            FiscalPeriod = fiscalPeriod,
                            Revenue = revenue,
                            OperatingProfit = operatingProfit,
                            OrdinaryProfit = ordinaryProfit,
                            NetProft = netProfit,
                            AdjustedEarningsPerShare = adjustedEarningsPerShare,
                            AdjustedDividendPerShare = adjustedDividendPerShare,
                            AnnouncementDate = announcementDate,
                        };

                        stockInfo.FullYearPerformances.Add(p);
                    }
                }
            }

            // 修正履歴
            if (stockInfo.FullYearPerformances.Count > 2)
            {
                var p = stockInfo.FullYearPerformances[stockInfo.FullYearPerformances.Count - 2];

                FullYearPerformanceForcast f = new FullYearPerformanceForcast()
                {
                    FiscalPeriod = p.FiscalPeriod,
                    RevisionDate = ConvertToDateTime(p.AnnouncementDate, "yyyy-MM-dd"),
                    Category = CommonUtils.Instance.ForecastCategoryString.Initial,
                    RevisionDirection = string.Empty,
                    Revenue = p.Revenue,
                    OperatingProfit = p.OperatingProfit,
                    OrdinaryProfit = p.OrdinaryProfit,
                    NetProfit = p.NetProft,
                    RevisedDividend = p.AdjustedDividendPerShare,
                    PreviousForcast = null
                };

                stockInfo.FullYearPerformancesForcasts.Add(f);
            }

            // 四半期決算期間
            var termRows = htmlDocument.DocumentNode.SelectNodes("/html/body/div/div[2]/div[1]/main/div[8]/div[2]/div");
            if (termRows != null && termRows.Count >= 4)
            {
                short count = 0;
                foreach (var item in termRows)
                {
                    // < div class="bg-gradient-to-b px-3 py-0.5 text-gray-700 border border-gray-500 text-shadow-white from-pink to-salmon">１Ｑ</div>
                    bool containPink = false;
                    foreach (var row in item.GetClasses())
                    {
                        if (row.Contains("pink")) containPink = true;
                    }

                    if (!containPink) break;

                    count++;
                }

                stockInfo.LastQuarterPeriod = count switch
                {
                    short s when s == 1 => CommonUtils.Instance.QuarterString.Quarter1,
                    short s when s == 2 => CommonUtils.Instance.QuarterString.Quarter2,
                    short s when s == 3 => CommonUtils.Instance.QuarterString.Quarter3,
                    short s when s == 4 => CommonUtils.Instance.QuarterString.Quarter4,
                    _ => string.Empty // デフォルト値（変更しない場合）
                };
            }

            // 実績履歴
            var resultRows = htmlDocument.DocumentNode.SelectNodes("/html/body/div/div[2]/div[1]/main/div[9]/table/tbody/tr");
            if (resultRows != null && resultRows.Count != 0)
            {
                List<QuarterlyPerformance> performances = new List<QuarterlyPerformance>();

                foreach (var row in resultRows)
                {
                    var columns = row.SelectNodes("td|th");

                    if (columns != null && columns.Count >= 7)
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

                        performances.Add(p);
                    }
                }


                //0,1,2,3,4
                int count = 0;
                count = stockInfo.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter1 ? performances.Count - 2 : count;
                count = stockInfo.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter2 ? performances.Count - 3 : count;
                count = stockInfo.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter3 ? performances.Count - 4 : count;
                count = stockInfo.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter4 ? performances.Count - 5 : count;

                var sum = new QuarterlyPerformance();
                for (int i = count; i < performances.Count - 1; i++)
                {
                    sum.FiscalPeriod = performances[i].FiscalPeriod;
                    sum.OrdinaryProfit += performances[i].OrdinaryProfit;
                    sum.ReleaseDate = performances[i].ReleaseDate;
                }

                stockInfo.QuarterlyPerformances.Add(sum);
            }
        }
        catch (Exception ex)
        {
            CommonUtils.Instance.Logger.LogError(ex.Message, ex);
        }
    }

    private double ConvertJapaneseNumberToDouble(string input)
    {
        // 単位とその倍率を定義
        var units = new (string Unit, double Multiplier)[]
        {
            ("兆", 1e12),
            ("億", 1e8),
            ("万", 1e4)
        };

        double result = 0;
        foreach (var (unit, multiplier) in units)
        {
            int unitIndex = input.IndexOf(unit);
            if (unitIndex != -1)
            {
                // 単位の前の数値部分を抽出
                string numberPart = input.Substring(0, unitIndex);
                // 数値部分からカンマを削除
                numberPart = numberPart.Replace(",", "");
                // 数値部分をdoubleに変換
                if (double.TryParse(numberPart, out double number))
                {
                    result += number * multiplier;
                }
                // 単位の後の部分を再設定
                input = input.Substring(unitIndex + unit.Length);
            }
        }

        // 残りの部分があれば、それを加算
        input = input.Replace(",", "").Replace("ドル", "").Replace("ﾄﾞﾙ", "");
        if (double.TryParse(input, out double remainingNumber))
        {
            result += remainingNumber;
        }

        return result;
    }

    /// <summary>
    /// 日本個別株向け
    /// </summary>
    /// <param name="stockInfo"></param>
    /// <returns></returns>
    private async Task ScrapeFinanceJP(AssetInfo stockInfo)
    {
        try
        {
            var urlBaseKabutanFinance = $"https://kabutan.jp/stock/finance?code={stockInfo.Code}";

            var htmlDocument = new HtmlDocument();

            var url = string.Empty;
            var html = string.Empty;
            HtmlNodeCollection rows = null;

            /** 株探（かぶたん） */
            url = urlBaseKabutanFinance;
            html = await CommonUtils.Instance.HttpClient.GetStringAsync(url);
            htmlDocument.LoadHtml(html);

            CommonUtils.Instance.Logger.LogInformation(url);

            // 市場
            var sectionNode = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"stockinfo_i1\"]/div[1]/div/span");
            if (sectionNode != null)
            {
                stockInfo.Section = sectionNode.InnerText.Trim();
            }

            // 業種
            var industryNode = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"stockinfo_i2\"]/div/a");
            if (industryNode != null)
            {
                stockInfo.Industry = industryNode.InnerText.Trim();
            }

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
                            OperatingProfit = operatingIncome,
                            OrdinaryProfit = ordinaryIncome,
                            NetProft = netIncome,
                            AdjustedEarningsPerShare = adjustedEarningsPerShare,
                            AdjustedDividendPerShare = adjustedDividendPerShare,
                            AnnouncementDate = announcementDate,
                        };

                        stockInfo.FullYearPerformances.Add(p);
                    }
                }
            }

            // 修正履歴
            rows = htmlDocument.DocumentNode.SelectNodes("//*[@id=\"finance_box\"]/div[6]/table/tbody/tr");

            if (rows != null && rows.Count != 0)
            {
                var countHeader = 0;
                FullYearPerformanceForcast cloneP = null;

                var tempFiscalPeriod = string.Empty;

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

                        // 明細行に同値を設定するため退避しておく
                        tempFiscalPeriod = fiscalPeriod;
                    }
                    // 明細行
                    else if (columns != null && columns.Count >= 9)
                    {
                        fiscalPeriod = tempFiscalPeriod;
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
            CommonUtils.Instance.Logger.LogError(e.Message, e);
        }
    }

    private DateTime ConvertToDateTime(string dateString, string format = "yy/MM/dd")
    {
        try
        {
            // 指定された形式に従って文字列を解析します
            DateTime dateTime = DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
            return dateTime;
        }
        catch
        {
            return DateTime.MinValue;

        }
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