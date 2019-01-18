using System;
using System.Collections;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UI
{
    public class MapObject : MonoBehaviour
    {
        public bool isDirectorySelectBtn;
        public string mapName;
        [SerializeField]
        private Text nameText;
        [SerializeField]
        private RawImage background;

        public MapSelector mapSelector;

        public int[,] map;

        private bool selected;

        private void Awake()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
            nameText.text = mapName;
        }

        public void OnClick()
        {
            if (isDirectorySelectBtn)
            {
                OpenFileDirectory();
            }
            else
            {
                mapSelector.SelectMapInList(this);
            }
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
                        var newMapObject = mapSelector.AddMapElement(Path.GetFileName(url), Game.Boards.Board.GetMapFromString(map));
                        mapSelector.SelectMapInList(newMapObject);
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