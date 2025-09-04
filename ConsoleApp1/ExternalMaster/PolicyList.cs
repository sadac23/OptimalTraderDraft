// See https://aka.ms/new-console-template for more information
using ClosedXML.Excel;

namespace ConsoleApp1.ExternalMaster
{
    public class PolicyList
    {
        public static List<string> LoadXlsx()
        {
            try
            {
                using var workbook = new XLWorkbook(CommonUtils.Instance.FilepathOfPolicyList);
                var worksheet = workbook.Worksheet("方針");
                return ExtractPolicyItems(worksheet);
            }
            catch
            {
                return new List<string>();
            }
        }

        // テストしやすいようにpublic/internalにしても良い
        internal static List<string> ExtractPolicyItems(IXLWorksheet worksheet)
        {
            var result = new List<string>();
            foreach (var row in worksheet.RowsUsed())
            {
                var cellValue = row.Cell(1).GetString();
                if (string.IsNullOrWhiteSpace(cellValue)) break;
                result.Add(cellValue);
            }
            return result;
        }
    }
}