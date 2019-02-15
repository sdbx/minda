using System.Collections.Generic;
using System.Linq;
using Models;
using Network;
using UI.Toast;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

        private SkinElement selectedElement;
        private SkinElement equipedElement;
        private RectTransform rectTransform;
        private RectTransform contentRectTransform;
        
        private Dictionary<int,SkinElement> skins = new Dictionary<int,SkinElement>();
        
        private void Awake()
        {
            LoadMySkins();
            rectTransform = gameObject.GetComponent<RectTransform>();
            contentRectTransform = content.GetComponent<RectTransform>();
        }

        private void LoadMySkins()
        {
            LobbyServer.instance.Get<Skin[]>("/skins/me/",(skins,err)=>
            {
                if(err!=null)
                {
                    ToastManager.instance.Add("Can't load skins","Error");
                    return;
                }
                AddElements(skins);
            });
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
        }

        public void Equip(SkinElement element)
        {
            if (equipedElement != null)
                equipedElement.UnEquip();

            equipedElement = element;
            var value = (contentRectTransform.rect.width-rectTransform.rect.width)/2;

            content.DOLocalMoveX(Mathf.Clamp(-element.transform.localPosition.x,-value,value)+rectTransform.rect.width/2,0.5f).SetEase(Ease.InOutQuad);
            Debug.Log(Mathf.Clamp(-element.transform.localPosition.x+rectTransform.rect.width/2,-value,value)+" "+value);
        }

    }
}