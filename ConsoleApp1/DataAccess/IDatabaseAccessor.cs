public interface IDatabaseAccessor : IDisposable
{
    List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters);
}