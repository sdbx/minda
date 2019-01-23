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

    private bool isSizeUpdated;

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
        parent.gameObject.SetActive(true);
        parent.position = new Vector3(0,0,-10);
        parent.pivot = contextMenu.pivot;

        var menus = contextMenu.menus;

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
            current.sizeMatcher.forceSetMode = false;
            current.ResetMenu(menus[i]);
        }

        
        StartCoroutine(UpdateSizeAndPosition(pos));
    }

    private IEnumerator UpdateSizeAndPosition(Vector2 pos) 
    {
        //2프레임 뒤
        yield return 0;
        yield return 0;

        foreach (var element in contextMenuElements)
        {
            if (element.isActiveAndEnabled)
            {
                element.sizeMatcher.ForceSet(new Vector2(parent.rect.width, -1));
            }
        }
        parent.localPosition = new Vector3(pos.x,pos.y,0);
    }

    private void Update() 
    {
        HideIfClickedOutsideOrWheel(parent.gameObject);
    }

    private void HideIfClickedOutsideOrWheel(GameObject panel)
    {
        if (Input.GetAxis("Mouse ScrollWheel")!=0 || 
            Input.GetMouseButton(0) && panel.activeSelf &&
            !RectTransformUtility.RectangleContainsScreenPoint(
                panel.GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main))
        {
            panel.SetActive(false);
        }
    }
    
}
