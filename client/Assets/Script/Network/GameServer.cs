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
        public static GameServer instance;

        //socket
        private SocketClient asyncCallbackClient = new SocketClient();
        //event
        private JsonSerializerSettings eventJsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
        };
        public delegate void GameEventHandler(Event e);
        private Dictionary<Type, List<GameEventHandler>> handlers = new Dictionary<Type, List<GameEventHandler>>();

        public event Action<int,BallType> UserEnteredEvent;
        public event Action<int> UserLeftEvent;
        public event Action<Room> RoomConnectedEvent;
        public event Action<Conf> ConfedEvent;
        public event Action<Message> MessagedEvent;
        public event Action<GameStartedEvent> gameStarted;
        public event Action<EndedEvent> endedEvent;
        
        public bool isSpectator = false;
        public Room connectedRoom;
        private Dictionary<int, User> users = new Dictionary<int, User>();
        private Dictionary<int, Texture> profileImages = new Dictionary<int, Texture>();
        
        public GameStartedEvent gamePlaying;
        public bool isInGame;

        private Conf prevConf;

        private bool  isGameEnded = false;

        private void Awake()
        {
            //singleton
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            eventJsonSettings.Converters.Add(new EventConverter());
            InitHandlers();
            asyncCallbackClient.closeSocketCallback = OnSocketClose;
        }

        private void Update()
        {
            int dataCount = asyncCallbackClient.dataQueue.Count;
            if (dataCount > 0)
            {
                for (int i = 0; i < dataCount; i++)
                {
                    string data = asyncCallbackClient.dataQueue.Dequeue();
                    ReceiveEvent(data);
                }
            }

            int logCount = asyncCallbackClient.logQueue.Count;
            if (logCount > 0)
            {
                for (int i = 0; i < logCount; i++)
                {
                    Debug.Log(asyncCallbackClient.logQueue.Dequeue());
                }
            }

            int callbackCount = asyncCallbackClient.callbackQuene.Count;
            if (callbackCount > 0)
            {
                for (int i = 0; i < callbackCount; i++)
                {
                    asyncCallbackClient.callbackQuene.Dequeue()();
                }
            }
        }

        private void Connect(string ip, int port, Action callback)
        {
            if (asyncCallbackClient.state == ClientState.DISCONNECTED)
                asyncCallbackClient.Connect(ip, port, callback);
            else Debug.Log($"[AsyncCallbackClient] Already Connected {ip}:{port}");
        }

        private void ReceiveEvent(string data)
        {
            foreach (string splitedStr in data.Split('\n'))
            {
                if (splitedStr == "")
                    return;

                Event e = JsonConvert.DeserializeObject<Event>(splitedStr, eventJsonSettings);
                Debug.Log(e.GetType());
                foreach(var handle in handlers[e.GetType()])
                {
                    handle(e);
                }
            }
        }

        public void EndConnection()
        {
            ClearHandles();
            asyncCallbackClient.Close();
        }

        public void ClearHandles()
        {
            handlers.Clear();
        }

        public void SendCommand(Command command)
        {
            string json = JsonConvert.SerializeObject(command);
            asyncCallbackClient.SendData(json + "\n");
        }

        public void AddHandler<T>(GameEventHandler handler)
        {
            Type type = typeof(T);
            if(!handlers.ContainsKey(type))
            {
                handlers[type] = new List<GameEventHandler>();
            }
            handlers[type].Add(handler);
        }

        public void RemoveHandler<T>(GameEventHandler handler)
        {
            Type type = typeof(T);
            handlers[type].Remove(handler);
        }
        //game
        public void GetInGameUser(int id, Action<InGameUser> callback)
        {
            if(!connectedRoom.Users.Contains(id))
                return;

            if(!users.ContainsKey(id))
            {
                LobbyServerAPI.GetUserInformation(id, (User user) =>{
                    users[id] = user;
                    GetInGameUser(id,callback);
                });
                return;
            }
            var inGameUser = new InGameUser();
            inGameUser.user = users[id];
            inGameUser.isKing = (connectedRoom.conf.king == id);
            inGameUser.ballType = RoomUtils.GetBallType(id);

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
            connectedRoom = connected.room;
            RoomConnectedEvent?.Invoke(connected.room);
        }

        private void OnEntered(Event e)
        {
            var entered = (EnteredEvent)e;
            connectedRoom.Users.Add(entered.user);

            var conf = connectedRoom.conf;
            var me = LobbyServer.instance.loginUser;
            var emptyBallType = RoomUtils.GetEmptyBallType(conf);
            
            GetInGameUser(entered.user,(InGameUser inGameUser)=>{
                MessagedEvent?.Invoke(new SystemMessage("Notice",LanguageManager.GetText("joinmessage",inGameUser.user.username)));
            });
           

            //MyEnter
            if(entered.user == me.id&&RoomUtils.GetBallType(me.id)==BallType.None)
            {
                isSpectator = (emptyBallType == BallType.None);
            }

            if (emptyBallType != BallType.None && conf.king == me.id)
            {
                if (emptyBallType == BallType.Black)
                {
                    conf.black = entered.user;
                }
                else
                {
                    conf.white = entered.user;
                }
                UpdateConf();
            }
            Debug.Log(UserEnteredEvent);
            UserEnteredEvent?.Invoke(entered.user, emptyBallType);
        }

        public void GetProfileTexture(int id, Action<Texture> callback)
        {
            if(profileImages.ContainsKey(id))
            {
                callback(profileImages[id]);
                return;
            }
            if(!connectedRoom.Users.Contains(id))
                return;

            GetInGameUser(id, (InGameUser inGameUser)=>
            {
                if(inGameUser.user.picture==null)
                {
                    return;
                }
                LobbyServerAPI.DownloadImage(inGameUser.user.picture,(Texture texture)=>
                {
                    if(!profileImages.ContainsKey(id))
                        profileImages.Add(id,texture);
                    callback(texture);
                });
            });
        }

        private void OnLeft(Event e)
        {
            var left = (LeftEvent)e;
            MessagedEvent?.Invoke(new SystemMessage("Notice", LanguageManager.GetText("leftmessage",users[left.user].username)));
            connectedRoom.Users.Remove(left.user);
            users.Remove(left.user);
            UserLeftEvent?.Invoke(left.user);
        }

        private void OnConfed(Event e)
        {
            var confed = (ConfedEvent)e;
            var myId = LobbyServer.instance.loginUser.id;
            connectedRoom.conf = confed.conf;
            isSpectator = (confed.conf.black != myId && confed.conf.white != myId);
            ConfedEvent?.Invoke(confed.conf);
        }

        public void OnGameStarted(Event e)
        {
            var gameStarted = (GameStartedEvent)e;
            gamePlaying = gameStarted;
            SceneManager.LoadScene("Game",LoadSceneMode.Single);
            isInGame = true;
        }

        public void OnGameEnded(Event e)
        {
            var gameEnded = (EndedEvent)e;
            isInGame = false;
            endedEvent?.Invoke(gameEnded);
            isGameEnded = true;
        }
        
        public void OnError(Event e)
        {
            var error = (ErrorEvent)e;
            ToastManager.instance.Add(error.message,"Error");
        }

        public void OnChatted(Event e)
        {
            var chatted = (ChattedEvent)e;
            GetInGameUser(chatted.user,(InGameUser inGameUser)=>
            {
                MessagedEvent?.Invoke(new UserMessage(inGameUser,chatted.content));
            });
        }

        public void OnBanned(Event e)
        {
            var banned = (BannedEvent)e;
            GetInGameUser(banned.user, (InGameUser inGameUser) =>
            {
                MessagedEvent?.Invoke(new SystemMessage("Notice", LanguageManager.GetText("banmessage",inGameUser.user.username)));
            });
        }

        public void EnterRoom(string ip, int port, string invite)
        {
            Connect(ip, port, ()=> 
            { 
                ConnectCommand connectCommand = new ConnectCommand(invite);
                SendCommand(connectCommand);
            });
        }

        public void UpdateConf()
        {
            ConfCommand command = new ConfCommand { conf = connectedRoom.conf };
            SendCommand(command);
        }

        public void ChangeUserRole(int id, BallType ballType)
        {
            var originBallType = RoomUtils.GetBallType(id);

            //기존에 관전자일시
            if (originBallType == BallType.None)
            {
                if(ballType == BallType.Black)
                {
                    connectedRoom.conf.black = id;
                }
                if(ballType == BallType.White)
                {
                    connectedRoom.conf.white = id;
                }
            }
            //기존이 블랙일시
            else if(originBallType == BallType.Black)
            {
                if(ballType == BallType.None)
                {
                    connectedRoom.conf.black = -1;
                }
                if(ballType == BallType.White)
                {
                    var white = connectedRoom.conf.white;
                    connectedRoom.conf.white = id;
                    connectedRoom.conf.black = white;
                }
            }
            //기존이 화이트일시
            else
            {
                if(ballType == BallType.None)
                {
                    connectedRoom.conf.white = -1;
                }
                if(ballType == BallType.Black)
                {
                    var black = connectedRoom.conf.black;
                    connectedRoom.conf.black = id;
                    connectedRoom.conf.white = black;
                }
            }
            UpdateConf();
        }

        public void ChangeKingTo(int id)
        {
            connectedRoom.conf.king = id;
            UpdateConf();
        }

        public void ChangeMapTo(string map)
        {
            connectedRoom.conf.map = map;
            UpdateConf();
        }

        public void SendChat(string message)
        {
            ChatCommand command = new ChatCommand();
            command.content = message;
            SendCommand(command);
        }

        public void BanUser(int id)
        {
            BanCommnad command = new BanCommnad();
            command.user = id;
            SendCommand(command);
        }

        public void ExitGame()
        {
            asyncCallbackClient.Close();   
        }

        private void OnSocketClose()
        {
            ClearAll();
            if(isGameEnded&&connectedRoom.roomRank!=null)
                return;
            SceneManager.LoadSceneAsync("Menu",LoadSceneMode.Single);
        }
        public void Surrender()
        {
            SendCommand(new GGCommand());
        }

        private void ClearAll()
        {
            UserEnteredEvent = null;
            UserLeftEvent = null;
            RoomConnectedEvent = null;
            ConfedEvent = null;
            MessagedEvent = null;

            connectedRoom = null;
            users.Clear();
            profileImages.Clear();

            gamePlaying = null;
        }

        public bool CheckConfChanged()
        {
            if(prevConf!=connectedRoom.conf)
            {
                prevConf = connectedRoom.conf;
                return true;
            }
            return false;
        }
    }
}