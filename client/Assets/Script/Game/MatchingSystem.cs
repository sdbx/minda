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
                ToastManager.instance.Add(err + " Error!", "error");
            }
            State.text = "FOUND GAME";
            LobbyServer.instance.EnterRoom(Matched.roomId, (success) => { });
        });
        StartCoroutine(MatchTimer(timeTaken+1));
    }
}