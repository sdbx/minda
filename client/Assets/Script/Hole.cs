using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole
{


    private CubeCoord _cubeCoord;

    private BallType _ball = BallType.Empty;

    public Hole(CubeCoord cubeCoord)
    {
        _cubeCoord = cubeCoord;
    }

    public CubeCoord GetCubeCoord()
    {
        return _cubeCoord;
    }
    public Vector2 GetPixelPoint(float distance)
    {
        return _cubeCoord.GetPixelPoint(distance);
    }

    public BallType GetBall()
    {
        return _ball;
    }

    public void SetBall(BallType ball)
    {
        _ball = ball;
    }
}
