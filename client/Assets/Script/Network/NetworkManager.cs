using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Events;
using Models;
using Network;
using Newtonsoft.Json;
using Scene;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public enum LoginState
        {
            Logout,
            SendReq,
            GetReqid,
            Login,
        }
        public enum RoomState
        {
            Connected,
            Entered,
            Connecting,
            NotConnected,
        } 
        public LobbyServerRequestor lobbyServerRequestor;
        public static NetworkManager instance = null;
        public string token;
        public User loggedInUser;
        public LoginState loginState = LoginState.Logout;
        
        [SerializeField]
        private string addr = "http://127.0.0.1:8080";
        private AsyncCallbackClient asyncCallbackClient = null;
        private Dictionary<Type, Action<Game.Events.Event>> handle = new Dictionary<Type, Action<Game.Events.Event>>();
        private JsonSerializerSettings eventJsonSettings;

        private string loginReqid;
        private Action loginCallback;
        private float loginCheckingTimer = 2;

        private RoomState roomState;
        public Room connectedRoom;
        public GameStartedEvent game;

        public Action myEnterCallBack;
        public Action<int> otherUserEnterCallBack;
        public Action<RoomSettings> confedCallBack;
        public Action<int> userLeftCallBack;
        
        private void Awake() 
        {
            if(instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            lobbyServerRequestor = new LobbyServerRequestor(addr);
            asyncCallbackClient = new AsyncCallbackClient();

            eventJsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            };
            eventJsonSettings.Converters.Add(new EventConverter());
        }

        private void Start() 
        {
            SetHandler<ConnectedEvent>(ConnectedHandler);
            SetHandler<EnteredEvent>(UserEnteredHandler);
            SetHandler<LeftEvent>(UserLeftCallBack);
            SetHandler<ConfedEvent>(ConfedCallBack);
        }

        private void Update()
        {
            int dataCount = asyncCallbackClient.dataQueue.Count;
            if (dataCount > 0)
            {
                for (int i = 0; i < dataCount; i++)
                {
                    string data = asyncCallbackClient.dataQueue.Dequeue();
                    ReceiveData(data);
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
            if (loginState == LoginState.GetReqid)
            {
                if (loginCheckingTimer < 0)
                {
                    try
                    {
                        Get("/auth/reqs/" + loginReqid + "/", (LoginResult loginResult, string err) =>
                        {
                            if (err != null)
                            {
                                Debug.Log($"로그인 실패 {err}");
                                return;
                            }
                            this.token = loginResult.token;
                            loginCallback();
                            loginCallback = null;
                            loginState = LoginState.Login;
                            Debug.Log("로그인 성공");
                            RefreshLoggedInUser();
                        });
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        Debug.Log("로그인 실패");
                    }
                    loginCheckingTimer = 2;   
                }
                else loginCheckingTimer -= Time.deltaTime;
            }

        }

        void OnApplicationQuit()
        {
            asyncCallbackClient.Close();
        }

        public void EnterRoom(string ip, int port, string invite)
        {
            Connect(ip, port);
            asyncCallbackClient.connectedCallback = ()=> 
            { 
                ConnectCommand connectCommand = new ConnectCommand(invite);
                SendCommand(connectCommand);
            };
        }

        private void Connect(string ip, int port)
        {
            if (asyncCallbackClient.state == ClientState.DISCONNECTED)
                asyncCallbackClient.Connect(ip, port);
        }

        public void SendCommand(Command command)
        {
            string json = JsonConvert.SerializeObject(command);
            SendData(json + "\n");
        }

        public void SetHandler<T>(Action<Game.Events.Event> handler)
        {
            Type type = typeof(T);
            if(!handle.ContainsKey(type))
            {
                handle[type] = handler;
            }
        }

        public void ClearHandles()
        {
            handle.Clear();
        }

        private void ConnectedHandler(Game.Events.Event e)
        {
            var connected = (ConnectedEvent)e;
            roomState = RoomState.Connected;
            connectedRoom = connected.room;
        }

        private void UserEnteredHandler(Game.Events.Event e)
        {
            var entered = (EnteredEvent)e;
            connectedRoom.Users.Add(entered.user);
            if(entered.user == loggedInUser.id)
            {
                roomState = RoomState.Entered;
                myEnterCallBack();
            }
            else
            {
                otherUserEnterCallBack(entered.user);
            }    
        }

        private void UserLeftCallBack(Game.Events.Event e)
        {
            var left = (LeftEvent)e;
            connectedRoom.Users.Remove(left.user);
            userLeftCallBack(left.user);
        }

        public void UpdateConf()
        {
            ConfCommand command = new ConfCommand { conf = connectedRoom.conf };
            NetworkManager.instance.SendCommand(command);
        }

        private void ConfedCallBack(Game.Events.Event e)
        {
            var confed = (ConfedEvent)e;
            confedCallBack(confed.conf);
        }

        private void GameStartedCallBack(Game.Events.Event e)
        {
            var gameStarted = (GameStartedEvent)e;
            game = gameStarted;
            SceneChanger.instance.ChangeTo("Game");
        }
        
        public void EndConnection()
        {
            ClearHandles();
            asyncCallbackClient.Close();
            asyncCallbackClient.connectedCallback = null;
        }

        private void SendData(string data)
        {
            asyncCallbackClient.SendData(data);
        }
        

        public void Post<T>(string endPoint, string data, Action<T, string> callBack)
        {
            StartCoroutine(lobbyServerRequestor.Post(endPoint, data, token, callBack));
        }
        
        public void Get<T>(string endPoint, Action<T, string> callBack)
        {
            StartCoroutine(lobbyServerRequestor.Get(endPoint, token, callBack));
        }

        public void Put<T>(string endPoint, string data, Action<T, string> callBack)
        {
            StartCoroutine(lobbyServerRequestor.Put(endPoint, data, token, callBack));
        }

        public bool IsConnected()
        {
            return asyncCallbackClient.state == ClientState.CONNECTED;
        }
        
        public void DownloadImage(string imgUrl, Action<Texture> callBack)
        {
            StartCoroutine(GetTexture(imgUrl,callBack));
        }

        IEnumerator GetTexture(string imgUrl, Action<Texture> callBack)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(imgUrl);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                callBack(((DownloadHandlerTexture)www.downloadHandler).texture);
            }
        }

        private void ReceiveData(string data)
        {
            foreach (string splitedStr in data.Split('\n'))
            {
                if (splitedStr == "")
                    return;

                Game.Events.Event e = JsonConvert.DeserializeObject<Game.Events.Event>(splitedStr, eventJsonSettings);
                handle[e.GetType()](e);
            }
        }
        
        public void login(string service, Action callBack)
        {
            if(loginState != LoginState.Logout)
            {
                return;
            }
            loginState = LoginState.SendReq;
            loginCallback = callBack;
            Post("/auth/reqs/","",(Reqid reqid, string err)=>{
                if(err!=null)
                {
                    Debug.Log(err);
                    return;
                }
                Application.OpenURL(addr+"/auth/o/"+service+"/"+reqid.id+"/");
                loginState = LoginState.GetReqid;
                loginReqid = reqid.id;
            });
        }

        public void RefreshLoggedInUser()
        {
            Get<User>("/users/me/", (User me, string err) =>
            {
                if(err!=null)
                {
                    Debug.Log(err);
                    return;
                }
                loggedInUser = me;
            });
        }

        public void RefreshLoggedInUser(Action refreshed)
        {
            Get<User>("/users/me/", (User me, string err) =>
            {
                if(err!=null)
                {
                    Debug.Log(err);
                    return;
                }
                loggedInUser = me;
                refreshed();
            });
        }

        public void GetUserInformation(int id,Action<User> callback)
        {
            Network.NetworkManager.instance.Get<User>("/users/" + id + "/", (User user, string err) =>
            {
                if (err != null)
                {
                    callback(null);
                }
                else
                {
                    callback(user);
                }
            });
        }

    }
}