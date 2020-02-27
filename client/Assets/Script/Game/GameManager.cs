using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Network;
using Models.Events;
using Game.Coords;
using Game.Balls;
using Game.Boards;
using Utils;
using UI.Toast;
using UI;
using Models;
using UnityEngine.SceneManagement;
using Event = Models.Events.Event;

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
        [SerializeField]
        private WinScreen winScreen;

        private BallType _nowTurn;

        private void Awake()
        {
            boardManger.CreateBoard();
            var game = GameServer.Instance.gamePlaying;
            //boardManger.CreateBoard();

            StartGame(Board.GetMapFromString(game.Map), game.Turn, game.Black, game.White);
            InitTimer(game.Rule.TurnTimeout, game.Rule.GameTimeout);
            UpdateTimers(game.CurrentTime, game.Rule.GameTimeout, game.Rule.GameTimeout);
            InitHandlers();
        }

        private void InitHandlers()
        {
            GameServer.Instance.AddHandler<MoveEvent>(OnMoved);
            GameServer.Instance.AddHandler<EndedEvent>(OnEnded);
            GameServer.Instance.AddHandler<TickedEvent>(OnTicked);
            GameServer.Instance.AddHandler<ConfedEvent>(OnConfed);
        }

        private void OnDestroy()
        {
            GameServer.Instance.RemoveHandler<MoveEvent>(OnMoved);
            GameServer.Instance.RemoveHandler<EndedEvent>(OnEnded);
            GameServer.Instance.RemoveHandler<TickedEvent>(OnTicked);
            GameServer.Instance.RemoveHandler<ConfedEvent>(OnConfed);
        }

        private void OnConfed(Event e)
        {
            var confed = (ConfedEvent)e;
            var conf = confed.Conf;
            var myId = LobbyServer.Instance.loginUser.Id;
            if (conf.Black == myId && myBallType != BallType.Black)
            {
                ChangeBallType(BallType.Black);
            }
            else if (conf.White == myId && myBallType != BallType.White)
            {
                ChangeBallType(BallType.White);
            }
            else if (myBallType != BallType.White && myBallType != BallType.Black)
            {
                ChangeBallType(BallType.None);
            }

        }

        public void ChangeBallType(BallType ballType)
        {
            myBallType = ballType;
            if (_nowTurn == myBallType)
                ballManager.state = 1;
            SetRotation();
        }

        private void OnMoved(Event e)
        {
            var move = (MoveEvent)e;
            _nowTurn = OppositeBallType(move.Player);

            if (move.Player != myBallType)
            {
                ballManager.PushBalls(new BallSelection(move.Start, move.End), CubeCoord.ConvertDirectionToNum(move.Dir));
                if (_nowTurn == myBallType)
                {
                    ballManager.state = 1;
                }
            }

            CircularTimer turnTimeoutTimer;
            CircularTimer gameTimeoutTimer;

            if (GameServer.Instance.isSpectator)
            {
                if (move.Player == BallType.Black)
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
            else if (move.Player == myBallType)
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

        public void OnEnded(Event e)
        {
            var end = (EndedEvent)e;
            winScreen.Display(end, () => { SceneManager.LoadScene("RoomConfigure"); });
            ballManager.state = 0;
        }

        public void OnTicked(Event e)
        {
            var ticked = (TickedEvent)e;
            UpdateTimers(ticked.CurrentTime, ticked.BlackTime, ticked.WhiteTime);
        }

        private void UpdateTimers(float currentTime, float blackTime, float whiteTime)
        {
            CircularTimer blackGameTimer;
            CircularTimer whiteGameTimer;
            CircularTimer blackTurnTimer;
            CircularTimer whiteTurnTimer;

            if (GameServer.Instance.isSpectator || myBallType == BallType.Black)
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

            blackGameTimer.leftTime = blackTime;
            whiteGameTimer.leftTime = whiteTime;

            if (_nowTurn == BallType.Black)
            {
                SetTimer(blackGameTimer, blackTurnTimer, currentTime, blackTime, whiteTurnTimer, whiteGameTimer);
            }
            else
            {
                SetTimer(whiteGameTimer, whiteTurnTimer, currentTime, whiteTime, blackTurnTimer, blackGameTimer);
            }
        }

        private void SetTimer(CircularTimer nowGameTimer, CircularTimer nowTurnTimer, float turnTime, float gameTime, CircularTimer nextTurnTimer, CircularTimer nextGameTimer)
        {
            nowGameTimer.displayText = false;
            nowTurnTimer.gameObject.SetActive(true);
            nowTurnTimer.leftTime = turnTime;
            nowTurnTimer.CountDown(nowTurnTimer.wholeTime);
            nowGameTimer.CountDown(nowGameTimer.wholeTime);

            nextTurnTimer.gameObject.SetActive(false);
            nextGameTimer.displayText = true;
            nextGameTimer.UpdateTimer();
        }

        private void InitTimer(int turnTimeout, int gameTimeout)
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

        public void StartGame(int[,] map, BallType turn, int black, int white)
        {
            myBallType = RoomUtils.GetBallType(LobbyServer.Instance.loginUser.Id);
            ballManager.RemoveBalls();
            boardManger.SetMap(map);
            ballManager.CreateBalls(boardManger);


            GameServer.Instance.GetInGameUser(black, (inGameuser) =>
            {
                var skinId = inGameuser.User.Inventory.CurrentSkin;
                if (skinId == null)
                    return;
                Debug.Log(skinId + "블랙");
                LobbyServer.Instance.GetLoadedSkin(inGameuser.User.Inventory.CurrentSkin.Value, (loadedSkin) =>
                {
                    if (loadedSkin != null)
                    {
                        ballManager.SetBallsSkin(BallType.Black, (Texture2D)loadedSkin.BlackTexture);
                    }
                });
            });
            GameServer.Instance.GetInGameUser(white, (inGameuser) =>
            {
                var skinId = inGameuser.User.Inventory.CurrentSkin;
                if (skinId == null)
                    return;
                Debug.Log(skinId + "화이트");
                LobbyServer.Instance.GetLoadedSkin(inGameuser.User.Inventory.CurrentSkin.Value, (loadedSkin) =>
                {
                    if (loadedSkin != null)
                    {
                        ballManager.SetBallsSkin(BallType.White, (Texture2D)loadedSkin.WhiteTexture);
                    }
                });
            });



            SetRotation();
            _nowTurn = turn;
            if (turn == myBallType)
            {
                ballManager.state = 1;
            }
        }

        private void SetRotation()
        {
            if (myBallType == BallType.White)
            {
                Camera.main.transform.rotation = Quaternion.Euler(0, 0, 180);
                ballManager.gameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
                Physics2D.gravity = new Vector3(0, 9.8f, 0);
            }
            else
            {
                Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);
                ballManager.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                Physics2D.gravity = new Vector3(0, -9.8f, 0);
            }
        }

        public void SendBallMoving(BallSelection ballSelection, int direction)
        {
            var moveCommand = new MoveCommand(myBallType, ballSelection.First, ballSelection.End, CubeCoord.ConvertNumToDirection(direction));
            GameServer.Instance.SendCommand(moveCommand);
        }

        private BallType OppositeBallType(BallType ballType)
        {
            return (ballType == BallType.Black ? BallType.White : BallType.Black);
        }
    }
}
