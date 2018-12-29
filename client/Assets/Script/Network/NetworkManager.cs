using System;
using System.Collections.Generic;
using Game;
using Game.Events;
using Network;
using Newtonsoft.Json;
using UnityEngine;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public LobbyServerRequestor lobbyServerRequestor;
        public static NetworkManager instance = null;
        public Menu.Models.User loginUser;

        [SerializeField]
        private string addr = "http://127.0.0.1:8080";
        private AsyncCallbackClient asyncCallbackClient = null;
        private Dictionary<Type, Action<Game.Events.Event>> handle = new Dictionary<Type, Action<Game.Events.Event>>();
        private JsonSerializerSettings eventJsonSettings;

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
        }

        void OnApplicationQuit()
        {
            asyncCallbackClient.Close();
        }

        public void EnterRoom(string ip, int port, string invite, Action<Game.Events.Event> connectedCallback)
        {
            SetHandler<ConnectedEvent>(connectedCallback);
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
            SendData(json);
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


        public bool IsConnected()
        {
            return asyncCallbackClient.state == ClientState.CONNECTED;
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
        

    }
}