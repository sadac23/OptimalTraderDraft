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
using DocumentFormat.OpenXml.Bibliography;

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

    internal static void SaveFile(List<StockInfo> results)
    {
        var alertFilePath = CommonUtils.ReplacePlaceholder(
            CommonUtils.Instance.FilepathOfAlert, "{yyyyMMdd}", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));

        using (StreamWriter writer = new StreamWriter(alertFilePath))
        {
            // ファイルヘッダー
            writer.WriteLine($"{DateTime.Today.ToString("yyyyMMdd")}");

            short alertCount = 0;

            foreach (StockInfo r in results)
            {
                if (r.ShouldAlert())
                {
                    string s = string.Empty;

                    alertCount++;

                    // バッジの取得
                    string badge = string.Empty;
                    if (r.IsFavorite) badge += CommonUtils.Instance.BadgeString.IsFavorite;
                    if (r.LatestPrice.OversoldIndicator()) badge += CommonUtils.Instance.BadgeString.IsOversold;
                    if (r.IsOwnedNow()) badge += CommonUtils.Instance.BadgeString.IsOwned;
                    if (r.IsCloseToRecordDate()) badge += CommonUtils.Instance.BadgeString.IsCloseToRecordDate;
                    if (r.IsRecordDate()) badge += CommonUtils.Instance.BadgeString.IsRecordDate;
                    if (r.IsAfterRecordDate()) badge += CommonUtils.Instance.BadgeString.IsAfterRecordDate;
                    if (r.IsQuarterEnd()) badge += CommonUtils.Instance.BadgeString.IsQuarterEnd;
                    if (r.IsCloseToQuarterEnd()) badge += CommonUtils.Instance.BadgeString.IsCloseToQuarterEnd;
                    if (r.IsAfterQuarterEnd()) badge += CommonUtils.Instance.BadgeString.IsAfterQuarterEnd;
                    if (r.IsJustSold()) badge += CommonUtils.Instance.BadgeString.IsJustSold;
                    if (r.HasDisclosure()) badge += CommonUtils.Instance.BadgeString.HasDisclosure;
                    //if (r.IsGoldenCrossPossible()) badge += CommonUtils.Instance.BadgeString.IsGoldenCrossPossible;
                    //if (r.IsGranvilleCase1Matched()) badge += CommonUtils.Instance.BadgeString.IsGranvilleCase1Matched;
                    //if (r.IsGranvilleCase2Matched()) badge += CommonUtils.Instance.BadgeString.IsGranvilleCase2Matched;

                    writer.WriteLine("");
                    writer.WriteLine($"No.{alertCount.ToString("D2")}{badge}");
                    writer.WriteLine(r.ToOutputString());
                }
            }

            // ファイルフッター
            writer.WriteLine();
            writer.WriteLine($"出力件数：{alertCount}件");
        }
    }
}