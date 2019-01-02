using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Models;

namespace UI
{
    public class RoomObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public int index = -1;
        public RoomListManager roomListManger;
        private Text informationText;
        private Image shape;

        [SerializeField]
        private Color _focusedColor = Color.black, 
                      _selectedColor = Color.black;
        private Color originColor;

        private bool _selected = false;

        public Room room = null;

        private void Start()
        {
            informationText = gameObject.GetComponentInChildren<Text>();
            shape = gameObject.GetComponentInChildren<Image>();
            originColor = shape.color;
            Refresh();
        }

        private void Update()
        {
            Refresh();
            if(_selected)
                shape.color = _selectedColor;
        }

        public void Refresh()
        {
            informationText.text = CreateImformationText();
        }

        public void Select()
        {
            _selected = true;
        }

        public void UnSelect()
        {
            _selected = false;
            shape.color = originColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!_selected)
                shape.color = _focusedColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            shape.color = originColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!_selected)
            {
                roomListManger.SelectRoom(index);
                return;
            }
            roomListManger.EnterRoom(room);
        }

        private string CreateImformationText()
        {
            return $"#{index+1}  {room.conf.name}";
        }
    }

}
