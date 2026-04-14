using CodeSketch.Mono;
using UnityEngine;

namespace CodeSketch.Optimize
{
    /// <summary>
    /// OptCollisionRegister with three generic parameters: registers this instance for three types.
    /// Allows syntax: OptCollisionRegister<T1,T2,T3>
    /// </summary>
    public abstract class OptCollisionRegister<T1, T2, T3> : MonoCached
        where T1 : class
        where T2 : class
        where T3 : class
    {
        [SerializeField] protected Collider[] _colliders;
        [SerializeField] protected bool _manuallyAssignColliders = false;

        protected virtual void Awake()
        {
            if (_colliders == null || _colliders.Length == 0)
            {
                _colliders = GetComponentsInChildren<Collider>(true);
            }
        }

        protected virtual void OnEnable()
        {
            if (_colliders == null || _colliders.Length == 0)
                return;

            OptCollisionLookup.Register(typeof(T1), this, _colliders);
            OptCollisionLookup.Register(typeof(T2), this, _colliders);
            OptCollisionLookup.Register(typeof(T3), this, _colliders);
        }

        protected virtual void OnDisable()
        {
            if (_colliders == null || _colliders.Length == 0)
                return;

            OptCollisionLookup.Unregister(typeof(T1), this, _colliders);
            OptCollisionLookup.Unregister(typeof(T2), this, _colliders);
            OptCollisionLookup.Unregister(typeof(T3), this, _colliders);
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !_manuallyAssignColliders)
            {
                _colliders = GetComponentsInChildren<Collider>(true);
            }
#endif
        }
    }
}
