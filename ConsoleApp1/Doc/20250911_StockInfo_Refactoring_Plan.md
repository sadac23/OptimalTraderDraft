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

   #### 具体的な実施案

   - 計算・判定ロジックごとにインターフェースを定義し、Strategyパターンで差し替え可能にする。
     - 例：`IAssetCalculator`（資産全般の計算用）、`IStockCalculator`（株式固有の計算用）など
   - 代表的な計算・判定ロジック例
     - PER（株価収益率）計算
     - PBR（株価純資産倍率）計算
     - 配当利回り計算
     - 増配判定
     - 業績成長判定
     - 株価トレンド判定
   - 各ロジックは個別のStrategyクラスとして実装し、必要に応じて組み合わせて利用できるようにする。
     - 例：`DefaultStockCalculator`、`GrowthStockCalculator`、`EtfCalculator`など
   - AssetInfoや派生クラスは、コンストラクタ等で利用するCalculator（Strategy）を受け取り、計算・判定処理はStrategy経由で実行する。
   - テスト容易性向上のため、各CalculatorはMock化しやすい設計とする。

   > **補足：Strategy化の対象範囲について**
   >
   > Strategyパターンで分離・差し替え可能にするのは「算出値（計算や判定によって導出される値）」のみとする。
   > 外部から取得した情報（例：株価、業績データ、配当金などの生データ）はAssetInfoや関連データクラスがそのまま保持し、
   > それらの値をもとに算出・判定するロジック部分のみをStrategyとして実装する。
   >
   > - **Strategy化の対象**：PER、PBR、配当利回り、成長判定など「計算・判定によって導出される値や判定結果」
   > - **Strategy化の対象外**：APIやDB等から直接取得した「生データ」（株価、業績、配当など）
   >
   > **Strategy化の対象となる処理一覧（コード解析結果）**
   > 
   > - `IsPERUndervalued`（PER割安判定）
   > - `IsPBRUndervalued`（PBR割安判定）
   > - `IsROEAboveThreshold`（ROE基準判定）
   > - `IsAnnualProgressOnTrack`（通期進捗順調判定）
   > - `IsHighYield`（利回り高判定）
   > - `IsHighMarketCap`（時価総額高判定）
   > - `IsCloseToDividendRecordDate`（配当権利確定日接近判定）
   > - `IsCloseToShareholderBenefitRecordDate`（優待権利確定日接近判定）
   > - `IsCloseToQuarterEnd`（四半期決算日接近判定）
   > - `IsAfterQuarterEnd`（四半期決算直後判定）
   > - `IsQuarterEnd`（四半期決算当日判定）
   > - `IsJustSold`（売却直後判定）
   > - `IsOwnedNow`（現在保有判定）
   > - `IsGoldenCrossPossible`（ゴールデンクロス発生可能性判定）
   > - `HasRecentStockSplitOccurred`（株式分割発生判定）
   > - `ShouldAverageDown`（ナンピン判定）
   > - `IsGranvilleCase1Matched`（グランビル第1法則判定）
   > - `IsGranvilleCase2Matched`（グランビル第2法則判定）
   > - `HasDisclosure`（開示情報有無判定）
   > - `IsRecordDate`（権利確定日当日判定）
   > - `IsAfterRecordDate`（権利確定日直後判定）
   > - `IsCloseToRecordDate`（権利確定日直前判定）
   > - `ExtractAndValidateDateWithinOneMonth`（日付抽出・1か月以内判定）
   > - `UpdateProgress`（進捗率・営業利益率等の算出）
   > - `UpdateDividendPayoutRatio`（配当性向・DOE算出）
   > - `UpdateFullYearPerformanceForcastSummary`（業績・配当増減サマリ算出）
   > - `SetupChartPrices`（チャート用指標算出：SMA, RSI, 乖離率等）
   > - その他、`GetDividendPayoutRatio`、`GetDOE`、`GetIncreasedRate`、`GetDividendPerShareIncreased` などの算出系privateメソッド

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

## [2025/09/18] 旧AssetInfo→新AssetInfoリファクタリング移行状況・調査結果

### 1. 判定・計算ロジックの委譲とカバー状況

- 旧AssetInfoの判定・計算系メソッド（例：IsPERUndervalued, IsHighYield, UpdateFullYearPerformanceForcastSummary等）は、新AssetInfoでIAssetJudgementStrategy/IAssetCalculator等のStrategyパターンに委譲されている。
- これにより、ロジック自体は外部クラスに移動しているため、AssetInfo本体には直接的な実装がない。
- 参照先クラス（IAssetCalculator等）のインターフェースには必要なメソッドが定義されているが、**実装クラスで旧AssetInfoのロジックが正確に再現されているかは実装ファイルの確認が必要**。
  - 特に「増収増益増配記号や率計算」「Summaryの組み立て」など、旧クラスの細かいロジックが抜けていないか要注意。

### 2. DB操作・ログ出力

