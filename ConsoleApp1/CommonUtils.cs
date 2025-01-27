// See https://aka.ms/new-console-template for more information
using DocumentFormat.OpenXml.Bibliography;
using System.Configuration;
using System.Globalization;

internal class CommonUtils
{
    // 唯一のインスタンスを保持するための静的フィールド
    private static CommonUtils instance = null;

    /// <summary>
    /// アプリケーション実行日
    /// </summary>
    public DateTime ExecusionDate { get; set; } = DateTime.Today;
    /// <summary>
    /// 株価履歴の取得開始日
    /// </summary>
    public DateTime MasterStartDate { get; set; } = DateTime.Parse("2023/01/01");
    /// <summary>
    /// データベースの接続文字列
    /// </summary>
    public string ConnectionString { get; set; } = ConfigurationManager.ConnectionStrings["OTDB"].ConnectionString;
    /// <summary>
    /// ウォッチリストのファイルパス
    /// </summary>
    public string FilepathOfWatchList { get; set; } = ConfigurationManager.AppSettings["WatchListFilePath"];
    /// <summary>
    /// 約定履歴のファイルパス
    /// </summary>
    public string FilepathOfExecutionList { get; set; } = ConfigurationManager.AppSettings["ExecutionListFilePath"];
    /// <summary>
    /// PER/PBRの平均リスト
    /// </summary>
    public string FilepathOfAveragePerPbrList { get; set; } = ConfigurationManager.AppSettings["AveragePerPbrListFilePath"];
    /// <summary>
    /// 通知ファイルパス
    /// </summary>
    public string FilepathOfAlert { get; set; } = ConfigurationManager.AppSettings["AlertFilePath"];
    /// <summary>
    /// アプリケーション開始時のメッセージ
    /// </summary>
    public string MessageAtApplicationStartup { get; set; } = "*** Start ***";
    /// <summary>
    /// アプリケーション終了時のメッセージ
    /// </summary>
    public string MessageAtApplicationEnd { get; set; } = "*** End ***";

    /// <summary>
    /// 株価履歴を保持しておく月数
    /// </summary>
    public short StockPriceHistoryMonths { get; } = 4;

    /// <summary>
    /// 長期RSIの日数
    /// </summary>
    public short RSILongPeriodDays { get; } = 14;

    /// <summary>
    /// 短期RSIの日数
    /// </summary>
    public short RSIShortPeriodDays { get; } = 9;

    // プライベートコンストラクタにより、外部からのインスタンス化を防ぐ
    private CommonUtils()
    {
    }
    public class AssetClassificationClass
    {
        //日本個別株: JapaneseIndividualStocks
        //日本ETF: Japanese ETFs
        //日本投資信託: Japanese Mutual Funds
        //米国ETF: U.S.ETFs

        /// <summary>
        /// 日本個別株
        /// </summary>
        public string JapaneseIndividualStocks { get; } = "1";
        /// <summary>
        /// 日本ETF
        /// </summary>
        public string JapaneseETFs { get; } = "2";
    }
    public class BuyOrSellStringClass
    {
        public string Buy { get; } = "買";
        public string Sell { get; } = "売";
    }
    public class QuarterStringClass
    {
        public string Quarter1 { get; } = "Q1";
        public string Quarter2 { get; } = "Q2";
        public string Quarter3 { get; } = "Q3";
        public string Quarter4 { get; } = "Q4";
    }

    public AssetClassificationClass AssetClassification {  get; set; } = new AssetClassificationClass();

    /// <summary>
    /// チャートの表示日数
    /// </summary>
    public short ChartDays { get; } = 14;

    // 唯一のインスタンスを取得するための静的メソッド
    public static CommonUtils Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CommonUtils();
            }
            return instance;
        }
    }

    /// <summary>
    /// ナンピン閾値
    /// </summary>
    public double ThresholdOfAverageDown { get; } = -0.05;
    /// <summary>
    /// 注目マーク
    /// </summary>
    public string WatchMark { get; } = "★";
    /// <summary>
    /// 利回り閾値
    /// </summary>
    public double ThresholdOfYield { get; } = 0.0300;
    /// <summary>
    /// 時価総額閾値
    /// </summary>
    public double ThresholdOfMarketCap { get; } = 100000000000;
    /// <summary>
    /// ROE閾値
    /// </summary>
    public double ThresholdOfROE { get; } = 8.00;
    /// <summary>
    /// 下げすぎRSI閾値
    /// </summary>
    public double ThresholdOfOversoldRSI { get; } = 30.00;
    /// <summary>
    /// 売買文字列
    /// </summary>
    public BuyOrSellStringClass BuyOrSellString { get; } = new BuyOrSellStringClass();
    /// <summary>
    /// 四半期文字列
    /// </summary>
    public QuarterStringClass QuarterString { get; } = new QuarterStringClass();
    /// <summary>
    /// 進捗良好の判定基準値
    /// </summary>
    public ThresholdOfProgressSuccessClass ThresholdOfProgressSuccess { get; } = new ThresholdOfProgressSuccessClass();
    /// <summary>
    /// 上げすぎRSI閾値
    /// </summary>
    public double ThresholdOfOverboughtRSI { get; } = 65.00;
    /// <summary>
    /// バッジ文字列
    /// </summary>
    public BadgeStringClass BadgeString { get; } = new BadgeStringClass();

    internal static string ReplacePlaceholder(string? input, string placeholder, string newValue)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        }
        return input.Replace(placeholder, newValue);
    }
    internal string ConvertToPercentage(double value)
    {
        // パーセント形式の文字列に変換し、プラスの場合は"+"を付ける
        string formattedValue = (value * 100).ToString("F2", CultureInfo.InvariantCulture);
        if (value >= 0)
        {
            return "+" + formattedValue + "%";
        }
        else
        {
            return formattedValue + "%";
        }
    }

    internal string ConvertToYenNotation(double value)
    {
        if (value >= 1_000_000_000_000)
        {
            double trillions = Math.Floor(value / 1_000_000_000_000);
            double billions = (value % 1_000_000_000_000) / 100_000_000;
            return $"{trillions.ToString("N0", CultureInfo.InvariantCulture)}兆{billions.ToString("N0", CultureInfo.InvariantCulture)}億円";
        }
        else if (value >= 100_000_000)
        {
            double billions = value / 100_000_000;
            return $"{billions.ToString("N0", CultureInfo.InvariantCulture)}億円";
        }
        else if (value >= 10_000)
        {
            return $"{value.ToString("N0", CultureInfo.InvariantCulture)}円";
        }
        else
        {
            return value.ToString("N0", CultureInfo.InvariantCulture) + "円";
        }
    }

    internal string ConvertToMultiplierString(double value)
    {
        // 小数点以下2桁までの文字列に変換し、"倍"を追加
        return value.ToString("F2", CultureInfo.InvariantCulture) + "倍";
    }
    internal double GetDouble(string v)
    {
        double.TryParse(v, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double result);
        return result;
    }

    public class BadgeStringClass
    {
        public string ShouldWatch { get; } = "★";
        public string IsOwned { get; } = "【持】";
        public string IsRecordDateClose { get; } = "【権】";
        public string IsFavorite { get; } = "【注】";
    }

    public class ThresholdOfProgressSuccessClass
    {
        public double Q1 { get; } = 0.30;
        public double Q2 { get; } = 0.55;
        public double Q3 { get; } = 0.80;
        public double Q4 { get; } = 1.05;
    }
}