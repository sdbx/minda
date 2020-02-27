using UnityEngine;
using UnityEngine.UI;
using Network;
using Newtonsoft.Json;
using Models;
using UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class RoomCreateWindow : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Toggle isPublicToggle;
    [SerializeField]
    private ObjectToggler windowToggler;
    [SerializeField]
    private InputField nameText;
    [FormerlySerializedAs("CancelBtn")] [SerializeField]
    private Button cancelBtn;
    [FormerlySerializedAs("CreateBtn")] [SerializeField]
    private Button createBtn;

    private void Awake()
    {
        cancelBtn.onClick.AddListener(Cancel);
        createBtn.onClick.AddListener(Create);
    }

    public void Create()
    {
        var me = LobbyServer.Instance.loginUser;
        if (nameText.text == "")
        {
            nameText.text = $"{me.Username}'s room";
        }
        LobbyServer.Instance.Post<JoinRoomResult>("/rooms/", ToJson(), (JoinRoomResult result, int? err) =>
        {
            if (err != null)
            {
                Debug.Log(err);
                return;
            }
            var addr = result.Addr.Split(':');
            SceneManager.LoadScene("RoomConfigure");
            GameServer.Instance.EnterRoom(addr[0], int.Parse(addr[1]), result.Invite);
        });
    }

    public string ToJson()
    {
        var conf = new Conf
        {
            Open = isPublicToggle.isOn,
            Name = nameText.text,
            King = -1,
        };

        return JsonConvert.SerializeObject(conf);
    }

    public void Cancel()
    {
        nameText.text = "";
        windowToggler.UnActivate();
    }
}
