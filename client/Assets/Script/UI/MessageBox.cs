using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MessageBox : MonoBehaviour
    {
        public static MessageBox Instance;

        [SerializeField]
        private ObjectToggler toggler;
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

        private Action<bool> _callback;

        private void Awake()
        {
            //singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            button1.onClick.AddListener(OnButton1Clicked);
            button2.onClick.AddListener(OnButton2Clicked);
            transform.position = Vector3.zero;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (isActiveAndEnabled)
            {
                gameObject.SetActive(false);
            }
        }

        public void Show(string message, Action<bool> callback, string buttonText)
        {
            if (isActiveAndEnabled)
            {
                return;
            }
            toggler.Activate();
            messageText.text = message;
            toggler.UnActivate();
            button1.gameObject.SetActive(false);
            button2Text.text = buttonText;
            this._callback = callback;
        }

        public void Show(string message, Action<bool> callback, string agree, string disagree)
        {
            if (isActiveAndEnabled)
            {
                return;
            }
            toggler.Activate();
            messageText.text = message;
            button1.gameObject.SetActive(true);
            button1Text.text = agree;
            button2Text.text = disagree;
            this._callback = callback;
        }

        private void OnButton1Clicked()
        {
            if (_callback == null)
                return;
            _callback(true);
            messageText.text = "";
            toggler.UnActivate();
            _callback = null;
        }

        private void OnButton2Clicked()
        {
            if (_callback == null)
                return;

            //버튼이 한개일 때
            if (!button1.gameObject.activeSelf)
            {
                _callback(true);
            }
            else
            {
                _callback(false);
            }
            messageText.text = "";
            toggler.UnActivate();
            _callback = null;
        }
    }
}
