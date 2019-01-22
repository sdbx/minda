using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ContextMenuElement : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Button button;

    private void Awake() 
    {
        button = gameObject.GetComponent<Button>();
    }

    public void ResetMenu(ContextMenu.Menu menu)
    {
        nameText.text = menu.name;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(menu.action);
    }
}