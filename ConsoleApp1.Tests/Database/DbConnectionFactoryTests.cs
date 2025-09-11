using System;
using System.Data.SQLite;
using Xunit;
using ConsoleApp1.Database;

namespace ConsoleApp1.Tests.Database
{
    public class DbConnectionFactoryTests
    {
        [Fact]
        public void GetConnection_ReturnsSameInstance()
        {
            // Arrange
            DbConnectionFactory.Initialize("Data Source=:memory:;Version=3;New=True;");

            // Act
            var conn1 = DbConnectionFactory.GetConnection();
            var conn2 = DbConnectionFactory.GetConnection();

            // Assert
            Assert.Same(conn1, conn2);
            Assert.Equal(System.Data.ConnectionState.Open, conn1.State);

            // Clean up
            DbConnectionFactory.Dispose();
        }

        [Fact]
        public void CanExecuteCommand_UsingSingletonConnection()
        {
            // Arrange
            DbConnectionFactory.Initialize("Data Source=:memory:;Version=3;New=True;");
            var conn = DbConnectionFactory.GetConnection();

            // テーブル作成
            using (var cmd = new SQLiteCommand("CREATE TABLE test (id INTEGER PRIMARY KEY, name TEXT);", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // データ挿入
            using (var cmd = new SQLiteCommand("INSERT INTO test (name) VALUES ('foo');", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // データ取得
            using (var cmd = new SQLiteCommand("SELECT name FROM test WHERE id = 1;", conn))
            using (var reader = cmd.ExecuteReader())
            {
                Assert.True(reader.Read());
                Assert.Equal("foo", reader.GetString(0));
            }

            // Clean up
            DbConnectionFactory.Dispose();
        }
    }
}


