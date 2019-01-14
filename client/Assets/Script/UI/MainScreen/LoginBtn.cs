using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using DG.Tweening;

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

        private Vector3 originPosition;
        private CanvasGroup canvasGroup;

        private void Awake() 
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            var position = transform.position;
            originPosition = position;
            transform.position = new Vector3(position.x,position.y+height,position.z);
        }

        private void Update() 
        {
            if(timer<0)
            {
                timer = 0;
                transform.DOMove(originPosition,duration);
                DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, 1, duration);
            }
            else if(timer>0)
            {
                timer-=Time.deltaTime;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Network.NetworkManager.instance.login(serviceName,()=>{
                SceneManager.LoadSceneAsync("Menu",LoadSceneMode.Single);
                Debug.Log("로그인 완료");
            });
        }
    }

}
