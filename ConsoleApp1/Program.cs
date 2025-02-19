// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json.Nodes;
using System.Data.SQLite;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Data.Entity;
using System.Data;
using System.Transactions;
using static System.Data.Entity.Infrastructure.Design.Executor;
using System.Runtime.InteropServices;
using System.Configuration;
using static WatchList;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Drawing;
using System.Linq.Expressions;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

/* DONE
 * ・済：ETFの処理
 * ・済：上昇率の分析追加
 * ・済：名称をスクレイピング取得結果より反映
 * ・済：株式分割の異常値の判定（異常に大きい変動で判断）
 * ・済：ROEの取得
 * ・済：利回りの取得
 * ・済：PERの取得
 * ・済：PBRの取得
 * ・済：時価総額の取得
 * ・済：信用倍率の取得
 * ・済：レンジを12週（四半期）に拡大
 * ・済：通知フォーマットの検討
 * ・済：業績（増収増益増配）の取得
 * ・済：約定履歴を取得する
 * ・済：現在所有しているものは、分析する
 * ・済：yahooスクレイピング、1ページ目の件数を見て2ページ目が必要か判定する。
 * ・済：購入履歴を通知に出力する。
 * ・済：直近が上がっていたら、分析結果全体を通知しない
 * ・済：全銘柄登録する
 * ・済：ウォッチの削除フラグが見れてない
 * ・済：時価総額でのフィルター
 * ・済：利回りでのフィルター
 * ・済：時価総額の足きり
 * ・済：所有している場合は通知する
 * ・済：PBR2倍より低いものでフィルター
 * ・済：PER15倍より低いものでフィルター
 * ・済：時価総額の判定うまくできていない
 * ・済：直近株価をstockinfoに持つ
 * ・済：変動履歴の通知内容を簡素化
 * ・済：直近購入からどれだけ下げているか
 * ・済：お気に入りなら通知する
 * ・済：メモ情報の追加
 * ・済：所有しているものは、前回購入時より下がっていたら通知する
 * ・済：自己資本比率の取得
 * ・済：通期予想の変化率を表示
 * ・済：優待利回りを追加（みんかぶ）
 * ・済：決算時期情報の追加（みんかぶ）
 * ・済：配当性向を追加（みんかぶ）
 * ・済：配当利回りとの合計でフィルター
 * ・済：配当増額値の丸め処理を入れる
 * ・済：配当性向が前年実績値になっている、通期予想値に変更
 * ・済：スクレイピング処理のリファクタリング
 * ・済：市場、業種情報の取得
 * ・済：ROEの推移を表示
 * ・済：市場、業界毎のPER/PBRを表示する
 * ・済：土日はyahooのスクレイピング不要（カレントが休場日の場合、営業日まで遡って取得済であるかチェックする）
 * ・済：次回決算日を表示
 * ・済：直近株価をDBから取得する
 * ・済：直近のROE推移が向上しているものでフィルターする
 * ・済：お気に入りの場合はサインを表示する
 * ・済：下落幅、利回り、時価総額フィルター以外は解除する
 * ・済：5桁コードで強制終了する
 * ・済：メモが空で出る
 * ・済：変動履歴、直近週は必ず表示する
 * ・済：信用買い残と出来高を追加
 * ・済：基準値以下を●表示
 * ・済：配当月、優待月で強制通知
 * ・済：当月もしくは翌月までの権利日でフィルター
 * ・済：通知に連番振る
 * ・済：PER/PBRフィルターかける、0は出さない
 * ・済：価格をカンマ表記にする
 * ・済：実績を取得して通期目標に対しての進捗率を算出する
 * ・済：約定履歴を日付でソートする
 * ・済：PER/PBR、時価総額、進捗率でフィルタする
 * ・済：権利確定日を取得する
 * ・済：前期の進捗を追加する
 * ・済：4Qの判定ができていない（良品計画）
 * ・済：決算日の前後1か月はマーク
 * ・済：配当なし、優待のみのケースの権利日マーク（楽天）
 * ・済：期末決算日を追加
 * ・済：ナンピン基準内はマーク（最終購入日より5%以上下落している場合）
 * ・済：日本ETFの処理追加
 * ・済：変動履歴は常に表示して、閾値以上はマークする
 * ・済：ETFの株探取得がうまくできていない
 * ・済：RSI追加
 * ・済：RSI（14日）30以下で強制通知
 * ・済：実行後にシャットダウン
 * ・済：RegisterResult削除
 * ・済：決算情報表示を上に
 * ・済：翌月までの優待権利日は強制通知
 * ・済：4か月より前の株価履歴は削除
 * ・済：グロースの市場名称取得できていない　→　"135A"はyahooファイナンスのバグぽい
 * ・済：ビルドジョブ追加
 * ・済：通期予想修正履歴の追加
 * ・済：RSI短期値（9日）の追加
 * ・済：総件数を追加
 * ・済：総件数がバグっている
 * ・済：RSI上昇の閾値
 * ・済：下方修正はマーク
 * ・済：所有しているものは強制通知なのでRSIマーク判定でチェックする必要なし
 * ・済：当月権利日のみマーク
 * ・済：上方修正でマーク
 * ・済：通期予想配当に*がついてるケースへの対応（7740）
 * ・済：4Qは通期予想の1件前と比較必要
 * ・済：バッジ表示の追加
 * ・済：変動履歴の刷新。RSIの明細を出力する。
 * ・済：所有株の判定にバグがある。（7630など、分割されたもの）
 * ・済：進捗良好判定の基準値を厳しくする。
 * ・済：実行ログの実装
 * ・済：処理前にOneDriveをリフレッシュする。
 * ・済：所持している場合のみRSI上限バッジを表示する。
 * ・済：決算前バッジの追加。
 * ・済：処理の実行をフラグ制御する。
 * ・済：メール通知する
 * ・済：売った直後は情報見たい。売却バッジ追加。
 * ・済：RSIの上位閾値の見直し。
 * ・済：決算前後でバッジを分ける。
 * ・済：通知ファイルの日付をyy/MM/dd書式に変更。
 * ・済：決算前後の場合は通知する。
 * ・済：決算当日は決当バッジを表示する。
 * ・済：四半期決算実績の対前年同期の経常利益の上昇率を追加する。
 * ・済：直近が4Q発表の場合、前期の最終実績を修正履歴に追加する。
 * ・済：通期の前年同期比の値がおかしい。（6046）
 * ・済：購入履歴にナンピンサインを追加。（-3.00%）
 */

