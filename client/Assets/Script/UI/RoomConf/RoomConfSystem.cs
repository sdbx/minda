using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Models;
using Network;
using Utils;

namespace UI
{
    public class RoomConfSystem : MonoBehaviour
    {
        //player2 가 본인
        [SerializeField]
        private UserList userList;
        [SerializeField]
        private PlayerInfoDisplay player1InfoDisplay;
        [SerializeField]
        private PlayerInfoDisplay player2InfoDisplay;
        [SerializeField]
        private StartBtn startBtn;

        private void Awake()
        {
            GameServer.instance.UserEnteredEvent += UserEnter;
            GameServer.instance.UserLeftEvent += UserLeft;
            GameServer.instance.ConfedEvent += ConfedCallBack;
        }

        private void UserEnter(int id, BallType ballType)
        {
            if(LobbyServer.instance.loginUser.id == id)
                userList.Create(GameServer.instance.connectedRoom.Users.ToArray());
            else
            {
                userList.Add(id);
            }
            UpdateAllConf();
        }

        
        private void UserLeft(int user)
        {
            UpdateAllConf();
            userList.Remove(user);
        }

        private void ConfedCallBack(Conf conf)
        {
            UpdateAllConf();
        }

        private void SetPlayerInfo(Conf conf)
        {
            var isSpectator = GameServer.instance.isSpectator;
            var me = LobbyServer.instance.loginUser;

            int player2Id;
            int player1Id;

            if(isSpectator)
            {
                player2Id = conf.black;
                player1Id = conf.white;
            }
            else
            {
                player2Id = me.id;
                player1Id = GetOpponentId(me.id);
            }

            if(player1Id == -1 && player2Id == -1)
            {
                player1InfoDisplay.display(-1, BallType.White);
                player2InfoDisplay.display(-1, BallType.Black);
                return;
            }

            if(player1Id == -1)
            {
                player1InfoDisplay.display(-1, RoomUtils.GetBallType(-1));
            }
            else
            {
                player1InfoDisplay.display(player1Id, RoomUtils.GetBallType(player1Id));
            }

            if (player2Id == -1)
            {
                player2InfoDisplay.display(-1, RoomUtils.GetBallType(-1));
            }
            else
            {
                player2InfoDisplay.display(player2Id, RoomUtils.GetBallType(player2Id));
            }

            if (conf.king == me.id && conf.black != -1 && conf.white != -1)
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
            SetPlayerInfo(GameServer.instance.connectedRoom.conf);
            userList.RefreshAll();
        }

        private int GetOpponentId(int myId)
        {
            var room = GameServer.instance.connectedRoom;
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