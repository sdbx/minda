using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuCreater : MonoBehaviour
{
    [SerializeField]
    private ContextMenuElement prefab;
    private Vector2 prefabSize;
    [SerializeField]
    private RectTransform parent;

    private List<ContextMenuElement> contextMenuElements = new List<ContextMenuElement>();
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
        prefabSize = prefab.GetComponent<RectTransform>().rect.size;
    }

    public void Create(Vector2 pos, ContextMenu contextMenu)
    {
        parent.pivot = contextMenu.pivot;
        var menus = contextMenu.menus;
        parent.sizeDelta = new Vector2(prefabSize.x, prefabSize.y*menus.Count);

        for (int i = 0; i < Mathf.Max(contextMenuElements.Count, menus.Count); i++)
        {
            ContextMenuElement current;
            //이미 만들어진 메뉴가 더 많을 떄 비활성화
            if(menus.Count<=i)
            {
                contextMenuElements[i].gameObject.SetActive(false);
                continue;
            }
            //이미 만들어진 메뉴가 충분할 떄 사용
            else if(contextMenuElements.Count > i)
            {
                current = contextMenuElements[i];
                current.gameObject.SetActive(true);
                Debug.Log(i);
            }
            //만들어진 메뉴가 부족하면 생성
            else //if(contextMenuElements.Count <= i)
            {
                current = Instantiate<ContextMenuElement>(prefab,parent);
                contextMenuElements.Add(current);
            }
            
            current.ResetMenu(menus[i]);
        }
        parent.position = pos;
    }
}
