using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class ObjectToggler : MonoBehaviour
    {
        [SerializeField]
        private float duration;
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private bool ActiveFirst;
        [SerializeField]
        private bool onlyToggleAlpha;

        public bool isActivated{get;private set;}

        private bool isInitalized;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if(isInitalized)
                return;

            if (ActiveFirst)
            {
                if (!onlyToggleAlpha)
                    gameObject.SetActive(true);
                canvasGroup.alpha = 1;
            }
            else
            {
                if (!onlyToggleAlpha)
                    gameObject.SetActive(false);
                canvasGroup.alpha = 0;
            }
            isInitalized = true;
        }

        public void SetActivation(bool activation)
        {
            Init();
            if(activation != isActivated)
            {
                if(activation)
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
            Init();
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, duration);

            if (!onlyToggleAlpha)
                gameObject.SetActive(true);

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isActivated = true;
        }

        public void UnActivate()
        {
            Init();
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, duration).OnComplete(()=>
            {
                if(!onlyToggleAlpha)
                    gameObject.SetActive(false);

                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                isActivated = false;
            });
        }

        public void Toggle()
        {
            SetActivation(!isActivated);
        }
    }
}
