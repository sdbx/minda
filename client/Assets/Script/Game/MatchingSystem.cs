using System.Collections;
using Models;
using Network;
using UI.Toast;
using UnityEngine;
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
    private Text timeTaken;
    [SerializeField]
    private Text State;

    private IEnumerator MatchTimer(int timeTaken)
    {
        yield return new WaitForSeconds(1);
        LobbyServer.instance.Get<Matched>("/match/", (Matched, err) =>
        {
            if (err != null)
            {
                if (err == 404)
                    return;
                ToastManager.instance.Add(err + " Error!", "Error");
            }
            State.text = "FOUND GAME";
            LobbyServer.instance.EnterRoom(Matched.room_id, (success) => { });
        });
        StartCoroutine(MatchTimer(timeTaken+1));
    }
    private void OnCancelBtnClicked()
    {
        State.text="CANCELING";
        LobbyServer.instance.DELETE("/match/", (err) =>{
            Scene.SceneChanger.instance.ChangeTo("Menu");
        });
    }
}