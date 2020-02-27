using System.Collections;
using Models;
using Network;
using UI.Toast;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchingSystem : MonoBehaviour
{
    private void Awake()
    {
        cancelBtn.onClick.AddListener(OnCancelBtnClicked);
        StartCoroutine(MatchTimer(0));
    }
    [SerializeField]
    private Button cancelBtn;
    [SerializeField]
    private Text timeTakenTxt;
    [SerializeField]
    private Text stateTxt;

    private IEnumerator MatchTimer(int timeTaken)
    {
        timeTakenTxt.text = MakeTimeStr(timeTaken);
        yield return new WaitForSeconds(1);
        LobbyServer.Instance.Get<Matched>("/match/", (matched, err) =>
        {
            if (err != null)
            {
                if (err == 404)
                    return;
                ToastManager.Instance.Add(err + " Error!", "Error");
            }
            stateTxt.text = "FOUND GAME";
            LobbyServer.Instance.EnterRoom(matched.RoomId, (success) => { });
            return;
        });
        StartCoroutine(MatchTimer(timeTaken + 1));
    }
    private void OnCancelBtnClicked()
    {
        stateTxt.text = "CANCELING";
        LobbyServer.Instance.Delete("/match/", (err) =>
        {
            SceneManager.LoadScene("Menu");
        });
    }


    private string MakeTimeStr(int time)
    {
        var min = Mathf.CeilToInt(time / 60);
        string minStr, secStr;

        if (min < 10)
            minStr = "0" + min;
        else
            minStr = min.ToString();

        var sec = time - min * 60;

        if (sec < 10)
            secStr = "0" + sec;
        else
            secStr = sec.ToString();

        return $"{minStr} : {secStr}";

    }
}

