using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class InGameMenu : MonoBehaviour
    {
        [SerializeField]
        private Button ExitOrGGBtn;
        [SerializeField]
        private Text ExitOrGGBtnText;
        [SerializeField]
        private Button CancelBtn;
        [SerializeField]
        private ObjectToggler toggler;

        private bool isEscapeKeyUp = true;
        private bool isInGame;
        private bool isDisable;
        private void Awake() 
        {
            ExitOrGGBtn.onClick.AddListener(OnExitOrGGBtnClicked);
            CancelBtn.onClick.AddListener(OnCancelBtnClicked);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            toggler.UnActivate();
            isDisable = false;
        }

        private void ChangeButtonType()
        {   
            isInGame = GameServer.instance.isInGame;
            if(isInGame&&!GameServer.instance.isSpectator)
            {
                ExitOrGGBtnText.text = LanguageManager.GetText("surrender");
            }
            else
            {
                ExitOrGGBtnText.text = LanguageManager.GetText("exit");
            }
        }

        private void Update()
        {
            if(isDisable)
                return;
            //누를때 한번만 입력받기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("esc");
                if(isEscapeKeyUp)
                {
                    ChangeButtonType();
                    toggler.Toggle();
                    isEscapeKeyUp = false;
                }
            }
            else
            {
                isEscapeKeyUp = true;
            }
        }

        private void OnExitOrGGBtnClicked()
        {
            if(isInGame)
            {
                MessageBox.instance.Show(LanguageManager.GetText("surrender"),(bool agreed)=>
                {
                    if(agreed)
                    {
                        GameServer.instance.Surrender();
                        toggler.UnActivate();
                        isDisable = true;
                    }
                },"Yes","No");
                return;
            }
            MessageBox.instance.Show(LanguageManager.GetText("exit"),(bool agreed)=>
            {
                if(agreed)
                {
                    GameServer.instance.ExitGame();
                }
            },"Yes","No");
        }

        private void OnCancelBtnClicked()
        {
            toggler.UnActivate();
        }
    }
}
