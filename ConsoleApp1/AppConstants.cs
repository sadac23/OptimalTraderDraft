// See https://aka.ms/new-console-template for more information
internal class AppConstants
{
    // 唯一のインスタンスを保持するための静的フィールド
    private static AppConstants instance = null;

    // インスタンスメンバ
    public DateTime ExecusionDate { get; set; }
    public DateTime MasterStartDate { get; set; }

    // プライベートコンストラクタにより、外部からのインスタンス化を防ぐ
    private AppConstants()
    {
        ExecusionDate = DateTime.Today;
        MasterStartDate = DateTime.Parse("2023/01/01");
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
}