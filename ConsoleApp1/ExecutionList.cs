// See https://aka.ms/new-console-template for more information

using ClosedXML.Excel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;

internal class ExecutionList
{
    internal static List<Execution> GetExecutions(List<ListDetail> executionList, string code)
    {
        List<Execution> list = new List<Execution>();

        foreach (ListDetail detail in executionList) 
        {
            if (detail.Code == code)
            {
                if (!string.IsNullOrEmpty(detail.BuyDate))
                {
                    Execution execution = new Execution()
                    {
                        No = detail.No,
                        Code = detail.Code,
                        Name = detail.Name,
                        BuyOrSell = CommonUtils.Instance.BuyOrSellString.Buy,
                        Date = DateTime.Parse(detail.BuyDate),
                        Price = double.Parse(detail.BuyPrice),
                        Quantity = double.Parse(detail.BuyQuantity),
                        HasSellExecuted = !string.IsNullOrEmpty(detail.SellDate) ? true : false,
                    };
                    list.Add(execution);
                }

                if (!string.IsNullOrEmpty(detail.SellDate))
                {
                    Execution executionS = new Execution()
                    {
                        No = detail.No,
                        Code = detail.Code,
                        Name = detail.Name,
                        BuyOrSell = CommonUtils.Instance.BuyOrSellString.Sell,
                        Date = DateTime.Parse(detail.SellDate),
                        Price = double.Parse(detail.SellPrice),
                        Quantity= double.Parse(detail.SellQuantity),
                        HasSellExecuted = false
                    };
                    list.Add(executionS);
                }
            }
        }
        return list;
    }

    internal static List<ListDetail> LoadXlsx()
    {
        List<ListDetail> results = new List<ListDetail>();

        // 読み込むワークシート名を指定
        string sheetName = "約定履歴";

        // 読み込み開始行のインデックス（1ベース）
        int startRowIndex = 5;

        // Excelファイルを読み込む
        using (var workbook = new XLWorkbook(CommonUtils.Instance.FilepathOfExecutionList))
        {
            // 指定したワークシートを取得
            var worksheet = workbook.Worksheet(sheetName);

            if (worksheet != null)
            {
                // 指定行から順番にデータを取得
                var rows = worksheet.RowsUsed()
                    .Where(row => row.RowNumber() >= startRowIndex);

                foreach (var row in rows)
                {
                    ListDetail data = new ListDetail
                    {
                        No = row.Cell(2).Value.ToString(),
                        Code = row.Cell(4).Value.ToString(),
                        Name = row.Cell(5).Value.ToString(),
                        BuyDate = row.Cell(3).Value.ToString(),
                        BuyQuantity = row.Cell(6).Value.ToString(),
                        BuyPrice = row.Cell(7).Value.ToString(),
                        SellDate = row.Cell(12).Value.ToString(),
                        SellQuantity = row.Cell(13).Value.ToString(),
                        SellPrice = row.Cell(14).Value.ToString(),
                    };
                    results.Add(data);
                }
            }
            else
            {
                CommonUtils.Instance.Logger.LogError($"ワークシート '{sheetName}' が見つかりません。");
            }
        }

        return results;
    }

    internal static void UpdateFromGmail()
    {
        string[] Scopes = { GmailService.Scope.GmailReadonly };
        string ApplicationName = "Gmail API .NET Quickstart";

        UserCredential credential;

        string credentialFilepath = CommonUtils.Instance.FilepathOfGmailAPICredential;

        // クレデンシャルが存在しない場合は無視
        if (string.IsNullOrEmpty(credentialFilepath)) return;
        if (!File.Exists(credentialFilepath))
        {
            CommonUtils.Instance.Logger.LogInformation($"約定リスト更新（Gmail検索）スキップ：APICredentialFileなし({credentialFilepath})");
            return;
        }

        using (var stream =
            new FileStream(credentialFilepath, FileMode.Open, FileAccess.Read))
        {
            // The file token.json stores the user's access and refresh tokens, and is created
            // automatically when the authorization flow completes for the first time.
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

        // Define parameters of request.
        UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List("me");
        request.Q = "subject:国内株式の注文が約定しました"; // 検索クエリを指定

        // List messages.
        IList<Message> messages = request.Execute().Messages;

        CommonUtils.Instance.Logger.LogInformation("Messages:");
        if (messages != null && messages.Count > 0)
        {
            foreach (var messageItem in messages)
            {
                var message = service.Users.Messages.Get("me", messageItem.Id).Execute();
                CommonUtils.Instance.Logger.LogInformation($"- {message.Snippet}");
            }
        }
        else
        {
            CommonUtils.Instance.Logger.LogInformation("No messages found.");
        }
    }

    public class ListDetail
    {
        public string No { get; internal set; }
        public string BuyDate { get; internal set; }
        public string Code { get; internal set; }
        public string Name { get; internal set; }
        public string BuyPrice { get; internal set; }
        public string SellDate { get; internal set; }
        public string SellPrice { get; internal set; }
        public string BuyQuantity { get; internal set; }
        public string SellQuantity { get; internal set; }
    }

    internal class Execution
    {
        /// <summary>
        /// コード
        /// </summary>
        public string Code { get; internal set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// 買：買い付け
        /// 売：売り付け
        /// </summary>
        public string BuyOrSell { get; internal set; }
        /// <summary>
        /// 取引日
        /// </summary>
        public DateTime Date { get; internal set; }
        /// <summary>
        /// 価格
        /// </summary>
        public double Price { get; internal set; }
        /// <summary>
        /// 数量
        /// </summary>
        public double Quantity { get; set; }
        /// <summary>
        /// 約定リストのNo
        /// </summary>
        public string No { get; internal set; }
        /// <summary>
        /// 売り約定があるか？
        /// </summary>
        /// <remarks>
        /// 売り約定の場合はfalse固定</remarks>
        public bool HasSellExecuted { get; set; }
    }
}