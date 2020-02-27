using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RotaryHeart.Lib.SerializableDictionary;
using DG.Tweening;

public class DisplayChanger : MonoBehaviour
{
    [Serializable]
    public class ColorSettings : SerializableDictionaryBase<string, Color> { }

    [Serializable]
    private class Setting
    {
        public Setting()
        {

        }
        public Graphic element;
        [HideInInspector]
        private MonoBehaviour _component;
        [HideInInspector]
        public Color origin;
        public ColorSettings colors = new ColorSettings();
    }

    [SerializeField]
    private float duration;
    [SerializeField]
    private List<Setting> settings = new List<Setting>()
    {
        {new Setting()},
        {new Setting()},
        {new Setting()},
        {new Setting()},
        {new Setting()},
        {new Setting()},
    };

    private bool _isOriginColorSaved = false;

    private void Awake()
    {
        SaveOrigin();
    }

    private void SaveOrigin()
    {
        if (_isOriginColorSaved)
            return;

        foreach (var sett in settings)
        {
            sett.origin = sett.element.color;
        }
        _isOriginColorSaved = true;
    }

    public void SetMode(string mode)
    {
        SaveOrigin();
        foreach (var setting in settings)
        {
            if (setting.colors.ContainsKey(mode))
            {
                setting.element.DOColor(setting.colors[mode], duration);
            }
        }
    }

    public void SetOrigin()
    {
        SaveOrigin();
        foreach (var sett in settings)
        {
            sett.element.DOColor(sett.origin, duration);
        }
    }
}
