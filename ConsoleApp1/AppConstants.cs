// See https://aka.ms/new-console-template for more information
using System.Configuration;

internal class AppConstants
{
    // 唯一のインスタンスを保持するための静的フィールド
    private static AppConstants instance = null;

    // インスタンスメンバ
    public DateTime ExecusionDate { get; set; }
    public DateTime MasterStartDate { get; set; }
    public string ConnectionString { get; set; }
    public string FilepathOfWatchList { get; set; }
    public string FilepathOfExecutionList { get; set; }
    public string FilepathOfAveragePerPbrList { get; set; }
    public string FilepathOfAlert { get; set; }

    // プライベートコンストラクタにより、外部からのインスタンス化を防ぐ
    private AppConstants()
    {
        this.ExecusionDate = DateTime.Today;
        this.MasterStartDate = DateTime.Parse("2023/01/01");
        this.ConnectionString = ConfigurationManager.ConnectionStrings["OTDB"].ConnectionString;
        this.FilepathOfWatchList = ConfigurationManager.AppSettings["WatchListFilePath"];
        this.FilepathOfExecutionList = ConfigurationManager.AppSettings["ExecutionListFilePath"];
        this.FilepathOfAveragePerPbrList = ConfigurationManager.AppSettings["AveragePerPbrListFilePath"];
        this.FilepathOfAlert = ConfigurationManager.AppSettings["AlertFilePath"];
    }

    // 唯一のインスタンスを取得するための静的メソッド
    public static AppConstants Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AppConstants();
            }
            return instance;
        }
    }

    public static string ReplacePlaceholder(string? input, string placeholder, string newValue)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        }
        return input.Replace(placeholder, newValue);
    }
}