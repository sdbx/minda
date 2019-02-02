using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Boards;
using Models;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MapSelector : MonoBehaviour
    {
        [SerializeField]
        private MapPreview MapPreviewInMain;
        [SerializeField]
        private MapPreview mapPreviewInWindow;
        [SerializeField]
        ScrollRect scrollRect;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private MapSelectorToggler mapSelectorToggler;
        [SerializeField]
        public MapObject mapObjectPrefab;
        private List<MapObject> maps = new List<MapObject>();

        private MapObject selectedMap;
        private MapObject LoadBtn;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            gameObject.SetActive(false);
            LobbyServerAPI.GetMyMaps((Map[] maps, int? err)=>
            {
                if(err!=null)
                {
                    return;
                }
                AddMapElements(maps);
            });
            
            LoadBtn = AddMapElement("Load..", null);
            LoadBtn.isDirectorySelectBtn = true;

            var basic = AddMapElement("Basic", Board.GetMapFromString(
                "0@0@0@0@0@0@0@1@1#0@0@0@0@0@0@0@1@1#0@0@0@0@0@0@1@1@1#0@2@0@0@0@0@1@1@1#2@2@2@0@0@0@1@1@1#2@2@2@0@0@0@0@1@0#2@2@2@0@0@0@0@0@0#2@2@0@0@0@0@0@0@0#2@2@0@0@0@0@0@0@0"));
            basic.Select();
        }

        public void SelectMapInList(MapObject mapObject)
        {
            if (selectedMap != mapObject)
            {
                if (selectedMap != null)
                    selectedMap.UnSelect();
                Debug.Log($"[Map Selected] :{mapObject.mapName}");
                selectedMap = mapObject;
                mapPreviewInWindow.SetMap(mapObject.map);
            }
        }

        public MapObject AddMapElement(string name, int[,] map)
        {
            var mapObject = Instantiate(mapObjectPrefab, content.position, Quaternion.Euler(0, 0, 0), content);
            mapObject.mapName = name;
            mapObject.map = map;
            mapObject.mapSelector = this;
            maps.Add(mapObject);
            if(LoadBtn!=null)
                LoadBtn.transform.SetAsLastSibling();
            mapObject.transform.localPosition = Vector3.zero;
            return mapObject;
        }

        public MapObject AddMapElement(Map map)
        {
            var mapObject = Instantiate(mapObjectPrefab, content.position , Quaternion.Euler(0, 0, 0), content);
            mapObject.mapName = map.name;
            mapObject.map = Board.GetMapFromString(map.payload);
            mapObject.mapSelector = this;
            maps.Add(mapObject);
            if(LoadBtn!=null)
                LoadBtn.transform.SetAsLastSibling();
            mapObject.transform.localPosition = Vector3.zero;
            return mapObject;
        }

        public void AddMapElements(Map[] maps)
        {
            if(maps==null)
                return;
            foreach (var map in maps)
            {
                AddMapElement(map);
            }
        }

        public void ScorllBottom()
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }

        public void SetSelectedMap()
        {  
            MapPreviewInMain.SetMap(selectedMap.map);
            GameServer.instance.ChangeMapTo(Board.GetStringFromMap(selectedMap.map));
        }
    }

}
