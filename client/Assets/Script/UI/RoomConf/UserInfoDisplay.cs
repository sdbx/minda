using System.Collections;
using System.Collections.Generic;
using Game;
using Models;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UserInfoDisplay : MonoBehaviour
    {
        private int UserId;
        private BallType ballType = BallType.Black;

        [SerializeField]
        private RawImage profileImage;
        [SerializeField]
        private Text usernameText;
        [SerializeField]
        private Text imformationText;
        [SerializeField]
        private GameObject kingIcon;
        private Texture placeHolder;

        private void Start() 
        {
            placeHolder = UISettings.instance.placeHolder;
        }

        public void display(int id)
        {
            UserId = id;

            GameServer.instance.GetInGameUser(id, (InGameUser inGameUser)=>
            {
                usernameText.text = inGameUser.user.username;
                imformationText.text = "gorani 53";
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
        
    }

}