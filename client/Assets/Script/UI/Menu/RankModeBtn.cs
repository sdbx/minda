using UnityEngine;
using UnityEngine.UI;
using UI.Toast;

public class RankModeBtn : MonoBehaviour
{
    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(MoveToMatchScene);
    }

    private void MoveToMatchScene()
    {
        ToastManager.Instance.Add("Ranked mode will open after a while. Please enjoy the custom mode in the meantime.", "Info");
    }
}
