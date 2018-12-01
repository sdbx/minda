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

    private int _x, _y, _z;

	private Ball _ball = Ball.Empty;

	public Hole(int x, int y)
	{
		_x = x;
		_y = y;
		_z = -(x+y);
	}

	public Vector2 GetPoint()
	{
		return new Vector3(_x,_y,_z);
	}
	public Vector2 GetPixelPoint(float distance)
	{
		return new Vector2(_x*distance+_y*distance/2,-_y*Mathf.Sqrt(3)*distance/2);
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
