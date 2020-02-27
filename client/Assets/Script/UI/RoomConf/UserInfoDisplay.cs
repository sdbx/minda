using System.Collections;
using System.Collections.Generic;
using Game;
using Models;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class UserInfoDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [FormerlySerializedAs("UserId")] public int userId;
        private BallType _ballType = BallType.Black;
        private RectTransform _rectTransform;
        private RectTransform _rootRectTransform;
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

        private bool _destroyed;

        private Texture _placeHolder;

        private void Awake()
        {
            _destroyed = false;
            _rectTransform = gameObject.GetComponent<RectTransform>();
            _rootRectTransform = transform.root.GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            _destroyed = true;
        }

        private void Start()
        {
            _placeHolder = UiSettings.Instance.placeHolder;
            GameServer.Instance.GetInGameUser(userId, (InGameUser inGameUser) =>
            {
                if (!_destroyed)
                {
                    usernameText.text = inGameUser.User.Username;
                    kingIcon.SetActive(inGameUser.IsKing);

                    if (inGameUser.User.Picture != null)
                    {
                        GameServer.Instance.GetProfileTexture(userId, SetProfileImage);
                    }
                    else SetProfileImage(_placeHolder);
                }
            });
        }

        private void SetProfileImage(Texture texture)
        {
            profileImage.texture = (texture == null ? _placeHolder : texture);
        }

        private void CreateContextMenu()
        {
            var contextMenu = new ContextMenu(contextMenuPivot);
            var gameServer = GameServer.Instance;
            var myId = LobbyServer.Instance.loginUser.Id;
            //king menu
            GameServer.Instance.GetInGameUser(userId, (InGameUser inGameUser) =>
            {
                if (RoomUtils.CheckIsKing(myId))
                {
                    if (inGameUser.BallType != BallType.White)
                    {
                        contextMenu.Add(LanguageManager.GetText("towhite"), () =>
                         {
                             gameServer.ChangeUserRole(userId, BallType.White);
                         });
                    }
                    if (inGameUser.BallType != BallType.Black)
                    {
                        contextMenu.Add(LanguageManager.GetText("toblack"), () =>
                         {
                             gameServer.ChangeUserRole(userId, BallType.Black);
                         });
                    }
                    if (inGameUser.BallType != BallType.None)
                    {
                        contextMenu.Add(LanguageManager.GetText("tospec"), () =>
                         {
                             gameServer.ChangeUserRole(userId, BallType.None);
                         });
                    }
                    if (userId != myId)
                    {
                        contextMenu.Add("Give King", () =>
                         {
                             gameServer.ChangeKingTo(userId);
                         });
                    }
                    var conf = GameServer.Instance.connectedRoom.Conf;
                    if (!(conf.Black == userId || conf.White == userId || conf.King == userId))
                    {
                        contextMenu.Add(LanguageManager.GetText("ban"), () =>
                        {
                            GameServer.Instance.BanUser(userId);
                        });
                    }
                }


                var corners = new Vector3[4];
                _rectTransform.GetLocalCorners(corners);

                var pos = PositionUtils.WorldPosToLocalRectPos(transform.position, _rootRectTransform);
                ContextMenuManager.Instance.Create(new Vector2(corners[corner].x + pos.x, corners[corner].y + pos.y), contextMenu);
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
