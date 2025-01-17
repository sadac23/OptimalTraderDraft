// See https://aka.ms/new-console-template for more information
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.Data.Entity;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;

internal class StockInfo
{
    public StockInfo(WatchList.WatchStock watchStock)
    {
        Code = watchStock.Code;
        Classification = watchStock.Classification;
        IsFavorite = watchStock.IsFavorite == "1" ? true : false;
        Memo = watchStock.Memo;
        Prices = new List<StockInfo.Price>();
        FullYearPerformances = new List<StockInfo.FullYearPerformance>();
        FullYearProfits = new List<FullYearProfit>();
        this.QuarterlyPerformances = new List<StockInfo.QuarterlyPerformance>();
        this.FullYearPerformancesForcasts = new List<FullYearPerformanceForcast>();
    }
    /// <summary>
    /// コード
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// 名称
    /// </summary>
    public string ?Name { get; set; }
    /// <summary>
    /// 価格履歴
    /// </summary>
    public List<Price> Prices { get; set; }
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
    public List<FullYearPerformance> FullYearPerformances { get; internal set; }
    /// <summary>
    /// 通期業績予想概要（例：増収増益増配（+50%,+50%,+50））
    /// </summary>
    public string FullYearPerformanceForcastSummary { get; internal set; }
    /// <summary>
    /// 通期業績予想履歴
    /// </summary>
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
    public double LatestPrice { get; internal set; }
    /// <summary>
    /// 直近の株価日付
    /// </summary>
    public DateTime LatestPriceDate { get; internal set; }
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
    /// 四半期実績期間
    /// </summary>
    public string QuarterlyPerformancePeriod { get; internal set; }
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
    /// 直近の株価RSI（長期）
    /// </summary>
    public double LatestPriceRSIL { get; internal set; }
    /// <summary>
    /// 直近の株価RSI（短期）
    /// </summary>
    public double LatestPriceRSIS { get; internal set; }

    /// <summary>
    /// 現在、所有しているか？
    /// </summary>
    internal bool IsOwnedNow()
    {
        var result = false;

        double buy = 0;
        double sell = 0;

        foreach (var execution in Executions)
        {
            if (execution.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Buy) buy += execution.Quantity;
            if (execution.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Sell) sell += execution.Quantity;
        }
        if (buy > sell) result = true;

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
            this.FullYearPerformanceForcastSummary += GetOrdinaryIncomeIncreasedSummary(lastValue.OrdinaryIncome, secondLastValue.OrdinaryIncome);
            this.FullYearPerformanceForcastSummary += GetDividendPerShareIncreasedSummary(lastValue.AdjustedDividendPerShare, secondLastValue.AdjustedDividendPerShare);
            this.FullYearPerformanceForcastSummary += $"（{GetIncreasedRate(lastValue.Revenue, secondLastValue.Revenue)}";
            this.FullYearPerformanceForcastSummary += $",{GetIncreasedRate(lastValue.OrdinaryIncome, secondLastValue.OrdinaryIncome)}";
            this.FullYearPerformanceForcastSummary += $",{GetDividendPerShareIncreased(lastValue.AdjustedDividendPerShare, secondLastValue.AdjustedDividendPerShare)}）";
        }

        // 最後から3件前の値（前期）
        var secondLast = this.FullYearPerformances[this.FullYearPerformances.Count - 3];

