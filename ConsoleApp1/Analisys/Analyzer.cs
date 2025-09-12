// See https://aka.ms/new-console-template for more information

using ConsoleApp1.Assets;
using ConsoleApp1.Database;
using System.Data.SQLite;

internal class Analyzer
{
    DateTime _currentDate = CommonUtils.Instance.ExecusionDate;
    string _connectionString = string.Empty;
    const int _volatilityTermMax = 12;

    public Analyzer()
    {
        this._currentDate = CommonUtils.Instance.ExecusionDate;
        this._connectionString = CommonUtils.Instance.ConnectionString;
    }

    /// <summary>
    /// カトラー方式RSIの取得
    /// </summary>
    /// <param name="v"></param>
    /// <param name="endDate"></param>
    /// <param name="stockInfo"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://kabu.com/investment/guide/technical/08.html
    /// https://ad-van.co.jp/technical/article/rsi-calculation/
    /// </remarks>
    public double GetCutlerRSI(int v, DateTime endDate, AssetInfo stockInfo)
    {
        var priceTuples = GetCutlerRsiPrices(v, endDate, stockInfo);
        var prices = priceTuples.Select(x => x.Item2).ToList();
        return CalcCutlerRSI(v, prices);
    }

    /// <summary>
    /// カトラー方式RSI計算ロジック（DB非依存テスト用）
    /// </summary>
    internal static double CalcCutlerRSI(int v, List<double> prices)
    {
        if (prices == null || prices.Count < 2)
            return 0;

        double plus = 0;
        double minus = 0;

        for (int i = 1; i < prices.Count; i++)
        {
            double diff = prices[i] - prices[i - 1];
            if (diff >= 0)
                plus += diff;
            else
                minus += -diff;
        }

        double A = plus / v;
        double B = minus / v;

        if (A + B == 0) return 0;

        return (A / (A + B)) * 100;
    }

    /// <summary>
    /// RSI計算用の価格リストをDBから取得（テスト容易性のため分離）
    /// </summary>
    internal virtual List<(DateTime, double)> GetCutlerRsiPrices(int v, DateTime endDate, AssetInfo stockInfo)
    {
        var prices = new List<(DateTime, double)>();

        // コネクション取得
        SQLiteConnection connection = DbConnectionFactory.GetConnection();
        // GenericRepositoryのインスタンス生成
        var repo = new GenericRepository(connection);

        // パラメータ設定
        var parameters = new Dictionary<string, object>
        {
            { "@date", endDate.ToString("yyyy-MM-dd 23:59:59") },
            { "@code", stockInfo.Code }
        };

        // historyテーブルから該当データを取得（ORDER BY DESC, LIMIT, その後昇順ソート）
        var rows = repo.GetRows(
            table: "history",
            whereClause: "date <= @date and code = @code",
            parameters: parameters,
            orderBy: "date DESC",
            limit: v + 1
        );

        // endDateが直近営業日(stockInfo.LatestScrapedPrice.Date)、かつ、rowsに直近営業日の日付のデータが存在しなかった場合、rowsにスクレイピングした最新の株価データを追加する
        if (endDate.Date >= CommonUtils.Instance.LastTradingDate)
        {
            // 直近営業日のデータが含まれていない場合は追加
            if (rows.Count > 0 && ((DateTime)rows.Last()["date"]).Date < CommonUtils.Instance.LastTradingDate)
            {
                var latestPrice = stockInfo.LatestScrapedPrice;
                if (latestPrice != null && latestPrice.Date.Date <= CommonUtils.Instance.LastTradingDate)
                {
                    if (rows.Count == 0 || ((DateTime)rows.Last()["date"]).Date != latestPrice.Date.Date)
                    {
                        rows.Add(new Dictionary<string, object>
                        {
                            { "date", latestPrice.Date.Date },
                            { "close", latestPrice.Close }
                        });
                        // 追加後、件数がv+1を超える場合は最古のデータを削除
                        while (rows.Count > v + 1)
                        {
                            rows.RemoveAt(0);
                        }
                    }
                }
            }
        }

        // 昇順に並び替え
        rows.Sort((a, b) => ((DateTime)a["date"]).CompareTo((DateTime)b["date"]));

        foreach (var row in rows)
        {
            var date = (DateTime)row["date"];
            var close = Convert.ToDouble(row["close"]);
            prices.Add((date, close));
        }

        return prices;
    }

    internal class AnalysisResult
    {
        public AnalysisResult(AssetInfo stockInfo)
        {
            this.StockInfo = stockInfo;
            this.PriceVolatilities = new List<PriceVolatility>();
        }
        /// <summary>
        /// 銘柄
        /// </summary>
        public AssetInfo StockInfo { get; set; }
        /// <summary>
        /// 値幅履歴
        /// </summary>
        public List<PriceVolatility> PriceVolatilities { get; set; }

