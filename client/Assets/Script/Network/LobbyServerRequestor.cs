using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using UI.Toast;
using Models;
using System.Collections.Generic;

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

        public IEnumerator Post(string endPoint, WWWForm formData, string token, Action<byte[], int?> callBack)
        {
            UnityWebRequest www = UnityWebRequest.Post(Addr + endPoint, formData);

            if (token != "")
                www.SetRequestHeader("Authorization", token);

            yield return www.SendWebRequest();
            int? errorCode = null;
            if (www.isHttpError)
            {
                errorCode = (int)www.responseCode;
                Debug.Log(www.downloadHandler.text);
            }
            callBack(www.downloadHandler.data, errorCode);
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

        public IEnumerator Put(string endPoint, WWWForm formData, string token, Action<byte[], int?> callBack)
        {
            UnityWebRequest www = UnityWebRequest.Post(Addr + endPoint, formData);
            www.method = UnityWebRequest.kHttpVerbPUT;
            if (token != "")
                www.SetRequestHeader("Authorization", token);

            yield return www.SendWebRequest();
            int? errorCode = null;
            if (www.isHttpError)
            {
                errorCode = (int)www.responseCode;
                Debug.Log(www.downloadHandler.text);
            }
            callBack(www.downloadHandler.data, errorCode);
        }

        public IEnumerator DELETE(string endPoint, string token, Action<int?> callBack)
        {
            UnityWebRequest www = UnityWebRequest.Delete(Addr + endPoint);
            www.method = UnityWebRequest.kHttpVerbPUT;
            if (token != "")
                www.SetRequestHeader("Authorization", token);

            yield return www.SendWebRequest();
            int? errorCode = null;
            if (www.isHttpError)
            {
                errorCode = (int)www.responseCode;
            }
            callBack(errorCode);
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