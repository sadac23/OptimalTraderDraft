// See https://aka.ms/new-console-template for more information
using System.Text;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

internal class IndexInfo : StockInfo
{
    public IndexInfo(
        IExternalSourceUpdatable updater,
        IOutputFormattable formatter)
        : base(updater, formatter)
    {
    }

    // インデックス種別用の外部情報取得処理
    public class IndexUpdater : IExternalSourceUpdatable
    {
        public async Task UpdateFromExternalSourceAsync(StockInfo stockInfo)
        {
            // 例: Webスクレイピングでインデックス情報を取得
            // 実際のURLやパース処理は要件に応じて実装
            using var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync("https://example.com/indexdata");
            // ここでhtmlをパースしてstockInfoのプロパティを更新
            // 例: stockInfo.Price = ParsePrice(html);
        }
    }

    // インデックス種別用の出力処理
    public class IndexFormatter : IOutputFormattable
    {
        public string ToOutputString(StockInfo stockInfo)
        {
            // 例: インデックス情報を特定フォーマットで文字列化
            // 例: return $"Index: {stockInfo.Name}, Price: {stockInfo.Price}";
            return $"Index情報: {stockInfo}";
        }
    }
}