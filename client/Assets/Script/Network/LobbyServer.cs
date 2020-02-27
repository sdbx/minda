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

        public static LobbyServer Instance;

        public string address = "https://api.minda.games";
        private LobbyServerRequestor _requestor;

        public string token;
        public LoginState CurrentLoginState { private set; get; } = LoginState.Logout;
        private string _loginReqid;
        [SerializeField]
        private float loginCheckInterval = 3;

        private int _steamRetries;
        public User loginUser = null;
        private Texture _loginUserTexture = null;

        private Dictionary<int, LoadedSkin> _skins = new Dictionary<int, LoadedSkin>();

        private string _inviteCode = "";

        public void JoinInvitedRoom(string roomid)
        {
            if (CurrentLoginState == LoginState.Login)
            {
                EnterRoom(roomid, (b) => { });
            }
            else
            {
                _inviteCode = roomid;
            }
        }

        private void Awake()
        {
            // address = SettingManager.GetSetting("ServerAddress");
            //token = SettingManager.GetSetting("Token");
            //singleton
            if (Instance == null)
            {
                Instance = this;
            }

            var args = System.Environment.GetCommandLineArgs();
            var invite = "";
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "invite" && args.Length > i + 1)
                {
                    invite = args[i + 1];
                }
                if (args[i] == "ip" && args.Length > i + 1)
                {
                    address = args[i + 1];
                }
                if (args[i] == "token" && args.Length > i + 1)
                {
                    token = args[i + 1];
                }
            }

            if (!string.IsNullOrEmpty(invite))
            {
                _inviteCode = invite;
            }


            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);

            _requestor = new LobbyServerRequestor(address);
        }

        private void Start()
        {
            if (token != "" && token != "null")
            {
                SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
                CurrentLoginState = LoginState.Login;
                RefreshLoginUser((User user) =>
                {
                    ToastManager.Instance.Add(LanguageManager.GetText("hello", user.Username), "Success");
                });
                Debug.Log("로그인 성공");
                return;
            }
            if (SteamManager.IsSteamVersion)
            {
                _steamRetries = 10;
                TrySteamLogin();
            }
        }

        private void TrySteamLogin()
        {
            CurrentLoginState = LoginState.GetReqid;
            var (ticket, handle) = SteamManager.Instance.GetAuthTicket();
            Get("/auth/steam/?ticket=" + ticket, (LoginResult loginResult, int? err) =>
              {
                  SteamUser.CancelAuthTicket(handle);
                  if (err != null)
                  {
                      CurrentLoginState = LoginState.Logout;
                      _steamRetries--;
                      if (_steamRetries > 0)
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
            this.token = loginResult.Token;
            SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
            CurrentLoginState = LoginState.Login;
            RefreshLoginUser((User user) =>
            {
                ToastManager.Instance.Add(LanguageManager.GetText("hello", user.Username), "Success");
            });
            Debug.Log("로그인 성공");


            if (_inviteCode != "")
            {
                EnterRoom(_inviteCode, (b) => { });
            }

        }

        public void Login(string service)
        {
            CurrentLoginState = LoginState.SendReq;
            Post("/auth/reqs/", "", (Reqid reqid, int? err) =>
            {
                if (err != null)
                {
                    CurrentLoginState = LoginState.Logout;
                    Debug.Log(err);
                    return;
                }
                Application.OpenURL(address + "/auth/o/" + service + "/" + reqid.Id + "/");
                CurrentLoginState = LoginState.GetReqid;
                _loginReqid = reqid.Id;
                StartCoroutine(CheckLogin());
            });
        }

        private IEnumerator CheckLogin()
        {
            yield return new WaitForSeconds(loginCheckInterval);
            if (CurrentLoginState != LoginState.GetReqid)
            {
                yield break;
            }
            Get<LoginResult>("/auth/reqs/" + _loginReqid + "/", (LoginResult loginResult, int? err) =>
            {
                if (err != null)
                {
                    if (err == 403)
                    {
                        StartCoroutine(CheckLogin());
                        return;
                    }
                    ToastManager.Instance.Add(LanguageManager.GetText("loginfailed"), "Error");
                    return;
                }
                HandleLoginResult(loginResult);
            });
        }

        public void Logout()
        {
            token = "";
            CurrentLoginState = LoginState.Logout;
        }

        public void GetLoadedSkin(Skin skin, Action<LoadedSkin> callback)
        {
            if (_skins.ContainsKey(skin.Id))
                callback(_skins[skin.Id]);

            LoadedSkin.Get(skin, callback);
        }

        //requestor
        public void Post<T>(string endPoint, string data, Action<T, int?> callBack)
        {
            StartCoroutine(_requestor.Post(endPoint, data, token, callBack));
        }

        public void Post(string endPoint, WWWForm formData, Action<byte[], int?> callBack)
        {
            StartCoroutine(_requestor.Post(endPoint, formData, token, callBack));
        }

        public void Get<T>(string endPoint, Action<T, int?> callBack)
        {
            StartCoroutine(_requestor.Get(endPoint, token, callBack));
        }

        public void Put<T>(string endPoint, string data, Action<T, int?> callBack)
        {
            StartCoroutine(_requestor.Put(endPoint, data, token, callBack));
        }
        public void Put(string endPoint, WWWForm formData, Action<byte[], int?> callBack)
        {
            StartCoroutine(_requestor.Put(endPoint, formData, token, callBack));
        }
        public void Delete(string endPoint, Action<int?> callBack)
        {
            StartCoroutine(_requestor.Delete(endPoint, token, callBack));
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
                Debug.Log(me.Inventory.TwoColorSkin);
                if (callback != null)
                    callback(me);
            });
        }

        public void EnterRoom(string roomId, Action<bool> callback)
        {
            LobbyServer.Instance.Put("/rooms/" + roomId + "/", "", (JoinRoomResult joinRoomResult, int? err) =>
            {
                if (err != null)
                {
                    if (err == 404)
                    {
                        ToastManager.Instance.Add(LanguageManager.GetText("roomdoesntexist"), "Error");
                    }
                    else if (err == 500)
                    {
                        ToastManager.Instance.Add(LanguageManager.GetText("fatalerror"), "Error");
                    }
                    callback(false);
                    return;
                }
                var addr = joinRoomResult.Addr.Split(':');
                SceneManager.LoadScene("RoomConfigure");
                GameServer.Instance.EnterRoom(addr[0], int.Parse(addr[1]), joinRoomResult.Invite);
                callback(true);
            });
        }

        public void GetLoginUserProfileImage(Action<Texture> callback)
        {
            if (loginUser.Picture == null)
            {
                callback(null);
                return;
            }
            else if (_loginUserTexture != null)
            {
                callback(_loginUserTexture);
                return;
            }
            LobbyServerApi.DownloadImage(loginUser.Picture, (Texture texture) =>
             {
                 _loginUserTexture = texture;
                 callback(texture);
             });
        }

        public void RefreshLoginUserProfileImage(Action<Texture> callback)
        {
            if (loginUser.Picture == null)
            {
                callback(null);
                return;
            }
            LobbyServerApi.DownloadImage(loginUser.Picture, (Texture texture) =>
             {
                 _loginUserTexture = texture;
                 callback(texture);
             });
        }


        public void UploadImage(byte[] bytes, Action<Pic, int?> callback)
        {
            StartCoroutine(_requestor.PostImage("/pics/", bytes, token, callback));
        }

        public bool IsLoginId(int id)
        {
            return id == loginUser.Id;
        }

        public void EquipSkin(int? id, Action<int?> callback)
        {
            var json = JsonConvert.SerializeObject(new CurrentSkin(id));
            Put<EmptyResult>("/skins/me/current/", json, (result, err) =>
              {
                  callback(err);
              });
        }

        public void GetLoadedSkin(int id, Action<LoadedSkin> callback)
        {
            Get<Skin>($"/skins/{id}/", (skin, err) =>
             {
                 if (err != null)
                 {
                     ToastManager.Instance.Add(LanguageManager.GetText("skinloaderror"), "Error");
                     return;
                 }
                 LoadedSkin.Get(skin, callback);
             });
        }


        public void SendBuyRequest(Action callback)
        {
            Post<EmptyResult>("/skins/buy/", "", (result, err) =>
             {
                 callback();
             });
        }

    }
}
