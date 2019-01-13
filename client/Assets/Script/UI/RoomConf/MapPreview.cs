using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Boards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MapPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private float maxScale = 1.1f;
        [SerializeField]
        private float speed = 0.01f;
        private bool isMouseEnter;

        [SerializeField]
        private Transform content;
        [SerializeField]
        private MapListToggler mapList;
        [SerializeField]
        public MapObject mapObjectPrefab;
        private List<MapObject> maps = new List<MapObject>();

        [SerializeField]
        private float ballSize = 0.1f;
        [SerializeField]
        private float holeDistance = 0.1f;
        [SerializeField]
        private GameObject black;
        [SerializeField]
        private GameObject white;
        private Board board;
        private GameObject[,] balls;

        private MapObject selectedMap;


        void Start()
        {
            board = new Board(5);
            //test

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
            mapObject.mapPreview = this;
            maps.Add(mapObject);
        }

        void Update()
        {
            if (isMouseEnter && gameObject.transform.localScale.x < maxScale)
            {
                var transform = gameObject.transform;
                float value = transform.localScale.x + speed;
                transform.localScale = new Vector3(value, value, 1);
            }
            else if (!isMouseEnter && gameObject.transform.localScale.x > 1)
            {
                var transform = gameObject.transform;
                float value = transform.localScale.x - speed;
                transform.localScale = new Vector3(value, value, 1);
            }
        }

        public void SelectMapInList(MapObject map)
        {
            if (selectedMap != null)
                selectedMap.UnSelect();
            selectedMap = map;
        }

        private void AddMapElement(string name, int[,] map)
        {
            var mapObject = Instantiate(mapObjectPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), content);
            mapObject.mapName = name;
            mapObject.map = map;
            mapObject.mapPreview = this;
            maps.Add(mapObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseEnter = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseEnter = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            mapList.ToggleActivation();
        }

        public void SetMap(int[,] map)
        {
            if (balls != null)
            {
                foreach (var ball in balls)
                {
                    if (ball != null)
                        Destroy(ball);
                }
                balls = null;
            }
            board.SetMap(map);
            balls = BallCreator.CreatePreviewBalls(ballSize, transform, holeDistance, board, black, white);
        }

    }

}
