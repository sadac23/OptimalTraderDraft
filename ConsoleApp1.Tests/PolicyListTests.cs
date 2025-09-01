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
        public void LoadXlsx_����n_���j���X�g�擾()
        {
            // �e�X�g�pExcel�t�@�C���쐬
            var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.AddWorksheet("���j");
                    ws.Cell(1, 1).Value = "���j1";
                    ws.Cell(2, 1).Value = "���j2";
                    ws.Cell(3, 1).Value = "���j3";
                    ws.Cell(4, 1).Value = ""; // �u�����N�s
                    workbook.SaveAs(tempFile);
                }

                // �e�X�g�p�p�X���Z�b�g
                CommonUtils.Instance.FilepathOfPolicyList = tempFile;

                // ���s
                var result = PolicyList.LoadXlsx();

                // ����
                Assert.Equal(
                    new List<string> { "���j1", "���j2", "���j3" },
                    result
                );
            }
            finally
            {
                // ��n��
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadXlsx_�t�@�C���Ȃ�_�󃊃X�g()
        {
            CommonUtils.Instance.FilepathOfPolicyList = "not_exist.xlsx";
            var result = PolicyList.LoadXlsx();
            Assert.Empty(result);
        }
    }
}

