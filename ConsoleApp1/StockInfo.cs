// See https://aka.ms/new-console-template for more information
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SQLite;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static ExecutionList;
using static StockInfo;

internal class StockInfo
{
    public StockInfo(WatchList.WatchStock watchStock)
    {
        this.Code = watchStock.Code;
        this.Classification = watchStock.Classification;
        this.IsFavorite = watchStock.IsFavorite == "1" ? true : false;
        this.Memo = watchStock.Memo;
        this.ScrapedPrices = new List<StockInfo.ScrapedPrice>();
        this.FullYearPerformances = new List<StockInfo.FullYearPerformance>();
        this.FullYearProfits = new List<FullYearProfit>();
        this.QuarterlyPerformances = new List<StockInfo.QuarterlyPerformance>();
        this.FullYearPerformancesForcasts = new List<FullYearPerformanceForcast>();
        this.ChartPrices = new List<StockInfo.ChartPrice>();
    }
    /// <summary>
    /// コード
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 価格履歴
    /// </summary>
    public List<ScrapedPrice> ScrapedPrices { get; set; }
    /// <summary>
    /// 区分
    /// </summary>
    public string Classification { get; set; }
    /// <summary>
    /// ROE
    /// </summary>
    public double Roe { get; internal set; }
    /// <summary>
    /// PER
    /// </summary>
    public double Per { get; internal set; }
    /// <summary>
    /// PBR
    /// </summary>
    public double Pbr { get; internal set; }
    /// <summary>
    /// 利回り
    /// "3.58%"は"0.0358"で保持。
    /// </summary>
    public double DividendYield { get; internal set; }
    /// <summary>
    /// 信用倍率
    /// </summary>
    public string MarginBalanceRatio { get; internal set; }
    /// <summary>
    /// 時価総額
    /// </summary>
    public double MarketCap { get; internal set; }
    /// <summary>
    /// 通期業績履歴
    /// </summary>
    /// <remarks>
    /// [業績推移]タブの履歴
    /// </remarks>
    public List<FullYearPerformance> FullYearPerformances { get; internal set; }
    /// <summary>
    /// 通期業績予想概要（例：増収増益増配（+50%,+50%,+50））
    /// </summary>
    public string FullYearPerformanceForcastSummary { get; internal set; }
    /// <summary>
    /// 通期業績予想履歴
    /// </summary>
    /// <remarks>
    /// [修正履歴]タブの履歴
    /// </remarks>
    public List<FullYearPerformanceForcast> FullYearPerformancesForcasts { get; internal set; }
    /// <summary>
    /// 通期収益履歴
    /// </summary>
    public List<FullYearProfit> FullYearProfits { get; internal set; }
    /// <summary>
    /// 約定履歴
    /// </summary>
    public List<ExecutionList.Execution> Executions { get; set; }
    /// <summary>
    /// お気に入りか？
    /// </summary>
    public bool IsFavorite { get; set; }
    /// <summary>
    /// ウォッチリストのメモ
    /// </summary>
    public string Memo { get; private set; }
    /// <summary>
    /// 自己資本比率
    /// </summary>
    public string EquityRatio { get; internal set; }
    /// <summary>
    /// 配当性向
    /// "3.58%"は"0.0358"で保持。
    /// </summary>
    public double DividendPayoutRatio { get; internal set; }
    /// <summary>
    /// 配当権利確定月
    /// </summary>
    public string DividendRecordDateMonth { get; internal set; }
    /// <summary>
    /// 優待利回り
    /// "3.58%"は"0.0358"で保持。
    /// </summary>
    public double ShareholderBenefitYield { get; internal set; }
    /// <summary>
    /// 優待発生株数
    /// </summary>
    public string NumberOfSharesRequiredForBenefits { get; internal set; }
    /// <summary>
    /// 優待権利確定月
    /// </summary>
    public string ShareholderBenefitRecordMonth { get; internal set; }
    /// <summary>
    /// 優待内容
    /// </summary>
    public string ShareholderBenefitsDetails { get; internal set; }
    /// <summary>
    /// 市場区分
    /// </summary>
    public string Section { get; internal set; }
    /// <summary>
    /// 業種
    /// </summary>
    public string Industry { get; internal set; }
    /// <summary>
    /// 所属する業種の平均PER
    /// </summary>
    public double AveragePer { get; private set; }
    /// <summary>
    /// 所属する業種の平均PBR
    /// </summary>
    public double AveragePbr { get; private set; }
    /// <summary>
    /// 決算発表
    /// </summary>
    public string PressReleaseDate { get; internal set; }
    /// <summary>
    /// 直近の株価
    /// </summary>
    public ChartPrice LatestPrice { get { return this.ChartPrices.Count > 0 ? this.ChartPrices[0] : new ChartPrice(); } }
    /// <summary>
    /// 直近の株価日付
    /// </summary>
//    public DateTime LatestPriceDate { get; internal set; }
    /// <summary>
    /// 信用買残
    /// </summary>
    public object MarginBuyBalance { get; internal set; }
    /// <summary>
    /// 直近の出来高
    /// </summary>
    public object LatestTradingVolume { get; internal set; }
    /// <summary>
    /// 信用売残
    /// </summary>
    public object MarginSellBalance { get; internal set; }
    /// <summary>
    /// 信用残更新日付
    /// </summary>
    public object MarginBalanceDate { get; internal set; }
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
    public List<QuarterlyPerformance> QuarterlyPerformances { get; internal set; }
    /// <summary>
    /// 優待権利確定日
    /// </summary>
    public object ShareholderBenefitRecordDay { get; internal set; }
    /// <summary>
    /// 前期通期進捗率
    /// </summary>
    public double PreviousFullyearProgressRate { get; internal set; }
    /// <summary>
    /// 前期実績発表日
    /// </summary>
    public DateTime PreviousPerformanceReleaseDate { get; internal set; }
    /// <summary>
    /// 決算時期
    /// </summary>
    public string EarningsPeriod { get; internal set; }
    /// <summary>
    /// チャート価格（降順）
    /// </summary>
    public List<ChartPrice> ChartPrices { get; internal set; }
    /// <summary>
    /// 四半期決算実績の前年同期比の経常利益率
    /// </summary>
    public double QuarterlyOperatingProfitMarginYoY { get; internal set; }
    /// <summary>
    /// カレントの4Q決算月
    /// </summary>
    public DateTime CurrentFiscalMonth { get; internal set; }

