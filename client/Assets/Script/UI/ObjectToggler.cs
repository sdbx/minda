using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class ObjectToggler : MonoBehaviour
    {
        [SerializeField]
        private float duration;
        [SerializeField]
        private CanvasGroup canvasGroup;
        [FormerlySerializedAs("ActiveFirst")] [SerializeField]
        private bool activeFirst;
        [SerializeField]
        private bool onlyToggleAlpha;

        public bool isActivated { get; private set; }

        private bool _isInitalized;
        private bool _isAnimating = false;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (_isInitalized)
                return;

            if (activeFirst)
            {
                if (!onlyToggleAlpha)
                    gameObject.SetActive(true);
                canvasGroup.alpha = 1;
                isActivated = true;
            }
            else
            {
                if (!onlyToggleAlpha)
                    gameObject.SetActive(false);
                canvasGroup.alpha = 0;
            }
            _isInitalized = true;
        }

        public void SetActivation(bool activation)
        {
            Init();
            if (activation != isActivated)
            {
                if (activation)
                {
                    Activate();
                }
                else
                {
                    UnActivate();
                }
            }
        }

        public void Activate()
        {
            if (isActivated)
                return;
            isActivated = true;
            Init();
            _isAnimating = true;
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, duration).OnComplete(() =>
            {
                _isAnimating = false;
            });
            if (!onlyToggleAlpha)
                gameObject.SetActive(true);

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void UnActivate()
        {
            if (!isActivated)
                return;
            isActivated = false;
            Init();
            _isAnimating = true;
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, duration).OnComplete(() =>
            {
                if (!onlyToggleAlpha)
                    gameObject.SetActive(false);

                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                _isAnimating = false;
            });
        }

        public void UnActivateIfNotAnimating()
        {
            if (_isAnimating)
                return;
            UnActivate();
        }

        public void Toggle()
        {
            SetActivation(!isActivated);
        }
    }
}
