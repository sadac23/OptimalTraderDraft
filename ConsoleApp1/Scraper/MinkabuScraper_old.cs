//// See https://aka.ms/new-console-template for more information

//using ConsoleApp1.Assets;
//using HtmlAgilityPack;
//using Microsoft.Extensions.Logging;
//using System.Globalization;

//internal class MinkabuScraper
//{
//    public MinkabuScraper()
//    {
//    }

//    internal async Task ScrapeDividend(AssetInfo stockInfo)
//    {
//        try
//        {
//            var urlBaseMinkabuDividend = $"https://minkabu.jp/stock/{stockInfo.Code}/dividend";
//            var htmlDocument = new HtmlDocument();
//            var url = urlBaseMinkabuDividend;
//            var html = string.Empty;
//            HtmlNodeCollection rows = null;

//            CommonUtils.Instance.Logger.LogInformation(url);

//            try
//            {
//                html = await CommonUtils.Instance.HttpClient.GetStringAsync(url);
//            }
//            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
//            {
//                CommonUtils.Instance.Logger.LogWarning($"ScrapeDividend: 404 Not Found - Code:{stockInfo.Code} URL:{url}");
//                return;
//            }
//            catch (HttpRequestException ex)
//            {
//                CommonUtils.Instance.Logger.LogError(ex, $"ScrapeDividend: HttpRequestException - Code:{stockInfo.Code} URL:{url}");
//                return;
//            }
//            catch (Exception ex)
//            {
//                CommonUtils.Instance.Logger.LogError(ex, $"ScrapeDividend: Unexpected error - Code:{stockInfo.Code} URL:{url}");
//                return;
//            }

//            htmlDocument.LoadHtml(html);

//            // 配当利回り/配当性向/配当権利確定月
//            rows = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'ly_col ly_colsize_6 pt10')]/table/tr");

//            if (rows != null && rows.Count != 0)
//            {
//                short s = 0;
//                foreach (var row in rows)
//                {
//                    var columns = row.SelectNodes("td|th");

//                    // 配当利回り
//                    if (s == 0)
//                    {
//                        if (columns != null && columns.Count >= 2)
//                        {
//                            var dividendYield = columns[1].InnerText.Trim();
//                            // みんかぶの利回りを利用しても良い
//                            if (double.TryParse(dividendYield.Replace("%", "").Replace("％", ""), out double yield))
//                            {
//                                stockInfo.DividendYield = yield / 100.0;
//                            }
//                        }
//                    }
//                    // 配当性向
//                    if (s == 1)
//                    {
//                        if (columns != null && columns.Count >= 2)
//                        {
//                            var dividendPayoutRatio = columns[1].InnerText.Trim();
//                            // 前期の情報であるため採用しない場合はコメントアウト
//                            //if (double.TryParse(dividendPayoutRatio.Replace("%", "").Replace("％", ""), out double payout))
//                            //{
//                            //    stockInfo.DividendPayoutRatio = payout / 100.0;
//                            //}
//                        }
//                    }
//                    // 配当権利確定月
//                    if (s == 2)
//                    {
//                        if (columns != null && columns.Count >= 2)
//                        {
//                            var dividendRecordDateMonth = columns[1].InnerText.Trim();
//                            stockInfo.DividendRecordDateMonth = dividendRecordDateMonth.Replace(" ", "");
//                        }
//                    }
//                    s++;
//                }
//            }
//        }
//        catch (Exception e)
//        {
//            CommonUtils.Instance.Logger.LogError(e, $"ScrapeDividend: {e.Message} Code:{stockInfo.Code}");
//        }
//    }

//    internal async Task ScrapeYutai(AssetInfo stockInfo)
//    {
//        try
//        {
//            var urlBaseMinkabuYutai = $"https://minkabu.jp/stock/{stockInfo.Code}/yutai";

//            var htmlDocument = new HtmlDocument();

//            var url = string.Empty;
//            var html = string.Empty;
//            HtmlNodeCollection rows = null;

//            /** みんかぶ（優待） */
//            url = urlBaseMinkabuYutai;
//            html = await CommonUtils.Instance.HttpClient.GetStringAsync(url);
//            htmlDocument.LoadHtml(html);

//            CommonUtils.Instance.Logger.LogInformation(url);

//            // 優待内容
//            rows = htmlDocument.DocumentNode.SelectNodes("//*[@id=\"yutai_info\"]/div[4]/div[1]/div[2]/div/div[1]/h3");

//            if (rows != null && rows.Count != 0)
//            {
//                string shareholderBenefitsDetails = rows[0].InnerText.Trim();
//                stockInfo.ShareholderBenefitsDetails = shareholderBenefitsDetails;
//            }

//            // 優待利回り/優待発生株数/優待権利確定月
//            rows = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'md_table simple md_table_vertical')]/tbody/tr");

//            if (rows != null && rows.Count != 0)
//            {
//                short s = 0;
//                foreach (var row in rows)
//                {
//                    var columns = row.SelectNodes("td|th");

//                    // 優待利回り
//                    if (s == 0)
//                    {
//                        if (columns != null && columns.Count >= 4)
//                        {
//                            var shareholderBenefitYield = columns[3].InnerText.Trim();
//                            //stockInfo.DividendYield = ConvertToDoubleForDividendYield(dividendYield);
//                            stockInfo.ShareholderBenefitYield = ConvertToDoubleForYield(shareholderBenefitYield);
//                        }
//                    }
//                    // 優待発生株数
//                    if (s == 1)
//                    {
//                        if (columns != null && columns.Count >= 4)
//                        {
//                            var numberOfSharesRequiredForBenefits = columns[3].InnerText.Trim();
//                            stockInfo.NumberOfSharesRequiredForBenefits = numberOfSharesRequiredForBenefits;
//                        }
//                    }
//                    // 優待権利確定月
//                    if (s == 2)
//                    {
//                        if (columns != null && columns.Count >= 4)
//                        {
//                            var shareholderBenefitRecordMonth = columns[1].InnerText.Trim();
//                            var shareholderBenefitRecordDay = columns[3].InnerText.Trim();
//                            stockInfo.ShareholderBenefitRecordMonth = shareholderBenefitRecordMonth;
//                            stockInfo.ShareholderBenefitRecordDay = shareholderBenefitRecordDay;
//                        }
//                    }
//                    s++;
//                }
//            }
//        }
//        catch (Exception e) {
//            CommonUtils.Instance.Logger.LogError(e, "ScrapeYutai: " + e.Message);
//        }
//    }
//    private double ConvertToDoubleForYield(string percentString)
//    {
//        // パーセント記号を除去
//        percentString = percentString.Replace("％", "");
//        percentString = percentString.Replace("%", "");

//        // 文字列をdoubleに変換
//        if (double.TryParse(percentString, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
//        {
//            // パーセントから小数に変換
//            return value / 100.0;
//        }
//        else
//        {
//            return 0;
//        }
//    }

//}