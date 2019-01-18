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
