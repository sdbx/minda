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
        private Vector3 _originPosition;

        private State _state = State.UnActivated;
        public bool isActivated { get { return _state == State.Activated; } }
        public bool isUnActivated { get { return _state == State.UnActivated; } }

        [SerializeField]
        private float duration = 0.3f;
        private Vector3 _unActivatedScale = new Vector3(0f, 0f, 1);

        private CanvasGroup _canvasGroup;

        public void Awake()
        {
            _canvasGroup = mapSelector.GetComponent<CanvasGroup>();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void ToggleActivation()
        {
            if (_state == State.UnActivated)
            {
                Activate();
            }
            else if (_state == State.Activated)
            {
                UnActivate();
            }
        }

        public void UnActivate()
        {
            if (_state == State.Activated)
            {
                _canvasGroup.interactable = false;
                _state = State.UnActivating;
                mapSelector.transform.DOScale(_unActivatedScale, duration).SetEase(Ease.InQuart).OnComplete(() =>
                 {
                     _canvasGroup.blocksRaycasts = false;
                     mapSelector.SetActive(false);
                     _state = State.UnActivated;
                 });
            }
        }

        public void Activate()
        {
            if (_state == State.UnActivated)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
                _canvasGroup.alpha = 1;
                mapSelector.SetActive(true);
                mapSelector.transform.localScale = _unActivatedScale;
                _state = State.Activating;
                mapSelector.transform.DOScale(new Vector3(1f, 1f, 1), duration).SetEase(Ease.OutQuart).OnComplete(() =>
                  {
                      _state = State.Activated;
                  });
            }
        }

    }
}
