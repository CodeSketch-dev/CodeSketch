using System;
using UnityEngine;

namespace CodeSketch.Modules.Lifetime
{
    public class LifetimeBinding : MonoBehaviour
    {
        public event Action EventRelease;
        
        void OnDestroy()
        {
            EventRelease?.Invoke();
        }
    }
}
