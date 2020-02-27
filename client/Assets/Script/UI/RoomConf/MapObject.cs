using System;
using System.Collections;
using System.IO;
using SFB;
using UI.Toast;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UI
{
    public class MapObject : MonoBehaviour
    {
        private string _mapName;
        public int[,] map { private set; get; }
        private Action<MapObject> _clickedCallback;

        [SerializeField]
        private Text nameText;
        [SerializeField]
        private RawImage background;
        [SerializeField]
        private DisplayChanger displayChanger;

        private void Awake()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            _clickedCallback(this);
        }

        public void Select()
        {
            displayChanger.SetMode("Selected");
        }

        public void UnSelect()
        {
            displayChanger.SetOrigin();
        }

        public void Init(string name, int[,] map, Action<MapObject> clickedCallback)
        {
            transform.localPosition = Vector3.zero;
            _mapName = name;
            this.map = map;
            nameText.text = name;

            this._clickedCallback = clickedCallback;
        }











    }
}
