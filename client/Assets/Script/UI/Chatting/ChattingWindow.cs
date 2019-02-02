using System.Collections;
using Models;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chatting
{
    public class ChattingWindow : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect scrollRect;
        [SerializeField]
        private Text textBox;
        [SerializeField]
        private ChattingInput chattingInputToggler;
        [SerializeField]
        private float autoHideTime = 10;
        [SerializeField]
        private ObjectToggler toggler;

        private RectTransform rectTransform;

        public float lastTime = 0;

        private void Awake()
        {
            rectTransform = gameObject.GetComponent<RectTransform>();
            GameServer.instance.MessagedEvent += OnMessages;
            Clear();
        }

        private void Update()
        {
            //auto hide
            if (!chattingInputToggler.isActivated && toggler.isActivated)
            {
                if (lastTime > autoHideTime)
                {
                    toggler.UnActivate();
                    lastTime = 0;
                }
                else
                {
                    lastTime += Time.deltaTime;
                }
            }
            else
            {
                lastTime = 0;
            }

            //엔터키 입력
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (!chattingInputToggler.isActivated)
                {
                    toggler.Activate();
                    chattingInputToggler.Activate();
                    ScrollToBottom();
                }
            }
            
            //채팅창 외부 클릭
            if (Input.GetMouseButton(0) && gameObject.activeSelf &&
                !RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                Input.mousePosition,
                Camera.main))
            {
                chattingInputToggler.UnActivate();
            }

        }

        private void OnMessages(Message message)
        {
            var isBottom = scrollRect.verticalNormalizedPosition <= 10;
            AddLine(message.ToString());

            if (isBottom ||
             (message is UserMessage &&
              ((UserMessage)message).sendUser.user.id == LobbyServer.instance.loginUser.id))
            {
                ScrollToBottom();
                StartCoroutine(ScrollToBottomAfter1Frame());
            }
            lastTime = 0;
        }

        private void AddLine(string content)
        {
            toggler.Activate();
            lastTime = 0;
            if(textBox.text!="")
                textBox.text+="\n";
            textBox.text += content;
        }

        private void Clear()
        {
            textBox.text = "";
        }

        public void ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }

        public IEnumerator ScrollToBottomAfter1Frame()
        {
            yield return 0;
            ScrollToBottom();
        }
    }
}
