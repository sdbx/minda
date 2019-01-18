using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Game;
using Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Network
{
    public class LobbyServer : MonoBehaviour
    {
        private enum LoginState
        {
            Logout,
            SendReq,
            GetReqid,
            Login,
        }

        public static LobbyServer instance;

        [SerializeField]
        private string address = "http://minda.games:8080";
        LobbyServerRequestor requestor;

        public string token;
        private LoginState loginState = LoginState.Logout;
        private Action loginCallback;
        private string loginReqid;
        [SerializeField]
        private float loginCheckInterval = 3;
        private float loginTimeLeft = 0;

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
        
        private void Update()
        {
            if(loginState==LoginState.GetReqid)
            {
                if(loginTimeLeft<=0)
                {
                    Get<LoginResult>("/auth/reqs/" + loginReqid + "/", CheckLogin);
                    loginTimeLeft=loginCheckInterval;
                }
                else loginTimeLeft-=Time.deltaTime;
            }
        }

        //login
        private void CheckLogin(LoginResult loginResult, string err)
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
            RefreshLoginUser();
            Debug.Log("로그인 성공");
        }
        
        public void login(string service,Action callback)
        {
            if(loginState != LoginState.Logout)
            {
                return;
            }
            loginState = LoginState.SendReq;
            loginCallback = callback;
            Post("/auth/reqs/","",(Reqid reqid, string err)=>{
                if(err!=null)
                {
                    Debug.Log(err);
                    return;
                }
                Application.OpenURL(address+"/auth/o/"+service+"/"+reqid.id+"/");
                loginState = LoginState.GetReqid;
                loginReqid = reqid.id;
            });
        }

        public void logout()
        {
            token = "";
            loginState = LoginState.Logout;
        }

        //requestor
        public void Post<T>(string endPoint, string data, Action<T, string> callBack)
        {
            StartCoroutine(requestor.Post(endPoint, data, token, callBack));
        }
        
        public void Get<T>(string endPoint, Action<T, string> callBack)
        {
            StartCoroutine(requestor.Get(endPoint, token, callBack));
        }

        public void Put<T>(string endPoint, string data, Action<T, string> callBack)
        {
            StartCoroutine(requestor.Put(endPoint, data, token, callBack));
        }

        //loginImfomation
        public void RefreshLoginUser(Action<User> callback = null)
        {
            Get<User>("/users/me/", (User me, string err) =>
            {
                if(err!=null)
                {
                    Debug.Log(err);
                    return;
                }
                loginUser = me;
                if(callback !=null)
                    callback(me);
            });
        }
        
    }
}