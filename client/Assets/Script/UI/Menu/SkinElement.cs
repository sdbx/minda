using System;
using Models;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace UI.Menu
{
    public class SkinElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public LoadedSkin skin { get; private set; }

        [SerializeField]
        private RawImage blackImage;
        [SerializeField]
        private RawImage whiteImage;

        [SerializeField]
        private Vector3 MouseOnScale;
        [SerializeField]
        private Vector3 SelectedScale;
        [SerializeField]
        private float duration;

        [SerializeField]
        DisplayChanger displayChanger;
        [SerializeField]
        private GameObject equipIcon;

        private SkinSelector skinSelector;

        private bool isSelected;
        private bool isEquiped;

        private void Start()
        {
            equipIcon.SetActive(false);
        }

        public void Init(SkinSelector skinSelector, Skin skin)
        {
            this.skinSelector = skinSelector;
            SetSkin(skin);
        }

        public void SetSkin(Skin skin)
        {
            LoadedSkin.Get(skin, (LoadedSkin downloadedSkin) =>
             {
                 this.skin = downloadedSkin;
                 blackImage.texture = downloadedSkin.blackTexture;
                 whiteImage.texture = downloadedSkin.whiteTexture;
             });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isSelected)
            {
                return;
            }
            transform.DOScale(MouseOnScale, duration);
            displayChanger.SetMode("On");
        }

        public void UnSelect()
        {
            isSelected = false;
            displayChanger.SetOrigin();
            transform.DOScale(new Vector3(1, 1, 1), duration);
        }

        public void Select()
        {
            isSelected = true;
            transform.DOScale(SelectedScale, duration);
            displayChanger.SetMode("Selected");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isSelected)
            {
                return;
            }
            transform.DOScale(new Vector3(1, 1, 1), duration);
            displayChanger.SetOrigin();
        }

        public void UnEquip()
        {
            isEquiped = false;
            equipIcon.SetActive(false);
            UnSelect();
        }

        public void Equip()
        {
            isEquiped = true;
            equipIcon.SetActive(true);
            transform.DOScale(SelectedScale, duration);
            displayChanger.SetMode("Selected");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                Equip();
                skinSelector.Equip(this);
            }
            if (!isSelected)
            {
                Select();
                skinSelector.Select(this);
            }
        }
    }
}