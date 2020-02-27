using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuListBar : MonoBehaviour
{
    [SerializeField]
    private List<MenuElement> menuList = new List<MenuElement>();
    private MenuElement _selectedMenu;
    [SerializeField]
    private int startingIndex;

    private void Start()
    {

        foreach (var menuElement in menuList)
        {
            menuElement.AddClickListener(MenuElementClicked);
        }
        var startingMenu = menuList[startingIndex];
        startingMenu.Select();
        _selectedMenu = startingMenu;
    }

    private void MenuElementClicked(MenuElement element)
    {
        if (_selectedMenu != null)
            _selectedMenu.UnSelect();
        element.Select();
        _selectedMenu = element;
    }
}
