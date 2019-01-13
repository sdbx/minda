using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Events;
using Models;
using Network;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Scene
{
    public class SceneChanger : MonoBehaviour
    {
        public static SceneChanger instance = null;
        private Action loaded;
        private void Awake() 
        {
            if(instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,LoadSceneMode mode)
        {
            if(loaded!=null)
            {
                loaded();
                loaded = null;
            }
        }

        public void ChangeTo(string SceneName)
        {
            SceneManager.LoadScene(SceneName,LoadSceneMode.Single);
        }
    }
}
