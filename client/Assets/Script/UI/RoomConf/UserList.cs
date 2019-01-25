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
    private int[] users;
    private Dictionary<int, UserInfoDisplay> userInfoDisplays = new Dictionary<int, UserInfoDisplay>();

    public void Create(int[] users)
    {
        if(users==null)
            return;
        for(int i = 0;i<users.Length;i++)
        {
            Add(users[i]);
        }
    }

    public UserInfoDisplay Add(int user)
    {
        var userInfoDisplay = Instantiate<UserInfoDisplay>(prefab, content);
        userInfoDisplay.display(user);
        userInfoDisplays.Add(user,userInfoDisplay);
        return userInfoDisplay;
    }

    public void Remove(int user)
    {
        if(userInfoDisplays.ContainsKey(user))
        {
            Destroy(userInfoDisplays[user].gameObject);
            userInfoDisplays.Remove(user);
        }
    }

    public void RefreshAll()
    {
        foreach (var pair in userInfoDisplays)
        {
            ((UserInfoDisplay)pair.Value).Refresh();
        }
    }
}