    /// <summary>
    /// 現在、所有しているか？
    /// </summary>
    /// <remarks>
    /// 買売の株数比較で判定すると、分割時にうまく判定できない。
    /// 約定リストで売り約定が存在していない買い約定が存在しているか否かで判定する。
    /// </remarks>
    internal bool IsOwnedNow()
    {
        var result = false;

        //double buy = 0;
        //double sell = 0;

        //foreach (var execution in Executions)
        //{
        //    if (execution.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Buy) buy += execution.Quantity;
        //    if (execution.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Sell) sell += execution.Quantity;
        //}
        //if (buy > sell) result = true;

        foreach (var e in this.Executions)
        {
            if (e.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Buy && !e.HasSellExecuted) result = true;
        }

        return result;
    }

    /// <summary>
    /// 通期予想のサマリを更新する
    /// </summary>
    internal void UpdateFullYearPerformanceForcastSummary()
    {
        this.FullYearPerformanceForcastSummary = string.Empty;

        // リストの件数が3件以上あるか確認
        if (this.FullYearPerformances.Count >= 3)
        {
            // 最後から3件前の値（前期）
            var secondLastValue = this.FullYearPerformances[this.FullYearPerformances.Count - 3];
            // 最後から2件前の値（今期）
            var lastValue = this.FullYearPerformances[this.FullYearPerformances.Count - 2];

            // "増収増益増配（+50%,+50%,+50）"
            this.FullYearPerformanceForcastSummary += GetRevenueIncreasedSummary(lastValue.Revenue, secondLastValue.Revenue);
            this.FullYearPerformanceForcastSummary += GetOrdinaryIncomeIncreasedSummary(lastValue.OrdinaryProfit, secondLastValue.OrdinaryProfit);
            this.FullYearPerformanceForcastSummary += GetDividendPerShareIncreasedSummary(lastValue.AdjustedDividendPerShare, secondLastValue.AdjustedDividendPerShare);
            this.FullYearPerformanceForcastSummary += $"（{GetIncreasedRate(lastValue.Revenue, secondLastValue.Revenue)}";
            this.FullYearPerformanceForcastSummary += $",{GetIncreasedRate(lastValue.OrdinaryProfit, secondLastValue.OrdinaryProfit)}";
            this.FullYearPerformanceForcastSummary += $",{GetDividendPerShareIncreased(lastValue.AdjustedDividendPerShare, secondLastValue.AdjustedDividendPerShare)}）";
        }


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
                    return "増配";
                }
                else if (result1 == result2)
                {
                    return "同配";
                }
                else
                {
                    return "減配";
                }
            }
            else
            {
                return "？配";
            }
        }
        catch (Exception ex)
        {
            return "？配";
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
                    return "増益";
                }
                else
                {
                    return "減益";
                }
            }
            else
            {
                return "？益";
            }
        }
        catch (Exception ex)
        {
            return "？益";
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
                    return "増収";
                }
                else
                {
                    return "減収";
                }
            }
            else
            {
                return "？収";
            }
        }
        catch (Exception ex)
        {
            return "？収";
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
        }
    }

    internal void SetExecutions(List<ExecutionList.ListDetail> executionList)
    {
        // 日付でソートして約定履歴を格納
        this.Executions = ExecutionList.GetExecutions(executionList, this.Code).OrderBy(e => e.Date).ToList();
    }

    internal void SetAveragePerPbr(List<MasterList.AveragePerPbrDetails> masterList)
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
                { "11 ガラス・土石製品", "ガラス・土石製品" },
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
                { "24 倉庫・運輸関連業", "倉庫・運輸関連" },
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
        catch (Exception ex) { 
            Console.WriteLine(ex.Message);
        }
    }
    /// <summary>
    /// 配当権利確定日が近いか？
    /// </summary>
    /// <remarks>配当権利確定日が当月以内の場合にtrueを返す。</remarks>
    internal bool IsCloseToDividendRecordDate()
    {
        // TODO: 次回権利日の閾値以内かを判定する
        //bool result = false;

        //List<DateTime> recordDays = this.GetRecordDays();

        //foreach (var day in recordDays)
        //{
        //    // 権利日のN日前 <= 実行日 < 権利日
        //    if (day.AddDays(-1 * 14) <= CommonUtils.Instance.ExecusionDate 
        //        && CommonUtils.Instance.ExecusionDate < day)
        //    {
        //        result = true;
        //    }
        //}

        //return result;

        return IsWithinMonths(this.DividendRecordDateMonth, 0);
    }

    private List<DateTime> GetRecordDays()
    {
        List <DateTime> days = new List<DateTime>();

        if (!string.IsNullOrEmpty(this.DividendRecordDateMonth))
        {
            // 月文字列を分割してリストに変換
            string[] monthArray = this.DividendRecordDateMonth.Split(',');
            List<int> months = new List<int>();

            foreach (string monthStr in monthArray)
            {
                int month = ParseMonth(monthStr.Trim());
                months.Add(month);
            }
        }

        // TODO

        return days;
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
//            Console.WriteLine($"IsWithinTwoMonthsエラー： {ex.ToString()}");
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
    internal bool IsPERUndervalued()
    {
        bool result = false;
        if (this.Per > 0 && this.Per < this.AveragePer) result = true;
        return result;
    }

    /// <summary>
    /// PBRが割安か？
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal bool IsPBRUndervalued()
    {
        bool result = false;
        if (this.Pbr > 0 && this.Pbr < this.AveragePbr) result = true;
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

    private FullYearPerformance GetFullYearForecast(string period)
    {
        FullYearPerformance result = new FullYearPerformance();

        // 今期は2、前期は3
        var refIndex = period == CommonUtils.Instance.PeriodString.Current ? 2 : 3;

        // 通期進捗率の算出
        if (this.FullYearPerformances.Count >= refIndex)
        {
            // TODO: Q4の時は既に来期の予想しか取得できないため、キャッシュから取得する必要がある。（現状は2件前の通期実績を格納している。よって常に100%になる。）
            refIndex = this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter4 ? refIndex + 1 : refIndex;
            result = this.FullYearPerformances[this.FullYearPerformances.Count - refIndex];
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
    /// <returns></returns>
    internal bool IsAnnualProgressOnTrack()
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
    /// <returns></returns>
    internal bool IsHighYield()
    {
        bool result = false;

        if ((this.DividendYield + this.ShareholderBenefitYield) > CommonUtils.Instance.ThresholdOfYield) result = true;

        return result;
    }

    /// <summary>
    /// 時価総額が高いか？
    /// </summary>
    /// <returns></returns>
    internal bool IsHighMarketCap()
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

    /// <summary>
    /// 下げすぎ判定
    /// </summary>
    //internal bool OversoldIndicator(ChartPrice? price = null)
    //{
    //    bool result = false;

    //    // パラメータがあればターゲットとする
    //    var target = price != null ? price : this.ChartPrices[0];

    //    // RSIが閾値以下の場合
    //    if (target.RSIL <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;
    //    if (target.RSIS <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;

    //    return result;
    //}

    private void UpdateChartPrices()
    {
        using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
        {
            connection.Open();

            // プライマリーキーに条件を設定したクエリ
            string query =
                "SELECT * FROM (" +
                "SELECT * FROM history WHERE code = @code ORDER BY date DESC LIMIT @limit" +
                ") ORDER BY date ASC;";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", this.Code);
                command.Parameters.AddWithValue("@limit", CommonUtils.Instance.ChartDays + 1);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    ChartPrice previousPrice = null;
                    List<ChartPrice> prices = new List<ChartPrice>();

                    // 結果を読み取り
                    while (reader.Read())
                    {
                        // カラム名を指定してデータを取得
                        string code = reader.GetString(reader.GetOrdinal("code"));
                        DateTime date = reader.GetDateTime(reader.GetOrdinal("date"));
                        string dateString = date.ToString("yyyyMMdd");
                        double open = reader.GetDouble(reader.GetOrdinal("open"));
                        double high = reader.GetDouble(reader.GetOrdinal("high"));
                        double low = reader.GetDouble(reader.GetOrdinal("low"));
                        double close = reader.GetDouble(reader.GetOrdinal("close"));
                        double volume = reader.GetDouble(reader.GetOrdinal("volume"));

                        double sma25 = Analyzer.GetSMA(25, date, this.Code);
                        double sma75 = Analyzer.GetSMA(75, date, this.Code);

                        ChartPrice price = new ChartPrice()
                        {
                            Date = date,
                            Price = close,
                            Volatility = previousPrice != null ? (close / previousPrice.Price) - 1 : 0,
                            RSIL = Analyzer.GetCutlerRSI(CommonUtils.Instance.RSILongPeriodDays, date, this.Code),
                            RSIS = Analyzer.GetCutlerRSI(CommonUtils.Instance.RSIShortPeriodDays, date, this.Code),
                            SMA25 = sma25,
                            SMA75 = sma75,
                            SMAdev = sma25 - sma75,
                            MADS = (close - sma25) / sma25,
                            MADL = (close - sma75) / sma75,
                        };

                        prices.Add(price);

                        // 前回分の保持
                        previousPrice = (ChartPrice)price.Clone();
                    }

                    // 件数を絞って日付降順でソート
                    this.ChartPrices = prices.OrderByDescending(p => p.Date).Take(CommonUtils.Instance.ChartDays).ToList();
                }
            }
        }
    }

    /// <summary>
    /// 四半期決算日が近いか？
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal bool IsCloseToQuarterEnd()
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
    internal bool IsJustSold()
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
    /// データベースへのキャッシュ登録
    /// </summary>
    /// <remarks>
    /// Webサイトのスクライピング結果を必要に応じてデータベースに登録する。
    /// </remarks>
    private void RegisterCache()
    {
        // 株価履歴の登録
        foreach (var p in this.ScrapedPrices)
        {
            if (!IsInHistory(p))
            {
                RegsterHistory(p);
            }
        }

        // 修正履歴の登録
        foreach (var f in this.FullYearPerformancesForcasts)
        {
            if (!IsInForcastHistory(f))
            {
                RegsterForcastHistory(f);
            }
        }
    }

    private void RegsterForcastHistory(FullYearPerformanceForcast f)
    {
        using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
        {
            connection.Open();

            // 挿入クエリ
            string query = "INSERT INTO forcast_history (" +
                "code" +
                ", revision_date_string" +
                ", revision_date" +
                ", fiscal_period" +
                ", category" +
                ", revision_direction" +
                ", revenue" +
                ", operating_profit" +
                ", ordinary_income" +
                ", net_profit" +
                ", revised_dividend" +
                ") VALUES (" +
                "@code" +
                ", @revision_date_string" +
                ", @revision_date" +
                ", @fiscal_period" +
                ", @category" +
                ", @revision_direction" +
                ", @revenue" +
                ", @operating_profit" +
                ", @ordinary_income" +
                ", @net_profit" +
                ", @revised_dividend" +
                ")";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", this.Code);
                command.Parameters.AddWithValue("@revision_date_string", f.RevisionDate.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@revision_date", f.RevisionDate);
                command.Parameters.AddWithValue("@fiscal_period", f.FiscalPeriod);
                command.Parameters.AddWithValue("@category", f.Category);
                command.Parameters.AddWithValue("@revision_direction", f.RevisionDirection);
                command.Parameters.AddWithValue("@revenue", f.Revenue);
                command.Parameters.AddWithValue("@operating_profit", f.OperatingProfit);
                command.Parameters.AddWithValue("@ordinary_income", f.OrdinaryProfit);
                command.Parameters.AddWithValue("@net_profit", f.NetProfit);
                command.Parameters.AddWithValue("@revised_dividend", f.RevisedDividend);

                // クエリを実行
                int rowsAffected = command.ExecuteNonQuery();

                // 結果を表示
                CommonUtils.Instance.Logger.LogInformation("forcast_history rows inserted: " + rowsAffected);
            }
        }
    }

    /// <summary>
    /// 予想履歴の存在するか？
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private bool IsInForcastHistory(FullYearPerformanceForcast f)
    {
        using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
        {
            connection.Open();

            // プライマリーキーに条件を設定したクエリ
            string query = "SELECT count(code) as count FROM forcast_history WHERE code = @code and revision_date_string = @revision_date_string";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", this.Code);
                command.Parameters.AddWithValue("@revision_date_string", f.RevisionDate.ToString("yyyyMMdd"));

                // COUNTの結果を取得
                object result = command.ExecuteScalar();
                int count = Convert.ToInt32(result);

                return count > 0;
            }
        }
    }

    private void RegsterHistory(ScrapedPrice p)
    {
        using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
        {
            connection.Open();

            // 挿入クエリ
            string query = "INSERT INTO history (" +
                "code" +
                ", date_string" +
                ", date" +
                ", open" +
                ", high" +
                ", low" +
                ", close" +
                ", volume" +
                ") VALUES (" +
                "@code" +
                ", @date_string" +
                ", @date" +
                ", @open" +
                ", @high" +
                ", @low" +
                ", @close" +
                ", @volume" +
                ")";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", this.Code);
                command.Parameters.AddWithValue("@date_string", p.DateYYYYMMDD);
                command.Parameters.AddWithValue("@date", p.Date);
                command.Parameters.AddWithValue("@open", p.Open);
                command.Parameters.AddWithValue("@high", p.High);
                command.Parameters.AddWithValue("@low", p.Low);
                command.Parameters.AddWithValue("@close", p.Close);
                command.Parameters.AddWithValue("@volume", p.Volume);

                // クエリを実行
                int rowsAffected = command.ExecuteNonQuery();

                // 結果を表示
                CommonUtils.Instance.Logger.LogInformation("history rows inserted: " + rowsAffected);
            }
        }
    }

    /// <summary>
    /// 履歴に存在するか？
    /// </summary>
    private bool IsInHistory(ScrapedPrice p)
    {
        using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
        {
            connection.Open();

            // プライマリーキーに条件を設定したクエリ
            string query = "SELECT count(code) as count FROM history WHERE code = @code and date_string = @date_string";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", this.Code);
                command.Parameters.AddWithValue("@date_string", p.DateYYYYMMDD);

                // COUNTの結果を取得
                object result = command.ExecuteScalar();
                int count = Convert.ToInt32(result);

                return count > 0;
            }
        }
    }

    /// <summary>
    /// 四半期決算直後であるか？
    /// </summary>
    internal bool IsAfterQuarterEnd()
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
    internal bool IsQuarterEnd()
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
        // キャッシュ最新化
        RegisterCache();

        // 通期予想の更新
        UpdateFullYearPerformancesForcasts();

        // 実績進捗率の更新
        UpdateProgress();

        // 配当性向の更新
        UpdateDividendPayoutRatio();

        // 通期予想のサマリを更新
        UpdateFullYearPerformanceForcastSummary();

        // チャート情報を更新
        UpdateChartPrices();
    }

    /// <summary>
    /// 通期予想履歴の更新
    /// </summary>
    private void UpdateFullYearPerformancesForcasts()
    {
        // 4Q決算の場合、通期予想に前期の修正履歴と最終実績を追加する。
        if (this.LastQuarterPeriod == CommonUtils.Instance.QuarterString.Quarter4)
        {
            // 前期の修正履歴を取得
            List<FullYearPerformanceForcast> PreviousForcasts = GetPreviousForcasts();

            short count = 0;
            FullYearPerformanceForcast clone = new FullYearPerformanceForcast();
            foreach (var previous in PreviousForcasts)
            {
                count++;

                // 先頭に追加
                FullYearPerformancesForcasts.Insert(0, previous);

                // 最終を退避しておく
                if (count == PreviousForcasts.Count) clone = (FullYearPerformanceForcast)previous.Clone();
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

            // 先頭に追加
            FullYearPerformancesForcasts.Insert(0, f);
        }
    }

    /// <summary>
    /// 前期の通期予測を取得する
    /// </summary>
    /// <returns></returns>
    private List<FullYearPerformanceForcast> GetPreviousForcasts()
    {
        List<FullYearPerformanceForcast> result = new List<FullYearPerformanceForcast>();

        // カレント決算月が取得できていない場合は抜ける
        if(this.CurrentFiscalMonth == DateTime.MinValue) return result;

        using (SQLiteConnection connection = new SQLiteConnection(CommonUtils.Instance.ConnectionString))
        {
            connection.Open();

            // プライマリーキーに条件を設定したクエリ
            string query = "SELECT * FROM forcast_history WHERE code = @code and fiscal_period = @fiscal_period ORDER BY revision_date";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // パラメータを設定
                command.Parameters.AddWithValue("@code", this.Code);
                command.Parameters.AddWithValue("@fiscal_period", this.CurrentFiscalMonth.AddYears(-1).ToString("yyyy.MM"));

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    FullYearPerformanceForcast cloneF = null;

                    while (reader.Read())
                    {
                        // カラム名を指定してデータを取得
                        string code = reader.GetString(reader.GetOrdinal("code"));
                        string revisionDateString = reader.GetString(reader.GetOrdinal("revision_date_string"));
                        DateTime revisionDate = reader.GetDateTime(reader.GetOrdinal("revision_date"));
                        string fiscalPeriod = reader.GetString(reader.GetOrdinal("fiscal_period"));
                        string category = reader.GetString(reader.GetOrdinal("category"));
                        string revisionDirection = reader.GetString(reader.GetOrdinal("revision_direction"));
                        double revenue = reader.GetDouble(reader.GetOrdinal("revenue"));
                        double operatingProfit = reader.GetDouble(reader.GetOrdinal("operating_profit"));
                        double ordinaryIncome = reader.GetDouble(reader.GetOrdinal("ordinary_income"));
                        double netProfit = reader.GetDouble(reader.GetOrdinal("net_profit"));
                        double revisedDividend = reader.GetDouble(reader.GetOrdinal("revised_dividend"));

                        FullYearPerformanceForcast f = new FullYearPerformanceForcast()
                        {
                            Category = category,
                            FiscalPeriod = fiscalPeriod,
                            RevisionDate = revisionDate,
                            NetProfit = netProfit.ToString(),
                            OperatingProfit = operatingProfit.ToString(),
                            OrdinaryProfit = ordinaryIncome.ToString(),
                            RevisionDirection = revisionDirection,
                            Revenue = revenue.ToString(),
                            RevisedDividend = revisedDividend.ToString(),
                            Summary = string.Empty,
                            PreviousForcast = cloneF,
                        };
                        // 前回分の保持
                        cloneF = (FullYearPerformanceForcast)f.Clone();

                        result.Add(f);
                    }
                }
            }
        }
        return result;
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
    internal bool IsCloseToRecordDate()
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
    internal bool IsRecordDate()
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
    internal bool IsAfterRecordDate()
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
    /// 日次価格情報
    /// </summary>
    public class ScrapedPrice
    {
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 日付文字列
        /// </summary>
        public string DateYYYYMMDD { get; set; }
        /// <summary>
        /// 始値
        /// </summary>
        public double Open { get; set; }
        /// <summary>
        /// 高値
        /// </summary>
        public double High { get; set; }
        /// <summary>
        /// 安値
        /// </summary>
        public double Low { get; set; }
        /// <summary>
        /// 終値
        /// </summary>
        public double Close { get; set; }
        /// <summary>
        /// 出来高
        /// </summary>
        public double Volume { get; set; }
        /// <summary>
        /// 調整後終値
        /// </summary>
        public double AdjustedClose { get; internal set; }
    }

    public class FullYearPerformance
    {
        /// <summary>
        /// 決算期
        /// </summary>
        public string FiscalPeriod { get; set; }
        /// <summary>
        /// 売上高
        /// </summary>
        public string Revenue { get; set; }
        /// <summary>
        /// 営業益
        /// </summary>
        public string OperatingProfit { get; set; }
        /// <summary>
        /// 経常益
        /// </summary>
        public string OrdinaryProfit { get; set; }
        /// <summary>
        /// 最終益
        /// </summary>
        public string NetProft { get; set; }
        /// <summary>
        /// 修正1株益
        /// </summary>
        public string AdjustedEarningsPerShare { get; set; }
        /// <summary>
        /// 修正1株配
        /// </summary>
        public string AdjustedDividendPerShare { get; set; }
        /// <summary>
        /// 発表日
        /// </summary>
        public string AnnouncementDate { get; set; }
    }

    public class FullYearProfit
    {
        /// <summary>
        /// 決算期
        /// </summary>
        public string FiscalPeriod { get; set; }
        /// <summary>
        /// 売上高
        /// </summary>
        public string Revenue { get; set; }
        /// <summary>
        /// 営業益
        /// </summary>
        public string OperatingIncome { get; set; }
        /// <summary>
        /// 売上営業利益率
        /// </summary>
        public string OperatingMargin { get; set; }
        /// <summary>
        /// ＲＯＥ
        /// </summary>
        public double Roe { get; set; }
        /// <summary>
        /// ＲＯＡ
        /// </summary>
        public double Roa { get; set; }
        /// <summary>
        /// 総資産回転率
        /// </summary>
        public string TotalAssetTurnover { get; set; }
        /// <summary>
        /// 修正1株益
        /// </summary>
        public string AdjustedEarningsPerShare { get; set; }
    }
    public class QuarterlyPerformance
    {
        /// <summary>
        /// 決算期
        /// </summary>
        public string FiscalPeriod { get; set; }
        /// <summary>
        /// 売上高
        /// </summary>
        public string Revenue { get; set; }
        /// <summary>
        /// 営業益
        /// </summary>
        public string OperatingProfit { get; set; }
        /// <summary>
        /// 経常益
        /// </summary>
        public double OrdinaryProfit { get; internal set; }
        /// <summary>
        /// 最終益
        /// </summary>
        public string NetProfit { get; internal set; }
        /// <summary>
        /// 修正一株益
        /// </summary>
        public string AdjustedEarningsPerShare { get; set; }
        /// <summary>
        /// 修正一株配
        /// </summary>
        public string AdjustedDividendPerShare { get; internal set; }
        /// <summary>
        /// 発表日
        /// </summary>
        public string ReleaseDate { get; internal set; }
    }

    /// <summary>
    /// 通期予想
    /// </summary>
    public class FullYearPerformanceForcast : ICloneable
    {
        /// <summary>
        /// 決算期
        /// </summary>
        public string FiscalPeriod { get; internal set; }
        /// <summary>
        /// 修正日
        /// </summary>
        public DateTime RevisionDate { get; internal set; }
        /// <summary>
        /// 区分
        /// </summary>
        public string Category { get; internal set; }
        /// <summary>
        /// 修正方向
        /// </summary>
        public string RevisionDirection { get; internal set; }
        /// <summary>
        /// 売上高
        /// </summary>
        public string Revenue { get; internal set; }
        /// <summary>
        /// 営業益
        /// </summary>
        public string OperatingProfit { get; internal set; }
        /// <summary>
        /// 経常益
        /// </summary>
        public string OrdinaryProfit { get; internal set; }
        /// <summary>
        /// 最終益
        /// </summary>
        public string NetProfit { get; internal set; }
        /// <summary>
        /// 修正配当
        /// </summary>
        public string RevisedDividend { get; internal set; }
        /// <summary>
        /// 予想概要
        /// </summary>
        public string Summary { get; internal set; }
        /// <summary>
        /// 前回の予想
        /// </summary>
        public FullYearPerformanceForcast PreviousForcast { get; internal set; }

        public object Clone()
        {
            // 浅いコピー（メンバーコピー）を返す
            return this.MemberwiseClone();
        }

        /// <summary>
        /// 下方修正を含むか？
        /// </summary>
        internal bool HasDownwardRevision()
        {
            bool result = false;

            if (this.Category != CommonUtils.Instance.ForecastCategoryString.Initial 
                && this.Category != CommonUtils.Instance.ForecastCategoryString.Final)
            {
                // 売上
                if (CommonUtils.Instance.GetDouble(this.Revenue) < CommonUtils.Instance.GetDouble(this.PreviousForcast.Revenue)) result = true;
                // 経常利益
                if (CommonUtils.Instance.GetDouble(this.OrdinaryProfit) < CommonUtils.Instance.GetDouble(this.PreviousForcast.OrdinaryProfit)) result = true;
                // 配当
                if (CommonUtils.Instance.GetDouble(this.RevisedDividend) < CommonUtils.Instance.GetDouble(this.PreviousForcast.RevisedDividend)) result = true;
            }

            return result;
        }

        /// <summary>
        /// 上方修正を含むか？
        /// </summary>
        internal bool HasUpwardRevision()
        {
            bool result = false;

            if (this.Category != CommonUtils.Instance.ForecastCategoryString.Initial 
                && this.Category != CommonUtils.Instance.ForecastCategoryString.Final)
            {
                // 売上
                if (CommonUtils.Instance.GetDouble(this.Revenue) > CommonUtils.Instance.GetDouble(this.PreviousForcast.Revenue)) result = true;
                // 経常利益
                if (CommonUtils.Instance.GetDouble(this.OrdinaryProfit) > CommonUtils.Instance.GetDouble(this.PreviousForcast.OrdinaryProfit)) result = true;
                // 配当
                if (CommonUtils.Instance.GetDouble(this.RevisedDividend) > CommonUtils.Instance.GetDouble(this.PreviousForcast.RevisedDividend)) result = true;
            }

            return result;
        }
    }

    internal class ChartPrice : ICloneable
    {
        public DateTime Date { get; internal set; }
        public double Price { get; internal set; }
        public double Volatility { get; internal set; }
        public double RSIL { get; internal set; }
        public double RSIS { get; internal set; }
        /// <summary>
        /// 単純移動平均値（25日）
        /// </summary>
        public double SMA25 { get; internal set; }
        /// <summary>
        /// 単純移動平均値（75日）
        /// </summary>
        public double SMA75 { get; internal set; }

        /// <summary>
        /// Moving Average Deviation（平均移動乖離）
        /// 25日
        /// </summary>
        public double MADS { get; internal set; }
        /// <summary>
        /// Moving Average Deviation（平均移動乖離）
        /// 75日
        /// </summary>
        public double MADL { get; internal set; }
        /// <summary>
        /// SMA25、SMA75の乖離値
        /// </summary>
        public double SMAdev { get; internal set; }

        public object Clone()
        {
            // 浅いコピー（メンバーコピー）を返す
            return this.MemberwiseClone();
        }

        internal bool OverboughtIndicator()
        {
            bool result = false;

            // RSIが閾値以下の場合
            if (this.RSIL >= CommonUtils.Instance.ThresholdOfOverboughtRSI) result = true;
            if (this.RSIS >= CommonUtils.Instance.ThresholdOfOverboughtRSI) result = true;

            return result;
        }

        internal bool OversoldIndicator()
        {
            bool result = false;

            // RSIが閾値以下の場合
            if (this.RSIL <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;
            if (this.RSIS <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;

            return result;
        }
    }
}