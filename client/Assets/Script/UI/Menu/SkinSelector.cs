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

        private SkinElement selectedElement;
        private SkinElement equipedElement;
        private RectTransform rectTransform;
        private RectTransform contentRectTransform;

        private Dictionary<int, SkinElement> skins = new Dictionary<int, SkinElement>();
        
        public void Start()
        {
            rectTransform = gameObject.GetComponent<RectTransform>();
            contentRectTransform = content.GetComponent<RectTransform>();
            LobbyServer.instance.RefreshLoginUser((User user) =>
            {
                if (user.inventory.current_skin != null)
                {
                    LoadMySkinsAndEquipId(user.inventory.current_skin.Value);
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
            foreach(var skin in skins)
            {
                Destroy(skin.Value.gameObject);
            }
            skins = new Dictionary<int,SkinElement>();
            LobbyServer.instance.Get<Skin[]>("/skins/me/",(skins,err)=>
            {
                if(err!=null)
                {
                    ToastManager.instance.Add(LanguageManager.GetText("cantloadskins"),"Error");
                    return;
                }
                CreateBasicSkin();
                if(skins!=null)
                    AddElements(skins.Reverse().ToArray());
                if(callback!=null)
                    callback(skins);
            });
        }

        public void LoadMySkinsAndEquipIndex(int index)
        {
            LoadMySkins((loadedSkins) =>
            {
                var element = skins.Values.ElementAtOrDefault(index);
                if (element == null)
                    return;
                Equip(element, false);
                StartCoroutine(MoveToElementAfter1Frame(element));
            });
        }

        public void LoadMySkinsAndEquipId(int id)
        {
            LoadMySkins((loadedSkins)=>
            {
                if (!skins.ContainsKey(id))
                    return;
                var currentSkin = skins[id];
                Equip(currentSkin, false);
                StartCoroutine(MoveToElementAfter1Frame(currentSkin));
            });
        }

        private void CreateBasicSkin()
        {
            var element = Instantiate(prefab, content);
            element.Init(this, null);
            skins.Add(-1, element);
            element.SetTextures(basicSkinBlack, basicSkinWhite);
        }

        private void AddElements(Skin[] skinList)
        {
            foreach(var skin in skinList)
            {
                AddElement(skin);
            }
        }

        private void AddElement(Skin skin)
        {
            var element  = Instantiate(prefab,content);
            element.Init(this,skin);
            skins.Add(skin.id,element);
        }

        public void Select(SkinElement element)
        {
            if(selectedElement==element)
                return;

            if(selectedElement!=null)
                selectedElement.UnSelect();

            selectedElement = element;
            skinPreview.SetSkin(element.skin);
            if(element.skin == null)
            {
                skinPreview.SetTextures(basicSkinBlack,basicSkinWhite);
            }
        }

        public void Equip(int index,bool moveToElement)
        {
            var element = skins.Values.ElementAtOrDefault(index);
            if(element==null)
                return;
            Equip(element, moveToElement);
        }

        public void Equip(SkinElement element,bool moveToElement)
        {
            if (element != equipedElement && equipedElement != null)
                equipedElement.UnEquip();

            equipedElement = element;
            Select(element);
            element.Select();
            element.Equip();

            if (!moveToElement)
                return;
            MoveToElement(element);

            int? id = null;
            if(element.skin != null)
            {
                id = element.skin.id;
            }

            LobbyServer.instance.EquipSkin(id,(err)=>
            {
                if(err!=null)
                {
                    ToastManager.instance.Add(LanguageManager.GetText("equipfailed"),"Error");
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
            var value = (contentRectTransform.rect.width - rectTransform.rect.width) / 2;
            content.DOLocalMoveX(Mathf.Clamp(-element.transform.localPosition.x, -value, value) + rectTransform.rect.width / 2, 0.5f).SetEase(Ease.InOutQuad);
        }

    }
}