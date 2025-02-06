// See https://aka.ms/new-console-template for more information


using DocumentFormat.OpenXml.Wordprocessing;
using System.Configuration;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Runtime.ConstrainedExecution;
using DocumentFormat.OpenXml.Vml;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;
using Microsoft.Extensions.Logging;

internal class Alert
{
    internal static void SendMail_sample(List<Analyzer.AnalysisResult> results)
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
        //No.01【所持】【権利】【注目】【決前】【決後】【売後】
        //1928：積水ハウス(株)（建設業）
        //株価：1,234.0（2024/11/29：L99.99,S99.99）★
        //市場：東証プライム,名証プレミア
        //配当利回り：3.64%（50%,5月,11月）★
        //優待利回り：3.64%（QUOカード,100株,5月,11月,月末）★
        //通期予想（前期比）：
        //初：24/04/26：増収増益増配（+50%,+50%,+50）
        //修：24/07/31：増収増益増配（+50%,+50%,+50）★
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
        //チャート：
        //2024/10/14：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/13：3,951.0：-99.99%：L99.99,S99.99
        //2024/10/12：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/11：3,951.0：-99.99%：L99.99,S99.99
        //2024/10/10：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/09：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/08：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/07：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/06：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/05：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/04：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/03：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/02：3,951.0：+99.99%：L99.99,S99.99
        //2024/10/01：3,951.0：+99.99%：L99.99,S99.99
        //メモ：
        //ほげほげほげほげほげ。

        var alertFilePath = CommonUtils.ReplacePlaceholder(CommonUtils.Instance.FilepathOfAlert, "{yyyyMMdd}", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));
        var mark = CommonUtils.Instance.BadgeString.ShouldWatch;

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

                    // バッジの取得
                    string badge = string.Empty;
                    if (r.StockInfo.IsOwnedNow()) badge += CommonUtils.Instance.BadgeString.IsOwned;
                    if (r.StockInfo.IsCloseToDividendRecordDate() || r.StockInfo.IsCloseToShareholderBenefitRecordDate()) badge += CommonUtils.Instance.BadgeString.IsCloseToRecordDate;
                    if (r.StockInfo.IsFavorite) badge += CommonUtils.Instance.BadgeString.IsFavorite;
                    if (r.StockInfo.IsCloseToQuarterEnd()) badge += CommonUtils.Instance.BadgeString.IsCloseToQuarterEnd;
                    if (r.StockInfo.IsAfterQuarterEnd()) badge += CommonUtils.Instance.BadgeString.IsAfterQuarterEnd;
                    if (r.StockInfo.IsJustSold()) badge += CommonUtils.Instance.BadgeString.IsJustSold;

                    writer.WriteLine("");
                    writer.WriteLine($"No.{alertCount.ToString("D2")}{badge}");
                    writer.WriteLine($"{r.StockInfo.Code}：{r.StockInfo.Name}（{r.StockInfo.Industry}）");
                    writer.WriteLine($"株価：{r.StockInfo.LatestPrice.ToString("N1")}" +
                        $"（{r.StockInfo.LatestPriceDate.ToString("yyyy/MM/dd")}" +
                        $"：L{r.StockInfo.LatestPriceRSIL.ToString("N2")}" +
                        $",S{r.StockInfo.LatestPriceRSIS.ToString("N2")}" +
                        $"）{(r.StockInfo.OversoldIndicator() || (r.StockInfo.IsOwnedNow() && r.StockInfo.OverboughtIndicator()) ? mark : string.Empty)}");
                    writer.WriteLine($"市場：{r.StockInfo.Section}");
                    writer.WriteLine($"配当利回り：{CommonUtils.Instance.ConvertToPercentage(r.StockInfo.DividendYield)}（{CommonUtils.Instance.ConvertToPercentage(r.StockInfo.DividendPayoutRatio)},{r.StockInfo.DividendRecordDateMonth}）{(r.StockInfo.IsCloseToDividendRecordDate() ? mark : string.Empty)}");
                    if (!string.IsNullOrEmpty(r.StockInfo.ShareholderBenefitsDetails))
                        writer.WriteLine($"優待利回り：{CommonUtils.Instance.ConvertToPercentage(r.StockInfo.ShareholderBenefitYield)}（{r.StockInfo.ShareholderBenefitsDetails},{r.StockInfo.NumberOfSharesRequiredForBenefits},{r.StockInfo.ShareholderBenefitRecordMonth},{r.StockInfo.ShareholderBenefitRecordDay}）{(r.StockInfo.IsCloseToShareholderBenefitRecordDate() ? mark : string.Empty)}");

                    // 通期予想履歴
                    count = 0;
                    foreach (var p in r.StockInfo.FullYearPerformancesForcasts)
                    {
                        if (count == 0) writer.WriteLine($"通期予想（前期比）：");
                        writer.WriteLine($"{p.Category}：{p.RevisionDate}：{p.Summary}{(p.HasUpwardRevision() ? mark : string.Empty)}");
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

                    writer.WriteLine($"チャート：");
                    foreach (var p in r.StockInfo.ChartPrices)
                    {
                        writer.WriteLine($"{p.Date.ToString("yyyy/MM/dd")}" +
                            $"：{p.Price.ToString("N1")}" +
                            $"：{CommonUtils.Instance.ConvertToPercentage(p.Volatility)}" +
                            $"：L{p.RSIL.ToString("N2")}" +
                            $",S{p.RSIS.ToString("N2")}");
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

    internal static void SendMail()
    {
        string[] Scopes = { GmailService.Scope.GmailSend };
        string ApplicationName = "Gmail API .NET Quickstart";

        UserCredential credential;

        string credentialFilepath = CommonUtils.Instance.FilepathOfGmailAPICredential;
        if (!File.Exists(credentialFilepath))
        {
            CommonUtils.Instance.Logger.LogInformation($"Gmail送信スキップ：APICredentialFileなし({credentialFilepath})");
            return;
        }

        using (var stream =
            new FileStream(credentialFilepath, FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "sadac23@gmail.com",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            CommonUtils.Instance.Logger.LogInformation("Credential file saved to: " + credPath);
        }

        // Create Gmail API service.
        var service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // Create the email content
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Your Name", "sadac23@gmail.com"));
        emailMessage.To.Add(new MailboxAddress("Recipient Name", "sadac23@gmail.com"));
        emailMessage.Subject = CommonUtils.Instance.MailSubject;

        var body = new TextPart("plain")
        {
            Text = "OptimalTrader processing has completed."
        };

        var alertFilePath = CommonUtils.ReplacePlaceholder(CommonUtils.Instance.FilepathOfAlert, "{yyyyMMdd}", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));

        // テキストファイルの添付
        var attachment = new MimePart("text", "plain")
        {
            Content = new MimeContent(File.OpenRead(alertFilePath)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = System.IO.Path.GetFileName(alertFilePath)
        };

        // メールに本文と添付ファイルを追加
        var multipart = new Multipart("mixed");
        multipart.Add(body);
        multipart.Add(attachment);
        emailMessage.Body = multipart;

        // Encode the email content
        var message = new Message
        {
            Raw = Base64UrlEncode(emailMessage.ToString())
        };

        // Send the email
        service.Users.Messages.Send(message, "sadac23@gmail.com").Execute();
        CommonUtils.Instance.Logger.LogInformation("Email sent successfully!");
    }
    private static string Base64UrlEncode(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(inputBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}