// See https://aka.ms/new-console-template for more information
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using System.Globalization;
using static ExecutionList;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Assets.Repositories;
using ConsoleApp1.Assets.Calculators;
using ConsoleApp1.Assets.Setups;
using System.Threading.Tasks;

namespace ConsoleApp1.Assets;

public abstract class AssetInfo
{
    protected IExternalSourceUpdatable _updater;
    protected IOutputFormattable _formatter;
    protected IAssetRepository _repository;

    // 判定系ストラテジー
    protected IAssetJudgementStrategy _judgementStrategy;

    // Calculatorストラテジー
    protected IAssetCalculator _calculator;

    // セットアップストラテジー
    protected IAssetSetupStrategy _setupStrategy;

    // 生成はFactory経由のみ許可
    internal AssetInfo(WatchList.WatchStock watchStock, AssetInfoDependencies deps)
    {
        this.Code = watchStock.Code;
        this.Classification = watchStock.Classification;
        this.IsFavorite = watchStock.IsFavorite == "1" ? true : false;
        this.Memo = watchStock.Memo;
        this.ScrapedPrices = new List<ScrapedPrice>();
        this.FullYearPerformances = new List<FullYearPerformance>();
        this.FullYearProfits = new List<FullYearProfit>();
        this.QuarterlyPerformances = new List<QuarterlyPerformance>();
        this.FullYearPerformancesForcasts = new List<FullYearPerformanceForcast>();
        this.ChartPrices = new List<ChartPrice>();
        this.Disclosures = new List<Disclosure>();

        // 依存性注入
        this._updater = deps.Updater;
        this._formatter = deps.Formatter;
        this._repository = deps.Repository;
        this._judgementStrategy = deps.JudgementStrategy;
        this._calculator = deps.Calculator;
        this._setupStrategy = deps.SetupStrategy;
    }

    // Repositoryプロパティを公開するためのアクセサを追加
    public IAssetRepository Repository
    {
        get => _repository;
        set => _repository = value;
    }

