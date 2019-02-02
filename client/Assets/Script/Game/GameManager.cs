using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Network;
using Scene;
using Game.Events;
using Game.Coords;
using Game.Balls;
using Game.Boards;
using Utils;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public BoardManager boardManger;
        public BallManager ballManager;
        public BallType myBallType;

        public Text turnText;
        public Text blackTimeText;
        public Text whiteTimeText;
        public Text currentTimeText;

        void Start()
        {
            boardManger.CreateBoard();
            var game = GameServer.instance.gamePlaying;
            StartGame(Board.GetMapFromString(game.map), game.turn);

            InitHandlers();
        }

        private void InitHandlers()
        {
            GameServer.instance.AddHandler<MoveEvent>(OnMoved);
            GameServer.instance.AddHandler<EndedEvent>(OnEnded);
            GameServer.instance.AddHandler<TickedEvent>(OnTicked);
        }

        private void OnDestroy()
        {
            GameServer.instance.RemoveHandler<MoveEvent>(OnMoved);
            GameServer.instance.RemoveHandler<EndedEvent>(OnEnded);
            GameServer.instance.RemoveHandler<TickedEvent>(OnTicked);
        }

        private void OnMoved(Events.Event e)
        {
            var move = (MoveEvent)e;
            if (move.player != myBallType)
            {
                OppenetMovement(new BallSelection(move.start, move.end), CubeCoord.ConvertDirectionToNum(move.dir));
            }
        }

        public void OnEnded(Game.Events.Event e)
        {
            var end = (EndedEvent)e;
            SceneChanger.instance.ChangeTo("RoomConfigure");
        }

        public void OnTicked(Game.Events.Event e) {
            var tick = (TickedEvent)e;
            whiteTimeText.text = "Remaining time for the white:" + tick.white_time + " seconds";
            blackTimeText.text = "Remaining time for the black:" + tick.black_time + " seconds";
            currentTimeText.text = "Remaining time for this turn:" + tick.current_time + " seconds";
        }

        public void StartGame(int[,] map, BallType turn)
        {
            myBallType = RoomUtils.GetBallType(LobbyServer.instance.loginUser.id);
            ballManager.RemoveBalls();
            boardManger.SetMap(map);
            ballManager.CreateBalls(boardManger);
            if (turn == myBallType)
            {
                MyTurn();
            }
        }

        public void SendBallMoving(BallSelection ballSelection, int direction)
        {
            MoveCommand moveCommand = new MoveCommand(myBallType, ballSelection.first, ballSelection.end, CubeCoord.ConvertNumToDirection(direction));
            GameServer.instance.SendCommand(moveCommand);
        }

        public void MyTurn()
        {
            ballManager.state = 1;
            turnText.text = "My turn";
        }

        public void OppenetMovement(BallSelection ballSelection, int direction)
        {
            ballManager.PushBalls(ballSelection, direction);
            MyTurn();
        }
    }
}