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
using NLog;

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
 * ・済：通期進捗おかしい（7314）
 * ・済：4Qは前期修正履歴を全件出力する。
 * ・済：決算予定日情報、取得できない場合は行詰める。
 * ・済：短期/長期の移動平均値の算出。
 * ・済：テクニカル情報行を別途追加。
 * ・済：市場名が取得できない銘柄がある。（141A）
 * ・済：チャート価格にもRSIバッジ表示
 * ・済：StockInfo.GetPreviousForcasts()のバグ対応。（GetDoubleで例外）
 * ・済：PER/PBRは平均値+10%の幅を持たせる。
 * ・済：株式分割が発生したら株価履歴をリフレッシュする。（終値と調整後終値の不一致で判断。）
 * ・済：性能改善。（主にスクレイピングが遅い。）
 * ・済：DOE追加
 * ・済：営業利益率の追加
 * ・済：4Q進捗率は修正前予想との比較で算出する。（修正予想との比較はKPIにならない。）
 * ・済：直近開示情報（disclosure）を追加してマークする。
 */

/* TODO
 * ・ユニットテスト実装
 * ・前年よりも極端に利益減予想の場合はマーク
 * ・買残が出来高の何倍残っているか？
 * ・毎日実行して5日分ローテ
 * ・バッジ種類でまとめて出力する。
 * ・約定リストの自動更新。
 * ・アナリスト予想の取得。（楽天証券から取得？）
 * ・GmailAPIをテストユーザから本番ユーザに切り替え。（アプリ審査に動画が必要）
 * ・エラー時にメールしたい
 * ・権前、権後バッジを追加
 * ・購入検討バッジを追加（☆を付けておく）
 * ・日経平均の情報追加。
 * ・米国個別株対応。
 * ・マスタファイルをリポジトリから入れ替える仕組み。
 */

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 分析結果
            var results = new List<StockInfo>();

            try
            {
                // 開始
                CommonUtils.Instance.Logger.LogInformation(CommonUtils.Instance.MessageAtApplicationStartup);

                // OneDriveリフレッシュ
                if (CommonUtils.Instance.ShouldRefreshOneDrive) CommonUtils.Instance.OneDriveRefresh();

                // 約定履歴リストを更新
                if (CommonUtils.Instance.ShouldUpdateExecutionList) ExecutionList.UpdateFromGmail();

                // 約定履歴取得
                var executionList = ExecutionList.LoadXlsx();

                // ウォッチリスト取得
                var watchList = WatchList.LoadXlsx();

                // マスタ取得
                var masterList = MasterList.LoadXlsx();

                // ウォッチ銘柄毎に処理
                foreach (var watchStock in watchList)
                {
                    // 削除日が入っていたらスキップ
                    if (!string.IsNullOrEmpty(watchStock.DeleteDate)) continue;

                    // インスタンスの初期化
                    var stockInfo = new StockInfo(watchStock);

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
                Alert.SaveFile(results);

                // メール送信
                if (CommonUtils.Instance.ShouldSendMail) Alert.SendMail();

                // 終了
                CommonUtils.Instance.Logger.LogInformation(CommonUtils.Instance.MessageAtApplicationEnd);
            }
            catch (Exception ex)
            {
                // ログ出力
                CommonUtils.Instance.Logger.LogError($"Message:{ex.Message}, StackTrace:{ex.StackTrace}", ex);
            }
        }
    }
}