- DB操作（履歴登録・削除・重複チェック）は、旧クラスでは直接SQLiteを操作し、CommonUtils.Instance.Logger.LogInformationでログ出力していた。
- 新クラスではIAssetRepository経由の非同期メソッドに移行し、RegisterCacheAsync内でRegisterHistoryAsyncやRegisterForcastHistoryAsync、DeleteOldHistoryAsyncを呼び出している。
- **ログ出力については、Repository実装側でCommonUtils.Instance.Loggerを使って出力する設計にする必要がある**。
  - 旧クラス同様、DB登録・削除時にLogInformationが呼ばれているか、Repository実装を必ず確認すること。

### 3. FullYearPerformanceForcastSummaryプロパティ

- 旧クラスにはFullYearPerformanceForcastSummaryプロパティがあったが、新クラスには存在しない。
- 旧クラスのUpdateFullYearPerformanceForcastSummaryは、各FullYearPerformanceForcastのSummaryを組み立てていた。
- **新クラスでは、Summaryは各Forcastのプロパティとして保持されているため、クラス全体のSummaryプロパティは不要**。
  - もし「全体サマリ文字列」が必要な場合は、FullYearPerformancesForcasts.Select(f => f.Summary).Join(",")のように都度組み立てれば十分。

### 4. 不要なプロパティ

- FullYearPerformanceForcastSummaryは使われていないため不要（新設不要）。
- その他のプロパティは新旧でほぼ一致しており、不要なものはなし。

### 5. 重複チェック・個別登録ロジック

- 旧クラスのIsInHistoryやIsInForcastHistoryのような重複チェックは、新クラスではRepository側で担保されている。
- RegisterCacheAsyncのコメントにも「重複チェックはRepository側で実装」と明記されている。

### 6. 参照先クラスのカバー状況

- IAssetCalculator, IAssetJudgementStrategy, IAssetRepositoryのインターフェースは新クラスで正しく利用されている。
- ただし、**実装クラス（例：AssetCalculator, AssetRepositoryなど）で旧クラスのロジックが正確に再現されているかは、実装ファイルの確認が必要**。
  - 特に「DB操作時のログ出力」「Summary組み立てロジック」「重複チェック」など。

### 7. ログ出力要件

- **DB操作時のログ出力は新設計でも必須**。
- 旧クラス同様、CommonUtils.Instance.Logger.LogInformationをRepository実装の各DB操作メソッド内で必ず呼び出すこと。

---

#### 結論・アクション

- 機能的な抜けは基本的にないが、Repository/Calculator等の実装で
  - 旧クラスのロジック（特にSummary組み立て、DB操作時のログ出力、重複チェック）が正確に再現されているかを必ず確認すること。
- FullYearPerformanceForcastSummaryプロパティは不要。
- DB操作時のログ出力はRepository実装で必ず行うこと。

---

#### 補足

- IAssetCalculatorやIAssetRepositoryの実装ファイルが必要であれば、パスを指定して内容を取得し、追加調査可能。

---

## [2025/09/18] 旧クラスロジック再現性 検証結果

### 1. DBアクセス・重複チェック・履歴操作（AssetRepository）

- 履歴・予想履歴の重複チェック（`IsInHistoryAsync`, `IsInForcastHistoryAsync`）を経て、重複がなければINSERTするロジックは新実装でも再現されている。
- 履歴の取得・保存・削除、予想履歴の取得、チャート用データの取得、最新履歴日付の取得など、旧クラスのDB操作ロジックはすべてAssetRepositoryでカバーされている。
- 例外時のデフォルト値返却や握りつぶしも旧クラス同様。
- **結論：DBアクセス・重複チェック・履歴操作ロジックは正しく再現されている。**

### 2. 計算・サマリ・進捗率等（DefaultAssetCalculator）

- 配当性向・DOE、進捗率・営業利益率・前年同期比成長率の計算、サマリ（増収増益増配記号や率）の組み立て、チャート用指標（SMA, RSI, 乖離率等）の計算など、旧クラスの計算・サマリロジックはDefaultAssetCalculatorで再現されている。
- 各種privateメソッド（符号付き文字列、パーセント変換等）も移植済み。
- **結論：計算・サマリ・進捗率・チャート指標ロジックは正しく再現されている。**

### 3. 例外処理・デフォルト値

- 例外時のデフォルト値返却（例：日付取得失敗時はDateTime.NowやMasterStartDate）は旧クラスと同じ挙動。

### 4. ログ出力

- AssetRepository内で**DB操作時のログ出力（CommonUtils.Instance.Logger.LogInformation等）が未実装**。
- 旧クラス同様、履歴登録・削除・予想履歴登録等の操作ログ出力をRepository内で追加実装する必要あり。

---

#### 総合結論

- **DBアクセス・重複チェック・計算・サマリ・進捗率等のロジックは、旧クラスの内容が新実装で正しく再現されている。**
- **ただし、DB操作時のログ出力（LogInformation）はAssetRepositoryに追加実装が必要。**
- それ以外のロジック再現性は高く、旧クラスの機能要件を満たしている。

---

#### アクション推奨

- AssetRepositoryの各DB操作メソッド（履歴登録・削除・予想履歴登録等）で、  
  `CommonUtils.Instance.Logger.LogInformation`による操作ログ出力を追加すること。

---
