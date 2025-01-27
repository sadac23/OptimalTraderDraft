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
 */

/* TODO
 * ・投信の処理追加
 * ・アラートのメール通知
 * ・DBはキャッシュ利用とし、なければ作成する処理を入れる
 * ・前年マイナスでプラ転した場合の通期業績率がうまく算出できていない。（5214など）
 * ・PER/PBRの閾値に10%の幅を持たせる
 * ・実行ログの追加
 * ・ユニットテスト実装
 * ・前年よりも極端に利益減予想の場合はマーク
 * ・DOE追加
 * ・最終購入より下げてた場合はマーク
 * ・買残が出来高の何倍残っているか？
 * ・毎日実行して5日分ローテ
 * ・ラインに通知する
 * ・株価履歴のスクレイピング基準開始日は、3か月前でそろえる
 * ・4Q発表のタイミングで株探の通期予想が実績に置き換えられてしまうため、4Qの予実が算出できない。（常に100%になる。）
 * 　修正履歴も前期分は見れないため、取得不可。
 * 　4Q発表前に通期予想をキャッシュしておいて、それと比較する仕組みが必要。
 * 　若しくは、株探以外のサイトから取得する。
 * ・処理前にOneDriveをリフレッシュする。
 */

// 分析結果
var results = new List<Analyzer.AnalysisResult>();

// インスタンス生成
var analyzer = new Analyzer();
var yahooScraper = new YahooScraper();
var kabutanScraper = new KabutanScraper();
var minkabuScraper = new MinkabuScraper();

Console.WriteLine(CommonUtils.Instance.MessageAtApplicationStartup);

// OneDriveリフレッシュ
OneDriveRefresh();

// 約定履歴取得
var executionList = ExecutionList.GetXlsxExecutionStockList();

// ウォッチリスト取得
var watchList = WatchList.GetXlsxWatchStockList();

// マスタ取得
var masterList = MasterList.GetXlsxAveragePerPbrList();

// 直近の営業日を取得
var lastTradingDay = GetLastTradingDay();

// 過去の株価履歴キャッシュを削除
DeleteHistoryCache();

// ウォッチ銘柄を処理
foreach (var watchStock in watchList)
{
    // 削除日が入っていたらスキップ
    if (!string.IsNullOrEmpty(watchStock.DeleteDate)) continue;

    var stockInfo = new StockInfo(watchStock);

    // 株価更新開始日を取得（なければ基準開始日を取得）
    var startDate = GetStartDate(watchStock.Code);

    // 外部サイトの情報取得
    await yahooScraper.ScrapeTop(stockInfo);
    await yahooScraper.ScrapeProfile(stockInfo);
    if (lastTradingDay > startDate)
        await yahooScraper.ScrapeHistory(stockInfo, startDate, CommonUtils.Instance.ExecusionDate);
    await kabutanScraper.ScrapeFinance(stockInfo);
    await minkabuScraper.ScrapeDividend(stockInfo);
    await minkabuScraper.ScrapeYutai(stockInfo);

    // 約定履歴を設定
    stockInfo.SetExecutions(executionList);

    // マスタを設定
    stockInfo.SetAveragePerPbr(masterList);

    // キャッシュ更新
    UpdateMaster(stockInfo);

    // チャート価格を更新
    stockInfo.UpdateChartPrices();

    // 分析
    var result = analyzer.Analize(stockInfo);

    // 結果登録
    results.Add(result);
}

// アラート通知
Alert.SaveFile(results);

Console.WriteLine(CommonUtils.Instance.MessageAtApplicationEnd);

