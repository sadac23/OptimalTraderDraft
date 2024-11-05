// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

Console.WriteLine("Hello, World!");

string _connectionString = @"Data Source=..\..\..\..\example.db;Version=3;";
string _refreshtoken = string.Empty;

// ウォッチリスト取得
var watchList = GetWatchList();

foreach (var list in watchList)
{
    Console.WriteLine($"Code:{list.Code}、Name：{list.Name}");

    // 銘柄情報取得
    var info = GetListedInfo(list.Code);
    // 株価情報取得
    // マスタ更新
}

ListedInfo GetListedInfo(string code)
{
    return null;
}

List<WatchList> GetWatchList()
{
    var list = new List<WatchList>();

    using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
    {
        connection.Open();

        string query = "SELECT * FROM watch_list";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new WatchList {
                        Code = reader.GetString(reader.GetOrdinal("code"))
                        , Name = reader.GetString(reader.GetOrdinal("name")) 
                    });
                }
            }
        }
    }
    return list;

}
class WatchList
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}


public class ListedInfoResponse
{
    public List<ListedInfo> info { get; set; }
}

public class ListedInfo
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
