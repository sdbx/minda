using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Models;
using Network;

namespace UI
{
    public class RoomConfSystem : MonoBehaviour
    {
        //player2 가 본인
        [SerializeField]
        private UserInfoDisplay player1InfoDisplay;
        [SerializeField]
        private UserInfoDisplay player2InfoDisplay;
        [SerializeField]
        private StartBtn startBtn;
        
        private bool isWaitingForOppenent = false;

        public bool IsSpectator() {
            var me = NetworkManager.instance.loggedInUser;
            var room = NetworkManager.instance.connectedRoom;
            return room.conf.black != me.id && room.conf.white != me.id;
        }

        void Start()
        {
            NetworkManager.instance.otherUserEnterCallBack = UserEnter;
            NetworkManager.instance.myEnterCallBack = Entered;
            NetworkManager.instance.userLeftCallBack = UserLeft;
            NetworkManager.instance.confedCallBack = ConfedCallBack;
        }

        private void Entered()
        {
            Network.NetworkManager.instance.RefreshLoggedInUser(() =>
            {
                var me = NetworkManager.instance.loggedInUser;
                var room = NetworkManager.instance.connectedRoom;

                room.Users.Add(me.id);

                if(room.conf.king == me.id) 
                {
                    if (room.conf.black == -1)
                    {
                        room.conf.black = me.id;
                        NetworkManager.instance.UpdateConf();
                    }
                    else if (room.conf.white == -1)
                    {
                        room.conf.white = me.id;
                        NetworkManager.instance.UpdateConf();
                    }
                }
                IdentifyAndSetInfo();
            });
        }

        private void UserEnter(int userId)
        {
            var room = NetworkManager.instance.connectedRoom;
            var me = NetworkManager.instance.loggedInUser;

            if (room.conf.king == me.id) 
            {
                if (room.conf.black == -1)
                {
                    room.conf.black = userId;
                    NetworkManager.instance.UpdateConf();
                }
                else if (room.conf.white == -1)
                {
                    room.conf.white = userId;
                    NetworkManager.instance.UpdateConf();
                }
            }

            IdentifyAndSetInfo();
        }
        
        private void UserLeft(int user)
        {
            UpdateAllConf();
        }

        private void ConfedCallBack(RoomSettings conf)
        {
            var room = NetworkManager.instance.connectedRoom;
            room.conf = conf;
            UpdateAllConf();
        }

        private void IdentifyAndSetInfo()
        {
            var room = NetworkManager.instance.connectedRoom;
            var me = NetworkManager.instance.loggedInUser;
            int player2Id;
            int player1Id;

            if(!IsSpectator())
            {
                player2Id = NetworkManager.instance.loggedInUser.id;
                player1Id = GetOpponentId(player2Id);
            }
            else
            {
                player2Id = room.conf.black;
                player1Id = room.conf.white;
            }

            if(player1Id == -1 && player2Id == -1)
            {
                player1InfoDisplay.ballType = BallType.White;
                player1InfoDisplay.user = User.waiting;
                player2InfoDisplay.ballType = BallType.Black;
                player2InfoDisplay.user = User.waiting;

                player2InfoDisplay.Display();
                player1InfoDisplay.Display();
                return;
            }

            player1InfoDisplay.ballType = IdUtils.GetBallType(player1Id);
            player2InfoDisplay.ballType = IdUtils.GetBallType(player2Id);
            
            player1InfoDisplay.isKing = (player1Id == room.conf.king);
            player2InfoDisplay.isKing = (player2Id == room.conf.king);

            if(player1Id == -1)
            {
                player1InfoDisplay.user = User.waiting;
                player1InfoDisplay.Display();
            }
            else
            {
                NetworkManager.instance.GetUserInformation(player1Id, (User user) =>
                {
                    player1InfoDisplay.user = user;
                    player1InfoDisplay.Display();
                });
            }

            if (player2Id == -1)
            {
                player2InfoDisplay.user = User.waiting;
                player2InfoDisplay.Display();
            }
            else
            {
                NetworkManager.instance.GetUserInformation(player2Id, (User user) =>
                {
                    player2InfoDisplay.user = user;
                    player2InfoDisplay.Display();
                });
            }

            if (room.conf.king == me.id && room.conf.black != -1 && room.conf.white != -1)
            {
                startBtn.Active();
            } 
            else 
            {
                startBtn.UnActive();
            }
        }

        private void UpdateAllConf()
        {
            IdentifyAndSetInfo();
        }

        private int GetOpponentId(int myId)
        {
            var room = NetworkManager.instance.connectedRoom;
            if (myId == room.conf.black)
            {
                return room.conf.white;
            }
            else
            {
                return room.conf.black;
            }
        }
    }
}