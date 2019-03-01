using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System;

namespace UI.Toast
{
    [Serializable]
    public struct ToastType
    {
        public Texture icon;
        public Color backGroundColor;
        public Color textColor;
    }
    public class ToastMessage : MonoBehaviour
    {
        public ToastType toastType;
        public float lifeTime = 3;
        public string message = "Toast";
        public float animationDuration = 3;
        private float distanceToGoDown;

        [SerializeField]
        private Text text;
        [SerializeField]
        private Image backGround;
        [SerializeField]
        private Image backGround2;
        [SerializeField]
        private RawImage icon;
        private CanvasGroup canvasGroup;

        public Action<ToastMessage> destroyedCallback;

        private void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }

        private void Start()
        {
            text.text = message;
            backGround.color = toastType.backGroundColor;
            backGround2.color = toastType.backGroundColor;
            icon.texture = toastType.icon;

        }

        public void Appear()
        {
            DoFade(1);
        }

        public void Destroy()
        {
            MoveDown();
            DoFade(0).OnComplete(() =>
            {
                if(destroyedCallback!=null)
                {
                    destroyedCallback(this);
                }
                Destroy(gameObject);
            });
        }

        public DG.Tweening.Tweener MoveDown()
        {
            return transform.DOLocalMove(new Vector3(0, transform.localPosition.y + distanceToGoDown, 0), animationDuration);
        }

        public void Init(string message, float lifeTime, ToastType toastType, float animationDuration, float distanceToGoDown)
        {
            this.message = message;
            this.lifeTime = lifeTime;
            this.toastType = toastType;
            this.animationDuration = animationDuration;
            this.distanceToGoDown = distanceToGoDown;
            StartCoroutine(LifeTimeEnd());
        }

        private IEnumerator LifeTimeEnd()
        {
            yield return new WaitForSeconds(lifeTime);
            Destroy();
        }

        private DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> DoFade(float endValue)
        {
            return DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, endValue, animationDuration);
        }
    }
}