void OneDriveRefresh()
{
    //TODO：ダミーファイルを作成して削除する。

    //// 判定したいファイルのパスを指定
    //string filePath = @"C:\Program Files\Microsoft OneDrive\OneDrive.exe";

    //// OneDriveの同期をトリガーするコマンド
    //string command = "cmd.exe";
    //string arguments = $"\"{filePath}\" /background";

    //try
    //{
    //    // ファイルの存在を確認
    //    if (!File.Exists(filePath)) return;
        
    //    // プロセスを開始
    //    ProcessStartInfo processStartInfo = new ProcessStartInfo(command, arguments)
    //    {
    //        RedirectStandardOutput = true,
    //        UseShellExecute = false,
    //        CreateNoWindow = true
    //    };

    //    using (Process process = Process.Start(processStartInfo))
    //    {
    //        // 出力を読み取る
    //        string output = process.StandardOutput.ReadToEnd();
    //        process.WaitForExit();
    //        Console.WriteLine(output);
    //    }
    //}
    //catch (Exception ex)
    //{
    //    Console.WriteLine($"Error: {ex.Message}");
    //}
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
            Console.WriteLine("History Rows deleted: " + rowsAffected);
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

string ReplacePlaceholder(string? input, string placeholder, string newValue)
{
    if (string.IsNullOrEmpty(input))
    {
        throw new ArgumentException("Input cannot be null or empty.", nameof(input));
    }
    return input.Replace(placeholder, newValue);
}

void ResisterResult(Analyzer.AnalysisResult result)
{
    using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
    {
        connection.Open();

        foreach (var r in result.PriceVolatilities)
        {
            // 削除クエリ
            string delQuery = "DELETE FROM analysis_result WHERE code = @code AND date_string = @date_string AND volatility_term = @volatility_term";

            using (SQLiteCommand command = new SQLiteCommand(delQuery, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", result.StockInfo.Code);
                command.Parameters.AddWithValue("@date_string", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@volatility_term", r.VolatilityTerm);

                // クエリを実行
                int rowsAffected = command.ExecuteNonQuery();

                // 結果を表示
                //                Console.WriteLine("Rows deleted: " + rowsAffected);
            }

            // 挿入クエリ
            string insQuery = "INSERT INTO analysis_result (" +
                "code" +
                ", date_string" +
                ", date" +
                ", name" +
                ", volatility_rate" +
                ", volatility_rate_index1" +
                ", volatility_rate_index1_date" +
                ", volatility_rate_index2" +
                ", volatility_rate_index2_date" +
                ", volatility_term" +
                ", leverage_ratio" +
                ", market_cap" +
                ", roe" +
                ", equity_ratio" +
                ", revenue_profit_dividend" +
                ", minkabu_analysis" +
                ", should_alert" +
                ", per" +
                ", pbr" +
                ", dividend_yield" +
                ", margin_balance_ratio" +
                ", fullyear_performance_forcast_summary" +
                ") VALUES (" +
                "@code" +
                ", @date_string" +
                ", @date" +
                ", @name" +
                ", @volatility_rate" +
                ", @volatility_rate_index1" +
                ", @volatility_rate_index1_date" +
                ", @volatility_rate_index2" +
                ", @volatility_rate_index2_date" +
                ", @volatility_term" +
                ", @leverage_ratio" +
                ", @market_cap" +
                ", @roe" +
                ", @equity_ratio" +
                ", @revenue_profit_dividend" +
                ", @minkabu_analysis" +
                ", @should_alert" +
                ", @per" +
                ", @pbr" +
                ", @dividend_yield" +
                ", @margin_balance_ratio" +
                ", @fullyear_performance_forcast_summary" +
                ")";

            using (SQLiteCommand command = new SQLiteCommand(insQuery, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", result.StockInfo.Code);
                command.Parameters.AddWithValue("@date_string", CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@date",CommonUtils.Instance.ExecusionDate);
                command.Parameters.AddWithValue("@name", result.StockInfo.Name);
                command.Parameters.AddWithValue("@volatility_rate", r.VolatilityRate);
                command.Parameters.AddWithValue("@volatility_rate_index1", r.VolatilityRateIndex1);
                command.Parameters.AddWithValue("@volatility_rate_index1_date", r.VolatilityRateIndex1Date);
                command.Parameters.AddWithValue("@volatility_rate_index2", r.VolatilityRateIndex2);
                command.Parameters.AddWithValue("@volatility_rate_index2_date", r.VolatilityRateIndex2Date);
                command.Parameters.AddWithValue("@volatility_term", r.VolatilityTerm);
                command.Parameters.AddWithValue("@leverage_ratio", 0);
                command.Parameters.AddWithValue("@market_cap", result.StockInfo.MarketCap);
                command.Parameters.AddWithValue("@roe", result.StockInfo.Roe);
                command.Parameters.AddWithValue("@equity_ratio", 0);
                command.Parameters.AddWithValue("@revenue_profit_dividend", 0);
                command.Parameters.AddWithValue("@minkabu_analysis", "");
                command.Parameters.AddWithValue("@should_alert", r.ShouldAlert);
                command.Parameters.AddWithValue("@per", result.StockInfo.Per);
                command.Parameters.AddWithValue("@pbr", result.StockInfo.Pbr);
                command.Parameters.AddWithValue("@dividend_yield", result.StockInfo.DividendYield);
                command.Parameters.AddWithValue("@margin_balance_ratio", result.StockInfo.MarginBalanceRatio);
                command.Parameters.AddWithValue("@fullyear_performance_forcast_summary", result.StockInfo.FullYearPerformanceForcastSummary);

                // クエリを実行
                int rowsAffected = command.ExecuteNonQuery();

                // 結果を表示
//                Console.WriteLine("Rows inserted: " + rowsAffected);
            }
        }
    }
}

DateTime GetStartDate(string code)
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
            command.Parameters.AddWithValue("@code", code);
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

void UpdateMaster(StockInfo stockInfo)
{
    foreach (var p in stockInfo.Prices)
    {
        if (!IsExist(stockInfo.Code, p)) {
            InsertMaster(stockInfo.Code, p);
            Console.WriteLine($"Date: {p.Date}, DateYYYYMMDD: {p.DateYYYYMMDD}, Open: {p.Open}, High: {p.High}, Low: {p.Low}, Close: {p.Close}, Volume: {p.Volume}");
        }
    }
}

void InsertMaster(string code, StockInfo.Price p)
{
    using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
    {
        connection.Open();

        // 挿入クエリ
        string query = "INSERT INTO history (" +
            "code" +
            ", date_string" +
            ", date" +
            ", open" +
            ", high" +
            ", low" +
            ", close" +
            ", volume" +
            ") VALUES (" +
            "@code" +
            ", @date_string" +
            ", @date" +
            ", @open" +
            ", @high" +
            ", @low" +
            ", @close" +
            ", @volume" +
            ")";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            // パラメータを設定
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@date_string", p.DateYYYYMMDD);
            command.Parameters.AddWithValue("@date", p.Date);
            command.Parameters.AddWithValue("@open", p.Open);
            command.Parameters.AddWithValue("@high", p.High);
            command.Parameters.AddWithValue("@low", p.Low);
            command.Parameters.AddWithValue("@close", p.Close);
            command.Parameters.AddWithValue("@volume", p.Volume);

            // クエリを実行
            int rowsAffected = command.ExecuteNonQuery();

            // 結果を表示
            Console.WriteLine("Rows inserted: " + rowsAffected);
        }
    }
}

bool IsExist(string code, StockInfo.Price p)
{
    using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
    {
        connection.Open();

        // プライマリーキーに条件を設定したクエリ
        string query = "SELECT count(code) as count FROM history WHERE code = @code and date_string = @date_string";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            // パラメータを設定
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@date_string", p.DateYYYYMMDD);

            // COUNTの結果を取得
            object result = command.ExecuteScalar();
            int count = Convert.ToInt32(result);

            return count > 0;
        }
    }
}

const string _mailAddress = "sadac23@gmail.com";
const string _password = "1qaz2WSX3edc";
string _refreshtoken = string.Empty;

//string stockCode = "9432";
//string url = $"https://finance.yahoo.co.jp/quote/{stockCode}.T/history";
//var httpClient = new HttpClient();
//var html = await httpClient.GetStringAsync(url);

//var htmlDocument = new HtmlDocument();
//htmlDocument.LoadHtml(html);

//var title = htmlDocument.DocumentNode.SelectNodes("//title");

//var rows = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'StocksEtfReitPriceHistory')]/tbody/tr");

//var stockData = new List<StockPrice>();

//foreach (var row in rows)
//{
//    var columns = row.SelectNodes("td|th");

//    if (columns != null && columns.Count > 6)
//    {
//        var date = columns[0].InnerText.Trim();
//        var open = columns[1].InnerText.Trim();
//        var high = columns[2].InnerText.Trim();
//        var low = columns[3].InnerText.Trim();
//        var close = columns[4].InnerText.Trim();
//        var volume = columns[5].InnerText.Trim();

//        stockData.Add(new StockPrice
//        {
//            Date = date,
//            Open = open,
//            High = high,
//            Low = low,
//            Close = close,
//            Volume = volume
//        });
//    }
//}

//string[] parts = title[0].InnerText.Trim().Split('：');
//if (parts.Length > 0)
//{
//    Console.WriteLine($"Title: {parts[0]}");
//}

//foreach (var stock in stockData)
//{
//    Console.WriteLine($"Date: {stock.Date}, Open: {stock.Open}, High: {stock.High}, Low: {stock.Low}, Close: {stock.Close}, Volume: {stock.Volume}");
//}

//string url = "https://finance.yahoo.co.jp/quote/7203.T"; // トヨタ自動車の株価ページ

//using (HttpClient client = new HttpClient())
//{
//    HttpResponseMessage response = await client.GetAsync(url);
//    if (response.IsSuccessStatusCode)
//    {
//        string html = await response.Content.ReadAsStringAsync();
//        HtmlDocument doc = new HtmlDocument();
//        doc.LoadHtml(html);

//        // 株価情報を抽出するXPathを指定
//        var priceNode = doc.DocumentNode.SelectSingleNode("//span[@class='stoksPrice']");
//        if (priceNode != null)
//        {
//            string price = priceNode.InnerText;
//            Console.WriteLine($"株価: {price}");
//        }
//        else
//        {
//            Console.WriteLine("株価情報の取得に失敗しました。");
//        }
//    }
//    else
//    {
//        Console.WriteLine("データの取得に失敗しました。");
//    }
//}


//string symbol = "7203.T"; // トヨタ自動車のティッカーシンボル
//string url = $"https://finance.yahoo.com/quote/{symbol}/history/?p={symbol}";

//using (HttpClient client = new HttpClient())
//{
//    HttpResponseMessage response = await client.GetAsync(url);
//    if (response.IsSuccessStatusCode)
//    {
//        string pageContent = await response.Content.ReadAsStringAsync();
//        HtmlDocument document = new HtmlDocument();
//        document.LoadHtml(pageContent);

//        var rows = document.DocumentNode.SelectNodes("//table[contains(@class, 'W(100%) M(0)')]/tbody/tr");

//        if (rows != null)
//        {
//            foreach (var row in rows)
//            {
//                var cells = row.SelectNodes("td");
//                if (cells != null && cells.Count >= 7)
//                {
//                    string date = cells[0].InnerText.Trim();
//                    string open = cells[1].InnerText.Trim();
//                    string high = cells[2].InnerText.Trim();
//                    string low = cells[3].InnerText.Trim();
//                    string close = cells[4].InnerText.Trim();
//                    string adjClose = cells[5].InnerText.Trim();
//                    string volume = cells[6].InnerText.Trim();

//                    Console.WriteLine($"{date}, {open}, {high}, {low}, {close}, {adjClose}, {volume}");
//                }
//            }
//        }
//        else
//        {
//            Console.WriteLine("データの取得に失敗しました。");
//        }
//    }
//    else
//    {
//        Console.WriteLine("ページの取得に失敗しました。");
//    }
//}


//string apiKey = "HU03ZTWTEKGY3YRQ"; // Alpha VantageのAPIキー
////string symbol = "7203.T"; // トヨタ自動車のティッカーシンボル
//string symbol = "7203"; // トヨタ自動車のティッカーシンボル
////string symbol = "IBM"; // トヨタ自動車のティッカーシンボル
//string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval=1min&apikey={apiKey}";

//using (HttpClient client = new HttpClient())
//{
//    HttpResponseMessage response = await client.GetAsync(url);
//    if (response.IsSuccessStatusCode)
//    {
//        string jsonResponse = await response.Content.ReadAsStringAsync();
//        Console.WriteLine(jsonResponse);
//    }
//    else
//    {
//        Console.WriteLine("データの取得に失敗しました。");
//    }
//}

//string symbol = "7203.T"; // トヨタ自動車のティッカーシンボル
//string url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={symbol}";

//using (HttpClient client = new HttpClient())
//{
//    HttpResponseMessage response = await client.GetAsync(url);
//    if (response.IsSuccessStatusCode)
//    {
//        string jsonResponse = await response.Content.ReadAsStringAsync();
//        Console.WriteLine(jsonResponse);
//    }
//    else
//    {
//        Console.WriteLine("データの取得に失敗しました。");
//    }
//}

// ** J-QuantsAPIにはがっかりだ。
//// ウォッチリスト取得
//var watchList = GetWatchList();

//foreach (var list in watchList)
//{
//    Console.WriteLine($"Code:{list.Code}、Name：{list.Name}");

//    // 銘柄情報取得
//    var listedInfoResponse = GetListedInfoResponse(list.Code).Result;

//    // 株価情報取得
//    var pricesDailyQuotesResponse = GetPricesDailyQuotesResponse(list.Code, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1)).Result;

//    // マスタ更新
//}

async Task<PricesDailyQuotesResponse> GetPricesDailyQuotesResponse(string code, DateTime from, DateTime to)
{
    HttpResponseMessage response = null;
    string responseBody = string.Empty;

    using (HttpClient client = new HttpClient())
    {
        if (string.IsNullOrEmpty(_refreshtoken))
        {
            // コンテンツを作成
            var content = new StringContent(JsonConvert.SerializeObject(new { mailaddress = _mailAddress, password = _password }), Encoding.UTF8, "application/json");
            response = await client.PostAsync($"https://api.jquants.com/v1/token/auth_user", content);
            responseBody = await response.Content.ReadAsStringAsync();
            var r = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
            _refreshtoken = r.refreshtoken;
        }
        Console.WriteLine($"refreshtoken：{_refreshtoken}");

        response = await client.PostAsync($"https://api.jquants.com/v1/token/auth_refresh?refreshtoken={_refreshtoken}", null);
        responseBody = await response.Content.ReadAsStringAsync();
        var r1 = JsonConvert.DeserializeObject<AuthRefreshResponse>(responseBody);
        Console.WriteLine($"idToken：{r1.idToken}");

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Custom-Header", "HeaderValue");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {r1.idToken}");
        response = await client.GetAsync($"https://api.jquants.com/v1/prices/daily_quotes?code={code}&from={from.ToString("yyyyMMdd")}&to={to.ToString("yyyyMMdd")}");
        responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"responseBody：{responseBody}");
        PricesDailyQuotesResponse apiResponse = JsonConvert.DeserializeObject<PricesDailyQuotesResponse>(responseBody);
        foreach (var i in apiResponse.daily_quotes)
        {
            Console.WriteLine($"Date:{i.Date}, Code:{i.Code}, Open:{i.Open}, High:{i.High}, Low:{i.Low}, Close:{i.Close}");
        }
        return apiResponse;
    }
}

async Task<ListedInfoResponse> GetListedInfoResponse(string code)
{
    HttpResponseMessage response = null;
    string responseBody = string.Empty;

    using (HttpClient client = new HttpClient())
    {
        if (string.IsNullOrEmpty(_refreshtoken))
        {
            // コンテンツを作成
            var content = new StringContent(JsonConvert.SerializeObject(new { mailaddress = _mailAddress, password = _password }), Encoding.UTF8, "application/json");
            response = await client.PostAsync($"https://api.jquants.com/v1/token/auth_user", content);
            responseBody = await response.Content.ReadAsStringAsync();
            var r = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
            _refreshtoken = r.refreshtoken;
        }
        //Console.WriteLine($"refreshtoken：{_refreshtoken}");

        response = await client.PostAsync($"https://api.jquants.com/v1/token/auth_refresh?refreshtoken={_refreshtoken}", null);
        responseBody = await response.Content.ReadAsStringAsync();
        var r1 = JsonConvert.DeserializeObject<AuthRefreshResponse>(responseBody);
        //Console.WriteLine($"idToken：{r1.idToken}");

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Custom-Header", "HeaderValue");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {r1.idToken}");
        response = await client.GetAsync($"https://api.jquants.com/v1/listed/info?code={code}");
        responseBody = await response.Content.ReadAsStringAsync();
        //Console.WriteLine($"responseBody：{responseBody}");
        ListedInfoResponse apiResponse = JsonConvert.DeserializeObject<ListedInfoResponse>(responseBody);
        foreach (var i in apiResponse.info)
        {
            Console.WriteLine($"Date: {i.Date}, Code: {i.Code}, CompanyName: {i.CompanyName}");
        }
        return apiResponse;
    }
}

