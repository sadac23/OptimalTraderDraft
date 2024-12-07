// See https://aka.ms/new-console-template for more information

using DocumentFormat.OpenXml.Wordprocessing;
using System.Data.SQLite;
using System.Runtime.ConstrainedExecution;
using static Analyzer;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal class Analyzer
{
    string _connectionString = string.Empty;
    const int _volatilityTermMax = 12;

    public Analyzer(string connectionString)
    {
        this._connectionString = connectionString;
    }

    internal AnalysisResult Analize(StockInfo item)
    {
        var result = new AnalysisResult(item);

        // 週間変動値取得
        for (int i = 1; i <= _volatilityTermMax; i++)
        {
            result.PriceVolatilities.Add(Weeklyfluctuation(item, i));
        }

        return result;
    }

    AnalysisResult.PriceVolatility Weeklyfluctuation(StockInfo item, int term)
    {
        double startindex = 0;
        double endIndex = 0;
        DateTime startindexDate = DateTime.Now;
        DateTime endIndexDate = DateTime.Now;
        double lastFridayIndex = 0;
        DateTime lastFridayIndexDate = DateTime.Now;

        DateTime startDate = DateTime.Today;
        DateTime endDate = DateTime.Today;

        // 実行日が土日の場合は、終了日を金とする
        if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
        {
            endDate = GetLastFriday(DateTime.Today);
        }
        startDate = GetLastFriday(endDate).AddDays((term - 1) * -7);

        // 結果生成
        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();

            // プライマリーキーに条件を設定したクエリ
            string query = "SELECT * FROM history WHERE code = @code and date BETWEEN @date_start and @date_end ORDER BY date";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", item.Code);
                command.Parameters.AddWithValue("@date_start", startDate.ToString("yyyy-MM-dd 00:00:00"));    // 前週の金曜日
                command.Parameters.AddWithValue("@date_end", endDate.ToString("yyyy-MM-dd 23:59:59"));

                // データリーダーを使用して結果を取得
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    // 結果を読み取り
                    while (reader.Read())
                    {
                        // カラム名を指定してデータを取得
                        string code = reader.GetString(reader.GetOrdinal("code"));
                        DateTime date = reader.GetDateTime(reader.GetOrdinal("date"));
                        string dateString = date.ToString("yyyyMMdd");
                        double open = reader.GetDouble(reader.GetOrdinal("open"));
                        double high = reader.GetDouble(reader.GetOrdinal("high"));
                        double low = reader.GetDouble(reader.GetOrdinal("low"));
                        double close = reader.GetDouble(reader.GetOrdinal("close"));
                        double volume = reader.GetDouble(reader.GetOrdinal("volume"));

                        // 結果をコンソールに出力
                        //Console.WriteLine($"code: {code}, date: {date}, close: {close}");

                        if (startindex == 0)
                        {
                            startindex = close;
                            startindexDate = date;
                        }
                        endIndex = close;
                        endIndexDate = date;

                        // 実行日の前週金曜日の指数を取得しておく
                        if (dateString == GetLastFriday(DateTime.Today).ToString("yyyyMMdd"))
                        {
                            lastFridayIndex = close;
                            lastFridayIndexDate = date;
                        }
                    }
                }
            }
        }

        AnalysisResult.PriceVolatility result = new()
        {
            DateString = DateTime.Now.ToString("yyyyMMdd"),
            Date = DateTime.Now,
            VolatilityRate = (endIndex / startindex) - 1,
            VolatilityRateIndex1 = startindex,
            VolatilityRateIndex1Date = startindexDate,
            VolatilityRateIndex2 = endIndex,
            VolatilityRateIndex2Date = endIndexDate,
            VolatilityTerm = term,
            ShouldAlert = false,
        };

        // 個別
        if (item.Classification == "1")
        {
            // -10.0%以下（10week以内の下落幅）
            if (!result.ShouldAlert && result.VolatilityRate <= -0.100) { result.ShouldAlert = true; }

            // -9.9%～-9.0%（3week以内の下落幅）
            if (!result.ShouldAlert && (result.VolatilityRate <= -0.090 & result.VolatilityRate >= -0.099) & result.VolatilityTerm <= 3) { result.ShouldAlert = true; }

            // -8.9%～-8.0%（2week以内の下落幅）
            if (!result.ShouldAlert && (result.VolatilityRate <= -0.080 & result.VolatilityRate >= -0.089) & result.VolatilityTerm <= 2) { result.ShouldAlert = true; }

            // -7.9%～-7.0%（2week以内の下落幅）
            if (!result.ShouldAlert && (result.VolatilityRate <= -0.070 & result.VolatilityRate >= -0.079) & result.VolatilityTerm <= 2) { result.ShouldAlert = true; }

            // -50.0%以下は株式分割の異常値である可能性が高いためアラートしない
            if (result.VolatilityRate <= -0.500) { result.ShouldAlert = false; }

            // 上昇は一旦、除外。
            // 10.0%以上（10week以内の上昇幅）
            //if (!result.ShouldAlert && result.VolatilityRate >= 0.100) { result.ShouldAlert = true; }
        }
        // ETF
        if (item.Classification == "2")
        {
            // -5.0%以下（10week以内の下落幅）
            if (!result.ShouldAlert && result.VolatilityRate <= -0.050) { result.ShouldAlert = true; }
        }

        return result;
    }

    private DateTime AdjustToFriday(DateTime date)
    {
        // 曜日を取得
        DayOfWeek dayOfWeek = date.DayOfWeek;

        // 金曜日、土曜日、日曜日の場合の処理
        if (dayOfWeek == DayOfWeek.Friday)
        {
            return date; // すでに金曜日
        }
        else if (dayOfWeek == DayOfWeek.Saturday)
        {
            return date.AddDays(-1); // 土曜日の場合、前日の金曜日に補正
        }
        else if (dayOfWeek == DayOfWeek.Sunday)
        {
            return date.AddDays(-2); // 日曜日の場合、2日前の金曜日に補正
        }
        else
        {
            return date; // 月～木曜日の場合、そのままの入力日を返す
        }
    }

    internal class AnalysisResult
    {
        public AnalysisResult(StockInfo stockInfo)
        {
            this.StockInfo = stockInfo;
            this.PriceVolatilities = new List<PriceVolatility>();
        }
        /// <summary>
        /// 銘柄
        /// </summary>
        public StockInfo StockInfo { get; set; }
        /// <summary>
        /// 値幅履歴
        /// </summary>
        public List<PriceVolatility> PriceVolatilities { get; set; }

        /// <summary>
        /// 通知すべきか？
        /// </summary>
        internal bool ShouldAlert()
        {
            bool result = false;

            foreach (Analyzer.AnalysisResult.PriceVolatility v in this.PriceVolatilities)
            {
                if (v.ShouldAlert) result = true;
            }

            // 直近の変動幅が0以上だった場合は通知しない
            if (this.PriceVolatilities[0].VolatilityRate >= 0) result = false;

            // 利回りが3.00%より低い場合はアラートしない
            if (this.StockInfo.DividendYield < 0.0300) result = false;

            // ROEが8.00%より低い場合はアラートしない
            if (this.StockInfo.Roe < 8.00) result = false;

            // 時価総額が1000億より低い場合はアラートしない
            if (this.StockInfo.MarketCap < 100000000) result = false;

            return result;
        }

        /// <summary>
        /// 価格変動
        /// </summary>
        public class PriceVolatility
        {
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

}