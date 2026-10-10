using System.Collections.Generic;

namespace CodeSketch.Modules.StatSystem
{
    public sealed class Stat
    {
        public float BaseValue { get; private set; }
        public float CurrentValue { get; private set; }

        readonly List<StatModifierInstance> _modifiers = new();

        public Stat(float baseValue)
        {
            BaseValue = baseValue;
            Recalculate();
        }

        public void SetBase(float value)
        {
            BaseValue = value;
            Recalculate();
        }

        public void AddModifier(StatModifierInstance modifier)
        {
            _modifiers.Add(modifier);
            Recalculate();
        }

        public void RemoveModifier(StatModifierInstance modifier)
        {
            _modifiers.Remove(modifier);
            Recalculate();
        }

        public void Tick(float deltaTime)
        {
            bool dirty = false;

            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].Tick(deltaTime))
                {
                    _modifiers.RemoveAt(i);
                    dirty = true;
                }
            }

            if (dirty)
                Recalculate();
        }

        void Recalculate()
        {
            float flatSum = 0f;
            float percentSum = 0f;

            foreach (var m in _modifiers)
            {
                flatSum += m.Flat;
                percentSum += m.Percent;
            }

            CurrentValue = (BaseValue + flatSum) * (1f + percentSum);
        }
    }
}