using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Network;
using Newtonsoft.Json;
using Models;
using Scene;

public class RoomCreateWindow : MonoBehaviour
{
    [SerializeField]
    private InputField nameText;
    [SerializeField]
    private Button CancelBtn;
    [SerializeField]
    private Button CreateBtn;
    [SerializeField]
    private Button CreateWindowBtn;

    private void Awake() 
    {
        CancelBtn.onClick.AddListener(Cancel);
        CreateBtn.onClick.AddListener(Create);
    }

    public void Active()
    {
        gameObject.SetActive(true);
    }

    public void Create()
    {
        var me = LobbyServer.instance.loginUser;
        if (nameText.text == "")
        {
            nameText.text = $"{me.username}'s room";
        }
        LobbyServer.instance.Post<JoinRoomResult>("/rooms/", ToJson(), (JoinRoomResult result, string err) =>
        {
           if (err != null)
           {
               Debug.Log(err);
               return;
           }
           var Addr = result.addr.Split(':');
           SceneChanger.instance.ChangeTo("RoomConfigure");
           GameServer.instance.EnterRoom(Addr[0], int.Parse(Addr[1]), result.invite);
       });
    }

    public string ToJson()
    {
        Conf conf = new Conf{
            name = nameText.text,
            king = -1,
            black = -1,
            white = -1
        };

        return JsonConvert.SerializeObject(conf);
    }

    public void Cancel()
    {
        nameText.text = "";
        gameObject.SetActive(false);
    }
}
