using System;
using CodeSketch.Mono;
using PrimeTween;
using UnityEngine;

namespace CodeSketch.Patterns.Pool
{
    public class PoolAutoReleaseParticle : MonoCached
    {
        [SerializeField] ParticleSystem _particle;
        [SerializeField] PoolPrefabConfig _pool;

        Tween _tween;

        ParticleSystem.MainModule _main;

        public Action OnCompleted;
        Action _cachedOnDestructable;

        void OnDestroy()
        {
            OnCompleted?.Invoke();
            _tween.Stop();
        }

        protected virtual void Awake()
        {
            _cachedOnDestructable = OnDestructable;
        }

        protected virtual void OnDisable()
        {
            OnCompleted?.Invoke();
        }

        protected virtual void OnEnable()
        {
            _main = _particle.main;
            _main.playOnAwake = false;

            _particle.Play();
            _tween.Stop();

            float duration = _main.duration;
            _tween = Tween.Delay(duration, _cachedOnDestructable, false, false);
        }

        void OnDestructable()
        {
            if (_pool)
            {
                PoolPrefabGlobal.Release(_pool, GameObjectCached);
            }
            else
            {
                Destroy(GameObjectCached);
            }
        }

        void OnValidate()
        {
            if (_particle == null)
                _particle = GetComponent<ParticleSystem>();
            if (_particle == null)
                _particle = GetComponentInChildren<ParticleSystem>();
        }
    }
}
