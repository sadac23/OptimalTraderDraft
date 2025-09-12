// See https://aka.ms/new-console-template for more information
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using DocumentFormat.OpenXml;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using System.Data.SQLite;
using System.Globalization;
using System.Text.RegularExpressions;
using static ExecutionList;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Assets.Repositories;

namespace ConsoleApp1.Assets;

public abstract class AssetInfo
{
    protected IExternalSourceUpdatable _updater;
    protected IOutputFormattable _formatter;
    protected IAssetRepository _repository;

    protected AssetInfo(WatchList.WatchStock watchStock)
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
    }

    protected AssetInfo(
        WatchList.WatchStock watchStock
        , IExternalSourceUpdatable updater
        , IOutputFormattable formatter
        , IAssetRepository repository)
        : this(watchStock)
    {
        this._updater = updater;
        this._formatter = formatter;
        this._repository = repository;
    }

    // Repositoryプロパティを公開するためのアクセサを追加
    public IAssetRepository Repository => _repository;

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

    /// <summary>
    /// 現在、所有しているか？
    /// </summary>
    /// <remarks>
    /// 買売の株数比較で判定すると、分割時にうまく判定できない。
    /// 約定リストで売り約定が存在していない買い約定が存在しているか否かで判定する。
    /// </remarks>
    public virtual bool IsOwnedNow()
    {
        var result = false;

        foreach (var e in this.Executions)
        {
            if (e.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Buy && !e.HasSellExecuted) result = true;
        }

        return result;
    }

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
    {
        foreach (var p in this.FullYearPerformancesForcasts)
        {
            // 最後から3件前の値（前期）
            var secondLast = this.FullYearPerformances[this.FullYearPerformances.Count - 3];

            // 最終履歴の場合は既に今期の予想が追加されているため、最後から4件前を取得しなおし。
            if (p.Category == CommonUtils.Instance.ForecastCategoryString.Final)
            {
                secondLast = this.FullYearPerformances[this.FullYearPerformances.Count - 4];
            }

            p.Summary += GetRevenueIncreasedSummary(p.Revenue, secondLast.Revenue);
            p.Summary += GetOrdinaryIncomeIncreasedSummary(p.OrdinaryProfit, secondLast.OrdinaryProfit);
            p.Summary += GetDividendPerShareIncreasedSummary(p.RevisedDividend, secondLast.AdjustedDividendPerShare);
            p.Summary += $"（{GetIncreasedRate(p.Revenue, secondLast.Revenue)}";
            p.Summary += $",{GetIncreasedRate(p.OrdinaryProfit, secondLast.OrdinaryProfit)}";
            p.Summary += $",{GetDividendPerShareIncreased(p.RevisedDividend, secondLast.AdjustedDividendPerShare)}）";
        }
    }

    private double GetDividendPayoutRatio(string adjustedDividendPerShare, string adjustedEarningsPerShare)
    {
        try
        {
            // パースに成功したら判定
            if (double.TryParse(adjustedDividendPerShare, out double result1) && double.TryParse(adjustedEarningsPerShare, out double result2))
            {
                return result1 / result2;
            }
            else
            {
                return 0;
            }
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    private string GetDividendPerShareIncreased(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
    {
        try
        {
            // パースに成功したら判定
            if (double.TryParse(adjustedDividendPerShare1, out double result1) && double.TryParse(adjustedDividendPerShare2, out double result2))
            {
                return ConvertToStringWithSign(Math.Floor(result1 - result2));
            }
            else
            {
                return "？";
            }
        }
        catch (Exception ex)
        {
            return "？";
        }
    }

    private string GetIncreasedRate(string lastValue, string secondLastValue)
    {
        try
        {
            // パースに成功したら判定
            if (double.TryParse(lastValue, out double result1) && double.TryParse(secondLastValue, out double result2))
            {
                return ConvertToPercentageStringWithSign(CommonUtils.Instance.CalculateYearOverYearGrowth(result2, result1));
            }
            else
            {
                return "？";
            }
        }
        catch (Exception ex)
        {
            return "？";
        }
    }

    private string ConvertToPercentageStringWithSign(double v)
    {
        double percentage = v * 100;
        if (percentage >= 0)
        {
            return "+" + percentage.ToString("0.0") + "%";
        }
        else
        {
            return percentage.ToString("0.0") + "%";
        }
    }

    private string ConvertToStringWithSign(double number)
    {
        if (number >= 0)
        {
            return "+" + number.ToString();
        }
        else
        {
            return number.ToString();
        }
    }

    private string GetDividendPerShareIncreasedSummary(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
    {
        try
        {
            // パースに成功したら判定
            if (double.TryParse(adjustedDividendPerShare1, out double result1) && double.TryParse(adjustedDividendPerShare2, out double result2))
            {
                if (result1 > result2)
                {
                    return "↑";
                }
                else if (result1 == result2)
                {
                    return "→";
                }
                else
                {
                    return "↓";
                }
            }
            else
            {
                return "？";
            }
        }
        catch (Exception ex)
        {
            return "？";
        }
    }

    private string GetOrdinaryIncomeIncreasedSummary(string ordinaryIncome1, string ordinaryIncome2)
    {
        try
        {
            // パースに成功したら判定
            if (double.TryParse(ordinaryIncome1, out double result1) && double.TryParse(ordinaryIncome2, out double result2))
            {
                if (result1 > result2)
                {
                    return "↑";
                }
                else
                {
                    return "↓";
                }
            }
            else
            {
                return "？";
            }
        }
        catch (Exception ex)
        {
            return "？";
        }
    }

    private string GetRevenueIncreasedSummary(string revenue1, string revenue2)
    {
        try
        {
            // パースに成功したら判定
            if (double.TryParse(revenue1, out double result1) && double.TryParse(revenue2, out double result2))
            {
                if (result1 > result2)
                {
                    return "↑";
                }
                else
                {
                    return "↓";
                }
            }
            else
            {
                return "？";
            }
        }
        catch (Exception ex)
        {
            return "？";
        }
    }

    internal void UpdateDividendPayoutRatio()
    {
        this.DividendPayoutRatio = 0;

        // リストの件数が2件以上あるか確認
        if (this.FullYearPerformances.Count >= 2)
        {
            // 最後から2件前の値（今期）
            var lastValue = this.FullYearPerformances[this.FullYearPerformances.Count - 2];

            //  今期レコードより配当性向を算出
            this.DividendPayoutRatio = GetDividendPayoutRatio(lastValue.AdjustedDividendPerShare, lastValue.AdjustedEarningsPerShare);

            //  今期レコードより株主資本配当率（DOE）を算出
            this.Doe = GetDOE(this.DividendPayoutRatio, this.FullYearProfits.Last().Roe);
        }
    }

    private double GetDOE(double dividendPayoutRatio, double roe)
    {
        double result = 0;
        result = dividendPayoutRatio * (roe / 100);
        return result;
    }

    internal void UpdateExecutions(List<ExecutionList.ListDetail> executionList)
    {
        // 日付でソートして約定履歴を格納
        this.Executions = ExecutionList.GetExecutions(executionList, this.Code).OrderBy(e => e.Date).ToList();
    }

    internal void UpdateAveragePerPbr(List<MasterList.AveragePerPbrDetails> masterList)
    {
        try
        {
            // 市場区分(マスタ, 外部サイト)
            Dictionary<string, string> sectionTable = new Dictionary<string, string>
        {
            { "スタンダード市場", "東証Ｓ" },
            { "プライム市場", "東証Ｐ" },
            { "グロース市場", "東証Ｇ" },
        };

            // 種別(マスタ, 外部サイト)
            Dictionary<string, string> industryTable = new Dictionary<string, string>
        {
            { "1 水産・農林業", "水産・農林業" },
            { "2 鉱業", "鉱業" },
            { "3 建設業", "建設業" },
            { "4 食料品", "食料品" },
            { "5 繊維製品", "繊維製品" },
            { "6 パルプ・紙", "パルプ・紙" },
            { "7 化学", "化学" },
            { "8 医薬品", "医薬品" },
            { "9 石油・石炭製品", "石油・石炭製品" },
            { "10 ゴム製品", "ゴム製品" },
            { "11 ガラス・土石製品", "ガラス・土石" },
            { "12 鉄鋼", "鉄鋼" },
            { "13 非鉄金属", "非鉄金属" },
            { "14 金属製品", "金属製品" },
            { "15 機械", "機械" },
            { "16 電気機器", "電気機器" },
            { "17 輸送用機器", "輸送用機器" },
            { "18 精密機器", "精密機器" },
            { "19 その他製品", "その他製品" },
            { "20 電気・ガス業", "電気・ガス" },
            { "21 陸運業", "陸運業" },
            { "22 海運業", "海運業" },
            { "23 空運業", "空運業" },
            { "24 倉庫・運輸関連業", "倉庫・運輸" },
            { "25 情報・通信業", "情報・通信" },
            { "26 卸売業", "卸売業" },
            { "27 小売業", "小売業" },
            { "28 銀行業", "銀行業" },
            { "29 証券、商品先物取引業", "証券" },
            { "30 保険業", "保険" },
            { "31 その他金融業", "その他金融" },
            { "32 不動産業", "不動産" },
            { "33 サービス業", "サービス" },
        };

            foreach (var details in masterList)
            {
                bool sectionMatching = false;
                bool industryMatching = false;

                if (sectionTable.ContainsKey(details.Section))
                {
                    // プロパ側の値に変換テーブルのvalueが含まれているか？
                    if (!string.IsNullOrEmpty(this.Section) && this.Section.Contains(sectionTable[details.Section]))
                    {
                        sectionMatching = true;
                    }
                }

                if (industryTable.ContainsKey(details.Industry))
                {
                    // プロパ側の値に変換テーブルのvalueが含まれているか？
                    if (!string.IsNullOrEmpty(this.Industry) && this.Industry.Contains(industryTable[details.Industry]))
                    {
                        industryMatching = true;
                    }
                }

                if (sectionMatching && industryMatching)
                {
                    this.AveragePer = CommonUtils.Instance.GetDouble(details.AveragePer);
                    this.AveragePbr = CommonUtils.Instance.GetDouble(details.AveragePbr);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    /// <summary>
    /// 配当権利確定日が近いか？
    /// </summary>
    /// <remarks>配当権利確定日が当月以内の場合にtrueを返す。</remarks>
    internal bool IsCloseToDividendRecordDate()
    {
        return IsWithinMonths(this.DividendRecordDateMonth, 0);
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
    /// <summary>
    /// PERが割安か？
    /// </summary>
    /// <param name="isLenient">緩めに判定するか？</param>
    public virtual bool IsPERUndervalued(bool isLenient = false)
    {
        bool result = false;
        double threshold = isLenient ? this.AveragePer * CommonUtils.Instance.LenientFactor : this.AveragePer;
        if (this.Per > 0 && this.Per < threshold) result = true;
        return result;
    }

    /// <summary>
    /// PBRが割安か？
    /// </summary>
    internal virtual bool IsPBRUndervalued(bool isLenient = false)
    {
        bool result = false;
        double threshold = isLenient ? this.AveragePbr * CommonUtils.Instance.LenientFactor : this.AveragePbr;
        if (this.Pbr > 0 && this.Pbr < threshold) result = true;
        return result;
    }
    /// <summary>
    /// 最新のROE予想が基準値以上か？
    /// </summary>
    internal bool IsROEAboveThreshold()
    {
        return (this.FullYearProfits[this.FullYearProfits.Count - 1].Roe >= CommonUtils.Instance.ThresholdOfROE ? true : false);
    }
    internal void UpdateProgress()
    {
        double fullYearOrdinaryIncome = 0;
        double latestOrdinaryIncome = 0;
        double previousOrdinaryIncome = 0;

        // ** 当期進捗率

        // 四半期実績の取得
        var quarterlyPerformance = GetQuarterlyPerformance(CommonUtils.Instance.PeriodString.Current);

        // 通期予想の取得
        var fullYearPerformance = GetFullYearForecast(CommonUtils.Instance.PeriodString.Current);

        // 発表日の取得
        this.QuarterlyPerformanceReleaseDate = ConvertToDateTime(quarterlyPerformance.ReleaseDate);

        latestOrdinaryIncome = quarterlyPerformance.OrdinaryProfit;
        fullYearOrdinaryIncome = CommonUtils.Instance.GetDouble(fullYearPerformance.OrdinaryProfit);

        if (fullYearOrdinaryIncome > 0)
        {
            this.QuarterlyFullyearProgressRate = latestOrdinaryIncome / fullYearOrdinaryIncome;
        }

        // 営業利益率の算出
        var revenue = CommonUtils.Instance.GetDouble(quarterlyPerformance.Revenue);
        var operatingProfit = CommonUtils.Instance.GetDouble(quarterlyPerformance.OperatingProfit);
        this.OperatingProfitMargin = operatingProfit / revenue;

        // ** 前期進捗率

        // 四半期実績の取得
        var previousQuarterlyPerformance = GetQuarterlyPerformance(CommonUtils.Instance.PeriodString.Previous);

        // 通期予想の取得
        var previousFullYearPerformance = GetFullYearForecast(CommonUtils.Instance.PeriodString.Previous);

        // 発表日の取得
        this.PreviousPerformanceReleaseDate = ConvertToDateTime(previousQuarterlyPerformance.ReleaseDate);

        previousOrdinaryIncome = previousQuarterlyPerformance.OrdinaryProfit;
        fullYearOrdinaryIncome = CommonUtils.Instance.GetDouble(previousFullYearPerformance.OrdinaryProfit);

        if (fullYearOrdinaryIncome > 0)
        {
            this.PreviousFullyearProgressRate = previousOrdinaryIncome / fullYearOrdinaryIncome;
        }

        // ** 前年同期比の経常利益率を算出
        this.QuarterlyOperatingProfitMarginYoY = CommonUtils.Instance.CalculateYearOverYearGrowth(previousOrdinaryIncome, latestOrdinaryIncome);
    }

    private QuarterlyPerformance GetQuarterlyPerformance(string period)
    {
        QuarterlyPerformance result = new QuarterlyPerformance();

        // 今期は2、前期は3
        var refIndex = period == CommonUtils.Instance.PeriodString.Current ? 2 : 3;

        // *当期進捗率
        if (this.QuarterlyPerformances.Count >= refIndex)
        {
            result = this.QuarterlyPerformances[this.QuarterlyPerformances.Count - refIndex];
        }

        return result;
    }

    /// <summary>
    /// 通期予想の取得
    /// </summary>
    private FullYearPerformance GetFullYearForecast(string periodString)
    {
        FullYearPerformance result = new FullYearPerformance();

        // 今期は2、前期は3
        var refIndex = periodString == CommonUtils.Instance.PeriodString.Current ? 2 : 3;

        // 通期進捗率の算出
        if (this.FullYearPerformances.Count >= refIndex)
        {
            // TODO: Q4の時は既に来期の予想しか取得できないため、キャッシュから取得する必要がある。（現状は2件前の通期実績を格納している。よって常に100%になる。）
            //refIndex = this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter4 ? refIndex + 1 : refIndex;
            //result = this.FullYearPerformances[this.FullYearPerformances.Count - refIndex];

            // TODO: Q4の時は既に来期の予想しか取得できないため、キャッシュから取得する必要がある。（現状は2件前の通期実績を格納している。よって常に100%になる。）
            if (this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter4)
            {
                var previousFinalForcast = this.FullYearPerformancesForcasts.Where(forcast =>
                forcast.FiscalPeriod == this.CurrentFiscalMonth.AddYears(-1).ToString("yyyy.MM")
                && (forcast.Category == CommonUtils.Instance.ForecastCategoryString.Initial
                || forcast.Category == CommonUtils.Instance.ForecastCategoryString.Revised)).LastOrDefault();

                if (previousFinalForcast != null)
                {
                    result.FiscalPeriod = previousFinalForcast.FiscalPeriod;
                    result.Revenue = previousFinalForcast.Revenue;
                    result.OperatingProfit = previousFinalForcast.OperatingProfit;
                    result.OrdinaryProfit = previousFinalForcast.OrdinaryProfit;
                    result.NetProft = previousFinalForcast.NetProfit;
                    result.AdjustedEarningsPerShare = string.Empty;
                    result.AdjustedDividendPerShare = previousFinalForcast.RevisedDividend;
                    result.AnnouncementDate = previousFinalForcast.RevisionDate.ToString();
                }
                else
                {
                    result = this.FullYearPerformances[this.FullYearPerformances.Count - (refIndex + 1)];
                }
            }
            else
            {
                result = this.FullYearPerformances[this.FullYearPerformances.Count - refIndex];
            }
        }

        return result;
    }

    private DateTime ConvertToDateTime(string releaseDate)
    {
        try
        {
            // DateTimeオブジェクトに変換
            return DateTime.ParseExact(releaseDate, "yy/MM/dd", CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return DateTime.Now;
        }
    }
    /// <summary>
    /// 通期進捗が順調か？
    /// </summary>
    internal virtual bool IsAnnualProgressOnTrack()
    {
        bool result = false;

        // 前期以上かつ、進捗良好の判定基準値以上か？
        if (this.QuarterlyFullyearProgressRate >= this.PreviousFullyearProgressRate)
        {
            if (this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter1)
            {
                if (this.QuarterlyFullyearProgressRate >= CommonUtils.Instance.ThresholdOfProgressSuccess.Q1) result = true;
            }
            else if (this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter2)
            {
                if (this.QuarterlyFullyearProgressRate >= CommonUtils.Instance.ThresholdOfProgressSuccess.Q2) result = true;
            }
            else if (this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter3)
            {
                if (this.QuarterlyFullyearProgressRate >= CommonUtils.Instance.ThresholdOfProgressSuccess.Q3) result = true;
            }
            else if (this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter4)
            {
                if (this.QuarterlyFullyearProgressRate >= CommonUtils.Instance.ThresholdOfProgressSuccess.Q4) result = true;
            }
        }

        return result;
    }
    /// <summary>
    /// 利回りが高いか？
    /// </summary>
    public virtual bool IsHighYield()
    {
        bool result = false;

        if ((this.DividendYield + this.ShareholderBenefitYield) > CommonUtils.Instance.ThresholdOfYield) result = true;

        return result;
    }

    /// <summary>
    /// 時価総額が高いか？
    /// </summary>
    internal virtual bool IsHighMarketCap()
    {
        bool result = false;
        if (this.MarketCap > CommonUtils.Instance.ThresholdOfMarketCap) result = true;
        return result;
    }

    /// <summary>
    /// 優待権利確定日が近いか？
    /// </summary>
    /// <remarks>優待権利確定日が当月以内の場合にtrueを返す。</remarks>
    internal bool IsCloseToShareholderBenefitRecordDate()
    {
        return IsWithinMonths(this.ShareholderBenefitRecordMonth, 0);
    }

    internal bool ExtractAndValidateDateWithinOneMonth()
    {
        if (string.IsNullOrEmpty(this.PressReleaseDate)) return false;

        // 正規表現を使用して日付を抽出
        var datePattern = @"\d{4}年\d{1,2}月\d{1,2}日";
        var match = Regex.Match(this.PressReleaseDate, datePattern);

        if (match.Success)
        {
            // 抽出した日付をDateTimeに変換
            if (DateTime.TryParseExact(match.Value, "yyyy年M月d日", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime extractedDate))
            {
                // 指定日付から前後1か月以内かを判定
                var oneMonthBefore = CommonUtils.Instance.ExecusionDate.AddMonths(-1);
                var oneMonthAfter = CommonUtils.Instance.ExecusionDate.AddMonths(1);

                return extractedDate >= oneMonthBefore && extractedDate <= oneMonthAfter;
            }
        }

        // 日付が抽出できなかった場合、または変換に失敗した場合はfalseを返す
        return false;
    }

    /// <summary>
    /// 対象約定はナンピンするべきか？
    /// </summary>
    internal bool ShouldAverageDown(Execution e)
    {
        var result = false;

        // 売却済でない買約定
        if (e.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Buy && !e.HasSellExecuted)
        {
            // 変動率がナンピン閾値以下の場合
            if (((this.LatestPrice.Price / e.Price) - 1) <= CommonUtils.Instance.ThresholdOfAverageDown)
            {
                result = true;
            }
        }

        return result;
    }

    internal void SetupChartPrices()
    {
        var analizer = new Analyzer();

        // Repository経由でチャート用履歴データを取得
        var rows = _repository.GetChartPriceRows(this.Code, CommonUtils.Instance.ChartDays + 1);

        // rowsに直近営業日の日付のデータが存在しなかった場合、rowsにスクレイピングした最新の株価データを追加する
        if (this.LatestScrapedPrice != null &&
            !rows.Any(r => ((DateTime)r["date"]) == this.LatestScrapedPrice.Date))
        {
            var latestRow = new Dictionary<string, object>
            {
                { "code", this.Code },
                { "date", this.LatestScrapedPrice.Date },
                { "open", this.LatestScrapedPrice.Open },
                { "high", this.LatestScrapedPrice.High },
                { "low", this.LatestScrapedPrice.Low },
                { "close", this.LatestScrapedPrice.Close },
                { "volume", this.LatestScrapedPrice.Volume }
            };
            rows.Add(latestRow);
        }

        // 昇順に並び替え
        rows.Sort((a, b) => ((DateTime)a["date"]).CompareTo((DateTime)b["date"]));

        ChartPrice previousPrice = null;
        List<ChartPrice> prices = new List<ChartPrice>();

        foreach (var row in rows)
        {
            string code = row["code"].ToString();
            DateTime date = (DateTime)row["date"];
            string dateString = date.ToString("yyyyMMdd");
            double open = Convert.ToDouble(row["open"]);
            double high = Convert.ToDouble(row["high"]);
            double low = Convert.ToDouble(row["low"]);
            double close = Convert.ToDouble(row["close"]);
            double volume = Convert.ToDouble(row["volume"]);

            double sma25 = Analyzer.GetSMA(25, date, this.Code);
            double sma75 = Analyzer.GetSMA(75, date, this.Code);

            ChartPrice price = new ChartPrice()
            {
                Date = date,
                Price = close,
                Volatility = previousPrice != null ? (close / previousPrice.Price) - 1 : 0,
                RSIL = analizer.GetCutlerRSI(CommonUtils.Instance.RSILongPeriodDays, date, this),
                RSIS = analizer.GetCutlerRSI(CommonUtils.Instance.RSIShortPeriodDays, date, this),
                SMA25 = sma25,
                SMA75 = sma75,
                SMAdev = sma25 - sma75,
                MADS = (close - sma25) / sma25,
                MADL = (close - sma75) / sma75,
            };

            prices.Add(price);
            previousPrice = (ChartPrice)price.Clone();
        }

        // 件数を絞って日付降順でソート
        this.ChartPrices = prices.OrderByDescending(p => p.Date).Take(CommonUtils.Instance.ChartDays).ToList();
    }

    /// <summary>
    /// 四半期決算日が近いか？
    /// </summary>
    internal virtual bool IsCloseToQuarterEnd()
    {
        bool result = false;

        if (string.IsNullOrEmpty(this.PressReleaseDate)) return result;

        // 正規表現を使用して日付を抽出
        var datePattern = @"\d{4}年\d{1,2}月\d{1,2}日";
        var match = Regex.Match(this.PressReleaseDate, datePattern);

        if (match.Success)
        {
            // 抽出した日付をDateTimeに変換
            if (DateTime.TryParseExact(match.Value, "yyyy年M月d日", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime extractedDate))
            {
                // 指定日付から閾値日数以内かを判定
                // 実行日（before） < 決算日（extracted） <= 閾値日数(after)
                var oneMonthBefore = CommonUtils.Instance.ExecusionDate;
                var oneMonthAfter = CommonUtils.Instance.ExecusionDate.AddDays(CommonUtils.Instance.ThresholdOfDaysToQuarterEnd);
                result = extractedDate > oneMonthBefore && extractedDate <= oneMonthAfter;
            }
        }

        return result;
    }

    /// <summary>
    /// 売却直後か？
    /// </summary>
    internal virtual bool IsJustSold()
    {
        bool result = false;

        foreach (var execution in Executions)
        {
            if (execution.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Sell
                && execution.Date >= CommonUtils.Instance.ExecusionDate.AddDays(-1 * CommonUtils.Instance.ThresholdOfDaysJustSold))
            {
                result = true;
            }
        }

        return result;
    }

    /// <summary>
    /// 四半期決算直後であるか？
    /// </summary>
    internal virtual bool IsAfterQuarterEnd()
    {
        bool result = false;

        if (string.IsNullOrEmpty(this.PressReleaseDate)) return result;

        // 正規表現を使用して日付を抽出
        var datePattern = @"\d{4}年\d{1,2}月\d{1,2}日";
        var match = Regex.Match(this.PressReleaseDate, datePattern);

        if (match.Success)
        {
            // 抽出した日付をDateTimeに変換
            if (DateTime.TryParseExact(match.Value, "yyyy年M月d日", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime extractedDate))
            {
                // 指定日付より閾値日数以内かを判定
                // 閾値日数（before） <= 決算日（extracted） < 実行日(after)
                var oneMonthBefore = CommonUtils.Instance.ExecusionDate.AddDays(CommonUtils.Instance.ThresholdOfDaysFromQuarterEnd * -1);
                var oneMonthAfter = CommonUtils.Instance.ExecusionDate;
                result = extractedDate >= oneMonthBefore && extractedDate < oneMonthAfter;
            }
        }

        return result;
    }

    /// <summary>
    /// 四半期決算当日であるか？
    /// </summary>
    internal virtual bool IsQuarterEnd()
    {
        bool result = false;

        if (string.IsNullOrEmpty(this.PressReleaseDate)) return result;

        // 正規表現を使用して日付を抽出
        var datePattern = @"\d{4}年\d{1,2}月\d{1,2}日";
        var match = Regex.Match(this.PressReleaseDate, datePattern);

        if (match.Success)
        {
            // 抽出した日付をDateTimeに変換
            if (DateTime.TryParseExact(match.Value, "yyyy年M月d日", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime extractedDate))
            {
                // 指定日付と一致するかを判定
                result = CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd") == extractedDate.ToString("yyyyMMdd");
            }
        }

        return result;
    }

    /// <summary>
    /// 情報更新
    /// </summary>
    internal void Setup()
    {
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

    /// <summary>
    /// 通期予想履歴の更新
    /// </summary>
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

    /// <summary>
    /// 前期の通期予測を取得する（リポジトリパターン対応）
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 権利確定日直前か？
    /// </summary>
    internal virtual bool IsCloseToRecordDate()
    {
        bool result = false;

        // 配当
        if (this.IsCloseToDividendRecordDate()) result = true;

        // 優待
        if (this.IsCloseToShareholderBenefitRecordDate()) result = true;

        return result;
    }

    /// <summary>
    /// 権利確定日当日か？
    /// </summary>
    /// <returns></returns>
    internal virtual bool IsRecordDate()
    {
        bool result = false;

        // 配当
        if (this.IsDividendRecordDate()) result = true;

        // 優待
        if (this.IsShareholderBenefitRecordDate()) result = true;

        return result;
    }

    /// <summary>
    /// 権利確定日直後か？
    /// </summary>
    /// <returns></returns>
    internal virtual bool IsAfterRecordDate()
    {
        bool result = false;

        // 配当
        if (this.IsAfterDividendRecordDate()) result = true;

        // 優待
        if (this.IsAfterShareholderBenefitRecordDate()) result = true;

        return result;
    }

    /// <summary>
    /// ゴールデンクロス発生可能性があるか？
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// 短期/長期の移動平均乖離値がマイナスから0に近づいているかを判定する。
    /// </remarks>
    internal bool IsGoldenCrossPossible()
    {
        bool result = true;

        // TODO
        //        if (this.IsDivergenceDecreasing(this.ChartPrices, 3)) result = true;

        double tempSMAdev = 0;
        short count = 0;

        // 最新から遡って判定
        foreach (var item in this.ChartPrices)
        {
            // 判定日数を超過した場合は終了
            if (count > 7) break;

            // 既に発生している場合
            if (item.SMAdev > 0) result = false;

            // 2件目以降
            if (count > 0)
            {
                // 遡って乖離値が減少している場合（SMA25が上昇していない場合）
                if (item.SMAdev >= tempSMAdev) result = false;
            }

            tempSMAdev = item.SMAdev;

            count++;
        }

        return result;
    }

    /// <summary>
    /// 直近で株式分割が実施されたか？
    /// </summary>
    /// <returns></returns>
    internal virtual bool HasRecentStockSplitOccurred()
    {
        bool result = false;

        foreach (var item in this.ScrapedPrices)
        {
            // 調整後終値と異なる場合は分割実施と判断
            if (item.Close != item.AdjustedClose)
            {
                result = true;

                CommonUtils.Instance.Logger.LogInformation($"" +
                    $"Code:{this.Code}, " +
                    $"Message:株式分割あり（" +
                    $"日付：{item.Date.ToString("yyyyMMdd")}, " +
                    $"終値：{item.Close}, " +
                    $"調整後終値：{item.AdjustedClose}）");

                break;
            }
        }

        return result;
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

    /// <summary>
    /// 外部サイトからの取得情報を埋める
    /// </summary>
    /// <remarks>各サイトの情報を非同期で取得してインスタンスに設定する。</remarks>
    internal virtual async Task UpdateFromExternalSource()
     => await _updater.UpdateFromExternalSourceAsync(this);

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

    internal virtual bool IsGranvilleCase2Matched()
    {
        bool result = false;

        if (this.ChartPrices.Count == 0) return result;

        var start = this.ChartPrices.First().Price - this.ChartPrices.First().SMA25;
        var end = this.ChartPrices.Last().Price - this.ChartPrices.Last().SMA25;

        // 乖離値の開始がプラス、終了がプラスの場合
        if (start > 0 && end > 0)
        {
            foreach (var item in this.ChartPrices)
            {
                var dev = item.Price - item.SMA25;

                // マイナスが存在する場合
                if (dev < 0) result = true;
            }
        }

        return result;
    }

    /// <summary>
    /// 開示情報があるか？
    /// </summary>
    internal virtual bool HasDisclosure()
    {
        bool result = false;
        foreach (var item in this.Disclosures)
        {
            if (item.Datetime.ToString("yyyyMMdd") == CommonUtils.Instance.ExecusionDate.ToString("yyyyMMdd"))
            {
                result = true;
            }
        }
        return result;
    }

    internal static AssetInfo GetInstance(WatchList.WatchStock watchStock)
    {
        if ((watchStock.Classification == CommonUtils.Instance.Classification.JapaneseETFs))
        {
            return new JapaneseETFInfo(
                watchStock,
                new JapaneseETFUpdater(),
                new JapaneseETFFormatter(),
                new AssetRepository());
        }
        else if ((watchStock.Classification == CommonUtils.Instance.Classification.Indexs))
        {
            return new IndexInfo(
                watchStock,
                new IndexUpdater(),
                new IndexFormatter(),
                new AssetRepository());
        }
        else
        {
            return new JapaneseStockInfo(
                watchStock,
                new JapaneseStockUpdater(),
                new JapaneseStockFormatter(),
                new AssetRepository());
        }
    }

    internal virtual string ToOutputString()
        => _formatter.ToOutputString(this);

    internal virtual bool IsGranvilleCase1Matched()
    {
        bool result = false;

        if (this.ChartPrices.Count == 0) return result;

        var start = this.ChartPrices.First().Price - this.ChartPrices.First().SMA25;
        var end = this.ChartPrices.Last().Price - this.ChartPrices.Last().SMA25;

        // 乖離値の開始がマイナス、終了がプラスの場合
        if (start < 0 && end > 0) result = true;

        return result;
    }
}


