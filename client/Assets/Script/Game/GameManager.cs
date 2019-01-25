using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Network;
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
            var Ended = (EndedEvent)e;

        }

        public void StartGame(int[,] map, BallType turn)
        {
            myBallType = RoomUtils.GetBallType(LobbyServer.instance.loginUser.id);
            ballManager.RemoveBalls();
            boardManger.SetMap(map);
            ballManager.CreateBalls(boardManger);
            if (turn == myBallType)
            {
                myTurn();
            }
        }

        public void SendBallMoving(BallSelection ballSelection, int direction)
        {
            MoveCommand moveCommand = new MoveCommand(myBallType, ballSelection.first, ballSelection.end, CubeCoord.ConvertNumToDirection(direction));
            GameServer.instance.SendCommand(moveCommand);
        }

        public void myTurn()
        {
            ballManager.state = 1;
            turnText.text = "My turn";
        }

        public void OppenetMovement(BallSelection ballSelection, int direction)
        {
            ballManager.PushBalls(ballSelection, direction);
            myTurn();
        }
    }
}