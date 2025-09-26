# スクレイパー設計と拡張性に関する検討ドキュメント

## 1. 現状の課題

- 各Scraper（KabutanScraper, MinkabuScraper, YahooScraper）は、資産種別や情報源ごとに処理やURL/XPathがベタ書きされている。
- 種別追加や新サイト対応時、各Scraperの内部で分岐や重複処理が増え、保守性・拡張性が低下する。
- 例：米国個別株（USAssetInfo）追加時、既存Scraperの大幅な修正が必要。

---

### 1.1 現状の重複・分岐の洗い出し

- 以下のような重複・分岐が各Scraperで発生している。

#### 1.1.1 URL生成の分岐
- 資産種別や情報源ごとにURLの組み立てが異なり、各Scraper内で分岐。
  - KabutanScraper: 日本株/米国株でURLが異なる。
  - YahooScraper: 指数/日本株/ETF/米国株などでサフィックスやパスが異なる。
  - MinkabuScraper: 日本株前提だが、今後種別追加時に分岐が必要。

#### 1.1.2 XPath/HTMLノード選択の分岐・重複
- 資産種別や情報源ごとにXPathやHTMLノードの選択が異なり、同じようなパターンが複数箇所に存在。
  - KabutanScraper: 日本株/米国株でXPathが異なる。テーブル行のループ・カラム抽出処理が複数。
  - YahooScraper: 指数/ETF/個別株でXPathが異なる。株価情報や出来高、信用残などのノード抽出が重複。
  - MinkabuScraper: 配当・優待で似たようなテーブルパース処理が複数。

#### 1.1.3 値のパース・変換処理の重複
- パーセント・倍数・日本語単位（兆・億など）の数値変換処理が各Scraperで重複。
  - KabutanScraper: ConvertToDoubleForYield, ConvertToDoubleForPerPbr, ConvertJapaneseNumberToDouble など。
  - YahooScraper: ConvertToDouble, ConvertToDatetime など。
  - MinkabuScraper: ConvertToDoubleForYield。

#### 1.1.4 プロパティセット・データ格納処理の重複
- AssetInfoへの値セット処理が各Scraperでほぼ同じパターンで繰り返されている。
  - 例：stockInfo.DividendYield = ...、stockInfo.ShareholderBenefitYield = ... など。
  - テーブル行ループ→カラム抽出→プロパティセットの流れが各所で重複。

#### 1.1.5 例外処理・ロギングの重複
- try-catchやロギング処理が各メソッドでほぼ同じ形で繰り返されている。
  - 404やHttpRequestException、その他例外のハンドリング。
  - ログ出力（URLやエラー内容）。

#### 1.1.6 資産種別ごとの分岐（if/switch）の多用
- 資産種別（日本株/米国株/ETF/指数など）ごとにifやswitchで分岐し、処理が肥大化。
  - KabutanScraper, YahooScraperで顕著。
  - 今後種別追加時にさらに分岐が増える懸念。

---

## 2. AssetInfoの現状

- AssetInfoは抽象基底クラスであり、継承による拡張が可能。
- 依存性注入（DI）で各種ストラテジーやリポジトリを柔軟に差し替え可能。
- 主要な財務・株価・優待・配当・業績・チャート・開示情報などを網羅的にプロパティとして保持。
- 判定・計算ロジックはストラテジーパターンで委譲されている。

【インライン例（抜粋）】
public abstract class AssetInfo
{
    // 依存性注入
    protected IExternalSourceUpdatable _updater;
    protected IOutputFormattable _formatter;
    protected IAssetRepository _repository;
    protected IAssetJudgementStrategy _judgementStrategy;
    protected IAssetCalculator _calculator;
    protected IAssetSetupStrategy _setupStrategy;
    // ...（略）...
    public virtual string Code { get; set; }
    public virtual string Classification { get; set; }
    public virtual double DividendYield { get; set; }
    public virtual double ShareholderBenefitYield { get; set; }
    // ...（略）...
}

---

## 3. 今後の拡張性確保のためのリファクタリング案

### 3.1 Scraperのインターフェース化＋DI/Factory対応

【サンプル】
public interface IAssetScraper
{
    Task ScrapeAsync(AssetInfo assetInfo, ScrapeTarget target);
}
public enum ScrapeTarget
{
    Dividend, Yutai, Finance, History, Top, Profile, Disclosure
}
- Scraperはこのインターフェースを実装し、DIやFactoryで注入・生成。
- ScrapeTargetで「何を取得するか」を明示的に指定。

