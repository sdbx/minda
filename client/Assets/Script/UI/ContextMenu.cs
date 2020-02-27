using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;

public class ContextMenu
{
    public struct Menu
    {
        public Menu(string name, UnityAction action)
        {
            this.Name = name;
            this.Action = action;
        }
        public string Name;
        public UnityAction Action;
    }

    public Vector2 Pivot;
    public List<Menu> Menus = new List<Menu>();

    public ContextMenu(Vector2 pivot)
    {
        this.Pivot = pivot;
    }

    public ContextMenu(Vector2 pivot, Menu[] menus)
    {
        AddRange(menus);
        this.Pivot = pivot;
    }

    public ContextMenu Add(string name, UnityAction action)
    {
        Add(new Menu(name, action));
        return this;
    }

    public ContextMenu Add(Menu menu)
    {
        Menus.Add(menu);
        return this;
    }

    public ContextMenu AddRange(IEnumerable<Menu> menus)
    {
        this.Menus.AddRange(menus);
        return this;
    }

    public ContextMenu SetPivot(Vector2 pivot)
    {
        this.Pivot = pivot;
        return this;
    }

    public static ContextMenu operator +(ContextMenu a, ContextMenu b)
    {
        return new ContextMenu(a.Pivot).AddRange(a.Menus).AddRange(b.Menus);
    }
}
