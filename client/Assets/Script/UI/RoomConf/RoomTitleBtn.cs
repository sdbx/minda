using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI
{
    public class RoomTitleBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        [SerializeField]
        private DisplayChanger displayChanger;

        [SerializeField]
        private RectTransform parentTransform;
        private RectTransform rectTransform;

        [SerializeField]
        private Vector2 contextMenuPivot;
        [SerializeField]
        private Vector2 contextMenuPositionPivot;
        private ContextMenu contextMenu;

        private void Awake()
        {
            contextMenu = new ContextMenu(contextMenuPivot).Add("Edit Room Setting", null).Add("Copy Room Code", null).Add("Copy Invite Link",null);
            rectTransform = gameObject.GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            displayChanger.SetMode("Over");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            displayChanger.SetOrigin();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            displayChanger.SetMode("Pressed");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            displayChanger.SetOrigin();
            var size = rectTransform.rect.size;
            var position = PositionUtils.WorldPosToLocalRectPos(transform.position,parentTransform);
            var deltaPostion = new Vector2(size.x * (contextMenuPositionPivot.x-0.5f), size.y*(contextMenuPositionPivot.y-0.5f) );
            ContextMenuManager.instance.Create(position+deltaPostion,contextMenu);
        }
    }

}
