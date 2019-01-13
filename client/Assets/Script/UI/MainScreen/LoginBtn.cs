using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UI
{
    public class LoginBtn : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private string serviceName = "";

        public void OnPointerClick(PointerEventData eventData)
        {
            Network.NetworkManager.instance.login(serviceName,()=>{
                SceneManager.LoadSceneAsync("Menu",LoadSceneMode.Single);
                Debug.Log("로그인 완료");
            });
        }
    }

}