    /// <summary>
    /// コード
    /// </summary>
    public virtual string Code { get; set; }
    /// <summary>
    /// 名称
    /// </summary>
    public virtual string? Name { get; set; }
    /// <summary>
    /// 価格履歴
    /// </summary>
    public virtual List<ScrapedPrice> ScrapedPrices { get; set; }
    /// <summary>
    /// 最新の価格履歴
    /// </summary>
    public virtual ScrapedPrice LatestScrapedPrice { get; set; }
    /// <summary>
    /// 区分
    /// </summary>
    public virtual string Classification { get; set; }
    /// <summary>
    /// ROE
    /// </summary>
    public virtual double Roe { get; set; }
    /// <summary>
    /// PER
    /// </summary>
    public virtual double Per { get; set; }
    /// <summary>
    /// PBR
    /// </summary>
    public virtual double Pbr { get; set; }
    /// <summary>
    /// 利回り
    /// "3.58%"は"0.0358"で保持。
    /// </summary>
    public virtual double DividendYield { get; set; }
    /// <summary>
    /// 信用倍率
    /// </summary>
    public virtual string MarginBalanceRatio { get; set; }
    /// <summary>
    /// 時価総額
    /// </summary>
    public virtual double MarketCap { get; set; }
    /// <summary>
    /// 通期業績履歴
    /// </summary>
    /// <remarks>
    /// [業績推移]タブの履歴
    /// </remarks>
    public virtual List<FullYearPerformance> FullYearPerformances { get; set; }
    /// <summary>
    /// 通期業績予想履歴
    /// </summary>
    /// <remarks>
    /// [修正履歴]タブの履歴
    /// </remarks>
    public virtual List<FullYearPerformanceForcast> FullYearPerformancesForcasts { get; set; }
    /// <summary>
    /// 通期収益履歴
    /// </summary>
    public virtual List<FullYearProfit> FullYearProfits { get; set; }
    /// <summary>
    /// 約定履歴
    /// </summary>
    public virtual List<ExecutionList.Execution> Executions { get; set; }
    /// <summary>
    /// お気に入りか？
    /// </summary>
    public virtual bool IsFavorite { get; set; }
    /// <summary>
    /// ウォッチリストのメモ
    /// </summary>
    public virtual string Memo { get; set; }
    /// <summary>
    /// 自己資本比率
    /// </summary>
    public virtual string EquityRatio { get; set; }
    /// <summary>
    /// 配当性向
    /// "3.58%"は"0.0358"で保持。
    /// </summary>
    public virtual double DividendPayoutRatio { get; set; }
    /// <summary>
    /// 配当権利確定月
    /// </summary>
    public virtual string DividendRecordDateMonth { get; set; }
    /// <summary>
    /// 優待利回り
    /// "3.58%"は"0.0358"で保持。
    /// </summary>
    public virtual double ShareholderBenefitYield { get; set; }
    /// <summary>
    /// 優待発生株数
    /// </summary>
    public virtual string NumberOfSharesRequiredForBenefits { get; set; }
    /// <summary>
    /// 優待権利確定月
    /// </summary>
    public string ShareholderBenefitRecordMonth { get; set; }
    /// <summary>
    /// 優待内容
    /// </summary>
    public string ShareholderBenefitsDetails { get; set; }
    /// <summary>
    /// 市場区分
    /// </summary>
    public string Section { get; set; }
    /// <summary>
    /// 業種
    /// </summary>
    public string Industry { get; set; }
    /// <summary>
    /// 所属する業種の平均PER
    /// </summary>
    public double AveragePer { get; set; }
    /// <summary>
    /// 所属する業種の平均PBR
    /// </summary>
    public double AveragePbr { get; set; }
    /// <summary>
    /// 決算発表
    /// </summary>
    public string PressReleaseDate { get; set; }
    /// <summary>
    /// 直近の株価
    /// </summary>
    public virtual ChartPrice LatestPrice { get { return this.ChartPrices.Count > 0 ? this.ChartPrices[0] : new ChartPrice(); } }
    /// <summary>
    /// 信用買残
    /// </summary>
    public string MarginBuyBalance { get; internal set; }
    /// <summary>
    /// 直近の出来高
    /// </summary>
    public string LatestTradingVolume { get; internal set; }
    /// <summary>
    /// 信用売残
    /// </summary>
    public string MarginSellBalance { get; internal set; }
    /// <summary>
    /// 信用残更新日付
    /// </summary>
    public string MarginBalanceDate { get; internal set; }
    /// <summary>
    /// 直近の四半期決算期間
    /// </summary>
    public string LastQuarterPeriod { get; internal set; }
    /// <summary>
    /// 四半期通期進捗率
    /// </summary>
    public double QuarterlyFullyearProgressRate { get; internal set; }
    /// <summary>
    /// 四半期実績発表日
    /// </summary>
    public DateTime QuarterlyPerformanceReleaseDate { get; internal set; }
    /// <summary>
    /// 四半期実績履歴
    /// </summary>
    /// <remarks>
    /// [修正履歴]タブの実績
    /// </remarks>
    public List<QuarterlyPerformance> QuarterlyPerformances { get; set; }
    /// <summary>
    /// 優待権利確定日
    /// </summary>
    public object ShareholderBenefitRecordDay { get; set; }
    /// <summary>
    /// 前期通期進捗率
    /// </summary>
    public double PreviousFullyearProgressRate { get; set; }
    /// <summary>
    /// 前期実績発表日
    /// </summary>
    public DateTime PreviousPerformanceReleaseDate { get; set; }
    /// <summary>
    /// 決算時期
    /// </summary>
    public string EarningsPeriod { get; set; }
    /// <summary>
    /// チャート価格（降順）
    /// </summary>
    public List<ChartPrice> ChartPrices { get; set; }
    /// <summary>
    /// 四半期決算実績の前年同期比の経常利益率
    /// </summary>
    public double QuarterlyOperatingProfitMarginYoY { get; set; }
    /// <summary>
    /// カレントの4Q決算月
    /// </summary>
    public DateTime CurrentFiscalMonth { get; set; }
    /// <summary>
    /// 信託報酬率
    /// </summary>
    /// <remarks>
    /// "3.58%"は"0.0358"で保持。
    /// </remarks>
    public double TrustFeeRate { get; set; }
    /// <summary>
    /// 投資信託の運用会社
    /// </summary>
    public string FundManagementCompany { get; set; }
    /// <summary>
    /// 株主資本配当率
    /// </summary>
    /// <remarks>年間配当総額÷株主資本×100 or 配当性向×自己資本利益率(ROE)×100</remarks>
    public double Doe { get; set; }
    /// <summary>
    /// 営業利益率
    /// </summary>
    /// <remarks>営業利益 ÷ 売上高 × 100</remarks>
    public double OperatingProfitMargin { get; set; }
    /// <summary>
    /// 開示情報
    /// </summary>
    public List<Disclosure> Disclosures { get; set; }

