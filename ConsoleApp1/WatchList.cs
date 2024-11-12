// See https://aka.ms/new-console-template for more information

using System.Data.SQLite;

internal class WatchList
{
    internal static List<WatchStock> GetWatchStockList(string connectionString)
    {
        string query = "SELECT * FROM watch_list WHERE del_flag <> null";

        List<WatchStock> results = new List<WatchStock>();

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
//                command.Parameters.AddWithValue("@yourValue", "some_value");

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        WatchStock data = new WatchStock
                        {
                            Code = reader.GetString(0)
                        };
                        results.Add(data);
                    }
                }
            }
        }
        return results;
    }
    internal class WatchStock
    {
        public string Code { get; set; }
    }
}