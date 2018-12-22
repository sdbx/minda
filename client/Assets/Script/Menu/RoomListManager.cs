using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Menu.Models;
using Game;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class RoomListManager : MonoBehaviour
    {
        [SerializeField]
        NetworkManager networkManger;
        [SerializeField]
        private RoomObject roomPrefeb;
        [SerializeField]
        private ScrollRect scrollRect;
        private Transform content;
        private List<RoomObject> _roomList = new List<RoomObject>();
        
        private LobbyServerRequestor lobbyServerRequestor = new LobbyServerRequestor("http://127.0.0.1:8080");
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
            StartCoroutine(lobbyServerRequestor.Get<Models.Room[]>("/rooms/", CreateRoomList, "black"));
        }

        private void CreateRoomList(Models.Room[] rooms)
        {
            for(int i = 0;i<rooms.Length;i++)
            {
                Add(rooms[i]);
            }
        }
        public void TryEnterToRoom(Room room)
        {
             StartCoroutine(lobbyServerRequestor.Put<JoinRoomResult>("/rooms/"+room.id,"",TryConnectRoom,"black"));
        }

        public void TryConnectRoom(JoinRoomResult joinRoomResult)
        {
            Debug.Log(joinRoomResult.addr);
            var Addr = joinRoomResult.addr.Split(':');
            networkManger.StartManger(Addr[0], int.Parse(Addr[1]), joinRoomResult.invite);
            SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
        }
    }

}
