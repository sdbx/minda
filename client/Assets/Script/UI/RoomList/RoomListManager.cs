using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Game;
using UnityEngine.SceneManagement;
using Network;
using Models;
using Scene;
using System;

namespace UI
{
    public class RoomListManager : MonoBehaviour
    {
        [SerializeField]
        private RoomObject roomPrefeb;
        [SerializeField]
        private InputField SearchInputField;
        [SerializeField]
        private ScrollRect scrollRect;
        private Transform content;
        private List<RoomObject> _roomList = new List<RoomObject>();

        private void Awake()
        {
            content = scrollRect.transform.GetChild(0).GetChild(0);
            SearchInputField.onValueChanged.AddListener((string str)=>{RefreshRoomList();});
        }

        void Start()
        {
            RefreshRoomList();
        }

        public void Add(Room room)
        {
            RoomObject roomObject = Instantiate<RoomObject>(roomPrefeb,parent:content);
            roomObject.room = room;
            roomObject.roomListManger = this;
            roomObject.index = _roomList.Count;
            _roomList.Add(roomObject);
            roomObject.Refresh();
        }

        private void CreateRoomList(Room[] rooms)
        {
            Array.Sort(rooms, Sorter.SortRoomWithDateTime);
            for(int i = 0;i<rooms.Length;i++)
            {
                Add(rooms[i]);
            }
        }
        
        public void RefreshRoomList()
        {
            ClearRoomList();
            LobbyServer.instance.Get<Room[]>($"/rooms/?name={SearchInputField.text}", (Room[] rooms, int? err) =>
            {
                if (err != null)
                {
                    Toast.ToastManager.instance.Add(LanguageManager.GetText("roomreceivefailed"), "Error");
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
            LobbyServer.instance.Get<Room[]>("/rooms/", (Room[] rooms, int? err) =>
            {
                if (err != null)
                {
                    Toast.ToastManager.instance.Add(LanguageManager.GetText("roomreceivefailed"), "Error");
                    return;
                }
                if (rooms == null)
                    return;

                foreach(var room in rooms)
                {
                    if(room.id == roomId)
                    {
                        if(room.ingame)
                        {
                            MessageBox.instance.Show(LanguageManager.GetText("gamestarted"),(bool agreed)=>
                            {
                                if(!agreed)
                                    return;
                                LobbyServer.instance.EnterRoom(roomId, (bool success) =>{});
                            },"Enter","Cancel");
                        }
                        else 
                        {
                            LobbyServer.instance.EnterRoom(roomId, (bool success) =>{});
                        }
                        return;
                    }
                }
                //방이없음
                Toast.ToastManager.instance.Add(LanguageManager.GetText("roomdoesntexist"),"Error");
            });

        }
    }
}
