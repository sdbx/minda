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
        NetworkManager.instance.RefreshLoggedInUser(() =>
        {
            var me = NetworkManager.instance.loggedInUser;
            if (nameText.text == "")
            {
                nameText.text = $"{me.username}'s room";
            }

            NetworkManager.instance.Post<JoinRoomResult>("/rooms/", ToJson(me) , (JoinRoomResult result, string err)=>
            {
                if(err!=null)
                {
                    Debug.Log(err);
                    return;
                }
                var Addr = result.addr.Split(':');
                SceneChanger.instance.ChangeTo("RoomConfigure");
                NetworkManager.instance.EnterRoom(Addr[0],int.Parse(Addr[1]), result.invite);
            });

        });
    }

    public string ToJson(User me)
    {
        Models.RoomSettings roomSett = new RoomSettings{
            name = nameText.text,
            king = me.id,
            black = me.id,
            white = -1
        };

        return JsonConvert.SerializeObject(roomSett);
    }

    public void Cancel()
    {
        nameText.text = "";
        gameObject.SetActive(false);
    }
}
