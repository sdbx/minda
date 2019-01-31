using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;

public class MainScreen : MonoBehaviour
{
    [SerializeField]
    private RectTransform title;
    [SerializeField]
    private float smoothTime = 1;
    [SerializeField]
    private Vector3 destination = new Vector3();
    [SerializeField]
    private GameObject clickToLogin;
    [SerializeField]
    private GameObject socialLoginBtns;
    [SerializeField]
    private float time = 1;
    private Vector3 buttonVelocity = Vector3.zero;
    private bool Clicked = false;

    void Awake()
    {
        socialLoginBtns.gameObject.SetActive(false);
    }

    void Update()
    {
        if (LobbyServer.instance.loginState == LobbyServer.LoginState.Logout)
        {
            if (Input.GetMouseButtonDown(0)) Clicked = true;
            if (Clicked)
            {
                title.localPosition = Vector3.SmoothDamp(title.localPosition, destination, ref buttonVelocity, smoothTime);
                if (time > 0)
                {
                    time -= Time.deltaTime;
                }
                if (time <=0)
                {
                    time = 0;
                    clickToLogin.gameObject.SetActive(false);
                    socialLoginBtns.gameObject.SetActive(true);
                }
            }
        }
    }
}
