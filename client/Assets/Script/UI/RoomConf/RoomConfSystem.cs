using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

namespace UI
{
    public class RoomConfSystem : MonoBehaviour
    {
        [SerializeField]
        private UserInfo myInfo;
        [SerializeField]
        private UserInfo oppenentInfo;

        public Models.Room room;

        
        void Start()
        {
            Network.NetworkManager.instance.Get<Models.User>("/users/me/", (Models.User me) =>
            {
                Debug.Log($"{room.conf.black} {room.conf.white} {room.conf.king}");
                myInfo.user = me;
                if(me.id==room.conf.king)
                {
                    myInfo.isKing = true;
                    oppenentInfo.isKing = false;
                }
                else
                {
                    oppenentInfo.isKing = true;
                    myInfo.isKing = false;
                }

                

                int opponentID;

                if (me.id == room.conf.black)
                {
                    myInfo.ballType = BallType.Black;
                    oppenentInfo.ballType = BallType.White;
                    opponentID = room.conf.white;
                }
                else
                {
                    myInfo.ballType = BallType.White;
                    oppenentInfo.ballType = BallType.Black;
                    opponentID = room.conf.black;
                }

                Network.NetworkManager.instance.Get<Models.User>("/users/" + opponentID + "/", (Models.User opponent) =>
                {
                    oppenentInfo.user = opponent;
                    oppenentInfo.Display();
                });
                myInfo.Display();

            });
        }

        void Update()
        {

        }
    }
}