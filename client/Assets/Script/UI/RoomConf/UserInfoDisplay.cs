using System.Collections;
using System.Collections.Generic;
using Game;
using Models;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class UserInfoDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public int UserId;
        private BallType ballType = BallType.Black;
        private RectTransform rectTransform;
        private RectTransform rootRectTransform;
        [SerializeField]
        private RawImage profileImage;
        [SerializeField]
        private Text usernameText;
        //[SerializeField]
        //private Text imformationText;
        [SerializeField]
        private GameObject kingIcon;
        [SerializeField]
        private DisplayChanger displayChanger;
        [SerializeField]
        private Vector3 contextMenuOffset;
        [SerializeField]
        private Vector2 contextMenuPivot;
        [SerializeField]
        private int corner;

        private bool destroyed;

        private Texture placeHolder;
        
        private void Awake() 
        {
            destroyed = false;
            rectTransform = gameObject.GetComponent<RectTransform>();
            rootRectTransform = transform.root.GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            destroyed = true;
        }

        private void Start() 
        {
            placeHolder = UISettings.instance.placeHolder;
            GameServer.instance.GetInGameUser(UserId, (InGameUser inGameUser)=>
            {
                if (!destroyed)
                {
                    usernameText.text = inGameUser.user.username;
                    kingIcon.SetActive(inGameUser.isKing);
                    
                    if(inGameUser.user.picture != null)
                    {
                        GameServer.instance.GetProfileTexture(UserId, SetProfileImage);
                    }
                    else SetProfileImage(placeHolder);
                }
            });
        }

        private void SetProfileImage(Texture texture)
        {
            profileImage.texture = (texture == null ? placeHolder : texture);
        }

        private void CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu(contextMenuPivot);
            var gameServer = GameServer.instance;
            var myId = LobbyServer.instance.loginUser.id;
            //king menu
            GameServer.instance.GetInGameUser(UserId, (InGameUser inGameUser) =>
            {
                if (RoomUtils.CheckIsKing(myId))
                {
                    if (inGameUser.ballType != BallType.White)
                    {
                        contextMenu.Add(LanguageManager.GetText("towhite"), () =>
                         {
                             gameServer.ChangeUserRole(UserId, BallType.White);
                         });
                    }
                    if (inGameUser.ballType != BallType.Black)
                    {
                        contextMenu.Add(LanguageManager.GetText("toblack"), () =>
                         {
                             gameServer.ChangeUserRole(UserId, BallType.Black);
                         });
                    }
                    if (inGameUser.ballType != BallType.None)
                    {
                        contextMenu.Add(LanguageManager.GetText("tospec"), () =>
                         {
                             gameServer.ChangeUserRole(UserId, BallType.None);
                         });
                    }
                    if (UserId != myId)
                    {
                        contextMenu.Add("Give King", () =>
                         {
                             gameServer.ChangeKingTo(UserId);
                         });
                    }
                    var conf = GameServer.instance.connectedRoom.conf;
                    if (!(conf.black == UserId || conf.white == UserId || conf.king == UserId))
                    {
                        contextMenu.Add(LanguageManager.GetText("ban"), () =>
                        {
                            GameServer.instance.BanUser(UserId);
                        });
                    }
                }


                Vector3[] corners = new Vector3[4];
                rectTransform.GetLocalCorners(corners);

                Vector2 pos = PositionUtils.WorldPosToLocalRectPos(transform.position, rootRectTransform);
                ContextMenuManager.instance.Create(new Vector2(corners[corner].x + pos.x, corners[corner].y + pos.y), contextMenu);
            });
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
            CreateContextMenu();
        }
    }

}