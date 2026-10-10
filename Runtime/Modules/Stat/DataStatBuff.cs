using System.Collections.Generic;
using System.Linq;
using CodeSketch.Data;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public class DataStatBuff : DataBlock<DataStatBuff>
    {
        [SerializeField] Dictionary<string, StatBuffData> _data;

        public static List<StatBuffData> Values => Instance._data.Values.ToList();
        
        public static StatBuffData Get(string sourceID)
        {
            Instance._data.TryAdd(sourceID, new StatBuffData());
            return Instance._data[sourceID];
        }

        public static void Remove(string sourceID)
        {
            Instance._data.Remove(sourceID);
        }

        protected override void Init()
        {
            base.Init();

            _data ??= new Dictionary<string, StatBuffData>();
        }
    }
}
