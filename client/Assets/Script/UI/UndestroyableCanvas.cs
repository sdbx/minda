using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UndestroyableCanvas : MonoBehaviour 
{
    static private List<string> canvases = new List<string>();

    private Canvas canvas;
    [SerializeField]
    private string[] undestroySceneList;

    private void Awake() 
    {
        if(canvases.Contains(gameObject.name))
        {
            Destroy(gameObject);
            return;
        }
        canvases.Add(gameObject.name);
        canvas = gameObject.GetComponent<Canvas>();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (undestroySceneList.Length == 0 || Array.Exists(undestroySceneList, element => element == scene.name))
        {
            canvas.worldCamera = Camera.main;
        }
        else
        {
            canvases.Remove(gameObject.name);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }
}