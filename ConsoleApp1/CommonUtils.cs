// See https://aka.ms/new-console-template for more information
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Targets;
using NLog;
using NLog.Extensions.Logging;
using System.Configuration;
using System.Globalization;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System.Collections.Specialized;
using System.Diagnostics;

internal class CommonUtils : IDisposable
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
    /// ファイルログのファイルパス
    /// </summary>
    public string FilepathOfFilelog { get; set; } = ConfigurationManager.AppSettings["FilelogFilePath"];
    /// <summary>
    /// 通知ファイルパス
    /// </summary>
    public string FilepathOfAlert { get; set; } = ConfigurationManager.AppSettings["AlertFilePath"];
    /// <summary>
    /// GmailAPIのCredentialファイルパス
    /// </summary>
    public string FilepathOfGmailAPICredential { get; set; } = ConfigurationManager.AppSettings["GmailAPICredentialFilePath"];

    /// <summary>
    /// メールの件名
    /// </summary>
    public string MailSubject { get; set; } = ConfigurationManager.AppSettings["MailSubject"];

    /// <summary>
    /// OneDriveをリフレッシュするべきか？
    /// </summary>
    public bool ShouldRefreshOneDrive { get; set; } = false;
    /// <summary>
    /// 約定リストを更新すべきか？
    /// </summary>
    public bool ShouldUpdateExecutionList { get; set; } = false;
    /// <summary>
    /// メールを送信すべきか？
    /// </summary>
    public bool ShouldSendMail { get; set; } = false;

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
    public short StockPriceHistoryMonths { get; } = 6;

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
        // ロガー
        SetupLogger();
        // フラグ
        SetupFlag();
        // HttpClient
        this.HttpClient = new HttpClient();
    }

    /// <summary>
    /// フラグの準備
    /// </summary>
    private void SetupFlag()
    {
        var processSettings = (NameValueCollection)ConfigurationManager.GetSection("processSettings");
        this.ShouldUpdateExecutionList = processSettings["ShouldUpdateExecutionList"] == "1" ? true : false;
        this.ShouldSendMail = processSettings["ShouldSendMail"] == "1" ? true : false;
        this.ShouldRefreshOneDrive = processSettings["ShouldRefreshOneDrive"] == "1" ? true : false;
    }

    /// <summary>
    /// ロガーの準備
    /// </summary>
    private void SetupLogger()
    {
        // NLogの設定をコード内で行う
        var config = new LoggingConfiguration();

        // ファイルターゲットを作成
        var logfile = new FileTarget("logfile")
        {
            FileName = CommonUtils.ReplacePlaceholder(FilepathOfFilelog, "{yyyyMMdd}", ExecusionDate.ToString("yyyyMMdd")),
            Layout = "${longdate} ${uppercase:${level}} ${message}"
        };

        // コンソールターゲットを作成（オプション）
        var logconsole = new ConsoleTarget("logconsole")
        {
            Layout = "${longdate} ${uppercase:${level}} ${message}"
        };

        // ターゲットを設定に追加
        config.AddTarget(logfile);
        config.AddTarget(logconsole);

        // ルールを設定に追加
        config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
        config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);

        // NLogの設定を適用
        LogManager.Configuration = config;

        // サービスコレクションを作成し、ロギングサービスを追加
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        // サービスプロバイダーを作成
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // ロガーを取得
        this.Logger = serviceProvider.GetService<ILogger<ConsoleApp1.Program>>();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // NLogを使用するようにロギングを設定
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            loggingBuilder.AddNLog();
        });
    }

    public class ClassificationClass
    {
        //日本個別株: JapaneseIndividualStocks
        //日本ETF: Japanese ETFs
        //日本投資信託: Japanese Mutual Funds
        //米国ETF: U.S.ETFs
        //米国個別株：USIndividualStocks

        /// <summary>
        /// 日本個別株
        /// </summary>
        public string JapaneseIndividualStocks { get; } = "1";
        /// <summary>
        /// 日本ETF
        /// </summary>
        public string JapaneseETFs { get; } = "2";
        /// <summary>
        /// 米国個別株
        /// </summary>
        public string USIndividualStocks { get; } = "5";
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
    public class PeriodStringClass
    {
        public string Current { get; } = "今期";
        public string Previous { get; } = "前期";
        public string TwoAgo { get; } = "前々期";
    }

    public class ForecastCategoryStringClass
    {
        public string Initial { get; } = "初";
        public string Revised { get; } = "修";
        public string Final { get; } = "終";
    }

    public ClassificationClass Classification {  get; set; } = new ClassificationClass();

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
    public double ThresholdOfAverageDown { get; } = -0.03;
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
    /// 上げすぎRSI閾値
    /// </summary>
    public double ThresholdOfOverboughtRSI { get; } = 70.00;
    /// <summary>
    /// 四半期決算日直前かを判定する日数の閾値
    /// </summary>
    public short ThresholdOfDaysToQuarterEnd { get; } = 14;
    /// <summary>
    /// 四半期決算日直後かを判定する日数の閾値
    /// </summary>
    public short ThresholdOfDaysFromQuarterEnd { get; } = 7;
    /// <summary>
    /// 売却直後かを判定する日数の閾値
    /// </summary>
    public short ThresholdOfDaysJustSold { get; } = 3;
    /// <summary>
    /// 売買文字列
    /// </summary>
    public BuyOrSellStringClass BuyOrSellString { get; } = new BuyOrSellStringClass();
    /// <summary>
    /// 四半期文字列
    /// </summary>
    public QuarterStringClass QuarterString { get; } = new QuarterStringClass();
    /// <summary>
    /// 期間文字列
    /// </summary>
    public PeriodStringClass PeriodString { get; } = new PeriodStringClass();
    /// <summary>
    /// 通期予想区分文字列
    /// </summary>
    public ForecastCategoryStringClass ForecastCategoryString { get; } = new ForecastCategoryStringClass();
    /// <summary>
    /// 進捗良好の判定基準値
    /// </summary>
    public ThresholdOfProgressSuccessClass ThresholdOfProgressSuccess { get; } = new ThresholdOfProgressSuccessClass();
    /// <summary>
    /// バッジ文字列
    /// </summary>
    public BadgeStringClass BadgeString { get; } = new BadgeStringClass();
    /// <summary>
    /// ロガー
    /// </summary>
    public ILogger<ConsoleApp1.Program> Logger { get; private set; }
    /// <summary>
    /// Yahooの履歴ページをスクレイピングするための最大ページ数
    /// </summary>
    public int MaxPageCountToScrapeYahooHistory { get; } = 100;
    /// <summary>
    /// 割安を緩めに判定する際の係数
    /// </summary>
    public double LenientFactor { get; } = 1.1;
    /// <summary>
    /// HttpClientインスタンス
    /// </summary>
    public HttpClient HttpClient { get; internal set; }

    internal static string ReplacePlaceholder(string? input, string placeholder, string newValue)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        }
        return input.Replace(placeholder, newValue);
    }
    internal string ConvertToPercentage(double value, bool shouldAddSign = false, string format = "F2")
    {
        // パーセント形式の文字列に変換し、プラスの場合は"+"を付ける
        string formattedValue = (value * 100).ToString(format, CultureInfo.InvariantCulture);

        if (value >= 0)
        {
            return (shouldAddSign ? "+" : string.Empty) + formattedValue + "%";
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

    public void Dispose()
    {
        // NLogのシャットダウン
        LogManager.Shutdown();
    }

    /// <summary>
    /// 前年同期比の上昇率を算出する
    /// </summary>
    internal double CalculateYearOverYearGrowth(double previousYearValue, double currentYearValue)
    {
        double result = 0;

        result = (currentYearValue / previousYearValue) - 1;

        // 前年値がマイナスの場合は、算出値の符号を反転する。
        result = previousYearValue < 0 ? result * (-1) : result;

        return result;
    }
    /// <summary>
    /// 直近の東証営業日を取得する
    /// </summary>
    /// <returns></returns>
    internal DateTime GetLastTradingDay()
    {
        DateTime date = CommonUtils.Instance.ExecusionDate.Date;

        // 土日または祝日の場合、前日を確認
        while (TSEHolidayChecker.IsTSEHoliday(date))
        {
            date = date.AddDays(-1);
        }

        return date;
    }
    /// <summary>
    /// OneDriveのローカルファイルの最新化
    /// </summary>
    internal void OneDriveRefresh()
    {
        string oneDrivePath = @"C:\Program Files\Microsoft OneDrive\OneDrive.exe"; // OneDriveの実行ファイルのパス

        if (File.Exists(oneDrivePath))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = oneDrivePath,
                Arguments = "/sync", // 同期をトリガーする引数
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                this.Logger.LogInformation("OneDrive sync triggered.");
            }
        }
        else
        {
            this.Logger.LogInformation("OneDrive executable not found.");
        }
    }

    public class BadgeStringClass
    {
        public string ShouldWatch { get; } = "★";
        public string IsFavorite { get; } = "【注目】";
        public string IsOwned { get; } = "【所持】";
        public string IsCloseToRecordDate { get; } = "【権前】";
        public string IsRecordDate { get; } = "【権当】";
        public string IsAfterRecordDate { get; } = "【権後】";
        public string IsQuarterEnd { get; } = "【決当】";
        public string IsCloseToQuarterEnd { get; } = "【決前】";
        public string IsAfterQuarterEnd { get; } = "【決後】";
        public string IsJustSold { get; } = "【売後】";
        public string IsGoldenCrossPossible { get; } = "【金交】";
    }

    public class ThresholdOfProgressSuccessClass
    {
        public double Q1 { get; } = 0.30;
        public double Q2 { get; } = 0.55;
        public double Q3 { get; } = 0.80;
        public double Q4 { get; } = 1.05;
    }
}