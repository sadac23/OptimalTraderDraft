using System.Data.SQLite;
using System.Globalization;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Database;

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
            using var connection = DbConnectionFactory.GetConnection();
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
            return result;
        }

        public async Task SaveHistoryAsync(string code, List<ScrapedPrice> prices)
        {
            using var connection = DbConnectionFactory.GetConnection();
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
            }
        }

        public async Task DeleteHistoryAsync(string code, DateTime targetDate)
        {
            using var connection = DbConnectionFactory.GetConnection();
            await connection.OpenAsync();

            string query = "DELETE FROM history WHERE code = @code AND date = @date";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@date", targetDate);
            await command.ExecuteNonQueryAsync();
        }

        public List<FullYearPerformanceForcast> GetPreviousForcasts(string code, string fiscalPeriod)
        {
            var result = new List<FullYearPerformanceForcast>();
            try
            {
                using var connection = DbConnectionFactory.GetConnection();
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
            }
            catch
            {
                // 例外は無視
            }
            return result;
        }

        public DateTime GetLastHistoryUpdateDay(string code)
        {
            DateTime result = CommonUtils.Instance.MasterStartDate;
            try
            {
                using var connection = DbConnectionFactory.GetConnection();
                string query = "SELECT IFNULL(MAX(date), @max_date) FROM history WHERE code = @code";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@code", code);
                command.Parameters.AddWithValue("@max_date", CommonUtils.Instance.MasterStartDate);

                using var reader = command.ExecuteReader();
                if (reader.HasRows && reader.Read())
                {
                    result = reader.GetDateTime(0);
                }
            }
            catch
            {
                // 例外時はMasterStartDateを返す
            }
            return result;
        }

        public List<Dictionary<string, object>> GetChartPriceRows(string code, int limit)
        {
            var result = new List<Dictionary<string, object>>();
            try
            {
                using var connection = DbConnectionFactory.GetConnection();
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
            }
            catch
            {
                // 例外は無視
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
    }
}