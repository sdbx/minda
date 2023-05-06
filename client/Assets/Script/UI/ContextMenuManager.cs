using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuManager : MonoBehaviour
{
    [SerializeField]
    private ContextMenuElement prefab;
    private Vector2 _prefabSize;
    private RectTransform _rectTransform;

    private List<ContextMenuElement> _contextMenuElements = new List<ContextMenuElement>();
    public static ContextMenuManager Instance;

    private bool _isSizeUpdated;

    private void Awake()
    {
        //singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        _prefabSize = prefab.GetComponent<RectTransform>().rect.size;
        _rectTransform = gameObject.GetComponent<RectTransform>();
    }

    public void Create(Vector2 pos, ContextMenu contextMenu)
    {
        gameObject.SetActive(true);
        transform.position = new Vector3(0, 0, -10);
        _rectTransform.pivot = contextMenu.Pivot;

        var menus = contextMenu.Menus;

        for (var i = 0; i < Mathf.Max(_contextMenuElements.Count, menus.Count); i++)
        {
            ContextMenuElement current;
            //이미 만들어진 메뉴가 더 많을 떄 비활성화
            if (menus.Count <= i)
            {
                _contextMenuElements[i].gameObject.SetActive(false);
                continue;
            }
            //이미 만들어진 메뉴가 충분할 떄 사용
            else if (_contextMenuElements.Count > i)
            {
                current = _contextMenuElements[i];
                current.gameObject.SetActive(true);
                Debug.Log(i);
            }
            //만들어진 메뉴가 부족하면 생성
            else //if(contextMenuElements.Count <= i)
            {
                current = Instantiate<ContextMenuElement>(prefab, _rectTransform);
                _contextMenuElements.Add(current);
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

        foreach (var element in _contextMenuElements)
        {
            if (element.isActiveAndEnabled)
            {
                element.sizeMatcher.ForceSet(new Vector2(_rectTransform.rect.width, -1));
            }
        }
        _rectTransform.localPosition = new Vector3(pos.x, pos.y, 0);
    }

    private void Update()
    {
        HideIfClickedOutsideOrWheel(_rectTransform.gameObject);
    }

    private void HideIfClickedOutsideOrWheel(GameObject panel)
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0 ||
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
