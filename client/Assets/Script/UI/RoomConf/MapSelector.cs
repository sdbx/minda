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
    public class MapSelector : MapList
    {
        [SerializeField]
        private MapPreview mainPreview;
        [SerializeField]
        private MapPreview windowPreview;
        [SerializeField]
        private Button selectButton;

        protected override void Awake()
        {
            base.Awake();
            selectButton.onClick.AddListener(SetSelectedMap);
        }

        private void Start()
        {
            var basic = CreateBasicMapObject();
            Select(basic);
            LoadMyMaps();
        }

        private void LoadMyMaps()
        {
            // LobbyServerAPI.GetMyMaps((Map[] maps, int? err) =>
            // {
            //     if (err != null)
            //     {
            //         return;
            //     }
            //     AddMapElements(maps);
            // });
            AddMapElement("Standard", Board.GetMapFromString(Consts.standardMap));
            AddMapElement("Snake", Board.GetMapFromString(Consts.snakeMap));
            AddMapElement("Alien", Board.GetMapFromString(Consts.alienMap));
            AddMapElement("Domination", Board.GetMapFromString(Consts.dominationMap));
            AddMapElement("Alliance", Board.GetMapFromString(Consts.allianceMap));
            AddMapElement("Atomouche", Board.GetMapFromString(Consts.atomoucheMap));
            AddMapElement("Centrifuguse", Board.GetMapFromString(Consts.centrifuguseMap));
            AddMapElement("Wall", Board.GetMapFromString(Consts.wallMap));
            AddMapElement("Duel", Board.GetMapFromString(Consts.duelMap));
            AddMapElement("Fujiyama", Board.GetMapFromString(Consts.fujiyamaMap));
        }

        public override MapObject AddMapElement(string name, int[,] map)
        {
            var mapObject = base.AddMapElement(name, map);
            LoadButton.transform.SetAsLastSibling();
            return mapObject;
        }

        private MapObject CreateBasicMapObject()
        {
            return AddMapElement("Basic", Board.GetMapFromString(Consts.basicMap));
        }

        public void SetSelectedMap()
        {
            var map  = base.selectedMapObject.map;
            mainPreview.SetMap(map);
            GameServer.instance.ChangeMapTo(Board.GetStringFromMap(map));
        }

        public override void Select(MapObject mapObject)
        {
            base.Select(mapObject);
            windowPreview.SetMap(mapObject.map);
        }
        
    }
}
