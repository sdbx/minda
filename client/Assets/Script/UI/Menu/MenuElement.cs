using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuElement : MonoBehaviour
{
    private string _menuName;
    [SerializeField] private GameObject content;
    public bool isSelected { get; private set; } = true;
    private DisplayChanger _displayChanger;

    private void Awake()
    {
        _displayChanger = gameObject.GetComponent<DisplayChanger>();
    }

    private void Start()
    {
        UnSelect();
    }

    public void Select()
    {
        if (isSelected)
            return;
        _displayChanger.SetMode("Selected");
        isSelected = true;
        content.SetActive(true);
    }

    public void UnSelect()
    {
        if (!isSelected)
            return;
        _displayChanger.SetOrigin();
        isSelected = false;
        content.SetActive(false);
    }

    public void AddClickListener(Action<MenuElement> callback)
    {
        var btn = gameObject.AddComponent<Button>();
        btn.onClick.AddListener(() => { callback(this); });
    }
}
