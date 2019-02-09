using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using UI.Toast;
using Models;

namespace Network
{
    public class LobbyServerRequestor
    {
        string Addr = "";

        public LobbyServerRequestor(string Addr)
        {
            this.Addr = Addr;
        }

        public IEnumerator Post<T>(string endPoint, string data, string token, Action<T, int?> callBack)
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

        public IEnumerator Put<T>(string endPoint, string data, string token, Action<T, int?> callBack)
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

                RequestCallBack(www.downloadHandler.text, callBack ,www.error);
                Debug.Log(www.downloadHandler.text);
            }
        }

        public IEnumerator Get<T>(string endPoint, string token, Action<T, int?> callBack)
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

        private void RequestCallBack<T>(string data, Action<T, int?> callBack, string err)
        {
           
            if (err != null)
            {
                var errorCode = err.Split(' ')[1];
                
                if(int.TryParse(errorCode,out int parsedCode))
                {
                    
                    callBack(default(T), parsedCode);
                    return;
                }
                callBack(default(T), 500);
                return;
            }
            if(data == "")
            {
                callBack(default(T), null);
                return;
            }
            callBack(JsonConvert.DeserializeObject<T>(data),null);
        }


        public IEnumerator PostImage(string endPoint, byte[] data, string token, Action<Pic, int?> callBack)
        {
            // WWWForm postForm = new WWWForm();
            // postForm.AddBinaryData(data);
            using (UnityWebRequest www = UnityWebRequest.Put(Addr + endPoint, data))
            {
                www.method = UnityWebRequest.kHttpVerbPOST;
                if (token != "")
                    www.SetRequestHeader("Authorization", token);

                yield return www.SendWebRequest();

                RequestCallBack(www.downloadHandler.text, callBack, www.error);
                Debug.Log(www.downloadHandler.text);
            }
        }

    }
}