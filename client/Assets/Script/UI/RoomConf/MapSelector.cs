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
            LoadMyMaps();
            selectButton.onClick.AddListener(SetSelectedMap);
        }

        private void Start()
        {
            var basic = CreateBasicMapObject();
            Select(basic);
        }

        private void LoadMyMaps()
        {
            LobbyServerAPI.GetMyMaps((Map[] maps, int? err) =>
            {
                if (err != null)
                {
                    return;
                }
                AddMapElements(maps);
            });
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
