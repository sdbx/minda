using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Models.Events;
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
        public static SceneChanger Instance = null;
        private Action _loaded;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (_loaded != null)
            {
                _loaded();
                _loaded = null;
            }
        }

        public void ChangeTo(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
