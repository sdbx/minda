using System;
using Models;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

namespace UI.Menu
{
    public class SkinElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Skin skin { get; private set; }

        [SerializeField]
        private RawImage blackImage;
        [SerializeField]
        private RawImage whiteImage;

        [FormerlySerializedAs("MouseOnScale")] [SerializeField]
        private Vector3 mouseOnScale;
        [FormerlySerializedAs("SelectedScale")] [SerializeField]
        private Vector3 selectedScale;
        [SerializeField]
        private float duration;

        [SerializeField] private DisplayChanger displayChanger;
        [SerializeField]
        private GameObject equipIcon;

        private SkinSelector _skinSelector;

        private bool _isSelected;
        private bool _isEquiped;

        private void Awake()
        {
            equipIcon.SetActive(false);
        }

        public void Init(SkinSelector skinSelector, Skin skin)
        {
            this._skinSelector = skinSelector;
            SetSkin(skin);
        }

        public void SetSkin(Skin skin)
        {
            this.skin = skin;
            if (skin == null)
                return;
            LobbyServer.Instance.GetLoadedSkin(skin, (LoadedSkin downloadedSkin) =>
            {
                blackImage.texture = downloadedSkin.BlackTexture;
                whiteImage.texture = downloadedSkin.WhiteTexture;
            });
        }

        public void SetTextures(Texture black, Texture white)
        {
            blackImage.texture = black;
            whiteImage.texture = white;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isSelected)
            {
                return;
            }
            transform.DOScale(mouseOnScale, duration);
            displayChanger.SetMode("On");
        }

        public void UnSelect()
        {
            _isSelected = false;
            displayChanger.SetOrigin();
            transform.DOScale(new Vector3(1, 1, 1), duration);
        }

        public void Select()
        {
            _isSelected = true;
            transform.DOScale(selectedScale, duration);
            displayChanger.SetMode("Selected");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isSelected)
            {
                return;
            }
            transform.DOScale(new Vector3(1, 1, 1), duration);
            displayChanger.SetOrigin();
        }

        public void UnEquip()
        {
            _isEquiped = false;
            equipIcon.SetActive(false);
            UnSelect();
        }

        public void Equip()
        {
            _isEquiped = true;
            equipIcon.SetActive(true);
            transform.DOScale(selectedScale, duration);
            displayChanger.SetMode("Selected");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                Equip();
                _skinSelector.Equip(this, true);
            }
            if (!_isSelected)
            {
                Select();
                _skinSelector.Select(this);
            }
        }
    }
}
