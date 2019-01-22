using System.Collections;
using System.Collections.Generic;
using Game;
using Models;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerInfoDisplay : MonoBehaviour
    {
        private int UserId;
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
                return;
            }

            GameServer.instance.GetInGameUser(id, (InGameUser inGameUser)=>
            {
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
        
    }

}