        /// <summary>
        /// 通知すべきか？
        /// </summary>
        internal bool ShouldAlert()
        {
            // 初期値は通知
            bool result = true;

            // 注目 or 所有している or 売却直後の場合
            if (this.StockInfo.IsFavorite || this.StockInfo.IsOwnedNow() || this.StockInfo.IsJustSold())
            {
                // 強制通知
            }
            //// ゴールデンクロス発生可能性がある場合
            //else if (this.StockInfo.IsGoldenCrossPossible())
            //{
            //    // 利回りが低い場合
            //    if (!this.StockInfo.IsHighYield()) result = false;

            //    // 時価総額が低い場合
            //    if (!this.StockInfo.IsHighMarketCap()) result = false;

            //    // 進捗が良くない場合
            //    if (!this.StockInfo.IsAnnualProgressOnTrack()) result = false;
            //}
            // 権利確定月前後の場合
            else if (this.StockInfo.IsCloseToRecordDate() || this.StockInfo.IsRecordDate() || this.StockInfo.IsAfterRecordDate())
            {
                // 利回りが低い場合
                if (!this.StockInfo.IsHighYield()) result = false;

                // 直近で暴落していない場合
                if (!this.StockInfo.LatestPrice.OversoldIndicator()) result = false;

                // 時価総額が低い場合
                if (!this.StockInfo.IsHighMarketCap()) result = false;

                // 進捗が良くない場合
                if (!this.StockInfo.IsAnnualProgressOnTrack()) result = false;
            }
            // 四半期決算前後の場合
            else if (this.StockInfo.IsCloseToQuarterEnd()|| this.StockInfo.IsQuarterEnd() || this.StockInfo.IsAfterQuarterEnd())
            {
                // 利回りが低い場合
                if (!this.StockInfo.IsHighYield()) result = false;

                // 直近で暴落していない場合
                if (!this.StockInfo.LatestPrice.OversoldIndicator()) result = false;

                // 時価総額が低い場合
                if (!this.StockInfo.IsHighMarketCap()) result = false;

                // 進捗が良くない場合
                if (!this.StockInfo.IsAnnualProgressOnTrack()) result = false;
            }
            // それ以外
            else
            {
                // 利回りが低い場合
                if (!this.StockInfo.IsHighYield()) result = false;

                // 直近で暴落していない場合
                if (!this.StockInfo.LatestPrice.OversoldIndicator()) result = false;

                // 時価総額が低い場合
                if (!this.StockInfo.IsHighMarketCap()) result = false;

                // 進捗が良くない場合
                if (!this.StockInfo.IsAnnualProgressOnTrack()) result = false;

                // PERが割高の場合
                if (!this.StockInfo.IsPERUndervalued(true)) result = false;

                // PBRが割高の場合
                if (!this.StockInfo.IsPBRUndervalued(true)) result = false;
            }

            return result;
        }

        /// <summary>
        /// 価格変動
        /// </summary>
        public class PriceVolatility
        {
            public PriceVolatility()
            {
                this.DateString = string.Empty;
            }
            /// <summary>
            /// 分析日文字列
            /// </summary>
            public string DateString { get; internal set; }
            /// <summary>
            /// 分析日
            /// </summary>
            public DateTime Date { get; internal set; }
            /// <summary>
            /// 変動率
            /// </summary>
            public double VolatilityRate { get; internal set; }
            /// <summary>
            /// 変動指数1
            /// </summary>
            public double VolatilityRateIndex1 { get; internal set; }
            /// <summary>
            /// 変動指数1の記録日
            /// </summary>
            public DateTime VolatilityRateIndex1Date { get; internal set; }
            /// <summary>
            /// 変動指数2
            /// </summary>
            public double VolatilityRateIndex2 { get; internal set; }
            /// <summary>
            /// 変動指数2の記録日
            /// </summary>
            public DateTime VolatilityRateIndex2Date { get; internal set; }
            /// <summary>
            /// 変動期間（週数）
            /// </summary>
            public int VolatilityTerm { get; internal set; }
            /// <summary>
            /// 通知すべきか
            /// </summary>
            public bool ShouldAlert { get; internal set; }
            /// <summary>
            /// 変動指数1のRSI（14日）
            /// </summary>
            public double VolatilityRateIndex1RSI14 { get; internal set; }
            /// <summary>
            /// 変動指数1のRSI（5日）
            /// </summary>
            public double VolatilityRateIndex1RSI5 { get; internal set; }
        }
    }

    /// <summary>
    /// 直近の金曜日を取得
    /// </summary>
    /// <param name="currentDate"></param>
    /// <returns></returns>
    internal DateTime GetLastFriday(DateTime currentDate)
    {
        DateTime baseDay = currentDate;

        // 現在の日付の曜日を取得
        DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;

        // 金曜日までの日数を計算
        int daysToLastFriday = ((int)currentDayOfWeek + 1) % 7 + 1;

        // 前の金曜日の日付を計算
        DateTime lastFriday = baseDay.AddDays(-daysToLastFriday);

        return lastFriday;
    }

    /// <summary>
    /// MACDの取得
    /// </summary>
    /// <param name="date"></param>
    /// <param="code"></param>
    /// <returns></returns>
    internal static double GetMACD(DateTime date, string code)
    {
        double result = 0;

        double smaS = GetSMA(25, date, code);
        double smaL = GetSMA(75, date, code);

        result = smaS - smaL;

        return result;
    }

    /// <summary>
    /// 移動平均値の取得
    /// </summary>
    /// <param name="v"></param>
    /// <param="date"></param>
    /// <param="code"></param>
    /// <returns></returns>
    internal static double GetSMA(int v, DateTime date, string code)
    {
        double result = 0;

        SQLiteConnection connection = DbConnectionFactory.GetConnection();

        // プライマリーキーに条件を設定したクエリ
        string query =
            "SELECT date, close FROM (" +
            " SELECT date, close" +
            " FROM history" +
            " WHERE date <= @date and code = @code" +
            " ORDER BY date DESC" +
            " LIMIT @limit)" +
            " ORDER BY date ASC;";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            // パラメータを設定
            command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd 23:59:59"));
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@limit", v);

            double sum = 0;

            // データリーダーを使用して結果を取得
            using (SQLiteDataReader reader = command.ExecuteReader())
            {

                // 結果を読み取り
                while (reader.Read())
                {
                    sum += reader.GetDouble(reader.GetOrdinal("close"));
                }
            }

            result = sum / v;
        }

        return result;
    }
}