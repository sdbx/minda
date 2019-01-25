using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Game;
using Game.Events;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

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
        public delegate void GameEventHandler(Game.Events.Event e);
        private Dictionary<Type, List<GameEventHandler>> handlers = new Dictionary<Type, List<GameEventHandler>>();

        public event Action<int,BallType> UserEnteredEvent;
        public event Action<int> UserLeftEvent;
        public event Action<Room> RoomConnectedEvent;
        public event Action<Conf> ConfedEvent;
        
        public bool isSpectator = false;
        public Room connectedRoom;
        private Dictionary<int, User> users = new Dictionary<int, User>();
        private Dictionary<int, Texture> profileImages = new Dictionary<int, Texture>();

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

            AddHandler<ConnectedEvent>(OnConnected);
            AddHandler<EnteredEvent>(OnEntered);
            AddHandler<LeftEvent>(OnLeft);
            AddHandler<ConfedEvent>(OnConfed);
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
        }

        private void Connect(string ip, int port, Action callback)
        {
            if (asyncCallbackClient.state == ClientState.DISCONNECTED)
                asyncCallbackClient.Connect(ip, port, callback);
            else Debug.Log($"[AsyncCallbackClient]Already Connected {ip}:{port}");
        }

        private void ReceiveEvent(string data)
        {
            foreach (string splitedStr in data.Split('\n'))
            {
                if (splitedStr == "")
                    return;

                Game.Events.Event e = JsonConvert.DeserializeObject<Game.Events.Event>(splitedStr, eventJsonSettings);
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

        public void removeHandler<T>(GameEventHandler handler)
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
        private void OnConnected(Game.Events.Event e)
        {
            var connected = (ConnectedEvent)e;
            connectedRoom = connected.room;
            RoomConnectedEvent?.Invoke(connected.room);
        }

        private void OnEntered(Game.Events.Event e)
        {
            var entered = (EnteredEvent)e;
            connectedRoom.Users.Add(entered.user);

            var conf = connectedRoom.conf;
            var me = LobbyServer.instance.loginUser;
            var emptyBallType = RoomUtils.GetEmptyBallType(conf);

            //MyEnter
            if(entered.user == me.id)
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
                LobbyServerAPI.DownloadImage(inGameUser.user.picture,(Texture texture)=>
                {
                    if(!profileImages.ContainsKey(id))
                        profileImages.Add(id,texture);
                    callback(texture);
                });
            });
        }

        private void OnLeft(Game.Events.Event e)
        {
            var left = (LeftEvent)e;
            connectedRoom.Users.Remove(left.user);
            users.Remove(left.user);
            UserLeftEvent?.Invoke(left.user);
        }

        private void OnConfed(Game.Events.Event e)
        {
            var confed = (ConfedEvent)e;
            var myId = LobbyServer.instance.loginUser.id;
            isSpectator = (connectedRoom.conf.black != myId && connectedRoom.conf.white != myId);
            connectedRoom.conf = confed.conf;
            ConfedEvent?.Invoke(confed.conf);
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
    }
}