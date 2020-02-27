using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace UI
{
    public class UserListBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private bool _isMouseEnter;
        private RectTransform _rectTransform;
        [SerializeField]
        private UserList userList;
        private RectTransform _userListRectTransfom;
        [SerializeField]
        private Vector2 endPivot = new Vector2();
        private Vector2 _originPivot;
        [SerializeField]
        private float duration = 0.5f;
        private bool _activated = false;

        private void Awake()
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
            _originPivot = _rectTransform.pivot;
            _userListRectTransfom = userList.GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_activated)
                _rectTransform.DOPivot(endPivot, duration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_activated)
                _rectTransform.DOPivot(_originPivot, duration);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Active();
        }

        public void Active()
        {
            _activated = true;
            _rectTransform.DOPivotX(1, duration / 2).OnComplete(() =>
               {
                   _rectTransform.DOPivotX(0, duration);
                   _userListRectTransfom.DOPivotX(1, duration * 2).SetEase(Ease.InOutSine);
               });
        }

        public void UnActive()
        {
            _userListRectTransfom.DOPivotX(0, duration * 2).SetEase(Ease.InOutSine).OnComplete(() =>
               {
                   _rectTransform.DOPivotX(_originPivot.x, duration).OnComplete(() => { _activated = false; });
               });
        }
    }
}
