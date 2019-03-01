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

        private Action callback;

        public void Display(EndedEvent ended, Action callback)
        {
            winnerText.text = LanguageManager.GetText(ended.player+"win");
            //색 설정 추가  
            details.text = LanguageManager.GetText(ended.cause);
            this.callback = callback;
            
            toggler.Activate();
            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            yield return new WaitForSeconds(waitTime);
            callback();
        }

    }
}
