using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole 
{
	public enum Ball
	{
		Empty,
		Black,
		White,
	}

    private CubeCoord _cubeCoord;

	private Ball _ball = Ball.Empty;

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
        return new Vector2(_cubeCoord.x * distance + _cubeCoord.y * distance / 2, -_cubeCoord.y * Mathf.Sqrt(3) * distance / 2);
    }

    public Ball GetBall()
    {
        return _ball;
    }

    public void SetBall(Ball ball)
	{
		_ball = ball;
	}
}
