using System.Collections.Generic;
using System.Linq;
using Models;
using Network;
using UI.Toast;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;

namespace UI.Menu
{
    public class SkinSelector : MonoBehaviour
    {
        [SerializeField]
        private SkinPreview skinPreview;
        [SerializeField]
        private ScrollRect scrollRect;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private SkinElement prefab;
        [SerializeField]
        private Texture basicSkinBlack;
        [SerializeField]
        private Texture basicSkinWhite;

        private SkinElement _selectedElement;
        private SkinElement _equipedElement;
        private RectTransform _rectTransform;
        private RectTransform _contentRectTransform;

        private Dictionary<int, SkinElement> _skins = new Dictionary<int, SkinElement>();

        public void Start()
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
            _contentRectTransform = content.GetComponent<RectTransform>();
            LobbyServer.Instance.RefreshLoginUser((User user) =>
            {
                if (user.Inventory.CurrentSkin != null)
                {
                    LoadMySkinsAndEquipId(user.Inventory.CurrentSkin.Value);
                }
                else
                {
                    //basicskin
                    LoadMySkinsAndEquipIndex(0);
                }
            });
        }

        public void LoadMySkins(Action<Skin[]> callback = null)
        {
            foreach (var skin in _skins)
            {
                Destroy(skin.Value.gameObject);
            }
            _skins = new Dictionary<int, SkinElement>();
            LobbyServer.Instance.Get<Skin[]>("/skins/me/", (skins, err) =>
             {
                 if (err != null)
                 {
                     ToastManager.Instance.Add(LanguageManager.GetText("cantloadskins"), "Error");
                     return;
                 }
                 CreateBasicSkin();
                 if (skins != null)
                     AddElements(skins.Reverse().ToArray());
                 if (callback != null)
                     callback(skins);
             });
        }

        public void LoadMySkinsAndEquipIndex(int index)
        {
            LoadMySkins((loadedSkins) =>
            {
                var element = _skins.Values.ElementAtOrDefault(index);
                if (element == null)
                    return;
                Equip(element, false);
                StartCoroutine(MoveToElementAfter1Frame(element));
            });
        }

        public void LoadMySkinsAndEquipId(int id)
        {
            LoadMySkins((loadedSkins) =>
            {
                if (!_skins.ContainsKey(id))
                    return;
                var currentSkin = _skins[id];
                Equip(currentSkin, false);
                StartCoroutine(MoveToElementAfter1Frame(currentSkin));
            });
        }

        private void CreateBasicSkin()
        {
            var element = Instantiate(prefab, content);
            element.Init(this, null);
            _skins.Add(-1, element);
            element.SetTextures(basicSkinBlack, basicSkinWhite);
        }

        private void AddElements(Skin[] skinList)
        {
            foreach (var skin in skinList)
            {
                AddElement(skin);
            }
        }

        private void AddElement(Skin skin)
        {
            var element = Instantiate(prefab, content);
            element.Init(this, skin);
            _skins.Add(skin.Id, element);
        }

        public void Select(SkinElement element)
        {
            if (_selectedElement == element)
                return;

            if (_selectedElement != null)
                _selectedElement.UnSelect();

            _selectedElement = element;
            skinPreview.SetSkin(element.skin);
            if (element.skin == null)
            {
                skinPreview.SetTextures(basicSkinBlack, basicSkinWhite);
            }
        }

        public void Equip(int index, bool moveToElement)
        {
            var element = _skins.Values.ElementAtOrDefault(index);
            if (element == null)
                return;
            Equip(element, moveToElement);
        }

        public void Equip(SkinElement element, bool moveToElement)
        {
            if (element != _equipedElement && _equipedElement != null)
                _equipedElement.UnEquip();

            _equipedElement = element;
            Select(element);
            element.Select();
            element.Equip();

            if (!moveToElement)
                return;
            MoveToElement(element);

            int? id = null;
            if (element.skin != null)
            {
                id = element.skin.Id;
            }

            LobbyServer.Instance.EquipSkin(id, (err) =>
             {
                 if (err != null)
                 {
                     ToastManager.Instance.Add(LanguageManager.GetText("equipfailed"), "Error");
                 }
             });
        }

        private IEnumerator MoveToElementAfter1Frame(SkinElement element)
        {
            yield return 0;
            MoveToElement(element);
        }

        public void MoveToElement(SkinElement element)
        {
            scrollRect.elasticity = 0;
            var value = (_contentRectTransform.rect.width - _rectTransform.rect.width) / 2;
            content.DOLocalMoveX(Mathf.Clamp(-element.transform.localPosition.x, -value, value) + _rectTransform.rect.width / 2, 0.5f).SetEase(Ease.InOutQuad);
        }

    }
}
