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

    public Vector2 pivot;
    public List<Menu> menus = new List<Menu>();

    public ContextMenu(Vector2 pivot)
    {
        this.pivot = pivot;
    }

    public ContextMenu(Vector2 pivot, Menu[] menus)
    {
        AddRange(menus);
        this.pivot = pivot;
    }

    public ContextMenu Add(string name, UnityAction action)
    {
        Add(new Menu(name,action));
        return this;
    }

    public ContextMenu Add(Menu menu)
    {
        menus.Add(menu);
        return this;
    }

    public ContextMenu AddRange(IEnumerable<Menu> menus)
    {
        this.menus.AddRange(menus);
        return this;
    }

    public ContextMenu SetPivot(Vector2 pivot)
    {
        this.pivot = pivot;
        return this;
    }

    public static ContextMenu operator +(ContextMenu a, ContextMenu b)
    {
        return new ContextMenu(a.pivot).AddRange(a.menus).AddRange(b.menus);
    }
}