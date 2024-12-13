// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Policy;
using static WatchList;

internal class MinkabuScraper
{
    public MinkabuScraper()
    {
    }

    internal async Task ScrapeDividend(StockInfo stockInfo)
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
    }

    internal async Task ScrapeYutai(StockInfo stockInfo)
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
                        var shareholderBenefitRecordDateMonth = columns[1].InnerText.Trim();
                        stockInfo.ShareholderBenefitRecordDateMonth = shareholderBenefitRecordDateMonth;
                    }
                }
                s++;
            }
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