using CodeSketch.Data;

namespace CodeSketch.Debug
{
    public class DebugToggleAdsRewardSkip : DebugToggle
    {
        protected override bool IsOn 
        {
            get => DataMaster.AdsRewardSkip.Value;
            set => DataMaster.AdsRewardSkip.Value = value;
        }
    }
}