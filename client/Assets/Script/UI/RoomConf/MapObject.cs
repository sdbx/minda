using System;
using System.Collections;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UI
{
    public class MapObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public bool isDirectorySelectBtn;
        public string mapName;
        [SerializeField]
        private Text nameText;
        [SerializeField]
        private RawImage background;

        public MapPreview mapPreview;

        private bool isSelected;
        private bool loaded;
        [SerializeField]
        private Color focusedColor;
        [SerializeField]
        private Color selectedColor;
        private Color originColor;

        public int[,] map;

        private bool selected;

        private void Start()
        {
            originColor = background.color;
            nameText.text = mapName;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isSelected)
                background.color = focusedColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
             if (!isSelected)
                background.color = originColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDirectorySelectBtn)
            {
                mapPreview.SelectMapInList(this);
                OpenFileDirectory();
            }
            else if (isSelected)
            {
                if(loaded)
                    return;
                mapPreview.SetMap(map);
                loaded = true;
            }
            else
            {
                mapPreview.SelectMapInList(this);
                isSelected = true;
                background.color = selectedColor;
            }
        }

        public void UnSelect()
        {
            isSelected = false;
            loaded = false;
            background.color = originColor;
        }

        public void OpenFileDirectory()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Map", "", "map", false);
            if (paths.Length > 0)
            {
                StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri, (string map, string url) =>
                {
                    try
                    {
                        Debug.Log("맵 로드 중 : " + url);
                        mapPreview.SetMap(Game.Boards.Board.GetMapFromString(map));
                    }
                    catch (Exception e)
                    {
                        Debug.Log("맵 로드 오류 : " + e);
                    }
                }));
            }
        }

        private IEnumerator OutputRoutine(string url, Action<string, string> callBack)
        {
            var www = new UnityWebRequest(url);
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                callBack(www.downloadHandler.text, url);
            }
        }

    }
}