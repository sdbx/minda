using System;
using System.Collections;
using Models.Events;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WinScreen : MonoBehaviour
    {
        [SerializeField]
        private Text winnerText;
        [SerializeField]
        private Text details;
        [SerializeField]
        private ObjectToggler toggler;
        [SerializeField]
        private float waitTime;
        [SerializeField]
        private ParticleSystem one;
        [SerializeField]
        private ParticleSystem two;

        private Action callback;

        public void Display(EndedEvent ended, Action callback)
        {
            winnerText.text = LanguageManager.GetText(ended.player+"win");
            //색 설정 추가  
            details.text = LanguageManager.GetText(ended.cause);
            this.callback = callback;
            
            toggler.Activate();
            if(Camera.main.transform.rotation.z != 0)
            {
                var main = one.main;
                main.gravityModifier = -1;
                main = two.main;
                main.gravityModifier = -1;
            }
            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            yield return new WaitForSeconds(waitTime);
            callback();
        }

    }
}
