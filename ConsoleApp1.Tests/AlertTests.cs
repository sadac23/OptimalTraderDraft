using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace ConsoleApp1.Tests
{
    [Collection("CommonUtils collection")]
    public class AlertTests
    {
        [Fact]
        public void SaveFile_����n_�t�@�C���o�͓��e������()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var originalFilepath = CommonUtils.Instance.FilepathOfAlert;
            CommonUtils.Instance.FilepathOfAlert = tempFile;

            var policyList = new List<string> { "���jA", "���jB" };
            var stockInfoMock = new StockInfoMock { ShouldAlertResult = true, OutputString = "�e�X�g�o��" };
            var results = new List<StockInfo> { stockInfoMock };

            try
            {
                // Act
                Alert.SaveFile(results, policyList);

                // Assert
                var lines = File.ReadAllLines(tempFile);
                Assert.Contains("���j�F", lines);
                Assert.Contains("���jA", lines);
                Assert.Contains("���jB", lines);
                Assert.Contains("�e�X�g�o��", lines);
                Assert.Contains("�o�͌����F1��", lines);
            }
            finally
            {
                // ��n��
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                CommonUtils.Instance.FilepathOfAlert = originalFilepath;
            }
        }

        // StockInfo�̃e�X�g�p���b�N
        private class StockInfoMock : StockInfo
        {
            public bool ShouldAlertResult { get; set; }
            public string OutputString { get; set; } = "";

            public StockInfoMock() : base(new DummyWatchStock()) 
            {
                // �K�v�ȃR���N�V�������������i�e�X�g���艻�̂��߁j
                this.Executions = new List<ExecutionList.Execution>();
                this.Disclosures = new List<Disclosure>();
                this.ChartPrices = new List<StockInfo.ChartPrice>();
            }

            internal override bool ShouldAlert() => ShouldAlertResult;
            internal override string ToOutputString() => OutputString;

            // �K�v�ɉ����ăo�b�W�n���I�[�o�[���C�h
            public override bool IsFavorite => false;
            public override bool IsOwnedNow() => false;
            internal override bool IsCloseToRecordDate() => false;
            internal override bool IsRecordDate() => false;
            internal override bool IsAfterRecordDate() => false;
            internal override bool IsQuarterEnd() => false;
            internal override bool IsCloseToQuarterEnd() => false;
            internal override bool IsAfterQuarterEnd() => false;
            internal override bool IsJustSold() => false;
            internal override bool HasDisclosure() => false;
            public override StockInfo.ChartPrice LatestPrice => new LatestPriceMock();

            private class LatestPriceMock : StockInfo.ChartPrice
            {
                public override bool OversoldIndicator() => false;
            }
        }

        // �_�~�[WatchStock�N���X
        private class DummyWatchStock : WatchList.WatchStock
        {
            public DummyWatchStock()
            {
                this.Code = "TEST";
                this.Classification = "1";
                this.IsFavorite = "0";
                this.Memo = "�e�X�g";
            }
        }
    }
}