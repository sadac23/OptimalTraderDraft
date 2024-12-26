// See https://aka.ms/new-console-template for more information
using System.Configuration;

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
    public string MessageAtApplicationStartup { get; set; } = "Hello, World!";
    /// <summary>
    /// アプリケーション終了時のメッセージ
    /// </summary>
    public string MessageAtApplicationEnd { get; set; } = "End.";

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

    public AssetClassificationClass AssetClassification {  get; set; } = new AssetClassificationClass();

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
    public double AverageDownThreshold { get; } = -0.05;
    /// <summary>
    /// 注目マーク
    /// </summary>
    public string WatchMark { get; } = "★";

    public static string ReplacePlaceholder(string? input, string placeholder, string newValue)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        }
        return input.Replace(placeholder, newValue);
    }
}