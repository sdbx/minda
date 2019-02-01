using Models;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChattingSystem : MonoBehaviour
    {
        [SerializeField]
        private Text textBox;
        [SerializeField]
        private ChattingInput chattingInputToggler;

        private void Awake()
        {
            GameServer.instance.ChattedEvent += OnChat;
            Clear();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Enter감지");
                if (!chattingInputToggler.isActivated)
                {
                    chattingInputToggler.Activate();
                }
            }
        }

        private void OnChat(InGameUser inGameUser, string message)
        {
            AddLine(inGameUser.user.username, message);
        }

        private void AddLine(string username, string message)
        {
            textBox.text += $"{username}: {message}\n";
        }

        private void Clear()
        {
            textBox.text = "";
        }
    }
}
