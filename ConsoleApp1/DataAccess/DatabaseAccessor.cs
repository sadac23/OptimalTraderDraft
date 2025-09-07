using System.Data;

namespace ConsoleApp1.DatabaseAccess
{
    public class DatabaseAccessor
    {
        private readonly IDbConnection _connection;

        public DatabaseAccessor(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// SQLクエリを実行し、結果をList<Dictionary<string, object>>で返す
        /// </summary>
        public List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            var results = new List<Dictionary<string, object>>();

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = query;

                // パラメータ設定
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        var dbParam = command.CreateParameter();
                        dbParam.ParameterName = param.Key;
                        dbParam.Value = param.Value ?? DBNull.Value;
                        command.Parameters.Add(dbParam);
                    }
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                        results.Add(row);
                    }
                }
            }

            return results;
        }
    }

}

