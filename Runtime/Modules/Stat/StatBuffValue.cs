using Sirenix.OdinInspector;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    [System.Serializable]
    public struct StatBuffValue
    {
        public string SourceId;
        [LabelText("Flat")]
        public float FlatValue;
        [LabelText("Percent")]
        public float PercentValue;

        [LabelText("Buff Time")]
        public float Duration;
    }
}
