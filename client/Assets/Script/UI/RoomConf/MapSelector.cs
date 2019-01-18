using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Boards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MapSelector : MonoBehaviour
    {
        [SerializeField]
        private MapPreview mapPreview;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private MapSelectorToggler mapSelectorToggler;
        [SerializeField]
        public MapObject mapObjectPrefab;
        private List<MapObject> maps = new List<MapObject>();

        private MapObject selectedMap;


        void Start()
        {
            AddMapElement("test map1", Board.GetMapFromString(
                "0@0@0@0@0@0@0@0@1#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@2#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));
            AddMapElement("test map1", Board.GetMapFromString(
                "0@0@0@0@0@0@0@0@1#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@1#2@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));
            AddMapElement("test map1", Board.GetMapFromString(
            "0@0@0@0@0@0@0@0@1#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@2@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));
            AddMapElement("test map1", Board.GetMapFromString(
            "0@0@0@0@0@0@0@0@1#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));
            AddMapElement("test map1", Board.GetMapFromString(
            "0@0@0@0@0@0@0@0@1#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));
            AddMapElement("test map1", Board.GetMapFromString(
            "0@0@0@0@0@0@0@0@1#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@2@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));
            AddMapElement("test map1", Board.GetMapFromString(
            "0@0@0@0@0@0@0@0@1#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));
            AddMapElement("test map1", Board.GetMapFromString(
            "0@0@0@0@0@0@0@0@1#0@0@0@0@0@0@0@1@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));
            AddMapElement("test map1", Board.GetMapFromString(
            "0@0@0@0@0@0@0@0@1#1@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0#0@0@0@0@0@0@0@0@0"));


            var mapObject = Instantiate(mapObjectPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), content);
            mapObject.isDirectorySelectBtn = true;
            mapObject.mapName = "Load..";
            mapObject.mapSelector = this;
            maps.Add(mapObject);
        }

        public void SelectMapInList(MapObject mapObject)
        {
            if (selectedMap != null && selectedMap != mapObject)
            {
                Debug.Log($"[Map Selected] :{mapObject.mapName}");
                selectedMap = mapObject;
                mapPreview.SetMap(mapObject.map);
            }
        }

        public MapObject AddMapElement(string name, int[,] map)
        {
            var mapObject = Instantiate(mapObjectPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), content);
            mapObject.mapName = name;
            mapObject.map = map;
            mapObject.mapSelector = this;
            maps.Add(mapObject);
            return mapObject;
        }
    }

}
