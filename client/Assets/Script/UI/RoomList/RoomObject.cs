using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Models;
using Network;

namespace UI
{
    public class RoomObject : MonoBehaviour
    {
        public RoomListManager roomListManger;
        [SerializeField]
        private Text informationText;
        [SerializeField]
        private Button button;

        public int index = -1;
        public Room room = null;

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        public void Refresh()
        {
            informationText.text = CreateImformationText();
        }

        private string CreateImformationText()
        {
            return $"#{index + 1}  {room.Conf.Name}";
        }

        private void OnClick()
        {
            roomListManger.TryEnter(room.Id);
        }
    }

}
