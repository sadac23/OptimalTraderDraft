using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

public class DatabaseAccessor
{
    private readonly string _connectionString;

    public DatabaseAccessor(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// SQL�N�G�������s���A���ʂ�List&lt;Dictionary&lt;string, object&gt;&gt;�ŕԂ�
    /// </summary>
    public List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters = null)
    {
        var results = new List<Dictionary<string, object>>();

        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                // �p�����[�^�ݒ�
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
        }

        return results;
    }
}
