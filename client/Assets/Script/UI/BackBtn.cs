using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackBtn : MonoBehaviour
{
    [SerializeField]
    private string sceneName;
    private Button button;

    void Awake()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(Click);
    }

    void Click()
    {
        SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
    }
}
