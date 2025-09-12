// See https://aka.ms/new-console-template for more information


using System.Net.Mail;
using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;
using Microsoft.Extensions.Logging;
using ConsoleApp1.Assets;

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

    internal static void SendGmailViaSmtp()
    {
        string smtpServer = "smtp.gmail.com";
        int smtpPort = 587;
        string fromEmail = "sadac23@gmail.com";
        string appPassword = "llixzoitbcygegue";
        string toEmail = "sadac23@gmail.com"; // 自分宛て

        string subject = CommonUtils.Instance.MailSubject;
        string body = "OptimalTrader processing has completed.";

        // 添付ファイルパス
        var attachmentFilePath = CommonUtils.ReplacePlaceholder(CommonUtils.Instance.FilepathOfAlert, "{yyyyMMdd}", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));

        try
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmail);
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;

            // 添付ファイルの追加
            if (File.Exists(attachmentFilePath))
            {
                Attachment attachment = new Attachment(attachmentFilePath);
                mail.Attachments.Add(attachment);
            }
            else
            {
                Console.WriteLine("添付ファイルが存在しません: " + attachmentFilePath);
            }

            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.Credentials = new NetworkCredential(fromEmail, appPassword);
            smtpClient.EnableSsl = true;

            smtpClient.Send(mail);
            Console.WriteLine("メール送信に成功しました。");
        }
        catch (Exception ex)
        {
            Console.WriteLine("メール送信に失敗しました: " + ex.Message);
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

    /// <summary>
    /// アラートファイルを出力する
    /// </summary>
    /// <param name="results">通知対象の銘柄リスト</param>
    /// <param name="policyList">方針リスト</param>
    internal static void SaveFile(List<AssetInfo> results, List<string> policyList)
    {
        var alertFilePath = CommonUtils.ReplacePlaceholder(
            CommonUtils.Instance.FilepathOfAlert, "{yyyyMMdd}", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));

        using var writer = new StreamWriter(alertFilePath);

        // ヘッダー
        writer.WriteLine($"{DateTime.Today:yyyyMMdd}");

        // 方針
        writer.WriteLine();
        writer.WriteLine("方針：");
        foreach (var s in policyList)
            writer.WriteLine(s);

        short alertCount = 0;

        foreach (var r in results)
        {
            if (!r.ShouldAlert()) continue;

            alertCount++;
            var badge = string.Concat(
                r.IsFavorite ? CommonUtils.Instance.BadgeString.IsFavorite : "",
                r.LatestPrice.OversoldIndicator() ? CommonUtils.Instance.BadgeString.IsOversold : "",
                r.IsOwnedNow() ? CommonUtils.Instance.BadgeString.IsOwned : "",
                r.IsCloseToRecordDate() ? CommonUtils.Instance.BadgeString.IsCloseToRecordDate : "",
                r.IsRecordDate() ? CommonUtils.Instance.BadgeString.IsRecordDate : "",
                r.IsAfterRecordDate() ? CommonUtils.Instance.BadgeString.IsAfterRecordDate : "",
                r.IsQuarterEnd() ? CommonUtils.Instance.BadgeString.IsQuarterEnd : "",
                r.IsCloseToQuarterEnd() ? CommonUtils.Instance.BadgeString.IsCloseToQuarterEnd : "",
                r.IsAfterQuarterEnd() ? CommonUtils.Instance.BadgeString.IsAfterQuarterEnd : "",
                r.IsJustSold() ? CommonUtils.Instance.BadgeString.IsJustSold : "",
                r.HasDisclosure() ? CommonUtils.Instance.BadgeString.HasDisclosure : ""
            );

            writer.WriteLine();
            writer.WriteLine($"No.{alertCount:D2}{badge}");
            writer.WriteLine(r.ToOutputString());
        }

        // フッター
        writer.WriteLine();
        writer.WriteLine($"出力件数：{alertCount}件");
    }
}