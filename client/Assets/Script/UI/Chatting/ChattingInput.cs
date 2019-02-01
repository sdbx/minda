using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChattingInput : MonoBehaviour
    {
        [SerializeField]
        private ChattingSystem chattingSystem;
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
                if(inputField.text!="")
                {
                    GameServer.instance.SendChat(inputField.text);
                    inputField.text = "";
                    Focus();
                }
                else
                {
                    objectToggler.UnActivate();
                }
            }
            if(Input.GetKeyDown(KeyCode.Escape))
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
