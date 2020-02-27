using System.Collections;
using System.Collections.Generic;
using Scene;
using UnityEngine;
using UnityEngine.UI;

public class CustomModeBtn : MonoBehaviour
{
    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(MoveToRoomList);
    }

    private void MoveToRoomList()
    {
        SceneChanger.Instance.ChangeTo("RoomList");
    }
}
