﻿// See https://aka.ms/new-console-template for more information

using System.Data.SQLite;

internal class Analyzer
{
    string _connectionString = string.Empty;
    const int _volatilityTermMax = 10;

    public Analyzer(string connectionString)
    {
        this._connectionString = connectionString;
    }

    internal List<AnalysisResult> Analize(WatchList.WatchStock item)
    {
        List<AnalysisResult> results = new();

        // 週間変動値取得
        for (int i = 1; i <= _volatilityTermMax; i++)
        {
            results.Add(Weeklyfluctuation(item, i));
        }

        //foreach (AnalysisResult result in results)
        //{
        //    Console.WriteLine($"Code: {result.Code}, Name: {result.Name}, Term: {result.VolatilityTerm}, Rate: {result.VolatilityRate}, RateIndex1: {result.VolatilityRateIndex1}, RateIndex2: {result.VolatilityRateIndex2}");
        //}

        return results;
    }

    AnalysisResult Weeklyfluctuation(WatchList.WatchStock item, int term)
    {
        double startindex = 0;
        double endIndex = 0;
        DateTime startindexDate = DateTime.Now;
        DateTime endIndexDate = DateTime.Now;
        double lastFridyIndex = 0;
        DateTime lastFridyIndexDate = DateTime.Now;

        // 基準日を調整
        var baseDate = DateTime.Today.AddDays((term - 1) * -7);

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
                command.Parameters.AddWithValue("@date_start", GetLastFriday(baseDate).ToString("yyyy-MM-dd 00:00:00"));    // 前週の金曜日
                command.Parameters.AddWithValue("@date_end", DateTime.Today.ToString("yyyy-MM-dd 23:59:59"));

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
                            lastFridyIndex = close;
                            lastFridyIndexDate = date;
                        }
                    }
                }
            }
        }

        AnalysisResult result = new()
        {
            Code = item.Code,
            DateString = DateTime.Now.ToString("yyyyMMdd"),
            Date = DateTime.Now,
            Name = item.Name,
            VolatilityRate = (endIndex / startindex) - 1,
            VolatilityRateIndex1 = startindex,
            VolatilityRateIndex1Date = startindexDate,
            VolatilityRateIndex2 = endIndex,
            VolatilityRateIndex2Date = endIndexDate,
            VolatilityTerm = term,
            LeverageRatio = 0,
            MarketCap = 0,
            Roe = 0,
            EquityRatio = 0,
            RevenueProfitDividend = 0,
            MinkabuAnalysis = "",
            ShouldAlert = false,
        };

        // -10.0%以下（10week以内の下落幅）
        if (!result.ShouldAlert && result.VolatilityRate <= -0.100) { result.ShouldAlert = true; }

        // -9.9%～-9.0%（3week以内の下落幅）
        if (!result.ShouldAlert && (result.VolatilityRate <= -0.090 & result.VolatilityRate >= -0.099) & result.VolatilityTerm <= 3) { result.ShouldAlert = true; }

        // -8.9%～-8.0%（2week以内の下落幅）
        if (!result.ShouldAlert && (result.VolatilityRate <= -0.080 & result.VolatilityRate >= -0.089) & result.VolatilityTerm <= 2) { result.ShouldAlert = true; }

        // -7.9%～-7.0%（2week以内の下落幅）
        if (!result.ShouldAlert && (result.VolatilityRate <= -0.070 & result.VolatilityRate >= -0.079) & result.VolatilityTerm <= 2) { result.ShouldAlert = true; }

        // 直近週が下落していない場合はアラートしない
        if (result.VolatilityRate >= lastFridyIndex) { result.ShouldAlert = false; }

        return result;
    }
    internal class AnalysisResult
    {
        public string Code { get; set; }
        public string DateString { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public double VolatilityRate { get; set; }
        public double VolatilityRateIndex1 { get; set; }
        public DateTime VolatilityRateIndex1Date { get; set; }
        public double VolatilityRateIndex2 { get; set; }
        public DateTime VolatilityRateIndex2Date { get; set; }
        public int VolatilityTerm { get; set; }
        public double LeverageRatio { get; set; }
        public double MarketCap { get; set; }
        public double Roe { get; set; }
        public double EquityRatio { get; set; }
        public double RevenueProfitDividend { get; set; }
        public string MinkabuAnalysis { get; set; }
        public bool ShouldAlert { get; set; }

        internal bool ShouldAlertMethod()
        {
            bool result = false;

            // -10.0%以下（10week以内の下落幅）
            if (!result && this.VolatilityRate <= -0.100) { result = true; }

            // -9.9%～-9.0%（3week以内の下落幅）
            if (!result && (this.VolatilityRate <= -0.090 & this.VolatilityRate >= -0.099) & this.VolatilityTerm <= 3) { result = true; }

            // -8.9%～-8.0%（2week以内の下落幅）
            if (!result && (this.VolatilityRate <= -0.080 & this.VolatilityRate >= -0.089) & this.VolatilityTerm <= 2) { result = true; }

            // -7.9%～-7.0%（2week以内の下落幅）
            if (!result && (this.VolatilityRate <= -0.070 & this.VolatilityRate >= -0.079) & this.VolatilityTerm <= 2) { result = true; }

            //Console.WriteLine($"Code: {this.Code}, VolatilityRate: {this.VolatilityRate}, VolatilityTerm: {this.VolatilityTerm}, result: {result}");

            return result;
        }
    }
    internal DateTime GetStartOfWeek(DateTime date)
    {
        // 現在の日付の曜日を取得
        DayOfWeek dayOfWeek = date.DayOfWeek;

        // 日曜日を0、月曜日を1、...、土曜日を6とする
        int offset = dayOfWeek - DayOfWeek.Monday;

        // 日曜日の場合、オフセットを-6に設定
        if (offset < 0)
        {
            offset += 7;
        }

        // 週の最初の日を計算
        DateTime startOfWeek = date.AddDays(-offset);

        return startOfWeek;
    }

    internal DateTime GetLastFriday(DateTime currentDate)
    {
        DateTime baseDay = currentDate;

        // 現在の日付の曜日を取得
        DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;

        // 土日だったら基準日を金曜日まで調整
        if (currentDayOfWeek == DayOfWeek.Saturday) { baseDay.AddDays(-1); }
        if (currentDayOfWeek == DayOfWeek.Sunday) { baseDay.AddDays(-2); }

        // 金曜日までの日数を計算
        int daysToLastFriday = ((int)currentDayOfWeek + 1) % 7 + 1;

        // 前の金曜日の日付を計算
        DateTime lastFriday = baseDay.AddDays(-daysToLastFriday);

        return lastFriday;
    }

}