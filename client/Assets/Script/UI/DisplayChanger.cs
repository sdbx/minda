using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RotaryHeart.Lib.SerializableDictionary;

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
    private List<setting> settings = new List<setting>();


    private void Awake() 
    {
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
    }

    public void SetMode(string mode)
    {
        foreach (var setting in settings)
        {
            switch (setting.type)
            {

                case ElementType.Text:
                    {   
                        if(setting.colors.ContainsKey(mode))
                            setting.element.GetComponent<Text>().color = setting.colors[mode];
                        break;
                    }

                case ElementType.RawImage:
                    {
                        if (setting.colors.ContainsKey(mode))
                            setting.element.GetComponent<RawImage>().color = setting.colors[mode];
                        break;
                    }

            }
        }
    }

    public void SetOrigin()
    {
        foreach (var sett in settings)
        {
            switch (sett.type)
            {

                case ElementType.Text:
                    {
                        sett.element.GetComponent<Text>().color = sett.origin;
                        break;
                    }

                case ElementType.RawImage:
                    {
                        sett.element.GetComponent<RawImage>().color = sett.origin;
                        break;
                    }

            }
        }
    }
}
