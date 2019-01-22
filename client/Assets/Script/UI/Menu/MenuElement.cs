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
    private DisplayChanger displayChanger;

    private void Awake()
    {
        displayChanger = gameObject.GetComponent<DisplayChanger>();     
    }

    private void Start()
    {
        UnSelect();
    }

    public void Select()
    {
        if (isSelected)
            return;
        displayChanger.SetMode("Selected");
        isSelected = true;
        content.SetActive(true);
    }

    public void UnSelect()
    {
        if (!isSelected)
            return;
        displayChanger.SetOrigin();
        isSelected = false;
        content.SetActive(false);
    }

    public void AddClickListener(Action<MenuElement> callback)
    {
        var btn = gameObject.AddComponent<Button>();
        btn.onClick.AddListener(() => { callback(this); });
    }
}
