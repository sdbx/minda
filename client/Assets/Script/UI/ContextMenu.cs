using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;

public class ContextMenu
{
    public struct Menu
    {
        public Menu(string name, UnityAction action)
        {
            this.name = name;
            this.action = action;
        }
        public string name;
        public UnityAction action;
    }

    public List<Menu> menus = new List<Menu>();

    public ContextMenu()
    {

    }

    public ContextMenu(Menu[] menus)
    {
        AddRange(menus);
    }

    public void Add(string name, UnityAction action)
    {
        Add(new Menu(name,action));
    }

    public void Add(Menu menu)
    {
        Add(menu);
    }

    public void AddRange(Menu[] menus)
    {
        this.menus.AddRange(menus);
    }
}