    // --- 判定系メソッドはすべてストラテジー委譲に統一 ---

    public virtual bool IsPERUndervalued(bool isLenient = false)
        => _judgementStrategy.IsPERUndervalued(this, isLenient);

    internal virtual bool IsPBRUndervalued(bool isLenient = false)
        => _judgementStrategy.IsPBRUndervalued(this, isLenient);

    internal bool IsROEAboveThreshold()
        => _judgementStrategy.IsROEAboveThreshold(this);

    internal virtual bool IsAnnualProgressOnTrack()
        => _judgementStrategy.IsAnnualProgressOnTrack(this);

    public virtual bool IsHighYield()
        => _judgementStrategy.IsHighYield(this);

    internal virtual bool IsHighMarketCap()
        => _judgementStrategy.IsHighMarketCap(this);

    internal bool IsCloseToDividendRecordDate()
        => _judgementStrategy.IsCloseToDividendRecordDate(this);

    internal bool IsCloseToShareholderBenefitRecordDate()
        => _judgementStrategy.IsCloseToShareholderBenefitRecordDate(this);

    internal virtual bool IsCloseToQuarterEnd()
        => _judgementStrategy.IsCloseToQuarterEnd(this);

    internal virtual bool IsAfterQuarterEnd()
        => _judgementStrategy.IsAfterQuarterEnd(this);

    internal virtual bool IsQuarterEnd()
        => _judgementStrategy.IsQuarterEnd(this);

    internal virtual bool IsJustSold()
        => _judgementStrategy.IsJustSold(this);

    public virtual bool IsOwnedNow()
        => _judgementStrategy.IsOwnedNow(this);

    internal bool IsGoldenCrossPossible()
        => _judgementStrategy.IsGoldenCrossPossible(this);

    internal virtual bool HasRecentStockSplitOccurred()
        => _judgementStrategy.HasRecentStockSplitOccurred(this);

    internal bool ShouldAverageDown(Execution e)
        => _judgementStrategy.ShouldAverageDown(this, e);

    internal virtual bool IsGranvilleCase1Matched()
        => _judgementStrategy.IsGranvilleCase1Matched(this);

    internal virtual bool IsGranvilleCase2Matched()
        => _judgementStrategy.IsGranvilleCase2Matched(this);

    internal virtual bool HasDisclosure()
        => _judgementStrategy.HasDisclosure(this);

    internal virtual bool IsRecordDate()
        => _judgementStrategy.IsRecordDate(this);

    internal virtual bool IsAfterRecordDate()
        => _judgementStrategy.IsAfterRecordDate(this);

    internal virtual bool IsCloseToRecordDate()
        => _judgementStrategy.IsCloseToRecordDate(this);

    internal bool ExtractAndValidateDateWithinOneMonth()
        => _judgementStrategy.ExtractAndValidateDateWithinOneMonth(this);

    // --- 既存のデータ取得・計算・DBアクセス・出力等のメソッドはそのまま残す ---

    public async Task LoadHistoryAsync()
    {
        this.ScrapedPrices = await _repository.LoadHistoryAsync(this.Code);
    }

    public async Task SaveHistoryAsync()
    {
        await _repository.SaveHistoryAsync(this.Code, this.ScrapedPrices);
    }

    public async Task DeleteHistoryAsync(DateTime targetDate)
    {
        await _repository.DeleteHistoryAsync(this.Code, targetDate);
    }

    internal void UpdateFullYearPerformanceForcastSummary()
        => _calculator.UpdateFullYearPerformanceForcastSummary(this);

    internal void UpdateDividendPayoutRatio()
        => _calculator.UpdateDividendPayoutRatio(this);

    internal void UpdateProgress()
        => _calculator.UpdateProgress(this);

    internal void SetupChartPrices()
        => _calculator.SetupChartPrices(this);

    internal async Task SetupAsync()
    {
        // キャッシュ最新化
        await RegisterCacheAsync();

        // 通期予想の更新
        UpdateFullYearPerformancesForcasts();

        // 実績進捗率の更新
        UpdateProgress();

        // 配当性向の更新
        UpdateDividendPayoutRatio();

        // 通期予想のサマリを更新
        UpdateFullYearPerformanceForcastSummary();

        // チャート情報を更新
        SetupChartPrices();
    }

    internal void UpdateExecutions(List<ExecutionList.ListDetail> executionList)
        => _setupStrategy.UpdateExecutions(this, executionList);

    internal void UpdateAveragePerPbr(List<MasterList.AveragePerPbrDetails> masterList)
        => _setupStrategy.UpdateAveragePerPbr(this, masterList);

