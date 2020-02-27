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

        private RectTransform _blackRectTransform;
        private RectTransform _whiteRectTransform;

        private Vector3 _originBlackPos;
        private Vector3 _originWhitePos;
        private Vector2 _originBlackSize;
        private Vector2 _originWhiteSize;

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
            if (skin == null)
            {
                skinName.text = "Basic";
                return;
            }
            LobbyServer.Instance.GetLoadedSkin(skin, (LoadedSkin downloadedSkin) =>
            {
                blackImage.texture = downloadedSkin.BlackTexture;
                whiteImage.texture = downloadedSkin.WhiteTexture;
            });
            skinName.text = skin.Name;
        }

        public void SetTextures(Texture black, Texture white)
        {
            blackImage.texture = black;
            whiteImage.texture = white;
        }

        public void SetBlackTexture(Texture black)
        {
            blackImage.texture = black;
        }

        public void SetWhiteTexture(Texture white)
        {
            whiteImage.texture = white;
        }

        private void Awake()
        {
            _blackRectTransform = blackImage.GetComponent<RectTransform>();
            _whiteRectTransform = whiteImage.GetComponent<RectTransform>();

            _originBlackPos = blackImage.transform.localPosition;
            _originWhitePos = whiteImage.transform.localPosition;

            _originBlackSize = _blackRectTransform.rect.size;
            _originWhiteSize = _whiteRectTransform.rect.size;
        }

        public void CreatingMode()
        {
            blackImage.transform.DOLocalMove(blackPos, duration);
            whiteImage.transform.DOLocalMove(whitePos, duration);
            _blackRectTransform.DOSizeDelta(blackSize, duration);
            _whiteRectTransform.DOSizeDelta(whiteSize, duration);
            whiteImage.texture = UiSettings.Instance.basicWhiteSkin;
            blackImage.texture = UiSettings.Instance.basicBlackSkin;
            skinName.interactable = true;
            skinName.text = "";
        }

        public void SelectingMode()
        {
            blackImage.transform.DOLocalMove(_originBlackPos, duration);
            whiteImage.transform.DOLocalMove(_originWhitePos, duration);
            _blackRectTransform.DOSizeDelta(_originBlackSize, duration);
            _whiteRectTransform.DOSizeDelta(_originWhiteSize, duration);
            skinName.interactable = false;
        }
    }
}
