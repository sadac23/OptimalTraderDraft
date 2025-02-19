// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Policy;
using System.Text.RegularExpressions;
using static WatchList;

internal class MinkabuScraper
{
    public MinkabuScraper()
    {
    }

    internal async Task ScrapeDividend(StockInfo stockInfo)
    {
        try
        {
            var urlBaseMinkabuDividend = $"https://minkabu.jp/stock/{stockInfo.Code}/dividend";

            var httpClient = new HttpClient();
            var htmlDocument = new HtmlDocument();

            var url = string.Empty;
            var html = string.Empty;
            HtmlNodeCollection rows = null;

            /** みんかぶ（配当） */
            url = urlBaseMinkabuDividend;
            html = await httpClient.GetStringAsync(url);
            htmlDocument.LoadHtml(html);

            CommonUtils.Instance.Logger.LogInformation(url);

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
                            // 前期の情報であるため採用しない
                            //stockInfo.DividendPayoutRatio = dividendPayoutRatio;
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

            // 直近の4Q決算月
            var node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"dps_detail\"]/div[2]/div/div[6]/table[1]/tbody/tr[1]/th[1]/text()");

            if (node != null)
            {
                // カレントの4Q決算月を取得
                var currentFiscalMonth = node.InnerText.Trim();
                stockInfo.CurrentFiscalMonth = ExtractYearMonth(currentFiscalMonth);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("リクエストエラー: " + e.Message);
        }

    }

    private DateTime? ExtractYearMonth(string input)
    {
        // 正規表現パターンを定義
        string pattern = @"(\d{4})年(\d{1,2})月期";

        // 正規表現でマッチを検索
        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            // マッチした文字列から年と月を抽出
            int year = int.Parse(match.Groups[1].Value);
            int month = int.Parse(match.Groups[2].Value);

            // DateTimeオブジェクトを作成
            return new DateTime(year, month, 1);
        }

        // マッチしなかった場合はnullを返す
        return null;
    }

    internal async Task ScrapeYutai(StockInfo stockInfo)
    {
        try
        {
            var urlBaseMinkabuYutai = $"https://minkabu.jp/stock/{stockInfo.Code}/yutai";

            var httpClient = new HttpClient();
            var htmlDocument = new HtmlDocument();

            var url = string.Empty;
            var html = string.Empty;
            HtmlNodeCollection rows = null;

            /** みんかぶ（優待） */
            url = urlBaseMinkabuYutai;
            html = await httpClient.GetStringAsync(url);
            htmlDocument.LoadHtml(html);

            CommonUtils.Instance.Logger.LogInformation(url);

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
                            //stockInfo.DividendYield = ConvertToDoubleForDividendYield(dividendYield);
                            stockInfo.ShareholderBenefitYield = ConvertToDoubleForYield(shareholderBenefitYield);
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
                            var shareholderBenefitRecordMonth = columns[1].InnerText.Trim();
                            var shareholderBenefitRecordDay = columns[3].InnerText.Trim();
                            stockInfo.ShareholderBenefitRecordMonth = shareholderBenefitRecordMonth;
                            stockInfo.ShareholderBenefitRecordDay = shareholderBenefitRecordDay;
                        }
                    }
                    s++;
                }
            }
        }
        catch { 
            // 無視
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

}