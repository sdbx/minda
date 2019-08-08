using System.Collections;
using System.Collections.Generic;
using Scene;
using UnityEngine;
using UnityEngine.UI;

public class RankModeBtn : MonoBehaviour
{
    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(MoveToMatchScene);
    }

    private void MoveToMatchScene()
    {
        SceneChanger.instance.ChangeTo("Matching");
    }
}
