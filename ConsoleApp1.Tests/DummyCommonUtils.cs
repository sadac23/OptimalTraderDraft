using ConsoleApp1;

// DummyCommonUtils.cs（テスト用サブクラス）
namespace ConsoleApp1.Tests
{
    public class DummyCommonUtils : CommonUtils
    {
        public DummyCommonUtils() : base() { }

        public override double ThresholdOfYield => 0.04;
        public override BuyOrSellStringClass BuyOrSellString => new BuyOrSellStringClass { Buy = "買", Sell = "売" };

        // 設定値を使うプロパティをすべてオーバーライド
        public override string ConnectionString => "DummyConnectionString";
        public override string FilepathOfWatchList => "DummyWatchListPath";
        public override string FilepathOfExecutionList => "DummyExecutionListPath";
        public override string FilepathOfAveragePerPbrList => "DummyAveragePerPbrListPath";
        public override string FilepathOfFilelog => "C:\\Logs\\DummyLog_{yyyyMMdd}.txt";
        public override string FilepathOfAlert => "DummyAlertPath";
        public override string FilepathOfGmailAPICredential => "DummyGmailAPICredentialPath";
        public override string MailSubject => "DummyMailSubject";

        protected override void SetupFlag()
        {
            // テスト時は何もしない
        }
        protected override void SetupLogger()
        {
            // テスト時は何もしない
        }
    }
}