namespace ConsoleApp1.Assets.Setups
{
    public interface IAssetSetupStrategy
    {
        void UpdateExecutions(AssetInfo asset, List<ExecutionList.ListDetail> executionList);
        void UpdateAveragePerPbr(AssetInfo asset, List<MasterList.AveragePerPbrDetails> masterList);
    }
}