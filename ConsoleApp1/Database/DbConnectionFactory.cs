using System;
using System.Data.SQLite;

namespace ConsoleApp1.Database
{
    /// <summary>
    /// アプリ全体で利用できるシングルトンDBコネクションファクトリ
    /// </summary>
    public static class DbConnectionFactory
    {
        private static string? _connectionString;
        private static SQLiteConnection? _singletonConnection;
        private static readonly object _lock = new();

        /// <summary>
        /// アプリ起動時に一度だけ呼び出して接続文字列をセット
        /// </summary>
        public static void Initialize(string connectionString)
        {
            lock (_lock)
            {
                if (_singletonConnection != null)
                {
                    _singletonConnection.Dispose();
                    _singletonConnection = null;
                }
                _connectionString = connectionString;
            }
        }

        /// <summary>
        /// シングルトンのSQLiteConnectionインスタンスを返す
        /// </summary>
        public static SQLiteConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("DbConnectionFactory is not initialized.");

            lock (_lock)
            {
                if (_singletonConnection == null)
                {
                    _singletonConnection = new SQLiteConnection(_connectionString);
                    _singletonConnection.Open();
                }
                else if (_singletonConnection.State != System.Data.ConnectionState.Open)
                {
                    _singletonConnection.Open();
                }
                return _singletonConnection;
            }
        }

        /// <summary>
        /// コネクションを明示的にクローズ・破棄する場合に呼び出す
        /// </summary>
        public static void Dispose()
        {
            lock (_lock)
            {
                if (_singletonConnection != null)
                {
                    _singletonConnection.Dispose();
                    _singletonConnection = null;
                }
            }
        }

        /// <summary>
        /// テスト用: 外部で生成したSQLiteConnectionを直接セットする
        /// </summary>
        public static void SetConnection(SQLiteConnection connection)
        {
            lock (_lock)
            {
                if (_singletonConnection != null && !object.ReferenceEquals(_singletonConnection, connection))
                {
                    _singletonConnection.Dispose();
                }
                _singletonConnection = connection;
                _connectionString = connection.ConnectionString;
            }
        }
    }
}