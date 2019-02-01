using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using Game;
using Models;
using Newtonsoft.Json;
using Scene;
using UI.Toast;
using UnityEngine;

namespace Network
{
    public class LobbyServer : MonoBehaviour
    {
        public enum LoginState
        {
            Logout,
            SendReq,
            GetReqid,
            Login,
        }

        public static LobbyServer instance;

        public string address = "http://minda.games:8080";
        LobbyServerRequestor requestor;

        public string token;
        public LoginState loginState { private set; get; } = LoginState.Logout;
        private string loginReqid;
        [SerializeField]
        private float loginCheckInterval = 3;

        private int steamRetries;
        public User loginUser = null;


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

            requestor = new LobbyServerRequestor(address);
        }

        void Start()
        {
            if (SteamManager.isSteamVersion)
            {
                steamRetries = 10;
                TrySteamLogin();
            }
        }

        private void TrySteamLogin()
        {
            loginState = LoginState.GetReqid;
            var (ticket, handle) = SteamManager.instance.GetAuthTicket();
            Get("/auth/steam/?ticket="+ticket, (LoginResult loginResult, int? err) => 
            {
                SteamUser.CancelAuthTicket(handle);
                if (err != null)
                {
                    loginState = LoginState.Logout;
                    steamRetries --;
                    if (steamRetries > 0)
                    {
                        TrySteamLogin();
                    }
                    Debug.Log($"로그인 실패 {err}");
                    return;
                }
                HandleLoginResult(loginResult);
            });
        }

        private void Update()
        {

        }

        //login
        private void HandleLoginResult(LoginResult loginResult)
        {
            this.token = loginResult.token;
            SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
            loginState = LoginState.Login;
            RefreshLoginUser((User user) =>
            {
                ToastManager.instance.Add($"Hello, {user.username}", "Success");
            });
            Debug.Log("로그인 성공");
        }

        public void Login(string service)
        {
            loginState = LoginState.SendReq;
            Post("/auth/reqs/", "", (Reqid reqid, int? err) =>
            {
                if (err != null)
                {
                    loginState = LoginState.Logout;
                    Debug.Log(err);
                    return;
                }
                Application.OpenURL(address + "/auth/o/" + service + "/" + reqid.id + "/");
                loginState = LoginState.GetReqid;
                loginReqid = reqid.id;
                StartCoroutine(CheckLogin());
            });
        }

        private IEnumerator CheckLogin()
        {
            yield return new WaitForSeconds(loginCheckInterval);
            if (loginState != LoginState.GetReqid)
            {
                yield break;
            }
            Get<LoginResult>("/auth/reqs/" + loginReqid + "/", (LoginResult loginResult, int? err) =>
            {
                if (err != null)
                {
                    if (err == 403)
                    {
                        StartCoroutine(CheckLogin());
                        return;
                    }
                    ToastManager.instance.Add("Login Failed", "Error");
                    return;
                }
                HandleLoginResult(loginResult);
            });
        }

        public void Logout()
        {
            token = "";
            loginState = LoginState.Logout;
        }

        //requestor
        public void Post<T>(string endPoint, string data, Action<T, int?> callBack)
        {
            StartCoroutine(requestor.Post(endPoint, data, token, callBack));
        }

        public void Get<T>(string endPoint, Action<T, int?> callBack)
        {
            StartCoroutine(requestor.Get(endPoint, token, callBack));
        }

        public void Put<T>(string endPoint, string data, Action<T, int?> callBack)
        {
            StartCoroutine(requestor.Put(endPoint, data, token, callBack));
        }

        //loginImfomation
        public void RefreshLoginUser(Action<User> callback = null)
        {
            Get<User>("/users/me/", (User me, int? err) =>
            {
                if (err != null)
                {
                    Debug.Log(err);
                    return;
                }
                loginUser = me;
                if (callback != null)
                    callback(me);
            });
        }

        public void EnterRoom(string roomId, Action<bool> callback)
        {
            LobbyServer.instance.Put("/rooms/" + roomId + "/", "", (JoinRoomResult joinRoomResult, int? err) =>
            {
                if (err != null)
                {
                    if(err == 404)
                    {
                        ToastManager.instance.Add("Room doesn't exist", "Error");
                    }
                    else if(err == 500)
                    {
                        ToastManager.instance.Add("Fatal Error!", "Error");
                    }
                    callback(false);
                    return;
                }
                var Addr = joinRoomResult.addr.Split(':');
                SceneChanger.instance.ChangeTo("RoomConfigure");
                GameServer.instance.EnterRoom(Addr[0], int.Parse(Addr[1]), joinRoomResult.invite);
                callback(true);
            });
        }

    }
}