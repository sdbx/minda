using System;
using System.Collections;
using Menu.Models;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

namespace Menu
{
    public class LobbyServerRequestor
    {
        string ip = "http://127.0.0.1:8080";

        public LobbyServerRequestor(string ip)
        {
            this.ip = ip;
        }
        
        public IEnumerator Post<T>(string endPoint,string data,Action<T> callBack,string token)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(ip+endPoint, data))
            {

                www.SetRequestHeader("Authorization",token);
                www.SetRequestHeader("Content-Type","application/json");

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Post Complete");
                }
            }
        }

        public IEnumerator Put<T>(string endPoint, string data, Action<T> callBack, string token)
        {
            byte[] body;
            if (data == "")
            {
                var packetToSend = JsonUtility.ToJson(string.Empty);
                body = Encoding.UTF8.GetBytes(packetToSend);
            }
            else
            {
                body = Encoding.UTF8.GetBytes(data);
            }

            using (UnityWebRequest www = UnityWebRequest.Put(ip+endPoint, body))
            {

                www.SetRequestHeader("Authorization",token);
                www.SetRequestHeader("Content-Type","application/json");

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    RequestCallBack<T>(www.downloadHandler.text,callBack);
                    Debug.Log("Put Complete");
                }
            }
        }

        public IEnumerator Get<T>(string endPoint,Action<T> callBack,string token)
        {
            
            using (UnityWebRequest www = UnityWebRequest.Get(ip+endPoint))
            {
                Debug.Log(ip+endPoint);
                www.SetRequestHeader("Authorization",token);
                www.SetRequestHeader("Content-Type","application/json");

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    RequestCallBack(www.downloadHandler.text,callBack);
                    Debug.Log(www.downloadHandler.text);
                }
            }
        }


        private void RequestCallBack<T>(string data, Action<T> callBack)
        {
            callBack(JsonConvert.DeserializeObject<T>(data));
        }
    }
}
