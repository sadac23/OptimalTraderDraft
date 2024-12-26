// See https://aka.ms/new-console-template for more information
using System.Configuration;

internal class CommonUtils
{
    // 唯一のインスタンスを保持するための静的フィールド
    private static CommonUtils instance = null;

    /// <summary>
    /// アプリケーション実行日
    /// </summary>
    public DateTime ExecusionDate { get; set; }
    /// <summary>
    /// 株価履歴の取得開始日
    /// </summary>
    public DateTime MasterStartDate { get; set; }
    /// <summary>
    /// データベースの接続文字列
    /// </summary>
    public string ConnectionString { get; set; }
    /// <summary>
    /// ウォッチリストのファイルパス
    /// </summary>
    public string FilepathOfWatchList { get; set; }
    /// <summary>
    /// 約定履歴のファイルパス
    /// </summary>
    public string FilepathOfExecutionList { get; set; }
    /// <summary>
    /// PER/PBRの平均リスト
    /// </summary>
    public string FilepathOfAveragePerPbrList { get; set; }
    /// <summary>
    /// 通知ファイルパス
    /// </summary>
    public string FilepathOfAlert { get; set; }

    // プライベートコンストラクタにより、外部からのインスタンス化を防ぐ
    private CommonUtils()
    {
        this.ExecusionDate = DateTime.Today;
        this.MasterStartDate = DateTime.Parse("2023/01/01");
        this.ConnectionString = ConfigurationManager.ConnectionStrings["OTDB"].ConnectionString;
        this.FilepathOfWatchList = ConfigurationManager.AppSettings["WatchListFilePath"];
        this.FilepathOfExecutionList = ConfigurationManager.AppSettings["ExecutionListFilePath"];
        this.FilepathOfAveragePerPbrList = ConfigurationManager.AppSettings["AveragePerPbrListFilePath"];
        this.FilepathOfAlert = ConfigurationManager.AppSettings["AlertFilePath"];
        this.AverageDownThreshold = -0.05;
    }

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
    public double AverageDownThreshold { get; internal set; }

    public static string ReplacePlaceholder(string? input, string placeholder, string newValue)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        }
        return input.Replace(placeholder, newValue);
    }
}