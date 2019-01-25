using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ContextMenuElement : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    public Button button;
    public SizeMatcher sizeMatcher;

    private void Awake() 
    {
        button = gameObject.GetComponent<Button>();
    }

    public void ResetMenu(ContextMenu.Menu menu)
    {
        nameText.text = menu.name;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(menu.action);
        button.onClick.AddListener(UnActiveParent);
    }

    private void UnActiveParent()
    {
        transform.parent.gameObject.SetActive(false);
    }
}