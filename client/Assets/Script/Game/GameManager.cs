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
using UI.Toast;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public BoardManager boardManger;
        public BallManager ballManager;
        public BallType myBallType;

        [SerializeField]
        private CircularTimer player1GameTimer;
        [SerializeField]
        private CircularTimer player1TurnTimer;

        [SerializeField]
        private CircularTimer player2GameTimer;
        [SerializeField]
        private CircularTimer player2TurnTimer;

        private BallType nowTurn;

        void Awake()
        {
            boardManger.CreateBoard();
            var game = GameServer.instance.gamePlaying;
            InitTimer(game.rule.turn_timeout,game.rule.game_timeout);
            UpdateTimers(game.rule.turn_timeout,game.rule.game_timeout, game.rule.game_timeout);
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
            nowTurn = OppositeBallType(move.player);

            if (move.player != myBallType)
            {
                ballManager.PushBalls(new BallSelection(move.start, move.end), CubeCoord.ConvertDirectionToNum(move.dir));
                if (nowTurn == myBallType)
                {
                    ballManager.state = 1;
                }
            }

            CircularTimer turnTimeoutTimer;
            CircularTimer gameTimeoutTimer;

            if(GameServer.instance.isSpectator)
            {
                if(move.player == BallType.Black)
                {
                    turnTimeoutTimer = player2GameTimer;
                    gameTimeoutTimer = player2TurnTimer;
                }
                else
                {
                    turnTimeoutTimer = player1GameTimer;
                    gameTimeoutTimer = player1TurnTimer;
                }
            }
            else if (move.player == myBallType)
            {
                turnTimeoutTimer = player2GameTimer;
                gameTimeoutTimer = player2TurnTimer;
            }
            else
            {
                turnTimeoutTimer = player1GameTimer;
                gameTimeoutTimer = player1TurnTimer;
            }

            turnTimeoutTimer.Stop();
            gameTimeoutTimer.Stop();
        }

        public void OnEnded(Game.Events.Event e)
        {
            var end = (EndedEvent)e;
            SceneChanger.instance.ChangeTo("RoomConfigure");
        }

        public void OnTicked(Game.Events.Event e) 
        {
            var ticked = (TickedEvent)e;
            UpdateTimers(ticked.current_time,ticked.black_time,ticked.white_time);
        }

        private void UpdateTimers(int currentTime, int blackTime, int whiteTime)
        {
            CircularTimer blackGameTimer;
            CircularTimer whiteGameTimer;
            CircularTimer blackTurnTimer;
            CircularTimer whiteTurnTimer;

            if (GameServer.instance.isSpectator || myBallType == BallType.Black)
            {
                blackGameTimer = player2GameTimer;
                blackTurnTimer = player2TurnTimer;

                whiteGameTimer = player1GameTimer;
                whiteTurnTimer = player1TurnTimer;
            }
            else
            {
                blackGameTimer = player1GameTimer;
                blackTurnTimer = player1TurnTimer;

                whiteGameTimer = player2GameTimer;
                whiteTurnTimer = player2TurnTimer;
            }

            if (nowTurn == BallType.Black)
            {
                setTimer(blackGameTimer, blackTurnTimer, currentTime, blackTime, whiteTurnTimer, whiteGameTimer);
            }
            else
            {
                setTimer(whiteGameTimer, whiteTurnTimer, currentTime, whiteTime, blackTurnTimer, blackGameTimer);
            }
        }

        private void setTimer(CircularTimer nowGameTimer, CircularTimer nowTurnTimer, int turnTime, int gameTime, CircularTimer nextTurnTimer, CircularTimer nextGameTimer)
        {
            nowGameTimer.displayText = false;
            nowTurnTimer.gameObject.SetActive(true);
            nowTurnTimer.leftTime = turnTime;
            nowTurnTimer.CountDown(nowTurnTimer.wholeTime);
            nowGameTimer.leftTime = gameTime;
            nowGameTimer.CountDown(nowGameTimer.wholeTime);

            nextTurnTimer.gameObject.SetActive(false);
            nextGameTimer.displayText = true;
            nextGameTimer.UpdateTimer();
        }

        private void InitTimer(int turnTimeout,int gameTimeout)
        {
            player1GameTimer.wholeTime = gameTimeout;
            player1GameTimer.UpdateTimer();
            player1TurnTimer.wholeTime = turnTimeout;
            player1TurnTimer.UpdateTimer();
            player2GameTimer.wholeTime = gameTimeout;
            player2GameTimer.UpdateTimer();
            player2TurnTimer.wholeTime = turnTimeout;
            player2TurnTimer.UpdateTimer();
        }

        public void StartGame(int[,] map, BallType turn)
        {
            myBallType = RoomUtils.GetBallType(LobbyServer.instance.loginUser.id);
            ballManager.RemoveBalls();
            boardManger.SetMap(map);
            ballManager.CreateBalls(boardManger);
            if (turn == myBallType)
            {
                ballManager.state = 1;
            }
        }

        public void SendBallMoving(BallSelection ballSelection, int direction)
        {
            MoveCommand moveCommand = new MoveCommand(myBallType, ballSelection.first, ballSelection.end, CubeCoord.ConvertNumToDirection(direction));
            GameServer.instance.SendCommand(moveCommand);
        }

        private BallType OppositeBallType(BallType ballType)
        {
            return (ballType == BallType.Black?BallType.White:BallType.Black);
        }
    }
}