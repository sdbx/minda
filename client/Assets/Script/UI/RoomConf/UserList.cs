using System.Collections;
using System.Collections.Generic;
using Game;
using Models;
using Network;
using UI;
using UnityEngine;

public class UserList : MonoBehaviour
{

    [SerializeField]
    private Transform content;

    [SerializeField]
    private UserInfoDisplay prefab;
    private Dictionary<int, UserInfoDisplay> userInfoDisplays = new Dictionary<int, UserInfoDisplay>();

    private void Awake()
    {
        GameServer.instance.UserEnteredEvent += OnUserEnter;
        GameServer.instance.UserLeftEvent += OnUserLeft;
    }

    private void OnDestroy()
    {
        GameServer.instance.UserEnteredEvent -= OnUserEnter;
        GameServer.instance.UserLeftEvent -= OnUserLeft;
    }

    private void OnUserEnter(int id, BallType ballType)
    {
        if(LobbyServer.instance.IsLoginId(id))
        {
            Load(GameServer.instance.connectedRoom.Users.ToArray());
        }
        else
        {
            Add(id);
        }
    }

    private void OnUserLeft(int id)
    {
        Destroy(userInfoDisplays[id].gameObject);
        userInfoDisplays.Remove(id);
    }

    public void Load(int[] users)
    {
        if(users==null)
            return;
        foreach(var pair in userInfoDisplays)
        {
            Destroy(pair.Value.gameObject);
        }
        userInfoDisplays.Clear();
        for(int i = 0;i<users.Length;i++)
        {
            Add(users[i]);
        }
    }

    private void Add(int user)
    {
        if(userInfoDisplays.ContainsKey(user))
            return;
            
        var userInfoDisplay = Instantiate<UserInfoDisplay>(prefab, content);
        userInfoDisplay.UserId = user;
        userInfoDisplays.Add(user,userInfoDisplay);
        return;
    }
}
