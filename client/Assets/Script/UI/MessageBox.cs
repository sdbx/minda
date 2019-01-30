using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MessageBox : MonoBehaviour
    {
        public static MessageBox instance;

        [SerializeField]
        private WindowToggler windowToggler;
        [SerializeField]
        private Text messageText;
        [SerializeField]
        private Button button1;
        [SerializeField]
        private Button button2;
        [SerializeField]
        private Text button1Text;
        [SerializeField]
        private Text button2Text;

        private Action<bool> callback;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            //singleton
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
            button1.onClick.AddListener(OnButton1Clicked);
            button2.onClick.AddListener(OnButton2Clicked);
            transform.position = Vector3.zero;
            gameObject.SetActive(false);
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }

        public void Show(string message, Action<bool> callback, string buttonText)
        {
            if(isActiveAndEnabled)
            {
                return;
            }
            windowToggler.Activate();
            messageText.text = message;
            button1.gameObject.SetActive(false);
            button2Text.text = buttonText;
            this.callback = callback;
        }

        public void Show(string message, Action<bool> callback, string agree, string disagree)
        {
            if (isActiveAndEnabled)
            {
                return;
            }
            windowToggler.Activate();
            messageText.text = message;
            button1.gameObject.SetActive(true);
            button1Text.text = agree;
            button2Text.text = disagree;
            this.callback = callback;
        }

        private void OnButton1Clicked()
        {
            if (callback == null)
                return;
            callback(true);
            messageText.text="";
            windowToggler.UnActivate();
            callback = null;
        }

        private void OnButton2Clicked()
        {
            if(callback==null) 
                return;

            //버튼이 한개일 때
            if (!button1.gameObject.activeSelf)
            {
                callback(true);
            }
            else
            {
                callback(false);
            }
            windowToggler.Activate();
            callback = null;
        }
    }
}
