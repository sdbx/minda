﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Game;
using UnityEngine.SceneManagement;
using Network;
using Models;
using Scene;

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
        
        private int _selectedRoomIndex = -1;

        private void Awake()
        {
            content = scrollRect.transform.GetChild(0).GetChild(0);
        }

        public void Add(Room room)
        {
            RoomObject roomObject = Instantiate<RoomObject>(roomPrefeb,parent:content);
            roomObject.room = room;
            roomObject.roomListManger = this;
            roomObject.index = _roomList.Count;
            _roomList.Add(roomObject);
        }

        public void SelectRoom(int index)
        {
            Debug.Log(index);
            UnSelectRoom();
            _roomList[index].Select();
            _selectedRoomIndex = index;
        }

        public void UnSelectRoom()
        {
            if(_selectedRoomIndex==-1)
                return;
            _roomList[_selectedRoomIndex].UnSelect();
            _selectedRoomIndex = -1;
        }

        void Start()
        {
            NetworkManager.instance.Get<Room[]>("/rooms/",CreateRoomList);
        }

        private void CreateRoomList(Room[] rooms)
        {
            if(rooms==null)
                return;
            for(int i = 0;i<rooms.Length;i++)
            {
                Add(rooms[i]);
            }
        }
        public void EnterRoom(Room room)
        {
            NetworkManager.instance.Put("/rooms/" + room.id+"/", "", (JoinRoomResult joinRoomResult) =>{
                     Debug.Log(joinRoomResult.addr);
                     var Addr = joinRoomResult.addr.Split(':');
                     NetworkManager.instance.EnterRoom(Addr[0],int.Parse(Addr[1]), joinRoomResult.invite, StartConfigure);
                 });
        }

        public void StartConfigure(Game.Events.Event e)
        {
            var connected = (Game.Events.ConnectedEvent)e;
            if(connected.room.ingame)
            {
                NetworkManager.instance.EndConnection();
                //이미 플레이 중인 게임입니다. 알림
            }
            else
            {
                //방설정 씬으로 이동
                SceneChanger.instance.RoomListToRoomConfigure(connected.room);
            }
        }

    }

}