// See https://aka.ms/new-console-template for more information

using ClosedXML.Excel;
using System.Runtime.InteropServices.Marshalling;
using static WatchList;

internal class MasterList
{
    internal static List<AveragePerPbrDetails> LoadXlsx()
    {
        List<AveragePerPbrDetails> results = new List<AveragePerPbrDetails>();

        // 除外する行番号のリスト
        List<int> excludeRows = new List<int> { 5, 6, 7, 8, 42, 43, 44, 45, 79, 80, 81, 82, 116, 117, 118 };

        // 読み込むワークシート名を指定
        string sheetName = "規模別・業種別（連結）";

        // 読み込み開始行のインデックス（1ベース）
        int startRowIndex = 5;

        // Excelファイルを読み込む
        using (var workbook = new XLWorkbook(CommonUtils.Instance.FilepathOfAveragePerPbrList))
        {
            // 指定したワークシートを取得
            var worksheet = workbook.Worksheet(sheetName);

            if (worksheet != null)
            {
                // 指定行から順番にデータを取得
                var rows = worksheet.RowsUsed()
                    .Where(row => row.RowNumber() >= startRowIndex);

                int rowCount = 0;

                foreach (var row in rows)
                {
                    rowCount++;

                    // 除外行は読み飛ばす
                    if (excludeRows.Contains(row.RowNumber())) continue; 

                    var data = new AveragePerPbrDetails
                    {
                        YearMonth = row.Cell(1).Value.ToString(),
                        Section = row.Cell(2).Value.ToString(),
                        Industry = row.Cell(4).Value.ToString(),
                        NoOfCos = row.Cell(6).Value.ToString(),
                        AveragePer = row.Cell(7).Value.ToString(),
                        AveragePbr = row.Cell(8).Value.ToString(),
                        AverageEarningsPerShare = row.Cell(9).Value.ToString(),
                        AverageNetAssetsPerShare = row.Cell(10).Value.ToString(),
                        WeightedAveragePer = row.Cell(11).Value.ToString(),
                        WeightedAveragePbr = row.Cell(12).Value.ToString(),
                        WeightedAverageEarnings = row.Cell(13).Value.ToString(),
                        WeightedAverageNetAssets = row.Cell(14).Value.ToString(),
                    };
                    results.Add(data);
                }
            }
            else
            {
                Console.WriteLine($"ワークシート '{sheetName}' が見つかりません。");
            }
        }

        return results;
    }

    internal class AveragePerPbrDetails
    {
        public string YearMonth { get; set; }
        public string Section { get; set; }
        public string Industry { get; set; }
        public string NoOfCos { get; set; }
        public string AveragePer { get; set; }
        public string AveragePbr { get; set; }
        public string AverageEarningsPerShare { get; set; }
        public string AverageNetAssetsPerShare { get; set; }
        public string WeightedAveragePer { get; set; }
        public string WeightedAveragePbr { get; set; }
        public string WeightedAverageEarnings { get; set; }
        public string WeightedAverageNetAssets { get; set; }
    }
}