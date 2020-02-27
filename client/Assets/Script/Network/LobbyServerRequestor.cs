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
        private string _addr = "";

        public LobbyServerRequestor(string addr)
        {
            this._addr = addr;
        }

        public IEnumerator Post<T>(string endPoint, string data, string token, Action<T, int?> callBack)
        {
            if (data == "")
                data = "{}";
            var postData = System.Text.Encoding.UTF8.GetBytes(data);
            using (var www = UnityWebRequest.Put(_addr + endPoint, postData))
            {
                Debug.Log("[Post]:" + _addr + endPoint + " Body:" + data);
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
            var www = UnityWebRequest.Post(_addr + endPoint, formData);

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

            using (var www = UnityWebRequest.Put(_addr + endPoint, body))
            {
                Debug.Log("[Put]:" + _addr + endPoint + " Body:" + data);
                if (token != "")
                    www.SetRequestHeader("Authorization", token);
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                RequestCallBack(www.downloadHandler.text, callBack, www.error);
                Debug.Log(www.downloadHandler.text);
            }
        }

        public IEnumerator Put(string endPoint, WWWForm formData, string token, Action<byte[], int?> callBack)
        {
            var www = UnityWebRequest.Post(_addr + endPoint, formData);
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

        public IEnumerator Delete(string endPoint, string token, Action<int?> callBack)
        {
            var www = UnityWebRequest.Delete(_addr + endPoint);
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

            using (var www = UnityWebRequest.Get(_addr + endPoint))
            {
                Debug.Log("[Get]:" + _addr + endPoint);
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

                if (int.TryParse(errorCode, out var parsedCode))
                {
                    callBack(default(T), parsedCode);
                    return;
                }
                callBack(default(T), 500);
                return;
            }
            if (data == "")
            {
                callBack(default(T), null);
                return;
            }
            callBack(JsonConvert.DeserializeObject<T>(data), null);
        }


        public IEnumerator PostImage(string endPoint, byte[] data, string token, Action<Pic, int?> callBack)
        {
            using (var www = UnityWebRequest.Put(_addr + endPoint, data))
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
