using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuCreater : MonoBehaviour
{
    [SerializeField]
    private ContextMenuElement prefab;
    [SerializeField]
    private Transform parent;

    private List<ContextMenuElement> contextMenuElements;
    public static ContextMenuCreater instance;

    void Awake()
    {
        //singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Create(Vector2 pos, ContextMenu contextMenu)
    {
        var menus = contextMenu.menus;

        for (int i = 0; i < Mathf.Max(contextMenuElements.Count, menus.Count); i++)
        {
            ContextMenuElement current;

            if(contextMenuElements.Count > i)
            {
                current = contextMenuElements[i];
                current.gameObject.SetActive(true);
            }
            else if(contextMenuElements.Count <= i)
            {
                current = Instantiate<ContextMenuElement>(prefab,parent);
            }
            else
            {
                contextMenuElements[i].gameObject.SetActive(false);
                continue;
            }

            current.ResetMenu(menus[i]);
        }
    }
}
