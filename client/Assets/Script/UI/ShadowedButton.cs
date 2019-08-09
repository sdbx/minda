using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ShadowedButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image shadowImage;
        [SerializeField] private RectTransform buttonTransform;

        public Action onClick;

        private bool isMouseOver;

        private void Awake()
        {
            shadowImage.color = UISettings.instance.colors.transparent;
        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseOver = true;

            var uiSettings = UISettings.instance;
            var duration = uiSettings.durations.shadowShowDuration;

            shadowImage.DOColor(uiSettings.colors.shadowEnabled, duration);
            buttonTransform.DOLocalMoveY(10, duration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;

            var uiSettings = UISettings.instance;
            var duration = uiSettings.durations.shadowShowDuration;

            shadowImage.DOColor(uiSettings.colors.transparent, duration);
            buttonTransform.DOLocalMoveY(0, duration);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            buttonTransform.DOScale(new Vector3(0.97f, 0.97f), 0.1f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            buttonTransform.DOScale(new Vector3(1f, 1f), 0.3f).SetEase(Ease.OutBounce);
        }
    }
}
