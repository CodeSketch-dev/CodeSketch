using System;
using System.Collections;
using UnityEngine;

namespace CodeSketch.Modules.AdsSystem
{
    public class AdsLoading : MonoBehaviour
    {
        public static AdsLoading INSTANCE;

        public float displayDuration = 2.0f;
        public GameObject fullRect;
        public GameObject smallRect;

        Coroutine _delayRoutine;

        public virtual bool IsAdsBreakSmallRect { get; set; } = true;
        
        void Awake()
        {
            // Singleton persistent instance
            if (INSTANCE != null && INSTANCE != this)
            {
                Destroy(gameObject);
                return;
            }
            
            INSTANCE = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Show(Action actionComplete)
        {
            if (IsAdsBreakSmallRect)
            {
                smallRect.SetActive(true);
                fullRect.SetActive(false);
            }
            else
            {
                smallRect.SetActive(false);
                fullRect.SetActive(true);
            }

            if (_delayRoutine != null) StopCoroutine(_delayRoutine);
            _delayRoutine = StartCoroutine(DelayToHide(actionComplete));
        }

        IEnumerator DelayToHide(Action actionComplete)
        {
            yield return new WaitForSeconds(displayDuration);

            smallRect.SetActive(false);
            fullRect.SetActive(false);
            actionComplete?.Invoke();
        }
    }
}