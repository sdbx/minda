using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UndestroyableCanvas : MonoBehaviour 
{
    static private List<string> canvases = new List<string>();

    private Canvas canvas;

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
        canvas.worldCamera = Camera.main;
    }
}