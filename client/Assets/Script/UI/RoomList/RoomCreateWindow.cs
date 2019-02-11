using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Network;
using Newtonsoft.Json;
using Models;
using Scene;
using UI;

public class RoomCreateWindow : MonoBehaviour
{
    [SerializeField]
    private ObjectToggler windowToggler;
    [SerializeField]
    private InputField nameText;
    [SerializeField]
    private Button CancelBtn;
    [SerializeField]
    private Button CreateBtn;

    private void Awake() 
    {
        CancelBtn.onClick.AddListener(Cancel);
        CreateBtn.onClick.AddListener(Create);
    }

    public void Create()
    {
        var me = LobbyServer.instance.loginUser;
        if (nameText.text == "")
        {
            nameText.text = $"{me.username}'s room";
        }
        LobbyServer.instance.Post<JoinRoomResult>("/rooms/", ToJson(), (JoinRoomResult result, int? err) =>
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
            open = true,
        };

        return JsonConvert.SerializeObject(conf);
    }

    public void Cancel()
    {
        nameText.text = "";
        windowToggler.UnActivate();
    }
}
