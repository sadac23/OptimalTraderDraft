// See https://aka.ms/new-console-template for more information
using ConsoleApp1.Assets;
using ConsoleApp1.Database;
using ConsoleApp1.ExternalMaster;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // 分析結果
            var results = new List<AssetInfo>();

            try
            {
                // 開始
                CommonUtils.Instance.Logger.LogInformation(CommonUtils.Instance.MessageAtApplicationStartup);

                // DB接続初期化
                DbConnectionFactory.Initialize(CommonUtils.Instance.ConnectionString);

                // OneDriveリフレッシュ
                if (CommonUtils.Instance.ShouldRefreshOneDrive) CommonUtils.Instance.OneDriveRefresh();

                // 約定履歴リストを更新
                if (CommonUtils.Instance.ShouldUpdateExecutionList) ExecutionList.UpdateFromGmail();

                // 約定履歴取得
                var executionList = ExecutionList.LoadXlsx();

                // ウォッチリスト取得
                var watchList = WatchList.LoadXlsx();

                // マスタリスト取得
                var masterList = MasterList.LoadXlsx();

                // 方針リスト取得
                var policyList = PolicyList.LoadXlsx();

                // ウォッチ銘柄毎に処理
                foreach (var watchStock in watchList)
                {
                    // 削除日が入っていたらスキップ
                    if (!string.IsNullOrEmpty(watchStock.DeleteDate)) continue;

                    // インスタンスの初期化
                    var stockInfo = AssetInfo.GetInstance(watchStock);

                    try
                    {
                        // 外部サイトの情報を反映する
                        await stockInfo.UpdateFromExternalSource();

                        // 約定履歴を設定
                        stockInfo.UpdateExecutions(executionList);

                        // マスタを設定
                        stockInfo.UpdateAveragePerPbr(masterList);

                        // 情報更新
                        stockInfo.Setup();

                        // 結果登録
                        results.Add(stockInfo);
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        CommonUtils.Instance.Logger.LogError($"Code:{stockInfo.Code}, Message:{ex.Message}, StackTrace:{ex.StackTrace}", ex);
                    }
                }

                // ファイル保存
                Alert.SaveFile(results, policyList);

                // メール送信
                if (CommonUtils.Instance.ShouldSendMail) Alert.SendGmailViaSmtp();

                // 終了
                CommonUtils.Instance.Logger.LogInformation(CommonUtils.Instance.MessageAtApplicationEnd);
            }
            catch (Exception ex)
            {
                // ログ出力
                CommonUtils.Instance.Logger.LogError($"Message:{ex.Message}, StackTrace:{ex.StackTrace}", ex);
            }
            finally
            {
                // DB接続を閉じる
                DbConnectionFactory.Dispose();

                // ロガーを閉じる
                if (CommonUtils.Instance.Logger is IDisposable disposableLogger)
                {
                    disposableLogger.Dispose();
                }
            }
        }
    }
}