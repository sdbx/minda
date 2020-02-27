using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class InGameMenu : MonoBehaviour
    {
        [FormerlySerializedAs("ExitOrGGBtn")] [SerializeField]
        private Button exitOrGgBtn;
        [FormerlySerializedAs("ExitOrGGBtnText")] [SerializeField]
        private Text exitOrGgBtnText;
        [FormerlySerializedAs("CancelBtn")] [SerializeField]
        private Button cancelBtn;
        [SerializeField]
        private ObjectToggler toggler;

        private bool _isEscapeKeyUp = true;
        private bool _isInGame;
        private bool _isDisable;
        private void Awake()
        {
            exitOrGgBtn.onClick.AddListener(OnExitOrGgBtnClicked);
            cancelBtn.onClick.AddListener(OnCancelBtnClicked);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            toggler.UnActivate();
            _isDisable = false;
        }

        private void ChangeButtonType()
        {
            _isInGame = GameServer.Instance.isInGame;
            if (_isInGame && !GameServer.Instance.isSpectator)
            {
                exitOrGgBtnText.text = LanguageManager.GetText("gg");
            }
            else
            {
                exitOrGgBtnText.text = LanguageManager.GetText("exit");
            }
        }

        private void Update()
        {
            if (_isDisable)
                return;
            //누를때 한번만 입력받기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("esc");
                if (_isEscapeKeyUp)
                {
                    ChangeButtonType();
                    toggler.Toggle();
                    _isEscapeKeyUp = false;
                }
            }
            else
            {
                _isEscapeKeyUp = true;
            }
        }

        private void OnExitOrGgBtnClicked()
        {
            if (_isInGame)
            {
                MessageBox.Instance.Show(LanguageManager.GetText("surrendermessage"), (bool agreed) =>
                 {
                     if (agreed)
                     {
                         GameServer.Instance.Surrender();
                         toggler.UnActivate();
                         _isDisable = true;
                     }
                 }, "Yes", "No");
                return;
            }
            MessageBox.Instance.Show(LanguageManager.GetText("exitmessage"), (bool agreed) =>
             {
                 if (agreed)
                 {
                     GameServer.Instance.ExitGame();
                 }
             }, "Yes", "No");
        }

        private void OnCancelBtnClicked()
        {
            toggler.UnActivate();
        }
    }
}
