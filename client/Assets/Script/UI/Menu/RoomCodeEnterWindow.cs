using Network;
using UI;
using UI.Toast;
using UnityEngine;
using UnityEngine.UI;

public class RoomCodeEnterWindow : MonoBehaviour
{
    [SerializeField] private ObjectToggler windowToggler;
    [SerializeField] private InputField codeInput;
    [SerializeField] private Button enterBtn;
    [SerializeField] private Button cancelBtn;

    private void Awake()
    {
        enterBtn.onClick.AddListener(TryEnter);
        cancelBtn.onClick.AddListener(Cancel);
    }

    private void TryEnter()
    {
        if (codeInput.text == "")
        {
            ToastManager.Instance.Add(LanguageManager.GetText("inputfieldisempty"), "Error");
            return;
        }

        LobbyServer.Instance.EnterRoom(codeInput.text, (bool success) => { });
    }

    private void Cancel()
    {
        codeInput.text = "";
        windowToggler.UnActivate();
    }
}