---

### 3.2 Scrape戦略の分離（Strategyパターン）

【サンプル】
public interface IAssetScrapeStrategy
{
    string GetUrl(AssetInfo assetInfo, ScrapeTarget target);
    Dictionary<string, string> GetXPaths(ScrapeTarget target);
    void Parse(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target);
}
- 各資産種別ごとにStrategyを実装し、URLやXPath、パースロジックを分離。
- Scraper本体はStrategyに処理を委譲し、種別追加時はStrategy追加のみで済む。

---

### 3.3 AssetInfoの継承による拡張

- USAssetInfo : AssetInfo のように、米国株や新種別に特化したプロパティ・ロジックを追加可能。

---

### 3.4 マージ機構の導入

【サンプル】
public interface IAssetInfoMerger
{
    AssetInfo Merge(IEnumerable<AssetInfo> sources);
}
- 複数サイトから取得した情報をマージするクラスを用意し、優先順位やマージルールを柔軟に拡張可能。

---

### 3.5 DI/Factoryによる柔軟な組み合わせ

【サンプル】
public class ScrapeStrategyFactory
{
    public IAssetScrapeStrategy Create(AssetInfo assetInfo)
    {
        return assetInfo switch
        {
            JapaneseAssetInfo => new JapaneseStockScrapeStrategy(),
            USAssetInfo => new USStockScrapeStrategy(),
            _ => throw new NotSupportedException()
        };
    }
}
- DIコンテナやFactoryでScraperやStrategy、Mergerを組み合わせて注入。

---

## 4. 具体的な拡張例：米国個別株（USAssetInfo）対応

1. USAssetInfo : AssetInfo を追加
2. USStockScrapeStrategy を実装し、URLやXPath、パースロジックを定義
3. Factory/DIで「米国個別株」種別の場合は USStockScrapeStrategy を利用
4. Scraper本体はStrategyに処理を委譲するだけなので、既存コードへの影響は最小限

---

## 5. 結論

- 現状のAssetInfo構造は、継承・DI・ストラテジーパターンを活用することで高い拡張性を持つ。
- Scraper/Strategy/Factory/マージャの導入で、新種別・新サイト・情報マージにも柔軟に対応可能。
- 今後は「派生クラスの追加」「DI/Factoryの拡張」「Strategy/Updaterの実装追加」で、米国株や新種別、複数サイト対応もスムーズに実現できる。

---

## 6. リファクタリングの進め方（段階的な実施案）

本ドキュメントで挙げた課題とリファクタリング案を踏まえ、以下の手順で段階的に進めることを推奨する。

### 6.1 現状の重複・分岐のリストアップ
- 各Scraperで資産種別ごとに分岐している箇所や、重複している処理（URL生成、XPath選択、値のパース、プロパティセット、例外処理など）を洗い出す。
- どこを共通化・分離すべきかを明確にする。

### 6.2 インターフェース・戦略パターンの雛形作成
- IAssetScraperやIAssetScrapeStrategyなど、今後の拡張を見据えたインターフェースや抽象クラスの雛形を新規作成する。

### 6.3 小さな単位でのリファクタリング開始
- まずはURL生成やXPath選択など、影響範囲が小さい部分から共通化・分離を進める。
- 既存のテストや動作確認をしながら、段階的に進める。

### 6.4 Factory/DIの導入
- ScraperやStrategyの生成・注入方法（FactoryやDIコンテナの設定）を検討し、徐々に導入する。

### 6.5 AssetInfoの継承設計
- USAssetInfoなど、今後追加予定の派生クラスの設計方針を決める。
- 必要に応じて、既存のAssetInfoのプロパティやコンストラクタを見直す。

### 6.6 マージ機構の導入
- 複数サイトから取得したAssetInfoをマージするIAssetInfoMergerを実装し、情報統合の仕組みを整備する。

---

#### 補足
- いきなり全体を大きく書き換えるのではなく、「小さな単位で共通化→動作確認→次の単位へ」というサイクルで進めることで、リスクを抑えつつ着実に拡張性を高めることができる。
- 最初のステップとして「重複・分岐のリストアップ」と「インターフェース雛形の作成」から着手することを推奨する。

---