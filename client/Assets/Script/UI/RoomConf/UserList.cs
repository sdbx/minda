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
    private Dictionary<int, UserInfoDisplay> _userInfoDisplays = new Dictionary<int, UserInfoDisplay>();

    private void Awake()
    {
        GameServer.Instance.UserEnteredEvent += OnUserEnter;
        GameServer.Instance.UserLeftEvent += OnUserLeft;
    }

    private void OnDestroy()
    {
        GameServer.Instance.UserEnteredEvent -= OnUserEnter;
        GameServer.Instance.UserLeftEvent -= OnUserLeft;
    }

    private void OnUserEnter(int id, BallType ballType)
    {
        if (LobbyServer.Instance.IsLoginId(id))
        {
            Load(GameServer.Instance.connectedRoom.Users.ToArray());
        }
        else
        {
            Add(id);
        }
    }

    private void OnUserLeft(int id)
    {
        Destroy(_userInfoDisplays[id].gameObject);
        _userInfoDisplays.Remove(id);
    }

    public void Load(int[] users)
    {
        if (users == null)
            return;
        foreach (var pair in _userInfoDisplays)
        {
            Destroy(pair.Value.gameObject);
        }
        _userInfoDisplays.Clear();
        for (var i = 0; i < users.Length; i++)
        {
            Add(users[i]);
        }
    }

    private void Add(int user)
    {
        if (_userInfoDisplays.ContainsKey(user))
            return;

        var userInfoDisplay = Instantiate<UserInfoDisplay>(prefab, content);
        userInfoDisplay.userId = user;
        _userInfoDisplays.Add(user, userInfoDisplay);
        return;
    }
}
