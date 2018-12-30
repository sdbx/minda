using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Network;
using Game.Events;
using Game.Coords;
using Game.Balls;
using Game.Boards;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public BoardManager boardManger;
        public BallManager ballManager;
        public BallType myBallType = BallType.Black;

        public Text turnText;

        void Start()
        {
            SetHandlers();
        }

        void Update()
        {

        }

        private void SetHandlers()
        {
            NetworkManager.instance.SetHandler<ConnectedEvent>(GameStartHandler);
            NetworkManager.instance.SetHandler<EnteredEvent>(EnteredHandler);
            NetworkManager.instance.SetHandler<MoveEvent>(MoveHandler);
        }

        private void GameStartHandler(Events.Event e)
        {
            var game = (GameStartEvent)e;
            StartGame(game.board, game.turn);
        }

        private void EnteredHandler(Events.Event e)
        {
            var entered = (EnteredEvent)e;
        }

        private void MoveHandler(Events.Event e)
        {
            var move = (MoveEvent)e;
            if (move.player != myBallType)
            {
                OppenetMovement(new BallSelection(move.start, move.end), CubeCoord.ConvertDirectionToNum(move.dir));
            }
        }


        public void StartGame(int[,] map, BallType turn)
        {
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
            NetworkManager.instance.SendCommand(moveCommand);
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