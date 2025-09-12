# StockInfo系クラス リファクタリング方針案

## 目的

- 肥大化した`StockInfo`クラスの責務を分割し、共通化を図る
- 派生クラス（`IndexInfo`, `JapaneseETFInfo`等）の実装方針を統一する
- 今後の拡張性やテスト容易性を高める

---

## 方針概要

- 外部情報取得や出力処理の責務を、Strategyパターンによる委譲で実現する
- 共通ロジックは`StockInfo`に集約し、個別ロジックは委譲先（Strategy）で実装する
- 派生クラスは「委譲先の実装を差し替えるだけ」で拡張できる構造とする

---

## 構成イメージ

1. 外部情報取得・出力処理のためのインターフェースを用意し、責務を明確に分離する
2. `StockInfo`は共通プロパティ・ロジックを持ち、Updater/Formatter（Strategy）を委譲で保持する
3. 派生クラス（例：`IndexInfo`, `JapaneseETFInfo`）は、用途に応じたUpdater/Formatterを渡して初期化する
4. 新しい銘柄種別が増えても、Updater/Formatterを用意するだけで拡張できる

---

## メリット

- **拡張性**：新しい銘柄種別もStrategy実装を追加するだけで対応可能
- **テスト容易性**：Updater/Formatter単位でMock化・単体テストが容易
- **保守性**：共通ロジックはStockInfo、個別ロジックはStrategyで分離されるため、保守がしやすい

---

## まとめ

- `StockInfo`の肥大化を防ぎ、派生クラスの実装を統一
- 今後の拡張やテストが容易な設計に刷新することで、保守性・品質向上を実現

---

## AssetInfoクラス リファクタリング案

### 肥大化の主な要因

- プロパティ数が非常に多く、データ構造とロジックが混在している
- データ取得・計算・判定・DBアクセス・出力など異なる責務のメソッドが1クラスに集約されている
- 内部クラス（ScrapedPrice, FullYearPerformance等）も多く、1ファイルの行数が膨大
- Strategyパターンの導入は一部（updater/formatter）のみ

### リファクタリング方針

1. **責務ごとにクラス分割**
   - AssetInfoは「データ保持」と「ロジック委譲」のみ担当
   - 計算・判定・DBアクセス等は専用クラス/サービスに委譲

2. **Strategyパターンの適用範囲拡大**
   - 計算・判定ロジックもStrategy化し、差し替え可能に

3. **内部クラスの外部化・共通化**
   - ScrapedPrice, FullYearPerformance等は独立ファイル化し、再利用性を高める

4. **DBアクセスのリポジトリパターン化**
   - DB操作はRepositoryクラスに集約し、AssetInfoからDBアクセスを排除

5. **派生クラスの責務明確化**
   - 派生クラスはStrategyの組み合わせ指定のみとし、個別ロジックはStrategy実装側に集約

### サンプル構成イメージ

---

## AssetInfoリファクタリング 段階的実施案

### 段階的リファクタリングステップ

1. **データクラスの外部化・整理**
   - ScrapedPrice, FullYearPerformanceなどの内部クラスを独立ファイル化
   - AssetInfoのプロパティ定義を整理し、データ構造の明確化

   #### 外部化対象データクラス一覧

   AssetInfoクラスから外部化すべきデータクラスは以下の通りです。これらは `ConsoleApp1.Assets.Models` 名前空間配下に独立ファイルとして整理します。

   | クラス名                        | 主な役割・内容                           |
   |----------------------------------|------------------------------------------|
   | ScrapedPrice                    | 日次価格情報（株価履歴）                 |
   | FullYearPerformance             | 通期業績情報                             |
   | FullYearProfit                  | 通期収益情報                             |
   | QuarterlyPerformance            | 四半期業績情報                           |
   | FullYearPerformanceForcast      | 通期業績予想（および予想履歴）           |
   | ChartPrice                      | チャート用価格情報（テクニカル指標含む） |
   | Disclosure                      | 開示情報                                 |

   **配置・名前空間例**

   - ConsoleApp1\Assets\Models\ScrapedPrice.cs
   - ConsoleApp1\Assets\Models\FullYearPerformance.cs
   - ConsoleApp1\Assets\Models\FullYearProfit.cs
   - ConsoleApp1\Assets\Models\QuarterlyPerformance.cs
   - ConsoleApp1\Assets\Models\FullYearPerformanceForcast.cs
   - ConsoleApp1\Assets\Models\ChartPrice.cs
   - ConsoleApp1\Assets\Models\Disclosure.cs

   すべて `namespace ConsoleApp1.Assets.Models` で統一します。

