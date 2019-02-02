using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Scene;
using Network;
using Game;

public class StartBtn : MonoBehaviour
{

    [SerializeField]
    private float duration;
    private bool isActivated = false;

    void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void Active()
    {
        isActivated = true;
        gameObject.transform.GetComponent<RectTransform>().DOPivot(new Vector2(1,0), duration);
    }

    public void UnActive()
    {
        isActivated = false;
        gameObject.transform.GetComponent<RectTransform>().DOPivot(new Vector2(1,1), duration);
    }

    private void OnClicked()
    {
        if(!isActivated)
            return;
        GameServer.instance.SendCommand(new GameStart());
    }
}
