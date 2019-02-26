using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Models;
using Network;
using Utils;
using Game.Boards;
using UnityEngine.UI;

namespace UI
{
    public class RoomConfSystem : MonoBehaviour
    {
        [SerializeField]
        private Text roomNameText;
        [SerializeField]
        private StartBtn startBtn;
        [SerializeField]
        private MapBtn mapBtn;

        [SerializeField]
        private IntUpDown defeatLostMarble;
        [SerializeField]
        private IntUpDown turnTimeout;
        [SerializeField]
        private IntUpDown gameTimeout;
        [SerializeField]
        private MapPreview mapPreview;

        private User me = LobbyServer.instance.loginUser;

        private bool hasRecievedFirstConf = false;

        private void Awake()
        {
            GameServer.instance.UserEnteredEvent += UserEnter;
            GameServer.instance.UserLeftEvent += UserLeft;
            GameServer.instance.ConfedEvent += ConfedCallBack;

            defeatLostMarble.ValueChanged += DefeatLostMarbleValueChanged;
            turnTimeout.ValueChanged += TurnTimeoutValueChanged;
            gameTimeout.ValueChanged += GameTimeoutValueChanged;
        }
        
        private void DefeatLostMarbleValueChanged(int value)
        {                
            if(!RoomUtils.CheckIsKing(me.id))
                return;
            GameServer.instance.connectedRoom.conf.game_rule.defeat_lost_stones = value;
            GameServer.instance.UpdateConf();
        }
        private void TurnTimeoutValueChanged(int value)
        {
            if (!RoomUtils.CheckIsKing(me.id))
                return;
            GameServer.instance.connectedRoom.conf.game_rule.turn_timeout = value;
            GameServer.instance.UpdateConf();
        }
        private void GameTimeoutValueChanged(int value)
        {
            if (!RoomUtils.CheckIsKing(me.id))
                return;
            turnTimeout.ChangeMax(value*60);
            GameServer.instance.connectedRoom.conf.game_rule.game_timeout = value*60;
            GameServer.instance.UpdateConf();
        }

        private void Start()
        {
            UpdateAllConf();
        }

        private void OnDestroy()
        {
            GameServer.instance.UserEnteredEvent -= UserEnter;
            GameServer.instance.UserLeftEvent -= UserLeft;
            GameServer.instance.ConfedEvent -= ConfedCallBack;
        }

        private void UserEnter(int id, BallType ballType)
        {
            UpdateAllConf();        
        }

        
        private void UserLeft(int user)
        {
            UpdateAllConf();
        }

        private void ConfedCallBack(Conf conf)
        {
            if(!hasRecievedFirstConf)
            {
                roomNameText.text = conf.name;
                hasRecievedFirstConf = true;
                UpdateGameruleIntUpDowns();
            }
            UpdateAllConf();
        }

        private void UpdateAllConf()
        {
            var room = GameServer.instance.connectedRoom;
            if (room != null) 
            {
                //맵에서의 흰돌과 흑돌 각각 갯수 중 작은 값
                var max = Mathf.Min(StringUtils.ParticularCharCount(room.conf.map, '1'), StringUtils.ParticularCharCount(room.conf.map, '2'));
                mapPreview.SetMap(Board.GetMapFromString(room.conf.map));
                defeatLostMarble.ChangeMax(max);
                
                if(RoomUtils.CheckIsKing(me.id))
                {
                    defeatLostMarble.isButtonLocked = false;
                    turnTimeout.isButtonLocked = false;
                    gameTimeout.isButtonLocked = false;
                    mapBtn.isLocked = false;
                }
                else
                {
                    defeatLostMarble.isButtonLocked = true;
                    turnTimeout.isButtonLocked = true;
                    gameTimeout.isButtonLocked = true;
                    mapBtn.isLocked = true;

                    UpdateGameruleIntUpDowns();
                }
            }
        }

        private void UpdateGameruleIntUpDowns()
        {
            var gameRule = GameServer.instance.connectedRoom.conf.game_rule;
            defeatLostMarble.ChangeValue(gameRule.defeat_lost_stones);
            turnTimeout.ChangeValue(gameRule.turn_timeout);
            gameTimeout.ChangeValue(gameRule.game_timeout/60);
        }
    }
}