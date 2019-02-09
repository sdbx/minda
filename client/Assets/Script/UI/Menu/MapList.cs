using UI;
using UnityEngine;
using UnityEngine.UI;

public class MapList : MonoBehaviour
{
    [SerializeField]
    private MapObject prefab;
    [SerializeField]
    private Transform content;

    [SerializeField]
    private Button browseBtn;

    private void Awake()
    {
        browseBtn.onClick.AddListener(OnBrowseBtnClicked);
    }

    private void OnBrowseBtnClicked()
    {
        
    }

}