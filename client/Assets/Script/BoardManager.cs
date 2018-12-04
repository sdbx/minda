using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    public GameObject boardObject;
    public GameObject holePrefab;
    public GameObject boardBottomPrefab;
    public int boardSide = 9;
    public Vector2 boardCenter = new Vector2(0, 0);
    public float holeDistance = 5;

    private Board _board;
    private Hole.Ball _myBallType = Hole.Ball.Black;

    void Start()
    {

    }

    public void CreateBoard(Hole.Ball myBallType)
    {
        _board = new Board(boardSide);
        _myBallType = myBallType;
        BoardCreator.CreateBoard(_board, boardObject, holePrefab, boardBottomPrefab, boardCenter, holeDistance);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Board GetBoard()
    {
        return _board;
    }
    public Hole.Ball GetMyBallType()
    {
        return _myBallType;
    }
    public void SetMap(int[,] map)
    {
        int s = boardSide - 1;
        for (int x = 0; x <= 2 * s; x++)
        {
            for (int y = 0; y <= 2 * s; y++)
            {
                int type = map[x, y];
                if (type != 0)
                {
                    _board.Set(x, y, (type == 1 ? Hole.Ball.Black : Hole.Ball.White));
                }

            }
        }

    }
}
