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
        private ScrollRect scrollRect;
        private Transform content;
        private List<RoomObject> _roomList = new List<RoomObject>();

        private void Awake()
        {
            content = scrollRect.transform.GetChild(0).GetChild(0);
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

        private void CreateRoomList(Room[] rooms, int? err)
        {
            if(err!=null)
            {
                Debug.LogError("방 목록 받아오기 실패 " + err);
                return;
            }
            if(rooms==null)
                return;

            Array.Sort(rooms, Sorter.SortRoomWithDateTime);
            for(int i = 0;i<rooms.Length;i++)
            {
                Add(rooms[i]);
            }
        }
        
        public void RefreshRoomList()
        {
            foreach (var room in _roomList)
            {
                Destroy(room.gameObject);
            }
            _roomList.Clear();
            LobbyServer.instance.Get<Room[]>("/rooms/", CreateRoomList);
        }
    }
}
