using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Network;
using Models;

public class StartBtn : MonoBehaviour
{

    [SerializeField]
    private float duration;
    private bool _isActivated = false;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClicked);
        GameServer.Instance.ConfedEvent += Check;
    }

    private void OnDestroy()
    {
        GameServer.Instance.ConfedEvent -= Check;
    }

    private void Start()
    {
        if (GameServer.Instance.connectedRoom != null)
            Check(GameServer.Instance.connectedRoom.Conf);
    }

    private void Check(Conf conf)
    {
        var me = LobbyServer.Instance.loginUser;
        if (conf.King == me.Id && conf.Black != -1 && conf.White != -1)
        {
            Active();
        }
        else
        {
            UnActive();
        }
    }

    private void Active()
    {
        _isActivated = true;
        gameObject.transform.GetComponent<RectTransform>().DOPivot(new Vector2(1, 0), duration);
    }

    private void UnActive()
    {
        _isActivated = false;
        gameObject.transform.GetComponent<RectTransform>().DOPivot(new Vector2(1, 1), duration);
    }

    private void OnClicked()
    {
        if (!_isActivated)
            return;
        GameServer.Instance.SendCommand(new GameStart());
    }
}
