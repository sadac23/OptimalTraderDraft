using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

public class DatabaseAccessor
{
    private readonly SQLiteConnection _connection;

    public DatabaseAccessor(SQLiteConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// SQLクエリを実行し、結果をList&lt;Dictionary&lt;string, object&gt;&gt;で返す
    /// </summary>
    public List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters = null)
    {
        var results = new List<Dictionary<string, object>>();

        using (var command = new SQLiteCommand(query, _connection))
        {
            // パラメータ設定
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
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
