using System.Collections;
using System.Collections.Generic;
using Game;
using Models;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class PlayerInfoDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private int UserId;
        private InGameUser inGameUser;

        private BallType ballType = BallType.Black;

        [SerializeField]
        private RawImage profileImage;
        [SerializeField]
        private RawImage backGround;
        [SerializeField]
        private Text usernameText;
        [SerializeField]
        private Text imformationText;
        [SerializeField]
        private Texture backgroundBlack;
        [SerializeField]
        private Texture backgroundWhite;
        [SerializeField]
        private GameObject kingIcon;
        [SerializeField]
        private DisplayChanger displayChanger;
        [SerializeField]
        private Vector3 offset;
        [SerializeField]
        private Vector2 pivot;

        private Texture placeHolder;

        private void Awake()
        {
            placeHolder = UISettings.instance.placeHolder;
        }
        
        public void display(int id, BallType ballType = BallType.None)
        {
            UserId = id;
            if(id == -1)
            {
                usernameText.text = "Waiting..";
                imformationText.text = "";
                SetColor(ballType);
                kingIcon.SetActive(false);     
                inGameUser = null;
                return;
            }

            GameServer.instance.GetInGameUser(id, (InGameUser inGameUser)=>
            {
                this.inGameUser = inGameUser;
                usernameText.text = inGameUser.user.username;
                imformationText.text = "gorani 53";
                SetColor(inGameUser.ballType);
                kingIcon.SetActive(inGameUser.isKing);
                if(inGameUser.user.picture != null)
                {
                    GameServer.instance.GetProfileTexture(id, SetProfileImage);
                }
                else SetProfileImage(placeHolder);
            });
        }

        private void SetProfileImage(Texture texture)
        {
            profileImage.texture = (texture == null ? placeHolder : texture);
        }

        private void SetColor(BallType ballType)
        {
            if(ballType == BallType.Black)
            {
                backGround.texture = backgroundBlack;
                usernameText.color = Color.white;
                imformationText.color = Color.white;
            }
            else if(ballType == BallType.White)
            {
                backGround.texture = backgroundWhite;
                usernameText.color = Color.black;
                imformationText.color = Color.black;
            }
        }

        private void CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu(pivot);

            //king menu
            if(GameServer.instance.connectedRoom.conf.king == LobbyServer.instance.loginUser.id)
            {
                if(inGameUser.ballType != BallType.White)
                {
                    contextMenu.Add("Change to White",()=>{Debug.Log("화이트로 변경");});
                }
                if(inGameUser.ballType != BallType.Black)
                {
                    contextMenu.Add("Change to Black",()=>{Debug.Log("검은색으로 변경");});
                }
                
            }
            contextMenu.Add("Whisper",()=>{Debug.Log("귓속말");});

            ContextMenuCreater.instance.Create(transform.position + offset, contextMenu);
        }
        
        //user menucontext
        public void OnPointerEnter(PointerEventData eventData)
        {
            displayChanger.SetMode("Hightlighed");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            displayChanger.SetOrigin();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(inGameUser != null)
            {
                CreateContextMenu();
            }
        }
    }

}
