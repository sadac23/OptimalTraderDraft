// See https://aka.ms/new-console-template for more information
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
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
    public string ShareholderBenefitRecordDateMonth { get; internal set; }
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
    /// 現在、所有しているか？
    /// </summary>
    internal bool IsOwnedNow()
    {
        var result = false;

        double buy = 0;
        double sell = 0;

        foreach (var execution in Executions)
        {
            if (execution.BuyOrSell == "買") buy += execution.Quantity;
            if (execution.BuyOrSell == "売") sell += execution.Quantity;
        }
        if (buy > sell) result = true;

        return result;
    }

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
        this.Executions = ExecutionList.GetExecutions(executionList, this.Code);
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
                    this.AveragePer = GetDouble(details.AveragePer);
                    this.AveragePbr = GetDouble(details.AveragePbr);
                }
            }
        }
        catch (Exception ex) { 
            Console.WriteLine(ex.Message);
        }
    }
    private double GetDouble(string v)
    {
        double.TryParse(v, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double result);
        return result;
    }
    /// <summary>
    /// 配当権利確定日が近いか？
    /// </summary>
    /// <remarks>配当権利確定日が2か月以内の場合にtrueを返す。</remarks>
    internal bool IsRecordDateClose()
    {
        return IsWithinTwoMonths(this.DividendRecordDateMonth);
    }

    public static bool IsWithinTwoMonths(string monthsStr)
    {
        if (string.IsNullOrEmpty(monthsStr)) return false;

        // 現在の月を取得
        int currentMonth = DateTime.Now.Month;

        // 月文字列を分割してリストに変換
        var months = monthsStr.Split(',')
                              .Select(m => ParseMonth(m.Trim()))
                              .ToList();

        // 当月から2か月以内の月をリストにする
        var validMonths = Enumerable.Range(0, 3)
                                    .Select(i => (currentMonth + i - 1) % 12 + 1)
                                    .ToList();

        // 指定された月が当月から2か月以内に含まれているかを判定
        return months.Any(month => validMonths.Contains(month));
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
        return (this.Per < this.AveragePer ? true : false);
    }

    /// <summary>
    /// PBRが割安か？
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal bool IsPBRUndervalued()
    {
        return (this.Pbr < this.AveragePbr ? true : false);
    }
    /// <summary>
    /// 最新のROE予想が基準値以上か？
    /// </summary>
    internal bool IsROEAboveThreshold()
    {
        return (this.FullYearProfits[this.FullYearProfits.Count - 1].Roe >= 8.00 ? true : false);
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
}