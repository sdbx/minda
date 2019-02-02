using System.Collections;
using System.Collections.Generic;
using Models;
using UI;
using UnityEngine;

public class UserList : MonoBehaviour
{

    [SerializeField]
    private Transform content;

    [SerializeField]
    private UserInfoDisplay prefab;
    private Dictionary<int, UserInfoDisplay> userInfoDisplays = new Dictionary<int, UserInfoDisplay>();

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

    private UserInfoDisplay Add(int user)
    {
        var userInfoDisplay = Instantiate<UserInfoDisplay>(prefab, content);
        userInfoDisplay.UserId = user;
        userInfoDisplays.Add(user,userInfoDisplay);
        return userInfoDisplay;
    }
}
