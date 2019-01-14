using System.Collections;
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
            RefreshRoomList();
        }

        private void CreateRoomList(Room[] rooms, string err)
        {
            if(err!=null)
            {
                Debug.Log("방 목록 받아오기 실패 " + err);
                return;
            }
            if(rooms==null)
                return;
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
            NetworkManager.instance.Get<Room[]>("/rooms/",CreateRoomList);
        }

        public void EnterRoom(Room room)
        {
            NetworkManager.instance.Put("/rooms/" + room.id + "/", "", (JoinRoomResult joinRoomResult, string err) =>
            {
                if (err != null)
                {
                    Debug.Log(err);
                    return;
                }
                Debug.Log(joinRoomResult.addr);
                var Addr = joinRoomResult.addr.Split(':');
                SceneChanger.instance.ChangeTo("RoomConfigure");
                NetworkManager.instance.EnterRoom(Addr[0], int.Parse(Addr[1]), joinRoomResult.invite);
            });
        }
    }

}
