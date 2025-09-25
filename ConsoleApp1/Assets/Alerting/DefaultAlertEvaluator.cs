using ConsoleApp1.Assets;

namespace ConsoleApp1.Assets.Alerting
{
    public class DefaultAlertEvaluator : IAlertEvaluator
    {
        public bool ShouldAlert(AssetInfo assetInfo)
        {
            bool result = true;

            if (assetInfo.IsFavorite || assetInfo.IsOwnedNow() || assetInfo.IsJustSold())
            {
                // ã≠êßí ím
            }
            else if (assetInfo.IsCloseToRecordDate() || assetInfo.IsRecordDate() || assetInfo.IsAfterRecordDate())
            {
                if (!assetInfo.IsHighYield()) result = false;
                if (!assetInfo.LatestPrice.OversoldIndicator()) result = false;
                if (!assetInfo.IsHighMarketCap()) result = false;
                if (!assetInfo.IsAnnualProgressOnTrack()) result = false;
            }
            else if (assetInfo.IsCloseToQuarterEnd() || assetInfo.IsQuarterEnd() || assetInfo.IsAfterQuarterEnd())
            {
                if (!assetInfo.IsHighYield()) result = false;
                if (!assetInfo.LatestPrice.OversoldIndicator()) result = false;
                if (!assetInfo.IsHighMarketCap()) result = false;
                if (!assetInfo.IsAnnualProgressOnTrack()) result = false;
            }
            else
            {
                if (!assetInfo.IsHighYield()) result = false;
                if (!assetInfo.LatestPrice.OversoldIndicator()) result = false;
                if (!assetInfo.IsHighMarketCap()) result = false;
                if (!assetInfo.IsAnnualProgressOnTrack()) result = false;
                if (!assetInfo.IsPERUndervalued(true)) result = false;
                if (!assetInfo.IsPBRUndervalued(true)) result = false;
            }

            return result;
        }
    }
}