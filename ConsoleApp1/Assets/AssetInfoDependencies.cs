using ConsoleApp1.Assets.Alerting;
using ConsoleApp1.Assets.Calculators;
using ConsoleApp1.Assets.Repositories;
using ConsoleApp1.Assets.Setups;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

namespace ConsoleApp1.Assets
{
    public class AssetInfoDependencies
    {
        public IExternalSourceUpdatable Updater { get; set; }
        public IOutputFormattable Formatter { get; set; }
        public IAssetRepository Repository { get; set; }
        public IAssetJudgementStrategy JudgementStrategy { get; set; }
        public IAssetCalculator Calculator { get; set; }
        public IAssetSetupStrategy SetupStrategy { get; set; }
        public IAlertEvaluator AlertEvaluator { get; set; } // ’Ç‰Á
    }
}