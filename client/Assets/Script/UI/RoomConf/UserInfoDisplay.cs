using System.Collections;
using System.Collections.Generic;
using Game;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UserInfoDisplay : MonoBehaviour
    {
        public Models.User user;

        public bool isKing;
        [SerializeField]
        private RawImage profileImage;
        [SerializeField]
        private RawImage backGround;
        [SerializeField]
        private Text usernameText;
        [SerializeField]
        private Text informationText;
        [SerializeField]
        private Texture backgroundBlack;
        [SerializeField]
        private Texture backgroundWhite;
        [SerializeField]
        private GameObject kingIcon;
        public BallType ballType = BallType.White;

        public void Display()
        {
            if(ballType == BallType.Black)
            {
                backGround.texture = backgroundBlack;
                usernameText.color = Color.white;
                informationText.color = Color.white;
            }
            else
            {
                backGround.texture = backgroundWhite;
                usernameText.color = Color.black;
                informationText.color = Color.black;
            }
            kingIcon.SetActive(isKing);
            if(user.picture != null)
            {
                NetworkManager.instance.DownloadImage(user.picture, (Texture texture) =>
                {
                    Debug.Log(texture);
                    profileImage.texture = texture;
                });
            }
            usernameText.text = user.username;
            informationText.text = "LV.53 MP.500";
        }
        
    }

}
