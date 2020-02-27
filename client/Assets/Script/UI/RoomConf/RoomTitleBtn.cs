using System.Collections;
using System.Collections.Generic;
using Network;
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
        private RectTransform _rectTransform;

        [SerializeField]
        private Vector2 contextMenuPivot;
        [SerializeField]
        private Vector2 contextMenuPositionPivot;
        private ContextMenu _contextMenu;

        private void Awake()
        {
            _contextMenu = new ContextMenu(contextMenuPivot).Add("Copy Room Code", () =>
            {
                GUIUtility.systemCopyBuffer = GameServer.Instance.connectedRoom.Id;
                Toast.ToastManager.Instance.Add("Copied To Clipboard", "Info");
            });
            _rectTransform = gameObject.GetComponent<RectTransform>();
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
            var size = _rectTransform.rect.size;
            var position = PositionUtils.WorldPosToLocalRectPos(transform.position, parentTransform);
            var deltaPostion = new Vector2(size.x * (contextMenuPositionPivot.x - 0.5f), size.y * (contextMenuPositionPivot.y - 0.5f));
            ContextMenuManager.Instance.Create(position + deltaPostion, _contextMenu);
        }
    }

}
