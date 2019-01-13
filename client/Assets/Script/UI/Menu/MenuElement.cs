using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuElement : MonoBehaviour
{
    private string MenuName;
    [SerializeField]
    GameObject content;
    public bool isSelected { get; private set; } = true;
    private ElementDisplaySetter displaySetter;

    private void Awake()
    {
        displaySetter = gameObject.GetComponent<ElementDisplaySetter>();     
    }

    private void Start()
    {
        UnSelect();
    }

    public void Select()
    {
        if (isSelected)
            return;
        displaySetter.Select();
        isSelected = true;
        content.SetActive(true);
    }

    public void UnSelect()
    {
        if (!isSelected)
            return;
        displaySetter.UnSelect();
        isSelected = false;
        content.SetActive(false);
    }

    public void AddClickListener(Action<MenuElement> callback)
    {
        var btn = gameObject.AddComponent<Button>();
        btn.onClick.AddListener(() => { callback(this); });
    }
}
