using System.Collections.Generic;
using CodeSketch.Mono;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public class StatManager : MonoSingleton<StatManager>
    {
        protected override bool PersistAcrossScenes => true;

        [SerializeField] Dictionary<Stat, StatBuffSystem> _systems;

        public static Action<string, float> OnLoadBuffSaved;

        protected override void Start()
        {
            base.Start();

            var buffs = DataStatBuff.Values;

            foreach (var buff in buffs)
            {
                if (buff.RemainingTime <= 0)
                {
                    DataStatBuff.Remove(buff.SourceId);
                    continue;
                }
                
                // Code chỗ item tự phải bắt event, và tạo lại buff
                OnLoadBuffSaved?.Invoke(buff.SourceId, buff.RemainingTime);
            }
        }

        public static void Buff(Stat stat, StatBuffValue value)
        {
            GetSystem(stat).Apply(new StatBuff()
            {
                SourceId = value.SourceId,
                RemainingTime = value.Duration,
                Modifier = new StatModifier(value.FlatValue, value.PercentValue)
            });
        }
        
        public static void LoadBuff(Stat stat, StatBuffValue value)
        {
            GetSystem(stat).Load(new StatBuff()
            {
                SourceId = value.SourceId,
                RemainingTime = value.Duration,
                Modifier = new StatModifier(value.FlatValue, value.PercentValue)
            });
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            foreach (var system in _systems.Values)
            {
                if (system != null) system.Tick(deltaTime);
            }
        }

        static StatBuffSystem GetSystem(Stat stat)
        {
            Instance._systems.TryAdd(stat, new StatBuffSystem(stat));
            return Instance._systems[stat];
        }
    }
}
