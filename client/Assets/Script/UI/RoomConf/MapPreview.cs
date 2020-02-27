using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Boards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MapPreview : MonoBehaviour
    {
        [SerializeField]
        private Transform parent;
        [SerializeField]
        private float ballSize = 0.1f;
        [SerializeField]
        private float holeDistance = 0.1f;
        [SerializeField]
        private GameObject black;
        [SerializeField]
        private GameObject white;
        private Board _board;
        private GameObject[,] _balls;

        private MapObject _selectedMap;


        private void Awake()
        {
            _board = new Board(5);
        }

        public void SetMap(int[,] map)
        {
            if (_balls != null)
            {
                foreach (var ball in _balls)
                {
                    if (ball != null)
                        Destroy(ball);
                }
                _balls = null;
            }
            _board.SetMap(map);
            _balls = BallCreator.CreatePreviewBalls(ballSize, parent, holeDistance, _board, black, white);
        }

    }

}
