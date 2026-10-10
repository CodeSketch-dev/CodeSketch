using System.Collections.Generic;
using System.Diagnostics;

namespace CodeSketch.Modules.StatSystem
{
    public sealed class StatBuffSystem
    {
        readonly Dictionary<string, StatBuff> _activeBuffs = new();
        readonly Dictionary<string, StatModifierInstance> _instances = new();

        readonly List<StatBuff> _buffsToRemove = new();
        
        readonly Stat _stat;

        public StatBuffSystem(Stat stat)
        {
            _stat = stat;
        }

        /// <summary>
        /// Apply buff theo SourceId.
        /// Rule: cùng SourceId → refresh duration
        /// </summary>
        public void Apply(StatBuff buff)
        {
            var data = DataStatBuff.Get(buff.SourceId);
            data.RemainingTime += buff.RemainingTime;

            var instance = new StatModifierInstance(
                buff.Modifier,
                buff.RemainingTime
            );

            _activeBuffs[buff.SourceId] = buff;
            _instances[buff.SourceId] = instance;
            _stat.AddModifier(instance);
        }
        
        public void Load(StatBuff buff)
        {
            var instance = new StatModifierInstance(
                buff.Modifier,
                buff.RemainingTime
            );

            _activeBuffs[buff.SourceId] = buff;
            _instances[buff.SourceId] = instance;
            _stat.AddModifier(instance);
        }

        public void Tick(float deltaTime)
        {
            
        }

        public StatBuff[] GetActiveBuffs()
        {
            var list = new StatBuff[_activeBuffs.Count];
            int i = 0;
            foreach (var b in _activeBuffs.Values)
                list[i++] = b;
            return list;
        }

        public void Clear()
        {
            _activeBuffs.Clear();
            _instances.Clear();
        }
    }
}