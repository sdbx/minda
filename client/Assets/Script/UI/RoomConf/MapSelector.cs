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
            AddMapElement("Standard", Board.GetMapFromString(Consts.STANDARD_MAP));
            AddMapElement("Snake", Board.GetMapFromString(Consts.SNAKE_MAP));
            AddMapElement("Alien", Board.GetMapFromString(Consts.ALIEN_MAP));
            AddMapElement("Domination", Board.GetMapFromString(Consts.DOMINATION_MAP));
            AddMapElement("Alliance", Board.GetMapFromString(Consts.ALLIANCE_MAP));
            AddMapElement("Atomouche", Board.GetMapFromString(Consts.ATOMOUCHE_MAP));
            AddMapElement("Centrifuguse", Board.GetMapFromString(Consts.CENTRIFUGUSE_MAP));
            AddMapElement("Wall", Board.GetMapFromString(Consts.WALL_MAP));
            AddMapElement("Duel", Board.GetMapFromString(Consts.DUEL_MAP));
            AddMapElement("Fujiyama", Board.GetMapFromString(Consts.FUJIYAMA_MAP));
        }

        public override MapObject AddMapElement(string name, int[,] map)
        {
            var mapObject = base.AddMapElement(name, map);
            loadButton.transform.SetAsLastSibling();
            return mapObject;
        }

        private MapObject CreateBasicMapObject()
        {
            return AddMapElement("Basic", Board.GetMapFromString(Consts.BASIC_MAP));
        }

        public void SetSelectedMap()
        {
            var map = base.SelectedMapObject.map;
            mainPreview.SetMap(map);
            GameServer.Instance.ChangeMapTo(Board.GetStringFromMap(map));
        }

        public override void Select(MapObject mapObject)
        {
            base.Select(mapObject);
            windowPreview.SetMap(mapObject.map);
        }

    }
}
