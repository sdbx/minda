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
        private string mapName;
        public int[,] map{private set;get;}
        private Action<MapObject> clickedCallback;

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
            clickedCallback(this);
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
            mapName = name;
            this.map = map;
            nameText.text = name;
            
            this.clickedCallback = clickedCallback;
        }











    }
}