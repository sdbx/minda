using System;
using System.Collections;
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

        public IEnumerator Post<T>(string endPoint, string data, string token, Action<T, string> callBack)
        {
            if (data == "")
                data = "{}";
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(data);
            using (UnityWebRequest www = UnityWebRequest.Put(Addr + endPoint, postData))
            {
                Debug.Log("[Post]:" + Addr + endPoint + " Body:" + data);
                www.method = UnityWebRequest.kHttpVerbPOST;
                if (token != "")
                    www.SetRequestHeader("Authorization", token);
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                RequestCallBack(www.downloadHandler.text, callBack, www.error);
                Debug.Log(www.downloadHandler.text);
            }
        }

        public IEnumerator Put<T>(string endPoint, string data, string token, Action<T, string> callBack)
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

            using (UnityWebRequest www = UnityWebRequest.Put(Addr + endPoint, body))
            {
                Debug.Log("[Put]:" + Addr + endPoint + " Body:" + data);
                if (token != "")
                    www.SetRequestHeader("Authorization", token);
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                RequestCallBack(www.downloadHandler.text, callBack, www.error);
                Debug.Log(www.downloadHandler.text);
            }
        }

        public IEnumerator Get<T>(string endPoint, string token, Action<T, string> callBack)
        {

            using (UnityWebRequest www = UnityWebRequest.Get(Addr + endPoint))
            {
                Debug.Log("[Get]:" + Addr + endPoint);
                if (token != "")
                    www.SetRequestHeader("Authorization", token);
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                RequestCallBack(www.downloadHandler.text, callBack, www.error);
                Debug.Log(www.downloadHandler.text);
            }
        }

        private void RequestCallBack<T>(string data, Action<T, string> callBack, string err)
        {
            callBack(JsonConvert.DeserializeObject<T>(data), err);
        }
    }
}
