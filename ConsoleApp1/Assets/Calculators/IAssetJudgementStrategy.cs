using ConsoleApp1.Assets;

namespace ConsoleApp1.Assets.Calculators
{
    /// <summary>
    /// 判定系ストラテジーのインターフェース
    /// </summary>
    public interface IAssetJudgementStrategy
    {
        bool IsPERUndervalued(AssetInfo asset, bool isLenient = false);
        bool IsPBRUndervalued(AssetInfo asset, bool isLenient = false);
        bool IsROEAboveThreshold(AssetInfo asset);
        bool IsAnnualProgressOnTrack(AssetInfo asset);
        bool IsHighYield(AssetInfo asset);
        bool IsHighMarketCap(AssetInfo asset);

        // 追加: 他の判定メソッド
        bool IsCloseToDividendRecordDate(AssetInfo asset);
        bool IsCloseToShareholderBenefitRecordDate(AssetInfo asset);
        bool IsCloseToQuarterEnd(AssetInfo asset);
        bool IsAfterQuarterEnd(AssetInfo asset);
        bool IsQuarterEnd(AssetInfo asset);
        bool IsJustSold(AssetInfo asset);
        bool IsOwnedNow(AssetInfo asset);
        bool IsGoldenCrossPossible(AssetInfo asset);
        bool HasRecentStockSplitOccurred(AssetInfo asset);
        bool ShouldAverageDown(AssetInfo asset, ExecutionList.Execution e);
        bool IsGranvilleCase1Matched(AssetInfo asset);
        bool IsGranvilleCase2Matched(AssetInfo asset);
        bool HasDisclosure(AssetInfo asset);
        bool IsRecordDate(AssetInfo asset);
        bool IsAfterRecordDate(AssetInfo asset);
        bool IsCloseToRecordDate(AssetInfo asset);
        bool ExtractAndValidateDateWithinOneMonth(AssetInfo asset);
    }
}