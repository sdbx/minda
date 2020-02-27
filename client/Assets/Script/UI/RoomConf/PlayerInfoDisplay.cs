using System.Collections;
using System.Collections.Generic;
using Game;
using Models;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using static UnityEngine.Camera;

namespace UI
{
    public class PlayerInfoDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private int _userId;
        private InGameUser _inGameUser;

        private RectTransform _rectTransform;
        [SerializeField]
        private RawImage profileImage;
        [SerializeField]
        private RawImage backGround;
        [SerializeField]
        private Text usernameText;
        [SerializeField]
        private Texture backgroundBlack;
        [SerializeField]
        private Texture backgroundWhite;
        [SerializeField]
        private GameObject kingIcon;
        [SerializeField]
        private DisplayChanger backGroundDisplayChanger;
        [SerializeField]
        private DisplayChanger textDisplayChanger;
        [SerializeField]
        private int corner;
        [SerializeField]
        private Vector2 contextMenuPivot;
        [SerializeField]
        private int num;

        private Texture _placeHolder;

        private void Awake()
        {
            _placeHolder = UiSettings.Instance.placeHolder;
            _rectTransform = gameObject.GetComponent<RectTransform>();
            Debug.Log("콘프드 이벤트 등록");
            GameServer.Instance.ConfedEvent += OnConfed;
            GameServer.Instance.RoomConnectedEvent += OnConnected;
        }

        private void Start()
        {
            if (GameServer.Instance.connectedRoom != null)
                SetPlayerInfo(GameServer.Instance.connectedRoom.Conf);
            return;
        }


        private void OnConnected(Room room)
        {
            SetPlayerInfo(room.Conf);
        }

        private void OnDestroy()
        {
            GameServer.Instance.ConfedEvent -= OnConfed;
        }

        public void OnConfed(Conf conf)
        {
            Debug.Log("콘프드발생해서 플레이어인포 설정");
            SetPlayerInfo(conf);
            Debug.Log("콘프드 이벤트 발생끝");
        }

        public void Display(int id, BallType ballType = BallType.None)
        {
            Debug.Log(id + "사진, 정보 시작");
            _userId = id;
            if (id == -1)
            {
                usernameText.text = LanguageManager.GetText("waiting");
                SetColor(ballType);
                kingIcon.SetActive(false);
                _inGameUser = null;
                SetProfileImage(_placeHolder);
                Debug.Log(id + "끝");
                return;
            }

            GameServer.Instance.GetInGameUser(id, (InGameUser inGameUser) =>
            {

                this._inGameUser = inGameUser;
                usernameText.text = inGameUser.User.Username;
                SetColor(inGameUser.BallType);
                kingIcon.SetActive(inGameUser.IsKing);
                Debug.Log(id + "정보 등록 완료");
                if (inGameUser.User.Picture != null)
                {
                    GameServer.Instance.GetProfileTexture(id, SetProfileImage);
                    Debug.Log(id + "사진 등록완료");
                }
                else SetProfileImage(_placeHolder);
            });
        }

        private void SetProfileImage(Texture texture)
        {
            profileImage.texture = (texture == null ? _placeHolder : texture);
        }

        private void SetColor(BallType ballType)
        {
            if (ballType == BallType.Black)
            {
                backGround.texture = backgroundBlack;
                textDisplayChanger.SetMode("Black");
            }
            else if (ballType == BallType.White)
            {
                backGround.texture = backgroundWhite;
                textDisplayChanger.SetMode("White");
            }
        }

        private void CreateContextMenu()
        {
            var contextMenu = new ContextMenu(contextMenuPivot);
            var gameServer = GameServer.Instance;
            var myId = LobbyServer.Instance.loginUser.Id;
            //king menu
            if (gameServer.connectedRoom.Conf.King == myId)
            {
                if (RoomUtils.CheckIsKing(myId))
                {
                    if (_inGameUser.BallType != BallType.White)
                    {
                        contextMenu.Add(LanguageManager.GetText("towhite"), () =>
                         {
                             gameServer.ChangeUserRole(_userId, BallType.White);
                         });
                    }
                    if (_inGameUser.BallType != BallType.Black)
                    {
                        contextMenu.Add(LanguageManager.GetText("toblack"), () =>
                         {
                             gameServer.ChangeUserRole(_userId, BallType.Black);
                         });
                    }
                    if (_inGameUser.BallType != BallType.None)
                    {
                        contextMenu.Add(LanguageManager.GetText("tospec"), () =>
                         {
                             gameServer.ChangeUserRole(_userId, BallType.None);
                         });
                    }
                    if (_userId != myId)
                    {
                        contextMenu.Add(LanguageManager.GetText("makeking"), () =>
                         {
                             gameServer.ChangeKingTo(_userId);
                         });
                    }
                    var conf = GameServer.Instance.connectedRoom.Conf;
                    if (!(conf.Black == _userId || conf.White == _userId || conf.King == _userId))
                    {
                        contextMenu.Add(LanguageManager.GetText("ban"), () =>
                        {
                            GameServer.Instance.BanUser(_userId);
                        });
                    }
                }
            }

            var corners = new Vector3[4];
            _rectTransform.GetLocalCorners(corners);

            var position = corners[corner] + _rectTransform.localPosition;
            ContextMenuManager.Instance.Create(new Vector2(position.x, position.y), contextMenu);
        }

        //user menucontext
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_userId == -1)
                return;
            //BlackHightlighed, WhiteHightlighed
            backGroundDisplayChanger.SetMode(_inGameUser.BallType + "Hightlighed");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            backGroundDisplayChanger.SetOrigin();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_inGameUser != null)
            {
                CreateContextMenu();
            }
        }

        private void SetPlayerInfo(Conf conf)
        {
            var isSpectator = GameServer.Instance.isSpectator;

            int playerId;
            BallType ballType;

            //본인이 검은색이거나 관전자
            if (isSpectator || conf.Black == LobbyServer.Instance.loginUser.Id)
            {
                ballType = (num == 2 ? BallType.Black : BallType.White);
            }
            //본인이 흰색
            else
            {
                ballType = (num == 2 ? BallType.White : BallType.Black);
            }

            playerId = (ballType == BallType.Black ? conf.Black : conf.White);

            Display(playerId, ballType);
        }
    }

}
