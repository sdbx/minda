using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Game;
using Models.Events;
using Models;
using Newtonsoft.Json;
using UI;
using UI.Chatting;
using UI.Toast;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Event = Models.Events.Event;

namespace Network
{
    public class GameServer : MonoBehaviour
    {
        public static GameServer Instance;

        //socket
        private SocketClient _asyncCallbackClient = new SocketClient();
        //event
        private JsonSerializerSettings _eventJsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
        };
        public delegate void GameEventHandler(Event e);
        private Dictionary<Type, List<GameEventHandler>> _handlers = new Dictionary<Type, List<GameEventHandler>>();

        public event Action<int, BallType> UserEnteredEvent;
        public event Action<int> UserLeftEvent;
        public event Action<Room> RoomConnectedEvent;
        public event Action<Conf> ConfedEvent;
        public event Action<Message> MessagedEvent;
        public event Action<GameStartedEvent> GameStarted;
        public event Action<EndedEvent> EndedEvent;

        public bool isSpectator = false;
        public Room connectedRoom;
        private Dictionary<int, User> _users = new Dictionary<int, User>();
        private Dictionary<int, Texture> _profileImages = new Dictionary<int, Texture>();

        public GameStartedEvent gamePlaying;
        public bool isInGame;

        private Conf _prevConf;

        private bool _isGameEnded = false;

        private void Awake()
        {
            //singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            _eventJsonSettings.Converters.Add(new EventConverter());
            InitHandlers();
            _asyncCallbackClient.CloseSocketCallback = OnSocketClose;
        }

        private void Update()
        {
            var dataCount = _asyncCallbackClient.DataQueue.Count;
            if (dataCount > 0)
            {
                for (var i = 0; i < dataCount; i++)
                {
                    var data = _asyncCallbackClient.DataQueue.Dequeue();
                    ReceiveEvent(data);
                }
            }

            var logCount = _asyncCallbackClient.LogQueue.Count;
            if (logCount > 0)
            {
                for (var i = 0; i < logCount; i++)
                {
                    Debug.Log(_asyncCallbackClient.LogQueue.Dequeue());
                }
            }

            var callbackCount = _asyncCallbackClient.CallbackQuene.Count;
            if (callbackCount > 0)
            {
                for (var i = 0; i < callbackCount; i++)
                {
                    _asyncCallbackClient.CallbackQuene.Dequeue()();
                }
            }
        }

        private void Connect(string ip, int port, Action callback)
        {
            if (_asyncCallbackClient.State == ClientState.Disconnected)
                _asyncCallbackClient.Connect(ip, port, callback);
            else Debug.Log($"[AsyncCallbackClient] Already Connected {ip}:{port}");
        }

        private void ReceiveEvent(string data)
        {
            foreach (var splitedStr in data.Split('\n'))
            {
                if (splitedStr == "")
                    return;

                var e = JsonConvert.DeserializeObject<Event>(splitedStr, _eventJsonSettings);
                Debug.Log(e.GetType());
                foreach (var handle in _handlers[e.GetType()])
                {
                    handle(e);
                }
            }
        }

        public void EndConnection()
        {
            ClearHandles();
            _asyncCallbackClient.Close();
        }

        public void ClearHandles()
        {
            _handlers.Clear();
        }

        public void SendCommand(Command command)
        {
            var json = JsonConvert.SerializeObject(command);
            _asyncCallbackClient.SendData(json + "\n");
        }

