namespace ConsoleApp1.Assets.Setups
{
    public class DefaultAssetSetupStrategy : IAssetSetupStrategy
    {
        public void UpdateExecutions(AssetInfo asset, List<ExecutionList.ListDetail> executionList)
        {
            asset.Executions = ExecutionList.GetExecutions(executionList, asset.Code).OrderBy(e => e.Date).ToList();
        }

        public void UpdateAveragePerPbr(AssetInfo asset, List<MasterList.AveragePerPbrDetails> masterList)
        {
            try
            {
                // 市場区分(マスタ, 外部サイト)
                var sectionTable = new Dictionary<string, string>
                {
                    { "スタンダード市場", "東証Ｓ" },
                    { "プライム市場", "東証Ｐ" },
                    { "グロース市場", "東証Ｇ" },
                };

                // 種別(マスタ, 外部サイト)
                var industryTable = new Dictionary<string, string>
                {
                    { "1 水産・農林業", "水産・農林業" },
                    { "2 鉱業", "鉱業" },
                    { "3 建設業", "建設業" },
                    { "4 食料品", "食料品" },
                    { "5 繊維製品", "繊維製品" },
                    { "6 パルプ・紙", "パルプ・紙" },
                    { "7 化学", "化学" },
                    { "8 医薬品", "医薬品" },
                    { "9 石油・石炭製品", "石油・石炭製品" },
                    { "10 ゴム製品", "ゴム製品" },
                    { "11 ガラス・土石製品", "ガラス・土石" },
                    { "12 鉄鋼", "鉄鋼" },
                    { "13 非鉄金属", "非鉄金属" },
                    { "14 金属製品", "金属製品" },
                    { "15 機械", "機械" },
                    { "16 電気機器", "電気機器" },
                    { "17 輸送用機器", "輸送用機器" },
                    { "18 精密機器", "精密機器" },
                    { "19 その他製品", "その他製品" },
                    { "20 電気・ガス業", "電気・ガス" },
                    { "21 陸運業", "陸運業" },
                    { "22 海運業", "海運業" },
                    { "23 空運業", "空運業" },
                    { "24 倉庫・運輸関連業", "倉庫・運輸" },
                    { "25 情報・通信業", "情報・通信" },
                    { "26 卸売業", "卸売業" },
                    { "27 小売業", "小売業" },
                    { "28 銀行業", "銀行業" },
                    { "29 証券、商品先物取引業", "証券" },
                    { "30 保険業", "保険" },
                    { "31 その他金融業", "その他金融" },
                    { "32 不動産業", "不動産" },
                    { "33 サービス業", "サービス" },
                };

                foreach (var details in masterList)
                {
                    bool sectionMatching = false;
                    bool industryMatching = false;

                    if (sectionTable.ContainsKey(details.Section))
                    {
                        if (!string.IsNullOrEmpty(asset.Section) && asset.Section.Contains(sectionTable[details.Section]))
                        {
                            sectionMatching = true;
                        }
                    }

                    if (industryTable.ContainsKey(details.Industry))
                    {
                        if (!string.IsNullOrEmpty(asset.Industry) && asset.Industry.Contains(industryTable[details.Industry]))
                        {
                            industryMatching = true;
                        }
                    }

                    if (sectionMatching && industryMatching)
                    {
                        asset.AveragePer = CommonUtils.Instance.GetDouble(details.AveragePer);
                        asset.AveragePbr = CommonUtils.Instance.GetDouble(details.AveragePbr);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}