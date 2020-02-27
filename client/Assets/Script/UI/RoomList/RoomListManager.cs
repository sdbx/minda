using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Network;
using Models;
using System;
using UnityEngine.Serialization;

namespace UI
{
    public class RoomListManager : MonoBehaviour
    {
        [SerializeField]
        private RoomObject roomPrefeb;
        [FormerlySerializedAs("SearchInputField")] [SerializeField]
        private InputField searchInputField;
        [SerializeField]
        private ScrollRect scrollRect;
        private Transform _content;
        private List<RoomObject> _roomList = new List<RoomObject>();

        private void Awake()
        {
            _content = scrollRect.transform.GetChild(0).GetChild(0);
            searchInputField.onValueChanged.AddListener((string str) => { RefreshRoomList(); });
        }

        private void Start()
        {
            RefreshRoomList();
        }

        public void Add(Room room)
        {
            var roomObject = Instantiate<RoomObject>(roomPrefeb, parent: _content);
            roomObject.room = room;
            roomObject.roomListManger = this;
            roomObject.index = _roomList.Count;
            _roomList.Add(roomObject);
            roomObject.Refresh();
        }

        private void CreateRoomList(Room[] rooms)
        {
            Array.Sort(rooms, Sorter.SortRoomWithDateTime);
            for (var i = 0; i < rooms.Length; i++)
            {
                Add(rooms[i]);
            }
        }

        public void RefreshRoomList()
        {
            ClearRoomList();
            LobbyServer.Instance.Get<Room[]>($"/rooms/?name={searchInputField.text}", (Room[] rooms, int? err) =>
            {
                if (err != null)
                {
                    Toast.ToastManager.Instance.Add(LanguageManager.GetText("roomreceivefailed"), "Error");
                    return;
                }
                if (rooms == null)
                    return;
                CreateRoomList(rooms);
            });
        }

        public void RefreshRoomList(Room[] rooms)
        {
            ClearRoomList();
            CreateRoomList(rooms);
        }

        public void ClearRoomList()
        {
            foreach (var room in _roomList)
            {
                Destroy(room.gameObject);
            }
            _roomList.Clear();
        }

        public void TryEnter(string roomId)
        {
            LobbyServer.Instance.Get<Room[]>("/rooms/", (Room[] rooms, int? err) =>
            {
                if (err != null)
                {
                    Toast.ToastManager.Instance.Add(LanguageManager.GetText("roomreceivefailed"), "Error");
                    return;
                }
                if (rooms == null)
                    return;

                foreach (var room in rooms)
                {
                    if (room.Id == roomId)
                    {
                        if (room.Ingame)
                        {
                            MessageBox.Instance.Show(LanguageManager.GetText("gamestarted"), (bool agreed) =>
                             {
                                 if (!agreed)
                                     return;
                                 LobbyServer.Instance.EnterRoom(roomId, (bool success) => { });
                             }, "Enter", "Cancel");
                        }
                        else
                        {
                            LobbyServer.Instance.EnterRoom(roomId, (bool success) => { });
                        }
                        return;
                    }
                }
                //방이없음
                Toast.ToastManager.Instance.Add(LanguageManager.GetText("roomdoesntexist"), "Error");
            });

        }
    }
}
