using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Network;

namespace UI
{
    public class LoginBtn : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private string serviceName = "";
        [SerializeField]
        private float timer;
        [SerializeField]
        private float duration;
        [SerializeField]
        private float height;

        private Vector3 _originPosition;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            var position = transform.position;
            _originPosition = position;
            transform.position = new Vector3(position.x, position.y + height, position.z);
        }

        private void Update()
        {
            if (timer < 0)
            {
                timer = 0;
                transform.DOMove(_originPosition, duration);
                DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1, duration);
            }
            else if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var loginState = LobbyServer.Instance.CurrentLoginState;
            if (loginState == LobbyServer.LoginState.SendReq ||
            loginState == LobbyServer.LoginState.GetReqid)
            {
                MessageBox.Instance.Show(LanguageManager.GetText("retrylogin"), (bool agree) =>
                {
                    if (agree) TryLogin();
                }, "Yes", "No");
                return;
            }
            TryLogin();
        }

        public void TryLogin()
        {
            LobbyServer.Instance.Login(serviceName);
        }
    }

}
