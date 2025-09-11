# データベース周りのリファクタリング推奨事項（具体例付き）

## 1. DbConnectionの管理方法
- **現状**: 各所で `new SQLiteConnection(...)` を都度生成し、usingで閉じている。
- **推奨**:  
  - コネクション生成処理を `DbConnectionFactory` などのクラスにまとめることで、接続文字列や設定の一元管理が可能。
  - 例:  
    ```csharp
    public class DbConnectionFactory
    {
        private readonly string _connectionString;
        public DbConnectionFactory(string connectionString) => _connectionString = connectionString;
        public SQLiteConnection Create() => new SQLiteConnection(_connectionString);
    }
    ```
  - DI（依存性注入）を活用し、テスト時はインメモリDBやモックに差し替えやすくする。

## 2. SQLクエリの重複・ハードコーディング
- **現状**: SQL文が各クラス・メソッド内に直接記述されている。
- **推奨**:  
  - SQL文を定数や専用のリポジトリクラスにまとめる。
  - 例:  
    ```csharp
    public static class SqlQueries
    {
        public const string SelectHistory = "SELECT * FROM history WHERE code = @code ORDER BY date DESC LIMIT @limit";
    }
    ```
  - これにより、SQLの修正やレビューが容易になり、重複やミスを防げる。

## 3. DatabaseAccessorの活用・抽象化
- **現状**: 一部で `DatabaseAccessor` を使い、他は直接ADO.NETを利用している。
- **推奨**:  
  - すべてのDBアクセスを `IDatabaseAccessor` インターフェース経由に統一。
  - 例:  
    ```csharp
    public interface IDatabaseAccessor
    {
        List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters);
    }
    ```
  - これにより、テスト時にモックやスタブを注入でき、テスト容易性が向上する。

## 4. 例外処理・エラーハンドリング
- **現状**: try-catchで例外を握りつぶしている、またはcatchがない箇所がある。
- **推奨**:  
  - 例外発生時は必ずログ出力し、必要に応じて上位に再throwする。
  - 例:  
    ```csharp
    try
    {
        // DB操作
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "DB操作中にエラー発生");
        throw;
    }
    ```
  - 例外を無視せず、障害調査やユーザー通知に活用できるようにする。

## 5. テスト容易性の向上
- **現状**: DBアクセスが直接的なため、ユニットテストが困難。
- **推奨**:  
  - DBアクセス部分をインターフェース化し、テスト時はモックやインメモリDB（例: SQLiteの `Data Source=:memory:`）を利用。
  - 例:  
    ```csharp
    var accessor = new Mock<IDatabaseAccessor>();
    accessor.Setup(a => a.ExecuteQuery(...)).Returns(...);
    ```
  - テスト用のDB初期化・クリーンアップ処理も整備する。

## 6. トランザクション管理
- **現状**: 複数のDB更新を個別に実行している箇所がある。
- **推奨**:  
  - 複数のDB更新が一連の処理で必要な場合は、`SQLiteTransaction` などで明示的にトランザクション管理を行う。
  - 例:  
    ```csharp
    using (var connection = new SQLiteConnection(...))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            // 複数のコマンド
            transaction.Commit();
        }
    }
    ```
  - これにより、途中失敗時のロールバックが可能となる。

## 7. SQLインジェクション対策
- **現状**: パラメータ化されていないSQLが混在している可能性がある。
- **推奨**:  
  - すべてのSQLで必ずパラメータ化を徹底し、文字列連結によるSQL生成を避ける。
  - 例:  
    ```csharp
    command.Parameters.AddWithValue("@code", code);
    ```

## 8. コネクションプーリングの活用
- **現状**: SQLiteではコネクションプーリングの恩恵は限定的だが、将来的なDB移行も考慮する。
- **推奨**:  
  - コネクションの使い回しやプーリング設定を意識した設計にしておくと、RDBMS移行時も対応しやすい。

---

## まとめ

- DBアクセスのラッパー/インターフェース化とDI対応
- SQL文・コネクション生成の一元管理
- 例外処理・エラーハンドリングの強化
- テスト容易性のためのモック・インメモリDB対応
- トランザクション管理の明確化
- SQLインジェクション対策の徹底

---