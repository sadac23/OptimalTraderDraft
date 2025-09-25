using ConsoleApp1.Assets.Alerting;
using ConsoleApp1.Assets.Calculators;
using ConsoleApp1.Assets.Repositories;
using ConsoleApp1.Assets.Setups;

namespace ConsoleApp1.Assets
{
    public static class AssetInfoFactory
    {
        public static AssetInfo Create(WatchList.WatchStock watchStock)
        {
            var deps = new AssetInfoDependencies
            {
                Repository = new AssetRepository(),
                JudgementStrategy = new DefaultAssetJudgementStrategy(),
                Calculator = new DefaultAssetCalculator(),
                SetupStrategy = new DefaultAssetSetupStrategy(),
                AlertEvaluator = new DefaultAlertEvaluator() // í«â¡
            };

            // îhê∂ÉNÉâÉXÇ≤Ç∆Ç…Updater/FormatterÇêÿÇËë÷Ç¶
            if (watchStock.Classification == CommonUtils.Instance.Classification.JapaneseETFs)
            {
                deps.Updater = new JapaneseETFUpdater();
                deps.Formatter = new JapaneseETFFormatter();
                return new JapaneseETFInfo(watchStock, deps);
            }
            else if (watchStock.Classification == CommonUtils.Instance.Classification.Indexs)
            {
                deps.Updater = new IndexUpdater();
                deps.Formatter = new IndexFormatter();
                return new IndexInfo(watchStock, deps);
            }
            else
            {
                deps.Updater = new JapaneseStockUpdater();
                deps.Formatter = new JapaneseStockFormatter();
                return new JapaneseStockInfo(watchStock, deps);
            }
        }
    }
}