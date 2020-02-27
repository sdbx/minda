using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomModeBtn : MonoBehaviour
{
    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(MoveToRoomList);
    }

    private void MoveToRoomList()
    {
        SceneManager.LoadScene("RoomList");
    }
}
