using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class Room : MonoBehaviour
    {
        public RoomInformation roomInformation = null;

        private Text _informationText;
        private Image _shape;

        private Color FocusedColor = Color.black, OriginColor;

        void Start()
        {
            _informationText = gameObject.GetComponentInChildren<Text>();
            _shape = gameObject.GetComponentInChildren<Image>();
            OriginColor = _shape.color;
            Refresh();
        }

        public void Refresh()
        {
            _informationText.text = roomInformation.ToString();
        }

        void OnMouseEnter() 
        {
            Debug.Log("enter");
            _shape.color = FocusedColor;
        }
        void OnmouseExit() 
        {
            _shape.color = OriginColor;
        }
        void Update() 
        {
            Refresh();
        }
    }

}
