using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class MapUploader : MapList
    {
        [SerializeField]
        private Button uploadButton;
        [SerializeField]
        private Button deleteButton;

        protected override void Awake()
        {
            base.Awake();
            uploadButton.onClick.AddListener(OnUploadButtonClicked);
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        private void OnUploadButtonClicked()
        {
            
        }

        private void OnDeleteButtonClicked()
        {

        }
    }
}