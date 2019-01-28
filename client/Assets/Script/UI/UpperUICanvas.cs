using UnityEngine;
using UnityEngine.SceneManagement;

public class UpperUICanvas : MonoBehaviour 
{
    private Canvas canvas;
    private void Awake() 
    {
        canvas = gameObject.GetComponent<Canvas>();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        canvas.worldCamera = Camera.main;
    }
}