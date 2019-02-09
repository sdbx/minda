using UI;
using UnityEngine;
using UnityEngine.UI;

public class MapUploadSystem : MonoBehaviour
{
    [SerializeField]
    private MapObject prefab;
    [SerializeField]
    private Transform content;

    [SerializeField]
    private Button browseBtn;
    [SerializeField]
    private Button uploadBtn;

    private void Awake()
    {
        browseBtn.onClick.AddListener(OnBrowseBtnClicked);
    }

    private void OnBrowseBtnClicked()
    {
        
    }

}