    private void UpdateFullYearPerformancesForcasts()
    {
        if (this.QuarterlyPerformances.Count < 2) return;

        // 4Q決算の場合、通期予想に前期の修正履歴と最終実績を追加する。
        if (this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter4)
        {
            // 前期の修正履歴を取得
            List<FullYearPerformanceForcast> PreviousForcasts = GetPreviousForcasts();

            short count = 0;
            FullYearPerformanceForcast clone = new FullYearPerformanceForcast();

            foreach (var previous in PreviousForcasts)
            {
                // カウント位置に追加
                FullYearPerformancesForcasts.Insert(count, previous);

                // 最終を退避しておく
                if (count == PreviousForcasts.Count - 1) clone = (FullYearPerformanceForcast)previous.Clone();

                count++;
            }

            // 最終実績を取得
            var p = this.QuarterlyPerformances[this.QuarterlyPerformances.Count - 2];

            FullYearPerformanceForcast f = new FullYearPerformanceForcast()
            {
                FiscalPeriod = string.Empty,
                RevisionDate = ConvertToDateTime(p.ReleaseDate),
                Category = CommonUtils.Instance.ForecastCategoryString.Final,
                RevisionDirection = string.Empty,
                Revenue = p.Revenue,
                OperatingProfit = p.OperatingProfit.ToString(),
                OrdinaryProfit = p.OrdinaryProfit.ToString(),
                NetProfit = p.NetProfit,
                RevisedDividend = p.AdjustedDividendPerShare.ToString(),
                PreviousForcast = clone
            };

            // カウント位置に追加
            FullYearPerformancesForcasts.Insert(count, f);
        }
    }

    private List<FullYearPerformanceForcast> GetPreviousForcasts()
    {
        // カレント決算月が取得できていない場合は空リストを返す
        if (this.CurrentFiscalMonth == DateTime.MinValue) return new List<FullYearPerformanceForcast>();

        // repositoryに取得処理を委譲（IAssetRepositoryにGetPreviousForcastsを追加しておくこと）
        try
        {
            string fiscalPeriod = this.CurrentFiscalMonth.AddYears(-1).ToString("yyyy.MM");
            var result = _repository.GetPreviousForcasts(this.Code, fiscalPeriod);
            return result ?? new List<FullYearPerformanceForcast>();
        }
        catch
        {
            // 例外は無視し、空リストを返す
            return new List<FullYearPerformanceForcast>();
        }
    }

    public virtual DateTime GetLastHistoryUpdateDay()
    {
        // repositoryに処理を委譲する形にリファクタリング
        try
        {
            return _repository.GetLastHistoryUpdateDay(this.Code);
        }
        catch
        {
            // 例外時は従来通りMasterStartDateを返す
            return CommonUtils.Instance.MasterStartDate;
        }
    }

    internal virtual async Task UpdateFromExternalSource()
     => await _updater.UpdateFromExternalSourceAsync(this);

    internal virtual string ToOutputString()
        => _formatter.ToOutputString(this);

    internal virtual bool ShouldAlert()
    {
        // 初期値は通知
        bool result = true;

        // 注目 or 所有している or 売却直後の場合
        if (this.IsFavorite || this.IsOwnedNow() || this.IsJustSold())
        {
            // 強制通知
        }
        //// ゴールデンクロス発生可能性がある場合
        //else if (this.StockInfo.IsGoldenCrossPossible())
        //{
        //    // 利回りが低い場合
        //    if (!this.StockInfo.IsHighYield()) result = false;

        //    // 時価総額が低い場合
        //    if (!this.StockInfo.IsHighMarketCap()) result = false;

        //    // 進捗が良くない場合
        //    if (!this.StockInfo.IsAnnualProgressOnTrack()) result = false;
        //}
        //// グランビルケースに該当する場合
        //else if (this.IsGranvilleCase1Matched() || this.IsGranvilleCase2Matched())
        //{
        //    // 利回りが低い場合
        //    if (!this.IsHighYield()) result = false;

        //    // 時価総額が低い場合
        //    if (!this.IsHighMarketCap()) result = false;
        //}
        // 権利確定月前後の場合
        else if (this.IsCloseToRecordDate() || this.IsRecordDate() || this.IsAfterRecordDate())
        {
            // 利回りが低い場合
            if (!this.IsHighYield()) result = false;

            // 直近で暴落していない場合
            if (!this.LatestPrice.OversoldIndicator()) result = false;

            // 時価総額が低い場合
            if (!this.IsHighMarketCap()) result = false;

            // 進捗が良くない場合
            if (!this.IsAnnualProgressOnTrack()) result = false;
        }
        // 四半期決算前後の場合
        else if (this.IsCloseToQuarterEnd() || this.IsQuarterEnd() || this.IsAfterQuarterEnd())
        {
            // 利回りが低い場合
            if (!this.IsHighYield()) result = false;

            // 直近で暴落していない場合
            if (!this.LatestPrice.OversoldIndicator()) result = false;

            // 時価総額が低い場合
            if (!this.IsHighMarketCap()) result = false;

            // 進捗が良くない場合
            if (!this.IsAnnualProgressOnTrack()) result = false;
        }
        // それ以外
        else
        {
            // 利回りが低い場合
            if (!this.IsHighYield()) result = false;

            // 直近で暴落していない場合
            if (!this.LatestPrice.OversoldIndicator()) result = false;

            // 時価総額が低い場合
            if (!this.IsHighMarketCap()) result = false;

            // 進捗が良くない場合
            if (!this.IsAnnualProgressOnTrack()) result = false;

            // PERが割高の場合
            if (!this.IsPERUndervalued(true)) result = false;

            // PBRが割高の場合
            if (!this.IsPBRUndervalued(true)) result = false;
        }

        return result;
    }

