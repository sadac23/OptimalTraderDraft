using ConsoleApp1.Assets.Models;

namespace ConsoleApp1.Assets.Repositories
{
    /// <summary>
    /// 資産情報のDBアクセス用リポジトリインターフェース
    /// </summary>
    public interface IAssetRepository
    {
        Task<List<ScrapedPrice>> LoadHistoryAsync(string code);
        Task SaveHistoryAsync(string code, List<ScrapedPrice> prices);
        Task DeleteHistoryAsync(string code, DateTime targetDate);
        List<FullYearPerformanceForcast> GetPreviousForcasts(string code, string fiscalPeriod);

        // 追加: 履歴テーブルから最新日付を取得
        DateTime GetLastHistoryUpdateDay(string code);

        /// <summary>
        /// チャート用履歴データを取得
        /// </summary>
        /// <param name="code">銘柄コード</param>
        /// <param name="limit">取得件数</param>
        /// <returns>履歴データのリスト（各行はカラム名→値のDictionary）</returns>
        List<Dictionary<string, object>> GetChartPriceRows(string code, int limit);

        Task RegisterHistoryAsync(string code, List<ScrapedPrice> prices);
        Task DeleteOldHistoryAsync(string code, DateTime beforeDate);
        Task RegisterForcastHistoryAsync(string code, List<FullYearPerformanceForcast> forcasts);
    }
}