        public void AddHandler<T>(GameEventHandler handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = new List<GameEventHandler>();
            }
            _handlers[type].Add(handler);
        }

        public void RemoveHandler<T>(GameEventHandler handler)
        {
            var type = typeof(T);
            _handlers[type].Remove(handler);
        }
        //game
        public void GetInGameUser(int id, Action<InGameUser> callback)
        {
            if (!connectedRoom.Users.Contains(id))
                return;

            if (!_users.ContainsKey(id))
            {
                LobbyServerApi.GetUserInformation(id, (User user) =>
                {
                    _users[id] = user;
                    GetInGameUser(id, callback);
                });
                return;
            }
            var inGameUser = new InGameUser();
            inGameUser.User = _users[id];
            inGameUser.IsKing = (connectedRoom.Conf.King == id);
            inGameUser.BallType = RoomUtils.GetBallType(id);

            callback(inGameUser);

        }
        //EventHandlers
        private void InitHandlers()
        {
            AddHandler<ConnectedEvent>(OnConnected);
            AddHandler<EnteredEvent>(OnEntered);
            AddHandler<LeftEvent>(OnLeft);
            AddHandler<ConfedEvent>(OnConfed);
            AddHandler<ErrorEvent>(OnError);
            AddHandler<GameStartedEvent>(OnGameStarted);
            AddHandler<ChattedEvent>(OnChatted);
            AddHandler<EndedEvent>(OnGameEnded);
            AddHandler<BannedEvent>(OnBanned);
        }

        private void OnConnected(Event e)
        {
            var connected = (ConnectedEvent)e;
            connectedRoom = connected.Room;
            RoomConnectedEvent?.Invoke(connected.Room);
        }

        private void OnEntered(Event e)
        {
            var entered = (EnteredEvent)e;
            connectedRoom.Users.Add(entered.User);

            var conf = connectedRoom.Conf;
            var me = LobbyServer.Instance.loginUser;
            var emptyBallType = RoomUtils.GetEmptyBallType(conf);

            GetInGameUser(entered.User, (InGameUser inGameUser) =>
            {
                MessagedEvent?.Invoke(new SystemMessage("Notice", LanguageManager.GetText("joinmessage", inGameUser.User.Username)));
            });


            //MyEnter
            if (entered.User == me.Id && RoomUtils.GetBallType(me.Id) == BallType.None)
            {
                isSpectator = (emptyBallType == BallType.None);
                SteamManager.Instance.ActivateInvite(connectedRoom.Id);
            }

            if (emptyBallType != BallType.None && conf.King == me.Id)
            {
                if (emptyBallType == BallType.Black)
                {
                    conf.Black = entered.User;
                }
                else
                {
                    conf.White = entered.User;
                }
                UpdateConf();
            }
            Debug.Log(UserEnteredEvent);
            UserEnteredEvent?.Invoke(entered.User, emptyBallType);
        }

        public void GetProfileTexture(int id, Action<Texture> callback)
        {
            if (_profileImages.ContainsKey(id))
            {
                callback(_profileImages[id]);
                return;
            }
            if (!connectedRoom.Users.Contains(id))
                return;

            GetInGameUser(id, (InGameUser inGameUser) =>
            {
                if (inGameUser.User.Picture == null)
                {
                    return;
                }
                LobbyServerApi.DownloadImage(inGameUser.User.Picture, (Texture texture) =>
                 {
                     if (!_profileImages.ContainsKey(id))
                         _profileImages.Add(id, texture);
                     callback(texture);
                 });
            });
        }

        private void OnLeft(Event e)
        {
            var left = (LeftEvent)e;
            MessagedEvent?.Invoke(new SystemMessage("Notice", LanguageManager.GetText("leftmessage", _users[left.User].Username)));
            connectedRoom.Users.Remove(left.User);
            _users.Remove(left.User);
            UserLeftEvent?.Invoke(left.User);
        }

        private void OnConfed(Event e)
        {
            var confed = (ConfedEvent)e;
            var myId = LobbyServer.Instance.loginUser.Id;
            connectedRoom.Conf = confed.Conf;
            isSpectator = (confed.Conf.Black != myId && confed.Conf.White != myId);
            ConfedEvent?.Invoke(confed.Conf);
        }

        public void OnGameStarted(Event e)
        {
            var gameStarted = (GameStartedEvent)e;
            gamePlaying = gameStarted;
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
            isInGame = true;
        }

        public void OnGameEnded(Event e)
        {
            var gameEnded = (EndedEvent)e;
            isInGame = false;
            EndedEvent?.Invoke(gameEnded);
            _isGameEnded = true;
        }

        public void OnError(Event e)
        {
            var error = (ErrorEvent)e;
            ToastManager.Instance.Add(error.Message, "Error");
        }

        public void OnChatted(Event e)
        {
            var chatted = (ChattedEvent)e;
            GetInGameUser(chatted.User, (InGameUser inGameUser) =>
             {
                 MessagedEvent?.Invoke(new UserMessage(inGameUser, chatted.Content));
             });
        }

        public void OnBanned(Event e)
        {
            var banned = (BannedEvent)e;
            GetInGameUser(banned.User, (InGameUser inGameUser) =>
            {
                MessagedEvent?.Invoke(new SystemMessage("Notice", LanguageManager.GetText("banmessage", inGameUser.User.Username)));
            });
        }

        public void EnterRoom(string ip, int port, string invite)
        {
            Connect(ip, port, () =>
            {
                var connectCommand = new ConnectCommand(invite);
                SendCommand(connectCommand);
            });
        }

        public void UpdateConf()
        {
            var command = new ConfCommand { Conf = connectedRoom.Conf };
            SendCommand(command);
        }

        public void ChangeUserRole(int id, BallType ballType)
        {
            var originBallType = RoomUtils.GetBallType(id);

            //기존에 관전자일시
            if (originBallType == BallType.None)
            {
                if (ballType == BallType.Black)
                {
                    connectedRoom.Conf.Black = id;
                }
                if (ballType == BallType.White)
                {
                    connectedRoom.Conf.White = id;
                }
            }
            //기존이 블랙일시
            else if (originBallType == BallType.Black)
            {
                if (ballType == BallType.None)
                {
                    connectedRoom.Conf.Black = -1;
                }
                if (ballType == BallType.White)
                {
                    var white = connectedRoom.Conf.White;
                    connectedRoom.Conf.White = id;
                    connectedRoom.Conf.Black = white;
                }
            }
            //기존이 화이트일시
            else
            {
                if (ballType == BallType.None)
                {
                    connectedRoom.Conf.White = -1;
                }
                if (ballType == BallType.Black)
                {
                    var black = connectedRoom.Conf.Black;
                    connectedRoom.Conf.Black = id;
                    connectedRoom.Conf.White = black;
                }
            }
            UpdateConf();
        }

        public void ChangeKingTo(int id)
        {
            connectedRoom.Conf.King = id;
            UpdateConf();
        }

        public void ChangeMapTo(string map)
        {
            connectedRoom.Conf.Map = map;
            UpdateConf();
        }

        public void SendChat(string message)
        {
            var command = new ChatCommand();
            command.Content = message;
            SendCommand(command);
        }

        public void BanUser(int id)
        {
            var command = new BanCommnad();
            command.User = id;
            SendCommand(command);
        }

        public void ExitGame()
        {
            _asyncCallbackClient.Close();
        }

        private void OnSocketClose()
        {
            ClearAll();
            if (_isGameEnded && connectedRoom != null && connectedRoom.Rank != null)
                return;
            SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
        }
        public void Surrender()
        {
            SendCommand(new GgCommand());
        }

        private void ClearAll()
        {
            UserEnteredEvent = null;
            UserLeftEvent = null;
            RoomConnectedEvent = null;
            ConfedEvent = null;
            MessagedEvent = null;

            connectedRoom = null;
            _users.Clear();
            _profileImages.Clear();

            gamePlaying = null;
            SteamManager.Instance.UnActivateInvite();
        }

        public bool CheckConfChanged()
        {
            if (_prevConf != connectedRoom.Conf)
            {
                _prevConf = connectedRoom.Conf;
                return true;
            }
            return false;
        }
    }
}