        foreach (var p in this.FullYearPerformancesForcasts)
        {
            p.Summary += GetRevenueIncreasedSummary(p.Revenue, secondLast.Revenue);
            p.Summary += GetOrdinaryIncomeIncreasedSummary(p.OrdinaryIncome, secondLast.OrdinaryIncome);
            p.Summary += GetDividendPerShareIncreasedSummary(p.RevisedDividend, secondLast.AdjustedDividendPerShare);
            p.Summary += $"（{GetIncreasedRate(p.Revenue, secondLast.Revenue)}";
            p.Summary += $",{GetIncreasedRate(p.OrdinaryIncome, secondLast.OrdinaryIncome)}";
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
                return ConvertToPercentageStringWithSign((result1 / result2) - 1);
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
                { "スタンダード市場", "東証スタンダード" },
                { "プライム市場", "東証プライム" },
                { "グロース市場", "東証グロース" },
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
                { "20 電気・ガス業", "電気・ガス業" },
                { "21 陸運業", "陸運業" },
                { "22 海運業", "海運業" },
                { "23 空運業", "空運業" },
                { "24 倉庫・運輸関連業", "倉庫・運輸関連業" },
                { "25 情報・通信業", "情報・通信" },
                { "26 卸売業", "卸売業" },
                { "27 小売業", "小売業" },
                { "28 銀行業", "銀行業" },
                { "29 証券、商品先物取引業", "証券業" },
                { "30 保険業", "保険業" },
                { "31 その他金融業", "その他金融業" },
                { "32 不動産業", "不動産業" },
                { "33 サービス業", "サービス業" },
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
    internal bool IsDividendRecordDateClose()
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
        // 期間の取得
        this.QuarterlyPerformancePeriod = this.QuarterlyPerformancePeriod switch
        {
            string s when s.Contains("第１") => CommonUtils.Instance.QuarterString.Quarter1,
            string s when s.Contains("第２") => CommonUtils.Instance.QuarterString.Quarter2,
            string s when s.Contains("第３") => CommonUtils.Instance.QuarterString.Quarter3,
            string s when s.Contains("前期") => CommonUtils.Instance.QuarterString.Quarter4,
            _ => this.QuarterlyPerformancePeriod // デフォルト値（変更しない場合）
        };

        // 当期進捗率
        if (this.QuarterlyPerformances.Count >= 2)
        {
            // 発表日の取得
            this.QuarterlyPerformanceReleaseDate = ConvertToDateTime(this.QuarterlyPerformances[this.QuarterlyPerformances.Count - 2].ReleaseDate);

            // 通期進捗率の算出
            if (this.FullYearPerformances.Count >= 2)
            {
                var fullYearOrdinaryIncome = CommonUtils.Instance.GetDouble(this.FullYearPerformances[this.FullYearPerformances.Count - 2].OrdinaryIncome);
                var latestOrdinaryIncome = this.QuarterlyPerformances[this.QuarterlyPerformances.Count - 2].OrdinaryIncome;
                if (fullYearOrdinaryIncome > 0)
                {
                    this.QuarterlyFullyearProgressRate = latestOrdinaryIncome / fullYearOrdinaryIncome;
                }
            }
        }

        // 前期進捗率
        if (this.QuarterlyPerformances.Count >= 3)
        {
            // 発表日の取得
            this.PreviousPerformanceReleaseDate = ConvertToDateTime(this.QuarterlyPerformances[this.QuarterlyPerformances.Count - 3].ReleaseDate);

            // 通期進捗率の算出
            if (this.FullYearPerformances.Count >= 3)
            {
                var fullYearOrdinaryIncome = CommonUtils.Instance.GetDouble(this.FullYearPerformances[this.FullYearPerformances.Count - 3].OrdinaryIncome);
                var previousOrdinaryIncome = this.QuarterlyPerformances[this.QuarterlyPerformances.Count - 3].OrdinaryIncome;
                if (fullYearOrdinaryIncome > 0)
                {
                    this.PreviousFullyearProgressRate = previousOrdinaryIncome / fullYearOrdinaryIncome;
                }
            }
        }

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

        // 前期以上かつ、案分値以上か？
        if (this.QuarterlyFullyearProgressRate >= this.PreviousFullyearProgressRate)
        {
            if (this.QuarterlyPerformancePeriod == CommonUtils.Instance.QuarterString.Quarter1)
            {
                if (this.QuarterlyFullyearProgressRate >= 0.25) result = true;
            }
            else if (this.QuarterlyPerformancePeriod == CommonUtils.Instance.QuarterString.Quarter2)
            {
                if (this.QuarterlyFullyearProgressRate >= 0.50) result = true;
            }
            else if (this.QuarterlyPerformancePeriod == CommonUtils.Instance.QuarterString.Quarter3)
            {
                if (this.QuarterlyFullyearProgressRate >= 0.75) result = true;
            }
            else if (this.QuarterlyPerformancePeriod == CommonUtils.Instance.QuarterString.Quarter4)
            {
                if (this.QuarterlyFullyearProgressRate >= 1.00) result = true;
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
    internal bool IsShareholderBenefitRecordDateClose()
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
    /// ナンピンするべきか？
    /// </summary>
    internal bool ShouldAverageDown()
    {
        var result = false;

        // 所有中かつ、最終購入よりナンピン基準を下回る下落の場合
        if (this.IsOwnedNow())
        {
            double p = 0;
            foreach (var item in this.Executions)
            {
                if (item.BuyOrSell == CommonUtils.Instance.BuyOrSellString.Buy)
                {
                    p = item.Price;
                }
            }
            if (((this.LatestPrice / p) - 1) <= CommonUtils.Instance.ThresholdOfAverageDown)
            {
                result = true;
            }
        }

        return result;
    }

    /// <summary>
    /// 下げすぎ判定
    /// </summary>
    internal bool OversoldIndicator()
    {
        bool result = false;

        // RSIが閾値以下の場合
        if (this.LatestPriceRSIL <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;
        if (this.LatestPriceRSIS <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;

        return result;
    }

    /// <summary>
    /// 上げすぎ判定
    /// </summary>
    /// <returns></returns>
    internal bool OverboughtIndicator()
    {
        bool result = false;

        // RSIが閾値以下の場合
        if (this.LatestPriceRSIL >= CommonUtils.Instance.ThresholdOfOverboughtRSI) result = true;
        if (this.LatestPriceRSIS >= CommonUtils.Instance.ThresholdOfOverboughtRSI) result = true;

        return result;
    }

    /// <summary>
    /// 日次価格情報
    /// </summary>
    public class Price
    {
        public DateTime Date { get; set; }
        public string DateYYYYMMDD { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
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
        public string OperatingIncome { get; set; }
        /// <summary>
        /// 経常益
        /// </summary>
        public string OrdinaryIncome { get; set; }
        /// <summary>
        /// 最終益
        /// </summary>
        public string NetIncome { get; set; }
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
        public QuarterlyPerformance()
        {
        }

        public string FiscalPeriod { get; set; }
        public string Revenue { get; set; }
        public string OperatingIncome { get; set; }
        public string OperatingMargin { get; set; }
        public double Roe { get; set; }
        public double Roa { get; set; }
        public string TotalAssetTurnover { get; set; }
        public string AdjustedEarningsPerShare { get; set; }
        public double OrdinaryIncome { get; internal set; }
        public string NetIncome { get; internal set; }
        public string ProgressRate { get; internal set; }
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
        public string RevisionDate { get; internal set; }
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
        public string OrdinaryIncome { get; internal set; }
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
        public object Summary { get; internal set; }
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

            if (this.Category != "初") {
                // 売上
                if (CommonUtils.Instance.GetDouble(this.Revenue) < CommonUtils.Instance.GetDouble(this.PreviousForcast.Revenue)) result = true;
                // 経常利益
                if (CommonUtils.Instance.GetDouble(this.OrdinaryIncome) < CommonUtils.Instance.GetDouble(this.PreviousForcast.OrdinaryIncome)) result = true;
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

            if (this.Category != "初")
            {
                // 売上
                if (CommonUtils.Instance.GetDouble(this.Revenue) > CommonUtils.Instance.GetDouble(this.PreviousForcast.Revenue)) result = true;
                // 経常利益
                if (CommonUtils.Instance.GetDouble(this.OrdinaryIncome) > CommonUtils.Instance.GetDouble(this.PreviousForcast.OrdinaryIncome)) result = true;
                // 配当
                if (CommonUtils.Instance.GetDouble(this.RevisedDividend) > CommonUtils.Instance.GetDouble(this.PreviousForcast.RevisedDividend)) result = true;
            }

            return result;
        }
    }
}