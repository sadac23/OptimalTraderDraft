public class MockDatabaseAccessor : IDatabaseAccessor
{
    public List<Dictionary<string, object>> Results { get; set; } = new();

    public List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters)
        => Results;

    public void Dispose() { }
}