/* TODO
 * ・投信の処理追加
 * ・DBはキャッシュ利用とし、なければ作成する処理を入れる
 * ・前年マイナスでプラ転した場合の通期業績率がうまく算出できていない。（5214など）
 * ・PER/PBRの閾値に10%の幅を持たせる
 * ・ユニットテスト実装
 * ・前年よりも極端に利益減予想の場合はマーク
 * ・DOE追加
 * ・最終購入より下げてた場合はマーク
 * ・買残が出来高の何倍残っているか？
 * ・毎日実行して5日分ローテ
 * ・4Q発表のタイミングで株探の通期予想が実績に置き換えられてしまうため、4Qの予実が算出できない。（常に100%になる。）
 * 　修正履歴も前期分は見れないため、取得不可。
 * 　4Q発表前に通期予想をキャッシュしておいて、それと比較する仕組みが必要。
 * 　若しくは、株探以外のサイトから取得する。
 * ・バッジ種類でまとめて出力する。
 * ・約定リストの自動更新。
 * ・海外投信、ETFの対応。
 * ・株式分割が発生したら株価履歴をリフレッシュする。（直近50%以上の下落で判断。）
 * ・性能改善。（主にスクレイピングが遅い。）
 * ・アナリスト予想の取得。（楽天証券から取得？）
 * ・4Q進捗率は修正前予想との比較で算出する。（修正予想との比較はKPIにならない。）
 * ・GmailAPIをテストユーザから本番ユーザに切り替え。
 * ・分割時は株価履歴をリフレッシュする。
 * ・4Qは前期修正履歴を全件出力する。
 * ・エラー時にメールしたい
 * ・通期進捗おかしい（7314）
 */

var logger = CommonUtils.Instance.Logger;

