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
    private UserInfoDisplay[] userInfoDisplays;

    private void Awake()
    {
        
    }

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
        return userInfoDisplay;
    }
}
