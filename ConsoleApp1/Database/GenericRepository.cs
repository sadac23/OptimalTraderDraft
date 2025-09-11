using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ConsoleApp1.Database
{
    public class GenericRepository
    {
        private readonly SQLiteConnection _connection;

        public GenericRepository(SQLiteConnection connection)
        {
            _connection = connection;
        }

        // データ取得（全カラム）
        public List<Dictionary<string, object>> GetRows(
            string table,
            string whereClause,
            Dictionary<string, object> parameters,
            string orderBy = null,
            int? limit = null)
        {
            var query = $"SELECT * FROM {table}";
            if (!string.IsNullOrWhiteSpace(whereClause))
                query += $" WHERE {whereClause}";
            if (!string.IsNullOrWhiteSpace(orderBy))
                query += $" ORDER BY {orderBy}";
            if (limit.HasValue)
                query += $" LIMIT {limit.Value}";

            using var command = new SQLiteCommand(query, _connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                    command.Parameters.AddWithValue(param.Key, param.Value);
            }

            var result = new List<Dictionary<string, object>>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                result.Add(row);
            }
            return result;
        }
    }
}