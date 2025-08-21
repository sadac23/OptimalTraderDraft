using ConsoleApp1;

// DummyCommonUtils.cs�i�e�X�g�p�T�u�N���X�j
namespace ConsoleApp1.Tests
{
    public class DummyCommonUtils : CommonUtils
    {
        public DummyCommonUtils() : base() { }

        public override double ThresholdOfYield => 0.04;
        public override BuyOrSellStringClass BuyOrSellString => new BuyOrSellStringClass { Buy = "��", Sell = "��" };

        // �ݒ�l���g���v���p�e�B�����ׂăI�[�o�[���C�h
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
            // �e�X�g���͉������Ȃ�
        }
        protected override void SetupLogger()
        {
            // �e�X�g���͉������Ȃ�
        }
    }
}