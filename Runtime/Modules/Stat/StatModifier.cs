using System;

namespace CodeSketch.Modules.StatSystem
{
    [Serializable]
    public struct StatModifier
    {
        public float Flat;     // +50
        public float Percent;  // +0.2 = +20%

        public StatModifier(float flat = 0f, float percent = 0f)
        {
            Flat = flat;
            Percent = percent;
        }
    }
}