using System.Data.SQLite;
using System.Globalization;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Database;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Assets.Repositories
{
    /// <summary>
    /// 資産情報のDBアクセス実装（SQLite例）
    /// </summary>
    public class AssetRepository : IAssetRepository
    {
        public async Task<List<ScrapedPrice>> LoadHistoryAsync(string code)
        {
            var result = new List<ScrapedPrice>();
            var connection = DbConnectionFactory.GetConnection();
            await connection.OpenAsync();

            string query = "SELECT * FROM history WHERE code = @code ORDER BY date DESC";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@code", code);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var price = new ScrapedPrice
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("date")),
                    DateYYYYMMDD = reader["date"].ToString(),
                    Open = Convert.ToDouble(reader["open"]),
                    High = Convert.ToDouble(reader["high"]),
                    Low = Convert.ToDouble(reader["low"]),
                    Close = Convert.ToDouble(reader["close"]),
                    Volume = Convert.ToDouble(reader["volume"]),
                    AdjustedClose = Convert.ToDouble(reader["adjusted_close"])
                };
                result.Add(price);
            }

            // ログ出力を追加
            CommonUtils.Instance.Logger.LogInformation(
                $"[LoadHistory] code={code}, count={result.Count}");

            return result;
        }

        public async Task SaveHistoryAsync(string code, List<ScrapedPrice> prices)
        {
            var connection = DbConnectionFactory.GetConnection();
            await connection.OpenAsync();

            foreach (var price in prices)
            {
                string query = @"INSERT OR REPLACE INTO history 
                    (code, date, open, high, low, close, volume, adjusted_close) 
                    VALUES (@code, @date, @open, @high, @low, @close, @volume, @adjusted_close)";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@code", code);
                command.Parameters.AddWithValue("@date", price.Date);
                command.Parameters.AddWithValue("@open", price.Open);
                command.Parameters.AddWithValue("@high", price.High);
                command.Parameters.AddWithValue("@low", price.Low);
                command.Parameters.AddWithValue("@close", price.Close);
                command.Parameters.AddWithValue("@volume", price.Volume);
                command.Parameters.AddWithValue("@adjusted_close", price.AdjustedClose);
                await command.ExecuteNonQueryAsync();

                // ログ出力
                CommonUtils.Instance.Logger.LogInformation(
                    $"[SaveHistory] code={code}, date={price.Date:yyyy-MM-dd}, open={price.Open}, close={price.Close}");
            }
        }

        public async Task DeleteHistoryAsync(string code, DateTime targetDate)
        {
            var connection = DbConnectionFactory.GetConnection();
            await connection.OpenAsync();

            string query = "DELETE FROM history WHERE code = @code AND date = @date";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@date", targetDate);
            int affected = await command.ExecuteNonQueryAsync();

            // ログ出力
            CommonUtils.Instance.Logger.LogInformation(
                $"[DeleteHistory] code={code}, date={targetDate:yyyy-MM-dd}, affectedRows={affected}");
        }

        public List<FullYearPerformanceForcast> GetPreviousForcasts(string code, string fiscalPeriod)
        {
            var result = new List<FullYearPerformanceForcast>();
            try
            {
                var connection = DbConnectionFactory.GetConnection();
                string query = "SELECT * FROM forcast_history WHERE code = @code and fiscal_period = @fiscal_period ORDER BY revision_date";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@code", code);
                command.Parameters.AddWithValue("@fiscal_period", fiscalPeriod);

                using var reader = command.ExecuteReader();
                FullYearPerformanceForcast cloneF = null;
                while (reader.Read())
                {
                    string category = reader.GetString(reader.GetOrdinal("category"));
                    string fiscalPeriodValue = reader.GetString(reader.GetOrdinal("fiscal_period"));
                    DateTime revisionDate = reader.GetDateTime(reader.GetOrdinal("revision_date"));
                    string revisionDirection = reader.GetString(reader.GetOrdinal("revision_direction"));

                    double revenue = GetDouble(reader, "revenue");
                    double operatingProfit = GetDouble(reader, "operating_profit");
                    double ordinaryIncome = GetDouble(reader, "ordinary_income");
                    double netProfit = GetDouble(reader, "net_profit");
                    double revisedDividend = GetDouble(reader, "revised_dividend");

                    var f = new FullYearPerformanceForcast
                    {
                        Category = category,
                        FiscalPeriod = fiscalPeriodValue,
                        RevisionDate = revisionDate,
                        NetProfit = netProfit.ToString(CultureInfo.InvariantCulture),
                        OperatingProfit = operatingProfit.ToString(CultureInfo.InvariantCulture),
                        OrdinaryProfit = ordinaryIncome.ToString(CultureInfo.InvariantCulture),
                        RevisionDirection = revisionDirection,
                        Revenue = revenue.ToString(CultureInfo.InvariantCulture),
                        RevisedDividend = revisedDividend.ToString(CultureInfo.InvariantCulture),
                        Summary = string.Empty,
                        PreviousForcast = cloneF,
                    };
                    cloneF = (FullYearPerformanceForcast)f.Clone();
                    result.Add(f);
                }
                // ログ出力
                CommonUtils.Instance.Logger.LogInformation(
                    $"[GetPreviousForcasts] code={code}, fiscalPeriod={fiscalPeriod}, count={result.Count}");
            }
            catch (Exception ex)
            {
                CommonUtils.Instance.Logger.LogInformation(
                    $"[GetPreviousForcasts][Error] code={code}, fiscalPeriod={fiscalPeriod}, error={ex.Message}");
            }
            return result;
        }

        public DateTime GetLastHistoryUpdateDay(string code)
        {
            DateTime result = CommonUtils.Instance.MasterStartDate;
            try
            {
                var connection = DbConnectionFactory.GetConnection();
                string query = "SELECT IFNULL(MAX(date), @max_date) FROM history WHERE code = @code";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@code", code);
                command.Parameters.AddWithValue("@max_date", CommonUtils.Instance.MasterStartDate);

                using var reader = command.ExecuteReader();
                if (reader.HasRows && reader.Read())
                {
                    result = reader.GetDateTime(0);
                }
                // ログ出力
                CommonUtils.Instance.Logger.LogInformation(
                    $"[GetLastHistoryUpdateDay] code={code}, lastDate={result:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                CommonUtils.Instance.Logger.LogInformation(
                    $"[GetLastHistoryUpdateDay][Error] code={code}, error={ex.Message}");
            }
            return result;
        }

        public List<Dictionary<string, object>> GetChartPriceRows(string code, int limit)
        {
            var result = new List<Dictionary<string, object>>();
            try
            {
                var connection = DbConnectionFactory.GetConnection();
                string query = $"SELECT code, date, open, high, low, close, volume FROM history WHERE code = @code ORDER BY date DESC LIMIT @limit";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@code", code);
                command.Parameters.AddWithValue("@limit", limit);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>
                    {
                        { "code", reader["code"] },
                        { "date", reader["date"] },
                        { "open", reader["open"] },
                        { "high", reader["high"] },
                        { "low", reader["low"] },
                        { "close", reader["close"] },
                        { "volume", reader["volume"] }
                    };
                    result.Add(row);
                }
                // ログ出力
                CommonUtils.Instance.Logger.LogInformation(
                    $"[GetChartPriceRows] code={code}, count={result.Count}");
            }
            catch (Exception ex)
            {
                CommonUtils.Instance.Logger.LogInformation(
                    $"[GetChartPriceRows][Error] code={code}, error={ex.Message}");
            }
            return result;
        }

        private double GetDouble(SQLiteDataReader reader, string column)
        {
            double result = 0;
            try
            {
                result = ConvertToDouble(reader.GetString(reader.GetOrdinal(column)));
            }
            catch
            {
                try
                {
                    result = ConvertToDouble(reader.GetValue(reader.GetOrdinal(column)));
                }
                catch { }
            }
            return result;
        }

        private double ConvertToDouble(object input)
        {
            if (input == null) return 0;
            if (input is string strInput)
            {
                strInput = strInput.Replace(",", "");
                if (double.TryParse(strInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                {
                    return value;
                }
            }
            else if (input is IConvertible)
            {
                try
                {
                    return Convert.ToDouble(input, CultureInfo.InvariantCulture);
                }
                catch { }
            }
            return 0;
        }

        /// <summary>
        /// 株価履歴の登録（重複チェック付き）
        /// </summary>
        public async Task RegisterHistoryAsync(string code, List<ScrapedPrice> prices)
        {
            var connection = DbConnectionFactory.GetConnection();
            foreach (var p in prices)
            {
                if (!await IsInHistoryAsync(code, p, connection))
                {
                    string query = "INSERT INTO history (code, date_string, date, open, high, low, close, volume) " +
                                   "VALUES (@code, @date_string, @date, @open, @high, @low, @close, @volume)";
                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@date_string", p.DateYYYYMMDD);
                    command.Parameters.AddWithValue("@date", p.Date);
                    command.Parameters.AddWithValue("@open", p.Open);
                    command.Parameters.AddWithValue("@high", p.High);
                    command.Parameters.AddWithValue("@low", p.Low);
                    command.Parameters.AddWithValue("@close", p.AdjustedClose);
                    command.Parameters.AddWithValue("@volume", p.Volume);
                    await command.ExecuteNonQueryAsync();

                    // ログ出力
                    CommonUtils.Instance.Logger.LogInformation(
                        $"[RegisterHistory] code={code}, date={p.Date:yyyy-MM-dd}, open={p.Open}, close={p.AdjustedClose}");
                }
            }
        }

        /// <summary>
        /// 履歴の存在チェック
        /// </summary>
        private async Task<bool> IsInHistoryAsync(string code, ScrapedPrice p, SQLiteConnection connection)
        {
            string query = "SELECT count(code) as count FROM history WHERE code = @code and date_string = @date_string";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@date_string", p.DateYYYYMMDD);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// 古い株価履歴の削除
        /// </summary>
        public async Task DeleteOldHistoryAsync(string code, DateTime beforeDate)
        {
            var connection = DbConnectionFactory.GetConnection();
            string query = "DELETE FROM history WHERE code = @code and date <= @date";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@date", beforeDate);
            int affected = await command.ExecuteNonQueryAsync();

            // ログ出力
            CommonUtils.Instance.Logger.LogInformation(
                $"[DeleteOldHistory] code={code}, beforeDate={beforeDate:yyyy-MM-dd}, affectedRows={affected}");
        }

        /// <summary>
        /// 通期予想履歴の登録（重複チェック付き）
        /// </summary>
        public async Task RegisterForcastHistoryAsync(string code, List<FullYearPerformanceForcast> forcasts)
        {
            var connection = DbConnectionFactory.GetConnection();
            foreach (var f in forcasts)
            {
                if (!await IsInForcastHistoryAsync(code, f, connection))
                {
                    string query = "INSERT INTO forcast_history (" +
                        "code, revision_date_string, revision_date, fiscal_period, category, revision_direction, " +
                        "revenue, operating_profit, ordinary_income, net_profit, revised_dividend) " +
                        "VALUES (" +
                        "@code, @revision_date_string, @revision_date, @fiscal_period, @category, @revision_direction, " +
                        "@revenue, @operating_profit, @ordinary_income, @net_profit, @revised_dividend)";
                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@revision_date_string", f.RevisionDate.ToString("yyyyMMdd"));
                    command.Parameters.AddWithValue("@revision_date", f.RevisionDate);
                    command.Parameters.AddWithValue("@fiscal_period", f.FiscalPeriod);
                    command.Parameters.AddWithValue("@category", f.Category);
                    command.Parameters.AddWithValue("@revision_direction", f.RevisionDirection);
                    command.Parameters.AddWithValue("@revenue", double.TryParse(f.Revenue, out var r) ? r : 0);
                    command.Parameters.AddWithValue("@operating_profit", double.TryParse(f.OperatingProfit, out var op) ? op : 0);
                    command.Parameters.AddWithValue("@ordinary_income", double.TryParse(f.OrdinaryProfit, out var oi) ? oi : 0);
                    command.Parameters.AddWithValue("@net_profit", double.TryParse(f.NetProfit, out var np) ? np : 0);
                    command.Parameters.AddWithValue("@revised_dividend", double.TryParse(f.RevisedDividend, out var rd) ? rd : 0);
                    await command.ExecuteNonQueryAsync();

                    // ログ出力
                    CommonUtils.Instance.Logger.LogInformation(
                        $"[RegisterForcastHistory] code={code}, revisionDate={f.RevisionDate:yyyy-MM-dd}, category={f.Category}");
                }
            }
        }

        /// <summary>
        /// 予想履歴の存在チェック
        /// </summary>
        private async Task<bool> IsInForcastHistoryAsync(string code, FullYearPerformanceForcast f, SQLiteConnection connection)
        {
            string query = "SELECT count(code) as count FROM forcast_history WHERE code = @code and revision_date_string = @revision_date_string";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@revision_date_string", f.RevisionDate.ToString("yyyyMMdd"));
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
    }
}