namespace ConsoleApp1.Assets.Alerting
{
    public interface IAlertEvaluator
    {
        bool ShouldAlert(AssetInfo assetInfo);
    }
}