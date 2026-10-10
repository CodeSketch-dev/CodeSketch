using System;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    [Serializable]
    public sealed class StatBuff
    {
        public string SourceId;        // itemId / skillId
        public float RemainingTime;    // game time
        public StatModifier Modifier;
    }
}
