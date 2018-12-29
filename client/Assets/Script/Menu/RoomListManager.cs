using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Menu.Models;
using Game;
using UnityEngine.SceneManagement;
using Network;

namespace Menu
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
            StartCoroutine(NetworkManager.instance.lobbyServerRequestor.Get<Models.Room[]>("/rooms/", "black", CreateRoomList));
        }

        private void CreateRoomList(Models.Room[] rooms)
        {
            for(int i = 0;i<rooms.Length;i++)
            {
                Add(rooms[i]);
            }
        }
        public void EnterRoom(Room room)
        {
            StartCoroutine(NetworkManager.instance.lobbyServerRequestor.Put<JoinRoomResult>("/rooms/" + room.id, "", "black", (JoinRoomResult joinRoomResult) =>{
                     Debug.Log(joinRoomResult.addr);
                     var Addr = joinRoomResult.addr.Split(':');
                     NetworkManager.instance.EnterRoom(Addr[0],int.Parse(Addr[1]),joinRoomResult.invite, StartGame);
                 }));
        }
        public void StartGame(Game.Events.Event e)
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
                SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
            }
            
        }

    }

}
