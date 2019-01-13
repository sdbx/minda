using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuListBar : MonoBehaviour
{
    [SerializeField]
    private List<MenuElement> menuList = new List<MenuElement>();
    private MenuElement selectedMenu;
    [SerializeField]
    private int startingIndex;
    
    void Start()
    {

        foreach (var menuElement in menuList)
        {
            menuElement.AddClickListener(MenuElementClicked);
        }
        var startingMenu = menuList[startingIndex];
        startingMenu.Select();
        selectedMenu = startingMenu; 
    }
    
    void MenuElementClicked(MenuElement element)
    {
        if(selectedMenu!=null)
            selectedMenu.UnSelect();
        element.Select();
        selectedMenu = element;
    }
}
