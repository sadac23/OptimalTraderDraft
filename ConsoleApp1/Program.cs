// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json.Nodes;

Console.WriteLine("Hello, World!");

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

