using CodeSketch.Data;

namespace CodeSketch.Debug
{
    public class DebugToggleAdsInterSkip : DebugToggle
    {
        protected override bool IsOn
        { 
            get => DataMaster.AdsInterSkip.Value;
            set => DataMaster.AdsInterSkip.Value = value;
        }
    }
}
