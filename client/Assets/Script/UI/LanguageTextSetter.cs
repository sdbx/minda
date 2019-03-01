using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LanguageTextSetter : MonoBehaviour
    {
        [SerializeField]
        private string key;
        private void Start()
        {
            gameObject.GetComponent<Text>().text = LanguageManager.GetText(key);
        }
    }
}