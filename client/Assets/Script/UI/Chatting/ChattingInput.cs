using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chatting
{
    public class ChattingInput : MonoBehaviour
    {
        [SerializeField]
        private ChattingWindow chattingSystem;
        [SerializeField]
        private InputField inputField;
        [SerializeField]
        public ObjectToggler objectToggler;



        public bool isActivated
        {
            get
            {
                return objectToggler.isActivated;
            }
        }

        public void Activate()
        {
            objectToggler.Activate();
            Focus();
        }

        public void UnActivate()
        {
            objectToggler.UnActivate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (inputField.text != "")
                {
                    GameServer.Instance.SendChat(inputField.text);
                    inputField.text = "";
                    chattingSystem.ScrollToBottom();
                    Focus();
                    return;
                }

                objectToggler.UnActivate();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                inputField.text = "";
                objectToggler.UnActivate();
            }
        }

        public void Focus()
        {
            inputField.Select();
            inputField.ActivateInputField();
        }

    }
}