internal class PricesDailyQuotesResponse
{
    public List<DailyQuotes> daily_quotes { get; set; }
    public string pagination_key { get; set; }

    public class DailyQuotes
    {
        public string? Date { get; set; }
        public string? Code { get; set; }
        public string? Open { get; set; }
        public string? High { get; set; }
        public string? Low { get; set; }
        public string? Close { get; set; }
        public string? UpperLimit { get; set; }
        public string? LowerLimit { get; set; }
        public string? Volume { get; set; }
        public string? TurnoverValue { get; set; }
        public string? AdjustmentFactor { get; set; }
        public string? AdjustmentOpen { get; set; }
        public string? AdjustmentHigh { get; set; }
        public string? AdjustmentLow { get; set; }
        public string? AdjustmentClose { get; set; }
        public string? AdjustmentVolume { get; set; }
        public string? MorningOpen { get; set; }
        public string? MorningHigh { get; set; }
        public string? MorningLow { get; set; }
        public string? MorningClose { get; set; }
        public string? MorningUpperLimit { get; set; }
        public string? MorningLowerLimit { get; set; }
        public string? MorningVolume { get; set; }
        public string? MorningTurnoverValue { get; set; }
        public string? MorningAdjustmentOpen { get; set; }
        public string? MorningAdjustmentHigh { get; set; }
        public string? MorningAdjustmentLow { get; set; }
        public string? MorningAdjustmentClose { get; set; }
        public string? MorningAdjustmentVolume { get; set; }
        public string? AfternoonOpen { get; set; }
        public string? AfternoonHigh { get; set; }
        public string? AfternoonLow { get; set; }
        public string? AfternoonClose { get; set; }
        public string? AfternoonUpperLimit { get; set; }
        public string? AfternoonLowerLimit { get; set; }
        public string? AfternoonVolume { get; set; }
        public string? AfternoonTurnoverValue { get; set; }
        public string? AfternoonAdjustmentOpen { get; set; }
        public string? AfternoonAdjustmentHigh { get; set; }
        public string? AfternoonAdjustmentLow { get; set; }
        public string? AfternoonAdjustmentClose { get; set; }
        public string? AfternoonAdjustmentVolume { get; set; }

    }
}

