using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace UI
{
    public class UserListBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private bool isMouseEnter;
        private RectTransform rectTransform;
        [SerializeField]
        private UserList userList;
        private RectTransform userListRectTransfom;
        [SerializeField]
        private Vector2 endPivot = new Vector2();
        private Vector2 originPivot;
        [SerializeField]
        private float duration = 0.5f;
        private bool Activated = false;
        void Awake()
        {
            rectTransform = gameObject.GetComponent<RectTransform>();
            originPivot = rectTransform.pivot;
            userListRectTransfom = userList.GetComponent<RectTransform>();
        }

        void Update()
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!Activated)
                rectTransform.DOPivot(endPivot,duration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(!Activated)
                rectTransform.DOPivot(originPivot,duration);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Activated = true;
            rectTransform.DOPivotX(1,duration/2).OnComplete(()=>
            {
                rectTransform.DOPivotX(0,duration);
                userListRectTransfom.DOPivotX(1,duration*2).SetEase(Ease.InOutSine);
            });
            
        }
    }
}