    public static bool IsWithinMonths(string monthsStr, short m)
    {
        try
        {
            if (string.IsNullOrEmpty(monthsStr)) return false;

            // 現在の月を取得
            int currentMonth = DateTime.Now.Month;

            // 月文字列を分割してリストに変換
            string[] monthArray = monthsStr.Split(',');
            List<int> months = new List<int>();

            foreach (string monthStr in monthArray)
            {
                int month = ParseMonth(monthStr.Trim());
                months.Add(month);
            }

            // 当月から指定月数以内の月をリストにする
            List<int> validMonths = new List<int>();
            for (int i = 0; i <= m; i++)
            {
                int validMonth = (currentMonth + i - 1) % 12 + 1;
                validMonths.Add(validMonth);
            }

            // 指定された月が当月から1か月以内に含まれているかを判定
            foreach (int month in months)
            {
                if (validMonths.Contains(month))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// 配当権利確定日か？
    /// </summary>
    internal bool IsDividendRecordDate()
    {
        // TODO
        return false;
    }

    /// <summary>
    /// 優待権利確定日か？
    /// </summary>
    internal bool IsShareholderBenefitRecordDate()
    {
        // TODO
        return false;
    }

    /// <summary>
    /// 配当権利確定日直後か？
    /// </summary>
    internal bool IsAfterDividendRecordDate()
    {
        // TODO
        return false;
    }

    /// <summary>
    /// 優待権利確定日直後か？
    /// </summary>
    /// <returns></returns>
    internal bool IsAfterShareholderBenefitRecordDate()
    {
        // TODO
        return false;
    }

    private static int ParseMonth(string monthStr)
    {
        // "3月"のような文字列から月を抽出
        if (monthStr.EndsWith("月"))
        {
            monthStr = monthStr.Substring(0, monthStr.Length - 1);
        }

        // 月を整数に変換
        return int.Parse(monthStr, CultureInfo.InvariantCulture);
    }

    private DateTime ConvertToDateTime(string releaseDate)
    {
        try
        {
            return DateTime.ParseExact(releaseDate, "yy/MM/dd", CultureInfo.InvariantCulture);
        }
        catch
        {
            return DateTime.Now;
        }
    }

    /// <summary>
    /// キャッシュ（履歴・予想履歴）のDB登録・整理
    /// </summary>
    internal async Task RegisterCacheAsync()
    {
        // 株価履歴の登録（重複チェックはRepository側で実装）
        if (this.ScrapedPrices?.Count > 0)
        {
            await _repository.RegisterHistoryAsync(this.Code, this.ScrapedPrices);
        }

        // 古い履歴の削除
        var deleteBefore = CommonUtils.Instance.ExecusionDate.AddMonths(-1 * CommonUtils.Instance.StockPriceHistoryMonths);
        await _repository.DeleteOldHistoryAsync(this.Code, deleteBefore);

        // 通期予想履歴の登録（重複チェックはRepository側で実装）
        if (this.FullYearPerformancesForcasts?.Count > 0)
        {
            await _repository.RegisterForcastHistoryAsync(this.Code, this.FullYearPerformancesForcasts);
        }
    }
}