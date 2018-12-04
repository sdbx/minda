using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private int _side;
    private Hole[,] _holes;

    public Board(int side)
    {
        _side = side;

        int s = side - 1;

        _holes = new Hole[side + s, side + s];

        for (int x = -s; x <= s; x++)
        {
            for (int y = -s; y <= s; y++)
            {
                if (Mathf.Abs(x + y) <= s)
                    _holes[x + s, y + s] = new Hole(new CubeCoord(x, y));
            }
        }
    }

    public Hole[,] GetHoles()
    {
        return _holes;
    }

    public Hole GetHole(int x, int y)
    {
        return _holes[x + _side - 1, y + _side - 1];
    }

    public Hole GetHoleByCubeCoord(CubeCoord cubeCoord)
    {
        return _holes[cubeCoord.x + _side - 1, cubeCoord.y + _side - 1];
    }

    public int GetSide()
    {
        return _side;
    }

    public void Set(int x, int y, Hole.Ball ball)
    {
        _holes[x + (_side - 1), y + (_side - 1)].SetBall(ball);
    }
}
