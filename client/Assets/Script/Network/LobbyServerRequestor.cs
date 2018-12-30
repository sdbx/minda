using System;
using System.Collections;
using Menu.Models;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

namespace Network
{
    public class LobbyServerRequestor
    {
        string Addr = "";

        public LobbyServerRequestor(string Addr)
        {
            this.Addr = Addr;
        }
        
        public IEnumerator Post<T>(string endPoint, string data,string token, Action<T> callBack)
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

            using (UnityWebRequest www = UnityWebRequest.Post(Addr+endPoint, data))
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

        public IEnumerator Put<T>(string endPoint, string data,string token, Action<T> callBack)
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

            using (UnityWebRequest www = UnityWebRequest.Put(Addr+endPoint, body))
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

        public IEnumerator Get<T>(string endPoint, string token, Action<T> callBack)
        {

            using (UnityWebRequest www = UnityWebRequest.Get(Addr+endPoint))
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