2. **DBアクセスの分離**
   - DBアクセス処理（履歴取得・保存など）をIAssetRepository/AssetRepositoryへ移動
   - AssetInfoからDBアクセスコードを削除し、Repository経由に切り替え

   #### AssetInfoクラス内でIAssetRepositoryに処理を移譲すべきメソッド一覧

   - `LoadHistoryAsync()`  
     履歴データの取得。既にrepository経由で呼び出し済み。
   - `SaveHistoryAsync()`  
     履歴データの保存。既にrepository経由で呼び出し済み。
   - `DeleteHistoryAsync(DateTime targetDate)`  
     履歴データの削除。既にrepository経由で呼び出し済み。
   - `GetPreviousForcasts()`  
     通期業績予想履歴の取得。DBアクセス処理をrepositoryに移譲すべき。
   - `GetLastHistoryUpdateDay()`  
     履歴テーブルから最新日付を取得。DBアクセス処理をrepositoryに移譲すべき。
   - `SetupChartPrices()`  
     historyテーブルからチャート用データを取得。DBアクセス処理をrepositoryに移譲すべき。

   ※上記メソッドは、直接`SQLiteConnection`や`SQLiteCommand`等でDBアクセスしている、またはリポジトリ経由にすべきデータ取得・保存処理を含みます。

   - これらのメソッドのDBアクセス部分をIAssetRepositoryインターフェースで定義し、AssetRepository等の実装クラスに移動してください。
   - AssetInfoはIAssetRepository経由でのみDB操作を行うようにリファクタリングします。

   #### 具体的な実施案

   - AssetInfoクラス内に存在するDBアクセス関連メソッド（例：履歴の取得・保存、キャッシュ登録、履歴削除など）をすべて抽出し、IAssetRepositoryインターフェースで定義する。
   - DBアクセスの実装はAssetHistoryRepositoryクラスに集約し、SQLiteや他のDBアクセスロジックをこのクラスに移動する。
   - AssetInfoはIAssetRepository型のフィールド/プロパティを持ち、必要なDB操作はRepository経由で呼び出す。
   - 例：
     - `Task<List<ScrapedPrice>> LoadHistoryAsync(string code);`
     - `Task SaveHistoryAsync(string code, List<ScrapedPrice> prices);`
     - `Task DeleteHistoryAsync(string code, DateTime targetDate);`
   - テスト容易性向上のため、IAssetRepositoryはMock実装も容易にできるように設計する。
   - 既存のDBアクセスコード（SQLiteCommand等）はAssetInfoから完全に排除し、Repositoryクラスにのみ存在させる。
   - DI（依存性注入）を活用し、AssetInfoの利用側でリポジトリ実装を差し替え可能にする。

   **メリット**
   - DBアクセスの責務が明確になり、AssetInfoの肥大化を防止
   - テスト時にDBアクセスをMockに差し替えやすくなる
   - DBアクセスの共通化・再利用性向上
   - DBアクセス層の変更（例：SQLite→SQL Server等）もRepository実装の差し替えのみで対応可能

   **サンプル構成イメージ**

   - ConsoleApp1\Assets\Repositories\IAssetRepository.cs
   - ConsoleApp1\Assets\Repositories\AssetRepository.cs

3. **計算・判定ロジックのStrategy化**
   - PER計算や利回り判定などをIAssetCalculator/StockCalculator等に分離
   - AssetInfoはStrategyインターフェース経由でロジックを呼び出す形に変更

4. **アラート判定のStrategy化**
   - アラート判定ロジックをIAlertEvaluator/DefaultAlertEvaluatorに分離
   - AssetInfoはアラート判定もStrategy経由で実行

5. **派生クラスの整理・統一**
   - JapaneseStockInfo, JapaneseETFInfo, IndexInfo等の派生クラスをStrategyの組み合わせ指定のみで構築
   - 個別ロジックはStrategy実装側に集約

6. **テスト・リファクタリングの完了**
   - 各Strategy/Repositoryの単体テストを作成
   - AssetInfoの責務が「データ保持＋委譲」のみになっていることを確認

---

### 進め方のポイント

- 各ステップごとに既存の動作確認・テストを必ず実施
- 1ステップごとにコミットし、問題があればすぐにロールバックできるようにする
- 既存利用箇所への影響を最小限に抑えつつ、段階的に移行

---

### まとめ

- データ・ロジック・DBアクセス・出力・判定を明確に分離
- Strategy/Repositoryパターンを徹底し、拡張性・テスト容易性・保守性を向上
- 内部クラスや共通ロジックは独立ファイル化し再利用性を高める

---
