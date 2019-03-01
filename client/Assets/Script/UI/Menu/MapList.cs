using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Boards;
using Models;
using SFB;
using UI;
using UI.Toast;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapList : MonoBehaviour
{
    [SerializeField]
    private MapObject prefab;
    [SerializeField]
    private Transform content;
    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    protected Button LoadButton;

    protected MapObject selectedMapObject;
    protected List<MapObject> maps = new List<MapObject>();

    protected virtual void Awake()
    {
        LoadButton.onClick.AddListener(OnLoadButtonClicked);
    }

    protected void OnLoadButtonClicked()
    {
        OpenFileDirectory();
    }

    public virtual MapObject AddMapElement(string name, int[,] map)
    {
        var mapObject = Instantiate(prefab, content);
        mapObject.Init(name,map,Select);
        maps.Add(mapObject);

        //LoadButton.transform.SetAsLastSibling();

        return mapObject;
    }

    public MapObject AddMapElement(Map map)
    {
        return AddMapElement(map.name, Board.GetMapFromString(map.payload));
    }

    public void AddMapElements(Map[] maps)
    {
        if (maps == null)
            return;
        foreach (var map in maps)
        {
            AddMapElement(map);
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
                    var newMapObject = AddMapElement(Path.GetFileName(url), Game.Boards.Board.GetMapFromString(map));
                    Select(newMapObject);
                    ScorllBottom();
                }
                catch (Exception e)
                {
                    ToastManager.instance.Add(LanguageManager.GetText("maploaderror"), "Error");
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

    public void ScorllBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    public virtual void Select(MapObject mapObject)
    {
        if(selectedMapObject == mapObject)
        {
            return;
        }

        if (selectedMapObject != null)
        {
            selectedMapObject.UnSelect();
        }

        selectedMapObject = mapObject;
        mapObject.Select();
    }

}