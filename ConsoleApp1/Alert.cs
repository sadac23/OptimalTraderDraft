// See https://aka.ms/new-console-template for more information


using DocumentFormat.OpenXml.Wordprocessing;
using System.Configuration;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Runtime.ConstrainedExecution;

internal class Alert
{
    internal static void SendMail(List<Analyzer.AnalysisResult> results)
    {
        // YahooメールのSMTPサーバー情報
        string smtpServer = "smtp.mail.yahoo.com";
        int smtpPort = 587;

        // Yahooメールのアカウント情報
        string fromEmail = "your_yahoo_email@yahoo.com";
        string appPassword = "your_app_password"; // アプリパスワードを使用

        // 送信先のメールアドレス
        string toEmail = "recipient@example.com";

        // メールの件名と本文
        string subject = "Test Email from C#";
        string body = "This is a test email sent from a C# application using Yahoo Mail.";

        try
        {
            // メールメッセージの作成
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmail);
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;

            // SMTPクライアントの設定
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.Credentials = new NetworkCredential(fromEmail, appPassword);
            smtpClient.EnableSsl = true;

            // メールの送信
            smtpClient.Send(mail);
            Console.WriteLine("Email sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to send email. Error: " + ex.Message);
        }
    }
    internal static void SaveFile(List<Analyzer.AnalysisResult> results)
    {
        //No.01
        //1928：積水ハウス(株)（建設業）★
        //株価：1,234.0（2024/11/29：L99.99,S99.99）★
        //市場：東証プライム,名証プレミア
        //配当利回り：3.64%（50%,5月,11月）★
        //優待利回り：3.64%（QUOカード,100株,5月,11月,月末）★
        //通期予想（前期比）：
        //初：24/04/26：増収増益増配（+50%,+50%,+50）
        //修：24/07/31：増収増益増配（+50%,+50%,+50）
        //修：24/10/31：増収増益増配（+50%,+50%,+50）
        //通期進捗：3Q：80.0%（2024/12/23）★
        //前期進捗：3Q：80.0%（2024/12/23）
        //時価総額：2兆3,470億円
        //ROE：9.99→9.99→10.71★
        //PER：11.0倍（14.2）★
        //PBR：1.18倍（1.1）★
        //信用倍率：8.58倍
        //信用残：2,020,600/2,020,600（12/13）
        //出来高：2,020,600
        //自己資本比率：40.0%
        //決算：3月末
        //次回の決算発表日は2025年1月14日の予定です。★
        //約定履歴：
        //買：2024/12/04：2,068.0*300：-10.40%
        //売：2024/12/05：2,068.0*100：-10.40%
        //買：2024/12/06：2,060.0*100：-10.40%★
        //変動履歴：
        //2024/10/04：3,951.0：99.99：-10.40% (8)
        //2024/09/27：4,119.0：99.99：-10.40% (9)
        //2024/09/20：3,959.0：99.99：-10.40% (10)
        //メモ：
        //ほげほげほげほげほげ。

        var alertFilePath = CommonUtils.ReplacePlaceholder(CommonUtils.Instance.FilepathOfAlert, "{yyyyMMdd}", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));
        var mark = CommonUtils.Instance.WatchMark;

        using (StreamWriter writer = new StreamWriter(alertFilePath))
        {
            // ファイルヘッダー
            writer.WriteLine($"{DateTime.Today.ToString("yyyyMMdd")}");

            short alertCount = 0;

            foreach (Analyzer.AnalysisResult r in results)
            {
                if (r.ShouldAlert())
                {
                    short count = 0;
                    string s = string.Empty;

                    alertCount++;

                    writer.WriteLine("");
                    writer.WriteLine($"No.{alertCount.ToString("D2")}");
                    writer.WriteLine($"{r.StockInfo.Code}：{r.StockInfo.Name}（{r.StockInfo.Industry}）{(r.StockInfo.IsFavorite ? mark : string.Empty)}");
                    writer.WriteLine($"株価：{r.StockInfo.LatestPrice.ToString("N1")}（{r.StockInfo.LatestPriceDate.ToString("yyyy/MM/dd")}：L{r.StockInfo.LatestPriceRSIL.ToString("N2")},S{r.StockInfo.LatestPriceRSIS.ToString("N2")}）{(r.StockInfo.OversoldIndicator() ? mark : string.Empty)}");
                    writer.WriteLine($"市場：{r.StockInfo.Section}");
                    writer.WriteLine($"配当利回り：{CommonUtils.Instance.ConvertToPercentage(r.StockInfo.DividendYield)}（{CommonUtils.Instance.ConvertToPercentage(r.StockInfo.DividendPayoutRatio)},{r.StockInfo.DividendRecordDateMonth}）{(r.StockInfo.IsDividendRecordDateClose() ? mark : string.Empty)}");
                    if (!string.IsNullOrEmpty(r.StockInfo.ShareholderBenefitsDetails))
                        writer.WriteLine($"優待利回り：{CommonUtils.Instance.ConvertToPercentage(r.StockInfo.ShareholderBenefitYield)}（{r.StockInfo.ShareholderBenefitsDetails},{r.StockInfo.NumberOfSharesRequiredForBenefits},{r.StockInfo.ShareholderBenefitRecordMonth},{r.StockInfo.ShareholderBenefitRecordDay}）{(r.StockInfo.IsShareholderBenefitRecordDateClose() ? mark : string.Empty)}");

                    // 通期予想履歴
                    count = 0;
                    foreach (var p in r.StockInfo.FullYearPerformancesForcasts)
                    {
                        if (count == 0) writer.WriteLine($"通期予想（前期比）：");
                        writer.WriteLine($"{p.Category}：{p.RevisionDate}：{p.Summary}");
                        count++;
                    }

                    writer.WriteLine($"通期進捗：{r.StockInfo.QuarterlyPerformancePeriod}：{CommonUtils.Instance.ConvertToPercentage(r.StockInfo.QuarterlyFullyearProgressRate)}（{r.StockInfo.QuarterlyPerformanceReleaseDate.ToString("yyyy/MM/dd")}）{(r.StockInfo.IsAnnualProgressOnTrack() ? mark : string.Empty)}");
                    writer.WriteLine($"前期進捗：{r.StockInfo.QuarterlyPerformancePeriod}：{CommonUtils.Instance.ConvertToPercentage(r.StockInfo.PreviousFullyearProgressRate)}（{r.StockInfo.PreviousPerformanceReleaseDate.ToString("yyyy/MM/dd")}）");
                    writer.WriteLine($"時価総額：{CommonUtils.Instance.ConvertToYenNotation(r.StockInfo.MarketCap)}");

                    count = 0;
                    s = string.Empty;
                    foreach (StockInfo.FullYearProfit p in r.StockInfo.FullYearProfits)
                    {
                        if (count > 0) s += "→";
                        s += p.Roe;
                        count++;
                    }
                    if (!string.IsNullOrEmpty(s)) writer.WriteLine($"ROE：{s}{(r.StockInfo.IsROEAboveThreshold() ? mark : string.Empty)}");

                    writer.WriteLine($"PER：{CommonUtils.Instance.ConvertToMultiplierString(r.StockInfo.Per)}（{r.StockInfo.AveragePer}）{(r.StockInfo.IsPERUndervalued() ? mark : string.Empty)}");
                    writer.WriteLine($"PBR：{CommonUtils.Instance.ConvertToMultiplierString(r.StockInfo.Pbr)}（{r.StockInfo.AveragePbr}）{(r.StockInfo.IsPBRUndervalued() ? mark : string.Empty)}");
                    writer.WriteLine($"信用倍率：{r.StockInfo.MarginBalanceRatio}");
                    writer.WriteLine($"信用残：{r.StockInfo.MarginBuyBalance}/{r.StockInfo.MarginSellBalance}（{r.StockInfo.MarginBalanceDate}）");
                    writer.WriteLine($"出来高：{r.StockInfo.LatestTradingVolume}");
                    writer.WriteLine($"自己資本比率：{r.StockInfo.EquityRatio}");

                    s = string.Empty;
                    if (!string.IsNullOrEmpty(r.StockInfo.PressReleaseDate))
                    {
                        s += r.StockInfo.PressReleaseDate;
                        s += r.StockInfo.ExtractAndValidateDateWithinOneMonth() ? mark : string.Empty;
                    }
                    writer.WriteLine($"決算：{r.StockInfo.EarningsPeriod}");
                    writer.WriteLine($"{s}");

                    count = 0;
                    s = string.Empty;
                    var b = r.StockInfo.ShouldAverageDown();
                    foreach (ExecutionList.Execution e in r.StockInfo.Executions)
                    {
                        if (count == 0) writer.WriteLine($"約定履歴：{(b ? mark : string.Empty)}");
                        writer.WriteLine($"{e.BuyOrSell}：{e.Date.ToString("yyyy/MM/dd")}：{e.Price.ToString("N1")}*{e.Quantity}：{CommonUtils.Instance.ConvertToPercentage((r.StockInfo.LatestPrice / e.Price) - 1)}");
                        count++;
                    }

                    //count = 0;
                    //foreach(Analyzer.AnalysisResult.PriceVolatility p in r.PriceVolatilities)
                    //{
                    //    // 直近週は必ず表示
                    //    if (count == 0)
                    //    {
                    //        writer.WriteLine($"変動履歴：");
                    //        writer.WriteLine($"{p.VolatilityRateIndex1Date.ToString("yyyy/MM/dd")}：{p.VolatilityRateIndex1.ToString("N1")}：{ConvertToPercentage(p.VolatilityRate)}({p.VolatilityTerm})");
                    //    }
                    //    else
                    //    {
                    //        if (p.ShouldAlert)
                    //        {
                    //            writer.WriteLine($"{p.VolatilityRateIndex1Date.ToString("yyyy/MM/dd")}：{p.VolatilityRateIndex1.ToString("N1")}：{ConvertToPercentage(p.VolatilityRate)}({p.VolatilityTerm})");
                    //        }
                    //    }
                    //    count++;
                    //}

                    writer.WriteLine($"変動履歴：");
                    foreach (Analyzer.AnalysisResult.PriceVolatility p in r.PriceVolatilities)
                    {
                        writer.WriteLine($"{p.VolatilityRateIndex1Date.ToString("yyyy/MM/dd")}：{p.VolatilityRateIndex1.ToString("N1")}：{CommonUtils.Instance.ConvertToPercentage(p.VolatilityRate)}({p.VolatilityTerm}){(p.ShouldAlert ? CommonUtils.Instance.WatchMark : string.Empty)}");
                    }

                    if (!string.IsNullOrEmpty(r.StockInfo.Memo))
                    {
                        //メモ：
                        writer.WriteLine($"メモ：");
                        writer.WriteLine(r.StockInfo.Memo);
                    }
                }
            }

            // ファイルフッター
            writer.WriteLine();
            writer.WriteLine($"出力件数：{alertCount}件");
        }
    }
}