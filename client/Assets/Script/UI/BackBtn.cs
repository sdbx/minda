using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackBtn : MonoBehaviour
{
    [SerializeField]
    private string sceneName;
    private Button _button;

    private void Awake()
    {
        _button = gameObject.GetComponent<Button>();
        _button.onClick.AddListener(Click);
    }

    private void Click()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
