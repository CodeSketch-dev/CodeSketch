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
            ParticleSystem.MainModule particleMain = _particle.main;
            particleMain.playOnAwake = false;

            _particle.Play();
            _tween.Stop();

            float duration = _particle != null ? _particle.main.duration : 0f;
            _tween = Tween.Delay(duration, _cachedOnDestructable);
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
