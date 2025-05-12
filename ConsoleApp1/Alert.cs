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
    internal static void SaveFile(List<StockInfo> results)
    {
        //No.01【注目】【所持】【権前】【権当】【権後】【決当】【決前】【決後】【売後】【金交】【開示】
        //1928：積水ハウス(株)
        //株価：1,234.0（24/11/29：S99.99,L99.99）★
        //市場/業種：東証Ｐ/建設業
        //配当利回り：3.64%（50%,5月,11月）★
        //優待利回り：3.64%（QUOカード,100株,5月,11月,月末）★
        //通期予想（前期比）：
        //初：24/04/26：増収増益増配（+50%,+50%,+50）
        //修：24/07/31：増収増益増配（+50%,+50%,+50）★
        //修：24/10/31：増収増益増配（+50%,+50%,+50）
        //通期進捗：3Q：80.0%（24/12/23：+99.9%）★
        //前期進捗：3Q：80.0%（24/12/23）
        //時価総額：2兆3,470億円
        //ROE：9.99→9.99→10.71★
        //PER：11.0倍（14.2）★
        //PBR：1.18倍（1.1）★
        //営業利益率：99.9%
        //信用倍率：8.58倍
        //信用残：2,020,600/2,020,600（12/13）
        //出来高：2,020,600
        //自己資本比率：40.0%
        //決算：3月末
        //次回の決算発表日は2025年1月14日の予定です。★
        //チャート（RSI）：
        //10/14：3,951.0：-99.99%（S99.99,L99.99）
        //テクニカル（MAD）：
        //10/14：3,951.0：S+99.99%,L+99.99%
        //約定履歴：
        //買：24/12/04：2,068.0*300：-10.40%
        //売：24/12/05：2,068.0*100：-10.40%
        //買：24/12/06：2,060.0*100：-10.40%★
        //メモ：
        //ほげほげほげほげほげ。

        var alertFilePath = CommonUtils.ReplacePlaceholder(CommonUtils.Instance.FilepathOfAlert, "{yyyyMMdd}", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));
        var mark = CommonUtils.Instance.BadgeString.ShouldWatch;

        using (StreamWriter writer = new StreamWriter(alertFilePath))
        {
            // ファイルヘッダー
            writer.WriteLine($"{DateTime.Today.ToString("yyyyMMdd")}");

            short alertCount = 0;

            foreach (StockInfo r in results)
            {
                if (r.ShouldAlert())
                {
                    short count = 0;
                    string s = string.Empty;

                    alertCount++;

                    // バッジの取得
                    string badge = string.Empty;
                    if (r.IsFavorite) badge += CommonUtils.Instance.BadgeString.IsFavorite;
                    if (r.IsOwnedNow()) badge += CommonUtils.Instance.BadgeString.IsOwned;
                    if (r.IsCloseToRecordDate()) badge += CommonUtils.Instance.BadgeString.IsCloseToRecordDate;
                    if (r.IsRecordDate()) badge += CommonUtils.Instance.BadgeString.IsRecordDate;
                    if (r.IsAfterRecordDate()) badge += CommonUtils.Instance.BadgeString.IsAfterRecordDate;
                    if (r.IsQuarterEnd()) badge += CommonUtils.Instance.BadgeString.IsQuarterEnd;
                    if (r.IsCloseToQuarterEnd()) badge += CommonUtils.Instance.BadgeString.IsCloseToQuarterEnd;
                    if (r.IsAfterQuarterEnd()) badge += CommonUtils.Instance.BadgeString.IsAfterQuarterEnd;
                    if (r.IsJustSold()) badge += CommonUtils.Instance.BadgeString.IsJustSold;
                    if (r.IsGoldenCrossPossible()) badge += CommonUtils.Instance.BadgeString.IsGoldenCrossPossible;
                    if (r.HasDisclosure()) badge += CommonUtils.Instance.BadgeString.HasDisclosure;

                    writer.WriteLine("");
                    writer.WriteLine($"No.{alertCount.ToString("D2")}{badge}");
                    writer.WriteLine($"{r.Code}：{r.Name}");
                    writer.WriteLine($"株価：{r.LatestPrice.Price.ToString("N1")}" +
                        $"（{r.LatestPrice.Date.ToString("yy/MM/dd")}" +
                        $"：S{r.LatestPrice.RSIS.ToString("N2")}" +
                        $",L{r.LatestPrice.RSIL.ToString("N2")}" +
                        $"）{(r.LatestPrice.OversoldIndicator() || (r.IsOwnedNow() && r.LatestPrice.OverboughtIndicator()) ? mark : string.Empty)}");
                    // ETFのみ
                    if (r.Classification == CommonUtils.Instance.Classification.JapaneseETFs)
                    {
                        writer.WriteLine($"運用会社：{r.FundManagementCompany}");
                        writer.WriteLine($"信託報酬：{CommonUtils.Instance.ConvertToPercentage(r.TrustFeeRate, false, "F3")}");
                    }
                    writer.WriteLine($"市場/業種：{r.Section}{(!string.IsNullOrEmpty(r.Industry) ? $"/{r.Industry}" : string.Empty)}");
                    writer.WriteLine($"配当利回り：{CommonUtils.Instance.ConvertToPercentage(r.DividendYield)}" +
                        $"（{CommonUtils.Instance.ConvertToPercentage(r.DividendPayoutRatio)}" +
                        $",{CommonUtils.Instance.ConvertToPercentage(r.Doe)}" +
                        $",{r.DividendRecordDateMonth}" +
                        $"）{(r.IsCloseToDividendRecordDate() ? mark : string.Empty)}");
                    if (!string.IsNullOrEmpty(r.ShareholderBenefitsDetails))
                        writer.WriteLine($"優待利回り：{CommonUtils.Instance.ConvertToPercentage(r.ShareholderBenefitYield)}" +
                            $"（{r.ShareholderBenefitsDetails}" +
                            $",{r.NumberOfSharesRequiredForBenefits}" +
                            $",{r.ShareholderBenefitRecordMonth}" +
                            $",{r.ShareholderBenefitRecordDay}" +
                            $"）{(r.IsCloseToShareholderBenefitRecordDate() ? mark : string.Empty)}");

                    // 通期予想履歴
                    count = 0;
                    foreach (var p in r.FullYearPerformancesForcasts)
                    {
                        if (count == 0) writer.WriteLine($"通期予想（前期比）：");
                        writer.WriteLine($"{p.Category}：{p.RevisionDate.ToString("yy/MM/dd")}：{p.Summary}{(p.HasUpwardRevision() ? mark : string.Empty)}");
                        count++;
                    }

                    writer.WriteLine($"通期進捗：{r.LastQuarterPeriod}" +
                        $"：{CommonUtils.Instance.ConvertToPercentage(r.QuarterlyFullyearProgressRate)}" +
                        $"（{r.QuarterlyPerformanceReleaseDate.ToString("yy/MM/dd")}" +
                        $"：{CommonUtils.Instance.ConvertToPercentage(r.QuarterlyOperatingProfitMarginYoY, true)}）" +
                        $"{(r.IsAnnualProgressOnTrack() ? mark : string.Empty)}");
                    writer.WriteLine($"前期進捗：{r.LastQuarterPeriod}" +
                        $"：{CommonUtils.Instance.ConvertToPercentage(r.PreviousFullyearProgressRate)}" +
                        $"（{r.PreviousPerformanceReleaseDate.ToString("yy/MM/dd")}）");

                    writer.WriteLine($"時価総額：{CommonUtils.Instance.ConvertToYenNotation(r.MarketCap)}");

                    count = 0;
                    s = string.Empty;
                    foreach (StockInfo.FullYearProfit p in r.FullYearProfits)
                    {
                        if (count > 0) s += "→";
                        s += p.Roe;
                        count++;
                    }
                    if (!string.IsNullOrEmpty(s)) writer.WriteLine($"ROE：{s}{(r.IsROEAboveThreshold() ? mark : string.Empty)}");

                    writer.WriteLine($"PER：{CommonUtils.Instance.ConvertToMultiplierString(r.Per)}（{r.AveragePer.ToString("N1")}）{(r.IsPERUndervalued() ? mark : string.Empty)}");
                    writer.WriteLine($"PBR：{CommonUtils.Instance.ConvertToMultiplierString(r.Pbr)}（{r.AveragePbr.ToString("N1")}）{(r.IsPBRUndervalued() ? mark : string.Empty)}");
                    writer.WriteLine($"営業利益率：{CommonUtils.Instance.ConvertToPercentage(r.OperatingProfitMargin)}");
                    writer.WriteLine($"信用倍率：{r.MarginBalanceRatio}");
                    writer.WriteLine($"信用残：{r.MarginBuyBalance}/{r.MarginSellBalance}（{r.MarginBalanceDate}）");
                    writer.WriteLine($"出来高：{r.LatestTradingVolume}");
                    writer.WriteLine($"自己資本比率：{r.EquityRatio}");

                    writer.WriteLine($"決算：{r.EarningsPeriod}");

                    s = string.Empty;
                    if (!string.IsNullOrEmpty(r.PressReleaseDate))
                    {
                        s += r.PressReleaseDate;
                        s += r.ExtractAndValidateDateWithinOneMonth() ? mark : string.Empty;
                        writer.WriteLine($"{s}");
                    }

                    count = 0;
                    s = string.Empty;
                    foreach (ExecutionList.Execution e in r.Executions)
                    {
                        if (count == 0) writer.WriteLine($"約定履歴：");

                        writer.WriteLine($"{e.BuyOrSell}" +
                            $"：{e.Date.ToString("yy/MM/dd")}" +
                            $"：{e.Price.ToString("N1")}*{e.Quantity}" +
                            $"：{CommonUtils.Instance.ConvertToPercentage((r.LatestPrice.Price / e.Price) - 1, true)}" +
                            $"{(r.ShouldAverageDown(e) ? mark : string.Empty)}");

                        count++;
                    }

                    //チャート：
                    writer.WriteLine($"チャート（RSI）：");
                    foreach (var p in r.ChartPrices)
                    {
                        //チャート（RSI）：
                        //10/14：3,951.0：-99.99%（S99.99,L99.99）
                        writer.WriteLine(
                            $"{p.Date.ToString("MM/dd")}" +
                            $"：{p.Price.ToString("N1")}" +
                            $"：{CommonUtils.Instance.ConvertToPercentage(p.Volatility, true)}" +
                            $"（S{p.RSIS.ToString("N2")}" +
                            $",L{p.RSIL.ToString("N2")}）" +
                            $"{(p.OversoldIndicator() ? mark : string.Empty)}" +
                            $"");
                    }

                    ////テクニカル（MAD）：
                    //writer.WriteLine($"テクニカル（SMAdev：MAD）：");
                    //foreach (var p in r.StockInfo.ChartPrices)
                    //{
                    //    //10/14：3,951.0：999.0：S+99.99%,L+99.99%
                    //    writer.WriteLine(
                    //        $"{p.Date.ToString("MM/dd")}" +
                    //        $"：{p.SMAdev.ToString("N1")}" +
                    //        $"：S{CommonUtils.Instance.ConvertToPercentage(p.MADS, true)}" +
                    //        $",L{CommonUtils.Instance.ConvertToPercentage(p.MADL, true)}" +
                    //        $"");
                    //}

                    if (!string.IsNullOrEmpty(r.Memo))
                    {
                        //メモ：
                        writer.WriteLine($"メモ：");
                        writer.WriteLine(r.Memo);
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

    internal static void SaveFileAdvance(List<StockInfo> results)
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
                    if (r.IsGoldenCrossPossible()) badge += CommonUtils.Instance.BadgeString.IsGoldenCrossPossible;
                    if (r.HasDisclosure()) badge += CommonUtils.Instance.BadgeString.HasDisclosure;
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