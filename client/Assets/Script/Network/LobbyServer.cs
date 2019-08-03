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
using UnityEngine.Networking;

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

        public string address = "https://api.minda.games";
        LobbyServerRequestor requestor;

        public string token;
        public LoginState loginState { private set; get; } = LoginState.Logout;
        private string loginReqid;
        [SerializeField]
        private float loginCheckInterval = 3;

        private int steamRetries;
        public User loginUser = null;
        private Texture loginUserTexture = null;

        private Dictionary<int,LoadedSkin> skins = new Dictionary<int, LoadedSkin>();



        private void Awake()
        {
            address = SettingManager.GetSetting("ServerAddress");
            token = SettingManager.GetSetting("Token");
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
            if (token != "")
            {
                SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
                loginState = LoginState.Login;
                RefreshLoginUser((User user) =>
                {
                    ToastManager.instance.Add(LanguageManager.GetText("hello",user.username), "Success");
                });
                Debug.Log("로그인 성공");
            }
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

        //login
        private void HandleLoginResult(LoginResult loginResult)
        {
            this.token = loginResult.token;
            SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
            loginState = LoginState.Login;
            RefreshLoginUser((User user) =>
            {
                ToastManager.instance.Add(LanguageManager.GetText("hello",user.username), "Success");
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
                    ToastManager.instance.Add(LanguageManager.GetText("loginfailed"), "Error");
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

        public void GetLoadedSkin(Skin skin, Action<LoadedSkin> callback)
        {
            if(skins.ContainsKey(skin.id))
                callback(skins[skin.id]);

            LoadedSkin.Get(skin,callback);
        }

        //requestor
        public void Post<T>(string endPoint, string data, Action<T, int?> callBack)
        {
            StartCoroutine(requestor.Post(endPoint, data, token, callBack));
        }

        public void Post(string endPoint, WWWForm formData, Action<byte[], int?> callBack)
        {
            StartCoroutine(requestor.Post(endPoint, formData, token, callBack));
        }

        public void Get<T>(string endPoint, Action<T, int?> callBack)
        {
            StartCoroutine(requestor.Get(endPoint, token, callBack));
        }

        public void Put<T>(string endPoint, string data, Action<T, int?> callBack)
        {
            StartCoroutine(requestor.Put(endPoint, data, token, callBack));
        }
        public void Put(string endPoint, WWWForm formData, Action<byte[], int?> callBack)
        {
            StartCoroutine(requestor.Put(endPoint, formData, token, callBack));
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
                Debug.Log(me.inventory.two_color_skin);
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
                        ToastManager.instance.Add(LanguageManager.GetText("roomdoesntexist"), "Error");
                    }
                    else if(err == 500)
                    {
                        ToastManager.instance.Add(LanguageManager.GetText("fatalerror"), "Error");
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

        public void GetLoginUserProfileImage(Action<Texture> callback)
        {
            if(loginUser.picture == null)
            {
                callback(null);
                return;
            }
            else if(loginUserTexture != null)
            {
                callback(loginUserTexture);
                return;
            }
            LobbyServerAPI.DownloadImage(loginUser.picture,(Texture texture)=>
            {
                loginUserTexture = texture;
                callback(texture);
            });
        }

        public void RefreshLoginUserProfileImage(Action<Texture> callback)
        {
            if (loginUser.picture == null)
            {
                callback(null);
                return;
            }
            LobbyServerAPI.DownloadImage(loginUser.picture, (Texture texture) =>
             {
                 loginUserTexture = texture;
                 callback(texture);
             });
        }


        public void UploadImage(byte[] bytes,Action<Pic,int?> callback)
        {
            StartCoroutine(requestor.PostImage("/pics/", bytes, token, callback));
        }

        public bool IsLoginId(int id)
        {
            return id == loginUser.id;
        }
        
        public void EquipSkin(int? id,Action<int?> callback)
        {
            var json = JsonConvert.SerializeObject(new CurrentSkin(id));
            Put<EmptyResult>("/skins/me/current/",json,(result,err)=>
            {
                callback(err);
            });
        }

        public void GetLoadedSkin(int id,Action<LoadedSkin> callback)
        {
            Get<Skin>($"/skins/{id}/",(skin,err)=>
            {
                if(err!=null)
                {
                    ToastManager.instance.Add(LanguageManager.GetText("skinloaderror"),"Error");
                    return;
                }
                LoadedSkin.Get(skin,callback);
            });
        }


        public void SendBuyRequest(Action callback)
        {
            Post<EmptyResult>("/skins/buy/", "" ,(result, err) =>
             {
                 callback();
             });
        }

    }
}