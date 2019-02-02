using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameMenu : MonoBehaviour
    {
        [SerializeField]
        private Button ExitBtn;
        [SerializeField]
        private Button CancelBtn;
        [SerializeField]
        private ObjectToggler toggler;

        private bool isEscapeKeyUp = true;

        private void Awake() 
        {
            ExitBtn.onClick.AddListener(OnExitBtnClicked);
            CancelBtn.onClick.AddListener(OnCancelBtnClicked);
        }

        private void Update()
        {
            //누를때 한번만 입력받기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("esc");
                if(isEscapeKeyUp)
                {
                    toggler.Toggle();
                    isEscapeKeyUp = false;
                }
            }
            else
            {
                isEscapeKeyUp = true;
            }
        }

        private void OnExitBtnClicked()
        {
            MessageBox.instance.Show("zin za na gal geo ya?",((bool agreed)=>
            {
                if(agreed)
                {
                    GameServer.instance.ExitGame();
                }
            }),"Yes","No");
        }

        private void OnCancelBtnClicked()
        {
            toggler.UnActivate();
        }
    }
}
