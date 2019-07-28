using Models;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Network;

namespace UI.Menu
{
    public class SkinPreview : MonoBehaviour
    {
        // private enum Mode
        // {
        //     Selecting,
        //     Creating
        // }

        public Skin skin { get; private set; }

        [SerializeField]
        private RawImage blackImage;
        [SerializeField]
        private RawImage whiteImage;

        private RectTransform blackRectTransform;
        private RectTransform whiteRectTransform;

        private Vector3 originBlackPos;
        private Vector3 originWhitePos;
        private Vector2 originBlackSize;
        private Vector2 originWhiteSize;

        [SerializeField]
        private Vector3 whitePos;
        [SerializeField]
        private Vector3 blackPos;
        [SerializeField]
        private Vector2 whiteSize;
        [SerializeField]
        private Vector2 blackSize;
        [SerializeField]
        private float duration;

        [SerializeField]
        private InputField skinName;

        public void SetName(string name)
        {
            skinName.text = name;
        }

        public void SetSkin(Skin skin)
        {
            if(skin == null)
            {
                skinName.text = "Basic";
                return;
            }
            LobbyServer.instance.GetLoadedSkin(skin, (LoadedSkin downloadedSkin) =>
            {
                blackImage.texture = downloadedSkin.blackTexture;
                whiteImage.texture = downloadedSkin.whiteTexture;
            });
            skinName.text = skin.name;
        }

        public void SetTextures(Texture black,Texture White)
        {
            blackImage.texture = black;
            whiteImage.texture = White;
        }

        public void SetBlackTexture(Texture black)
        {
            blackImage.texture = black;
        }
        
        public void SetWhiteTexture(Texture White)
        {
            whiteImage.texture = White;
        }

        private void Awake()
        {
            blackRectTransform = blackImage.GetComponent<RectTransform>();
            whiteRectTransform = whiteImage.GetComponent<RectTransform>();

            originBlackPos = blackImage.transform.localPosition;
            originWhitePos = whiteImage.transform.localPosition;

            originBlackSize = blackRectTransform.rect.size;
            originWhiteSize = whiteRectTransform.rect.size;
        }

        public void CreatingMode()
        {
            blackImage.transform.DOLocalMove(blackPos, duration);
            whiteImage.transform.DOLocalMove(whitePos, duration);
            blackRectTransform.DOSizeDelta(blackSize, duration);
            whiteRectTransform.DOSizeDelta(whiteSize, duration);
            whiteImage.texture = UISettings.instance.basicWhiteSkin;
            blackImage.texture = UISettings.instance.basicBlackSkin;
            skinName.interactable = true;
            skinName.text = "";
        }

        public void SelectingMode()
        {
            blackImage.transform.DOLocalMove(originBlackPos, duration);
            whiteImage.transform.DOLocalMove(originWhitePos, duration);
            blackRectTransform.DOSizeDelta(originBlackSize, duration);
            whiteRectTransform.DOSizeDelta(originWhiteSize, duration);
            skinName.interactable = false;
        }
    }
}