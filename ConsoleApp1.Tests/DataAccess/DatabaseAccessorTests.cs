using System.Data.SQLite;
using ConsoleApp1.DatabaseAccess;

namespace ConsoleApp1.Tests.DatabaseAccess
{
    public class DatabaseAccessorTests
    {
        private const string InMemoryConnectionString = "Data Source=:memory:;Version=3;New=True;";

        private void SetupTestTable(SQLiteConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            CREATE TABLE test_table (
                id INTEGER PRIMARY KEY,
                name TEXT,
                value INTEGER
            );
            INSERT INTO test_table (name, value) VALUES ('Alice', 100);
            INSERT INTO test_table (name, value) VALUES ('Bob', 200);
        ";
            cmd.ExecuteNonQuery();
        }

        [Fact]
        public void ExecuteQuery_ReturnsCorrectResults()
        {
            using var connection = new SQLiteConnection(InMemoryConnectionString);
            connection.Open();
            SetupTestTable(connection);

            var accessor = new DatabaseAccessor(connection); // C³: Ú‘±‚ð’¼Ú“n‚·
            var query = "SELECT * FROM test_table WHERE value > @minValue";
            var parameters = new Dictionary<string, object> { { "@minValue", 150 } };

            var results = accessor.ExecuteQuery(query, parameters);

            Assert.Single(results);
            var row = results[0];
            Assert.Equal("Bob", row["name"]);
            Assert.Equal(200L, row["value"]); // SQLite‚ÍINTEGER‚ðlong‚Å•Ô‚·
        }

        [Fact]
        public void ExecuteQuery_EmptyResult()
        {
            using var connection = new SQLiteConnection(InMemoryConnectionString);
            connection.Open();
            SetupTestTable(connection);

            var accessor = new DatabaseAccessor(connection); // C³: Ú‘±‚ð’¼Ú“n‚·
            var query = "SELECT * FROM test_table WHERE value > @minValue";
            var parameters = new Dictionary<string, object> { { "@minValue", 1000 } };

            var results = accessor.ExecuteQuery(query, parameters);

            Assert.Empty(results);
        }
    }

}

