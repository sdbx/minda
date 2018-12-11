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
    private BallType _myBallType;

    void Start()
    {

    }

    public void CreateBoard(BallType myBallType)
    {
        _board = new Board(boardSide);
        _myBallType = myBallType;
        BoardCreator.CreateBoard(_board, boardObject, holePrefab, boardBottomPrefab, boardCenter, holeDistance);
    }

    public Board GetBoard()
    {
        return _board;
    }

    public BallType GetMyBallType()
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
                    _board.Set(x-s, y-s, (BallType)type);
                }

            }
        }

    }
    
	public bool CheckBallObjectIsMine(GameObject ballObject)
    {
        return ballObject.GetComponent<Ball>().GetBall() == GetMyBallType();
    }
}
