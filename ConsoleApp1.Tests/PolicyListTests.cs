using Xunit;
using ClosedXML.Excel;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApp1.Tests
{
    [Collection("CommonUtils collection")]
    public class PolicyListTests
    {
        [Fact]
        public void LoadXlsx_正常系_方針リスト取得()
        {
            // テスト用Excelファイル作成
            var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.AddWorksheet("方針");
                    ws.Cell(1, 1).Value = "方針1";
                    ws.Cell(2, 1).Value = "方針2";
                    ws.Cell(3, 1).Value = "方針3";
                    ws.Cell(4, 1).Value = ""; // ブランク行
                    workbook.SaveAs(tempFile);
                }

                // テスト用パスをセット
                CommonUtils.Instance.FilepathOfPolicyList = tempFile;

                // 実行
                var result = PolicyList.LoadXlsx();

                // 検証
                Assert.Equal(
                    new List<string> { "方針1", "方針2", "方針3" },
                    result
                );
            }
            finally
            {
                // 後始末
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadXlsx_ファイルなし_空リスト()
        {
            CommonUtils.Instance.FilepathOfPolicyList = "not_exist.xlsx";
            var result = PolicyList.LoadXlsx();
            Assert.Empty(result);
        }
    }
}

