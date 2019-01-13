using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementDisplaySetter : MonoBehaviour
{
    private enum ElementType
    {
        Text,
        RawImage,
    }
    [Serializable]
    private class setting
    {
        public GameObject element;
        public ElementType type;
        [HideInInspector]
        public Color origin;
        public Color selected;
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

    public void Select()
    {
        foreach (var sett in settings)
        {
            switch (sett.type)
            {

                case ElementType.Text:
                    {
                        sett.element.GetComponent<Text>().color = sett.selected;
                        break;
                    }

                case ElementType.RawImage:
                    {
                        sett.element.GetComponent<RawImage>().color = sett.selected;
                        break;
                    }

            }
        }
    }

    public void UnSelect()
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
