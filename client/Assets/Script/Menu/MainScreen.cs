using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreen : MonoBehaviour
{
    [SerializeField]
    private RectTransform title;
    [SerializeField]
    private float moveSpeed = 1;
    [SerializeField]
    private Vector3 destination = new Vector3();
    [SerializeField]
    private CanvasGroup clickToLogin;
    [SerializeField]
    private CanvasGroup socialLoginBtns;
    [SerializeField]
    private float timer = 1;
    private float opacitySpeed = 0.05f;
    private Vector3 buttonVelocity = Vector3.zero;
    private bool Clicked = false;

    void Start()
    {
        socialLoginBtns.alpha = 0;
        socialLoginBtns.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) Clicked = true;
        if (Clicked)
        {
            socialLoginBtns.gameObject.SetActive(true);
            title.localPosition = Vector3.SmoothDamp(title.localPosition, destination, ref buttonVelocity, 0.5f);
            clickToLogin.alpha -= opacitySpeed;
            if(timer>0)
            {
                timer -= Time.deltaTime;
            }
            if (timer < 0 && socialLoginBtns.alpha < 1)
            {
                socialLoginBtns.alpha += opacitySpeed;
            }
        }
    }
}
