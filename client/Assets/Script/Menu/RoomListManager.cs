using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Menu
{
    public class RoomListManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject roomPrefeb;
        [SerializeField]
        private Transform content;
        private List<Room> _roomList = new List<Room>();

        public void Add(RoomInformation roomInformation)
        {
            GameObject roomObject = Instantiate(roomPrefeb,parent:content);
            Room room = roomObject.GetComponent<Room>();
            room.roomInformation = roomInformation;
            _roomList.Add(room);
        }

        void Start()
        {
            for(int i = 0; i<50;i++)
            {
                Add(new RoomInformation("testroom "+i,i));
            }
            
        }

        void Update()
        {

        }
    }

}
