using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RotaryHeart.Lib.SerializableDictionary;
using DG.Tweening;

public class DisplayChanger : MonoBehaviour
{
    private enum ElementType
    {
        Text,
        RawImage,
    }

    [Serializable]
    public class ColorSettings : SerializableDictionaryBase<string, Color> { }

    [Serializable]
    private class setting
    {
        public GameObject element;
        public ElementType type;
        [HideInInspector]
        private MonoBehaviour component;
        [HideInInspector]
        public Color origin;
        public ColorSettings colors = new ColorSettings();
    }
    [SerializeField]
    private float duration;
    [SerializeField]
    private List<setting> settings = new List<setting>();

    private bool isOriginColorSaved = false;

    private void Awake() 
    {
        SaveOrigin();
    }

    private void SaveOrigin()
    {
        if(isOriginColorSaved)
            return;

        foreach (var sett in settings)
        {
            switch (sett.type)
            {

                case ElementType.Text:
                    {
                        sett.origin = sett.element.GetComponent<Text>().color;
                        break;
                    }

                case ElementType.RawImage:
                    {
                        sett.origin = sett.element.GetComponent<RawImage>().color;
                        break;
                    }
            }
        }
        isOriginColorSaved = true;
    }

    public void SetMode(string mode)
    {
        SaveOrigin();
        foreach (var setting in settings)
        {
            switch (setting.type)
            {

                case ElementType.Text:
                    {   
                        if(setting.colors.ContainsKey(mode))
                        {
                            setting.element.GetComponent<Text>().DOColor(setting.colors[mode],duration);
                        }
                        break;
                    }

                case ElementType.RawImage:
                    {
                        if (setting.colors.ContainsKey(mode))
                        {
                            setting.element.GetComponent<RawImage>().DOColor(setting.colors[mode],duration);
                        }
                        break;
                    }

            }
        }
    }

    public void SetOrigin()
    {
        SaveOrigin();
        foreach (var sett in settings)
        {
            switch (sett.type)
            {

                case ElementType.Text:
                    {
                        sett.element.GetComponent<Text>().DOColor(sett.origin,duration);
                        break;
                    }

                case ElementType.RawImage:
                    {
                        sett.element.GetComponent<RawImage>().DOColor(sett.origin,duration);
                        break;
                    }

            }
        }
    }
}
