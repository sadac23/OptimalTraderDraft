using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace ConsoleApp1.Tests.Analisys
{
    [Collection("CommonUtils collection")]
    public class AlertTests
    {
        [Fact]
        public void SaveFile_正常系_ファイル出力内容を検証()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var originalFilepath = CommonUtils.Instance.FilepathOfAlert;
            CommonUtils.Instance.FilepathOfAlert = tempFile;

            var policyList = new List<string> { "方針A", "方針B" };
            var stockInfoMock = new StockInfoMock { ShouldAlertResult = true, OutputString = "テスト出力" };
            var results = new List<StockInfo> { stockInfoMock };

            try
            {
                // Act
                Alert.SaveFile(results, policyList);

                // Assert
                var lines = File.ReadAllLines(tempFile);
                Assert.Contains("方針：", lines);
                Assert.Contains("方針A", lines);
                Assert.Contains("方針B", lines);
                Assert.Contains("テスト出力", lines);
                Assert.Contains("出力件数：1件", lines);
            }
            finally
            {
                // 後始末
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                CommonUtils.Instance.FilepathOfAlert = originalFilepath;
            }
        }

        // StockInfoのテスト用モック
        private class StockInfoMock : StockInfo
        {
            public bool ShouldAlertResult { get; set; }
            public string OutputString { get; set; } = "";

            public StockInfoMock() : base(new DummyWatchStock()) 
            {
                // 必要なコレクションも初期化（テスト安定化のため）
                Executions = new List<ExecutionList.Execution>();
                Disclosures = new List<Disclosure>();
                ChartPrices = new List<ChartPrice>();
            }

            internal override bool ShouldAlert() => ShouldAlertResult;
            internal override string ToOutputString() => OutputString;

            // 必要に応じてバッジ系もオーバーライド
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
            public override ChartPrice LatestPrice => new LatestPriceMock();

            private class LatestPriceMock : ChartPrice
            {
                public override bool OversoldIndicator() => false;
            }
        }

        // ダミーWatchStockクラス
        private class DummyWatchStock : WatchList.WatchStock
        {
            public DummyWatchStock()
            {
                Code = "TEST";
                Classification = "1";
                IsFavorite = "0";
                Memo = "テスト";
            }
        }
    }
}