using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UI
{
    public class MapSelectorToggler : MonoBehaviour
    {
        public enum State
        {
            UnActivated,
            Activating,
            Activated,
            UnActivating,
        }
        [SerializeField]
        private GameObject mapSelector;
        private Vector3 originPosition;
        
        private State state = State.UnActivated;
        public bool isActivated { get { return state == State.Activated; } }
        public bool isUnActivated { get { return state == State.UnActivated; } }

        [SerializeField]
        private float duration = 0.3f;
        private Vector3 unActivatedScale = new Vector3(0f, 0f, 1);

        private CanvasGroup canvasGroup;

        void Start()
        {
            canvasGroup = mapSelector.AddComponent<CanvasGroup>();
        }

        public void ToggleActivation()
        {
            if (state == State.UnActivated)
            {
                Activate();
            }
            else if (state == State.Activated)
            {
                UnActivate();
            }
        }

        public void UnActivate()
        {
            if (state == State.Activated)
            {
                state = State.UnActivating;
                mapSelector.transform.DOScale(unActivatedScale,duration).OnComplete(()=>
                {
                    mapSelector.SetActive(false);
                    state = State.UnActivated;
                });
            }
        }

        public void Activate()
        {
            if (state == State.UnActivated)
            {
                canvasGroup.alpha = 1;
                mapSelector.SetActive(true);
                mapSelector.transform.localScale = unActivatedScale;
                state = State.Activating;
                mapSelector.transform.DOScale(new Vector3(1f,1f,1), duration).OnComplete(() =>
                {
                    state = State.Activated;
                });
            }
        }

    }
}
