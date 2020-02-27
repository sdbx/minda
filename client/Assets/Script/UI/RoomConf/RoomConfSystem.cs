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

        private User _me = LobbyServer.Instance.loginUser;

        private bool _hasRecievedFirstConf = false;

        private void Awake()
        {
            GameServer.Instance.UserEnteredEvent += UserEnter;
            GameServer.Instance.UserLeftEvent += UserLeft;
            GameServer.Instance.ConfedEvent += ConfedCallBack;
            GameServer.Instance.RoomConnectedEvent += RoomConnected;

            defeatLostMarble.ValueChanged += DefeatLostMarbleValueChanged;
            turnTimeout.ValueChanged += TurnTimeoutValueChanged;
            gameTimeout.ValueChanged += GameTimeoutValueChanged;
            Debug.Log("이벤트 등록완료");
        }

        private void RoomConnected(Room room)
        {
            UpdateAllConf();
        }

        private void DefeatLostMarbleValueChanged(int value)
        {
            if (!RoomUtils.CheckIsKing(_me.Id))
                return;
            GameServer.Instance.connectedRoom.Conf.GameRule.DefeatLostStones = value;
            GameServer.Instance.UpdateConf();
        }
        private void TurnTimeoutValueChanged(int value)
        {
            if (!RoomUtils.CheckIsKing(_me.Id))
                return;
            GameServer.Instance.connectedRoom.Conf.GameRule.TurnTimeout = value;
            GameServer.Instance.UpdateConf();
        }
        private void GameTimeoutValueChanged(int value)
        {
            if (!RoomUtils.CheckIsKing(_me.Id))
                return;
            turnTimeout.ChangeMax(value * 60);
            GameServer.Instance.connectedRoom.Conf.GameRule.GameTimeout = value * 60;
            GameServer.Instance.UpdateConf();
        }

        private void Start()
        {
            if (GameServer.Instance.connectedRoom != null)
                ConfedCallBack(GameServer.Instance.connectedRoom.Conf);
        }

        private void OnDestroy()
        {
            GameServer.Instance.UserEnteredEvent -= UserEnter;
            GameServer.Instance.UserLeftEvent -= UserLeft;
            GameServer.Instance.ConfedEvent -= ConfedCallBack;
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
            if (!_hasRecievedFirstConf)
            {
                roomNameText.text = conf.Name;
                _hasRecievedFirstConf = true;
                UpdateGameruleIntUpDowns();
            }
            UpdateAllConf();
        }

        private void UpdateAllConf()
        {
            var room = GameServer.Instance.connectedRoom;
            if (room == null)
                return;
            var conf = room.Conf;
            //맵에서의 흰돌과 흑돌 각각 갯수 중 작은 값
            var max = Mathf.Min(StringUtils.ParticularCharCount(conf.Map, '1'), StringUtils.ParticularCharCount(conf.Map, '2'));
            mapPreview.SetMap(Board.GetMapFromString(conf.Map));
            defeatLostMarble.ChangeMax(max);

            if (RoomUtils.CheckIsKing(_me.Id))
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

        private void UpdateGameruleIntUpDowns()
        {
            var gameRule = GameServer.Instance.connectedRoom.Conf.GameRule;
            defeatLostMarble.ChangeValue(gameRule.DefeatLostStones);
            turnTimeout.ChangeValue(gameRule.TurnTimeout);
            gameTimeout.ChangeValue(gameRule.GameTimeout / 60);
        }
    }
}