public class ListedInfoResponse
{
    public List<Info> info { get; set; }
}

public class Info
{
    public string? Date { get; set; }
    public string? Code { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyNameEnglish { get; set; }
    public string? Sector17Code { get; set; }
    public string? Sector17CodeName { get; set; }
    public string? Sector33Code { get; set; }
    public string? Sector33CodeName { get; set; }
    public string? ScaleCategory { get; set; }
    public string? MarketCode { get; set; }
    public string? MarketCodeName { get; set; }
}


public class AuthRefreshResponse
{
    public string? idToken { get; set; }
}
// レスポンスデータを受け取るためのクラス
public class ApiResponse
{
    public string? refreshtoken { get; set; }

    public static async Task<string> PostDataAsync(string url, object data)
    {
        // JSONデータをシリアライズ
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        using (HttpClient client = new HttpClient())
        {
            // POSTリクエストを送信
            var response = await client.PostAsync(url, content);
            // レスポンスの内容を取得
            var responseString = await response.Content.ReadAsStringAsync();


            return responseString;
        }
    }

    public static async Task<string> GetDataAsync(string url, Dictionary<string, string> headers)
    {
        using (HttpClient client = new HttpClient())
        {
            // ヘッダーを設定
            client.DefaultRequestHeaders.Clear();
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            // GETリクエストを送信
            var response = await client.GetAsync(url);

            // レスポンスの内容を取得
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
    }
    
    // 他のプロパティも必要に応じて追加
    public static async Task<T> GetResponse<T>(string url, string accessKey, string inputJson)
    {
        using (HttpClient client = new HttpClient())
        {
            // コンテンツを作成
            StringContent content = new StringContent(inputJson, Encoding.UTF8, "application/json");

            if (accessKey != string.Empty) {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessKey}");
            }

            // POSTリクエストを送信
            HttpResponseMessage response = await client.PostAsync(url, content);

            // レスポンスの内容を読み取る
            string responseBody = await response.Content.ReadAsStringAsync();
            //       Console.WriteLine("Response: " + responseBody);

            // JSONをオブジェクトにデシリアライズ
            T t = JsonConvert.DeserializeObject<T>(responseBody);
            return t;
        }
    }
}

class Sample
{
    public static async Task<string>  TestMethod()
    {
        using (HttpClient client = new HttpClient())
        {
            // コンテンツを作成
            StringContent content = new StringContent(JsonConvert.SerializeObject(new { mailaddress = "sadac23@gmail.com", password = "1qaz2WSX3edc" }), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync($"https://api.jquants.com/v1/token/auth_user", content);
            string responseBody = await response.Content.ReadAsStringAsync();
            var r = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
            //    Console.WriteLine($"refreshtoken：{r.refreshtoken}");

            response = await client.PostAsync($"https://api.jquants.com/v1/token/auth_refresh?refreshtoken={r.refreshtoken}", content);
            responseBody = await response.Content.ReadAsStringAsync();
            var r1 = JsonConvert.DeserializeObject<AuthRefreshResponse>(responseBody);
            //    Console.WriteLine($"idToken：{r1.idToken}");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Custom-Header", "HeaderValue");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {r1.idToken}");
            response = await client.GetAsync($"https://api.jquants.com/v1/listed/info?code=6503");
            responseBody = await response.Content.ReadAsStringAsync();
            //    Console.WriteLine($"responseBody：{responseBody}");
            ListedInfoResponse apiResponse = JsonConvert.DeserializeObject<ListedInfoResponse>(responseBody);
            foreach (var i in apiResponse.info)
            {
                Console.WriteLine($"Date: {i.Date}, Code: {i.Code}, CompanyName: {i.CompanyName}");
            }

            // データベースファイルのパス
            string dataSource = "Data Source=example.db";

            // SQLite接続を作成
            using (var connection = new SQLiteConnection(dataSource))
            {
                connection.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS `watch_list` (
                  `code` integer PRIMARY KEY,
                  `name` varchar(255),
                  `memo` text,
                  `del_flag` bool,
                  `add_timestamp` timestamp,
                  `del_timestamp` timestamp
                )";

                // コマンドを実行
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // 接続を閉じる
                connection.Close();
            }

        }
        return null;
    }
}

public class StockPrice
{
    public string Date { get; set; }
    public string Open { get; set; }
    public string High { get; set; }
    public string Low { get; set; }
    public string Close { get; set; }
    public string Volume { get; set; }
}

