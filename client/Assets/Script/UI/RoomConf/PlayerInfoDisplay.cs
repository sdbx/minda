using System.Collections;
using System.Collections.Generic;
using Game;
using Models;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Camera;

namespace UI
{
    public class PlayerInfoDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private int UserId;
        private InGameUser inGameUser;

        private RectTransform rectTransform;
        [SerializeField]
        private RawImage profileImage;
        [SerializeField]
        private RawImage backGround;
        [SerializeField]
        private Text usernameText;
        //[SerializeField]
        //private Text imformationText;
        [SerializeField]
        private Texture backgroundBlack;
        [SerializeField]
        private Texture backgroundWhite;
        [SerializeField]
        private GameObject kingIcon;
        [SerializeField]
        private DisplayChanger displayChanger;
        [SerializeField]
        private int corner;
        [SerializeField]
        private Vector2 contextMenuPivot;

        private Texture placeHolder;

        private void Awake()
        {
            placeHolder = UISettings.instance.placeHolder;
            rectTransform = gameObject.GetComponent<RectTransform>();
        }
        
        public void display(int id, BallType ballType = BallType.None)
        {
            UserId = id;
            if(id == -1)
            {
                usernameText.text = "Waiting..";
                SetColor(ballType);
                kingIcon.SetActive(false);
                inGameUser = null;
                return;
            }

            GameServer.instance.GetInGameUser(id, (InGameUser inGameUser)=>
            {
                this.inGameUser = inGameUser;
                usernameText.text = inGameUser.user.username;
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
                displayChanger.SetMode("Black");
            }
            else if(ballType == BallType.White)
            {
                backGround.texture = backgroundWhite;
                displayChanger.SetMode("White");
            }
        }

        private void CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu(contextMenuPivot);
            var gameServer = GameServer.instance;
            var myId = LobbyServer.instance.loginUser.id;
            //king menu
            if(gameServer.connectedRoom.conf.king == myId)
            {
                if(inGameUser.ballType != BallType.White)
                {
                    contextMenu.Add("To White",()=>
                    {
                        gameServer.ChangeUserRole(UserId, BallType.White);
                    });
                }
                if(inGameUser.ballType != BallType.Black)
                {
                    contextMenu.Add("To Black",()=>
                    {
                        gameServer.ChangeUserRole(UserId, BallType.Black);
                    });
                }
                if(inGameUser.ballType != BallType.None)
                {
                    contextMenu.Add("To Spectator",()=>
                    {
                        gameServer.ChangeUserRole(UserId, BallType.None);
                    });
                }
                if(UserId != myId)
                {
                    contextMenu.Add("Give King", () =>
                     {
                         gameServer.ChangeKingTo(UserId);
                     });
                }
            }

            contextMenu.Add("Whisper",()=>{Debug.Log("귓속말");});
            
            Vector3[] corners = new Vector3[4];
            rectTransform.GetLocalCorners(corners);

            var position = corners[corner] + rectTransform.localPosition;
            ContextMenuManager.instance.Create(new Vector2(position.x,position.y), contextMenu);
        }
        
        //user menucontext
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(UserId == -1)
                return;
            //BlackHightlighed, WhiteHightlighed
            displayChanger.SetMode(inGameUser.ballType + "Hightlighed");
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
