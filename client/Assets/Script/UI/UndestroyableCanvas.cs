using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UndestroyableCanvas : MonoBehaviour
{
    static private List<string> _canvases = new List<string>();

    private Canvas _canvas;
    [SerializeField]
    private string[] undestroySceneList;

    private void Awake()
    {
        if (_canvases.Contains(gameObject.name))
        {
            Destroy(gameObject);
            return;
        }
        _canvases.Add(gameObject.name);
        _canvas = gameObject.GetComponent<Canvas>();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (undestroySceneList.Length == 0 || Array.Exists(undestroySceneList, element => element == scene.name))
        {
            _canvas.worldCamera = Camera.main;
        }
        else
        {
            _canvases.Remove(gameObject.name);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }
}
