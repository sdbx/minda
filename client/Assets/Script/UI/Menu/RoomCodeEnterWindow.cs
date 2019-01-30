using Models;
using Network;
using Scene;
using UI;
using UI.Toast;
using UnityEngine;
using UnityEngine.UI;

public class RoomCodeEnterWindow : MonoBehaviour 
{
    [SerializeField]
    private WindowToggler windowToggler;
    [SerializeField]
    private InputField codeInput;
    [SerializeField]
    private Button enterBtn;
    [SerializeField]
    private Button cancelBtn;

    private void Awake()
    {
        enterBtn.onClick.AddListener(TryEnter);
        cancelBtn.onClick.AddListener(Cancel);
    }

    private void TryEnter()
    {
        if(codeInput.text == "")
        {
            ToastManager.instance.Add("Input field is empty","Error");
            return;
        }
        LobbyServer.instance.EnterRoom(codeInput.text,(bool success)=>{});
    }

    private void Cancel()
    {
        codeInput.text = "";
        windowToggler.UnActivate();
    }
}

