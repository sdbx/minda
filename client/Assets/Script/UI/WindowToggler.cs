using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class WindowToggler : MonoBehaviour
    {
        [SerializeField]
        private float duration;
        private CanvasGroup canvasGroup;
        [SerializeField]
        private bool ActiveFirst;

        private void Awake()
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }
            if(ActiveFirst)
            {
                gameObject.SetActive(true);
                canvasGroup.alpha = 1;
            }
            else
            {
                gameObject.SetActive(false);
                canvasGroup.alpha = 0;
            }
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, duration);
        }

        public void UnActivate()
        {
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, duration).OnComplete(()=>{gameObject.SetActive(false);});
        }
    }
}
