using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using UnityEngine;

namespace Game.Boards
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField]
        private GameManager gameManager;
        public GameObject holePrefab;
        public GameObject boardBottomPrefab;
        public int boardSide = 9;
        public Vector2 boardCenter = new Vector2(0, 0);
        public float holeDistance = 5;

        private Board _board;

        void Start()
        {

        }

        public void CreateBoard()
        {
            _board = new Board(boardSide);
            BoardCreator.CreateBoard(_board, gameObject, holePrefab, boardBottomPrefab, boardCenter, holeDistance);
        }

        public Board GetBoard()
        {
            return _board;
        }

        public void SetMap(int[,] map)
        {
            _board.SetMap(map);
        }

        public int[,] GetMapFromString(string mapStr)
        {
            string[] firstArray = mapStr.Split('#');
            int[,] map = new int[firstArray.Length, firstArray.Length];
            for (int x = 0; x < firstArray.Length; x++)
            {
                string[] secondArray = firstArray[x].Split('@');
                for (int y = 0; y < firstArray.Length; y++)
                {
                    int parsedInt;
                    if (!int.TryParse(secondArray[x], out parsedInt))
                    {
                        return null;
                    }
                    map[x, y] = parsedInt;
                }
            }
            return map;
        }

        public bool CheckBallObjectIsMine(GameObject ballObject)
        {
            return ballObject.GetComponent<Ball>().GetBall() == gameManager.myBallType;
        }
    }
}