try
{
    // 分析結果
    var results = new List<Analyzer.AnalysisResult>();

    // インスタンス生成
    var analyzer = new Analyzer();
    var yahooScraper = new YahooScraper();
    var kabutanScraper = new KabutanScraper();
    var minkabuScraper = new MinkabuScraper();

    logger.LogInformation(CommonUtils.Instance.MessageAtApplicationStartup);

    // OneDriveリフレッシュ
    if (CommonUtils.Instance.ShouldRefreshOneDrive) OneDriveRefresh();

    // 約定履歴リストを更新
    if (CommonUtils.Instance.ShouldUpdateExecutionList) UpdateXlsxExecutionStockList();

    // 約定履歴取得
    var executionList = ExecutionList.GetXlsxExecutionStockList();

    // ウォッチリスト取得
    var watchList = WatchList.GetXlsxWatchStockList();

    // マスタ取得
    var masterList = MasterList.GetXlsxAveragePerPbrList();

    // 直近の営業日を取得
    var lastTradingDay = GetLastTradingDay();

    // ウォッチ銘柄毎に処理
    foreach (var watchStock in watchList)
    {
        // 削除日が入っていたらスキップ
        if (!string.IsNullOrEmpty(watchStock.DeleteDate)) continue;

        // インスタンスの初期化
        var stockInfo = new StockInfo(watchStock);

        // 履歴更新の最終日を取得（なければ基準開始日を取得）
        var lastUpdateDay = GetLastHistoryUpdateDay(stockInfo);

        // 外部サイトの情報取得
        await kabutanScraper.ScrapeFinance(stockInfo);
        await minkabuScraper.ScrapeDividend(stockInfo);
        await minkabuScraper.ScrapeYutai(stockInfo);
        await yahooScraper.ScrapeTop(stockInfo);
        await yahooScraper.ScrapeProfile(stockInfo);
        // 最終更新後に直近営業日がある場合は取得
        if (lastTradingDay > lastUpdateDay) 
            await yahooScraper.ScrapeHistory(stockInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate);

        // 約定履歴を設定
        stockInfo.SetExecutions(executionList);

        // マスタを設定
        stockInfo.SetAveragePerPbr(masterList);

        // インスタンス更新
        stockInfo.Setup();

        // 分析
        var result = analyzer.Analize(stockInfo);

        // 結果登録
        results.Add(result);
    }

    // ファイル保存
    Alert.SaveFile(results);

    // 過去の株価履歴キャッシュを削除
    DeleteHistoryCache();

    // メール送信
    if (CommonUtils.Instance.ShouldSendMail) Alert.SendMail();

    logger.LogInformation(CommonUtils.Instance.MessageAtApplicationEnd);
}
catch(Exception ex)
{
    logger.LogError(ex.Message, ex);
}

void OneDriveRefresh()
{
    string oneDrivePath = @"C:\Program Files\Microsoft OneDrive\OneDrive.exe"; // OneDriveの実行ファイルのパス

    if (File.Exists(oneDrivePath))
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = oneDrivePath,
            Arguments = "/sync", // 同期をトリガーする引数
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            process.WaitForExit();
            logger.LogInformation("OneDrive sync triggered.");
        }
    }
    else
    {
        logger.LogInformation("OneDrive executable not found.");
    }
}

void DeleteHistoryCache()
{
    using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
    {
        connection.Open();

        // 挿入クエリ
        string query = "DELETE FROM history WHERE date <= @date";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            // パラメータを設定
            command.Parameters.AddWithValue("@date", CommonUtils.Instance.ExecusionDate.AddMonths(-1 * CommonUtils.Instance.StockPriceHistoryMonths));

            // クエリを実行
            int rowsAffected = command.ExecuteNonQuery();

            // 結果を表示
            logger.LogInformation($"History Rows deleted: {rowsAffected}");
        }
    }
}

DateTime GetLastTradingDay()
{
    DateTime date = CommonUtils.Instance.ExecusionDate.Date;

    // 土日または祝日の場合、前日を確認
    while (TSEHolidayChecker.IsTSEHoliday(date))
    {
        date = date.AddDays(-1);
    }

    return date;
}

DateTime GetLastHistoryUpdateDay(StockInfo stockInfo)
{
    DateTime result = CommonUtils.Instance.MasterStartDate;

    using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
    {
        connection.Open();

        // プライマリーキーに条件を設定したクエリ
        string query = $"SELECT IFNULL(MAX(date), @max_date) FROM history WHERE code = @code";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            // パラメータを設定
            command.Parameters.AddWithValue("@code", stockInfo.Code);
            command.Parameters.AddWithValue("@max_date", CommonUtils.Instance.MasterStartDate);

            // データリーダーを使用して結果を取得
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows && reader.Read())
                {
                    result = reader.GetDateTime(0);
                }
            }
        }
    }

    return result;
}

void UpdateXlsxExecutionStockList()
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

        logger.LogInformation("Credential file saved to: " + credPath);
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

    logger.LogInformation("Messages:");
    if (messages != null && messages.Count > 0)
    {
        foreach (var messageItem in messages)
        {
            var message = service.Users.Messages.Get("me", messageItem.Id).Execute();
            logger.LogInformation($"- {message.Snippet}");
        }
    }
    else
    {
        logger.LogInformation("No messages found.");
    }
}
