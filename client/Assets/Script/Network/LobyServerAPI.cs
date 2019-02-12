using System;
using System.Collections;
using Models;
using Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public static class LobbyServerAPI
    {
        public static void GetUserInformation(int id, Action<User> callback)
        {
            LobbyServer.instance.Get<User>("/users/" + id + "/", (User user, int? err) =>
            {
                if (err != null)
                {
                    callback(null);
                }
                else
                {
                    callback(user);
                }
            });
        }
        
        public static void DownloadImage(string picUrl, Action<Texture> callBack)
        {
            LobbyServer.instance.StartCoroutine(GetTexture(picUrl,callBack));
        }

        private static IEnumerator GetTexture(string imgUrl, Action<Texture> callBack)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(imgUrl);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                callBack(((DownloadHandlerTexture)www.downloadHandler).texture);
            }
        }
        
        public static void GetMyMaps(Action<Map[], int?> callback)
        {
            LobbyServer.instance.Get<Map[]>("/maps/",callback);
        }
    }
}

