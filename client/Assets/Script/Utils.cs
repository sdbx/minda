using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    static public bool CheckCircleAndPointCollision(Vector2 p1, Vector3 p2, float r)
	{
		return Mathf.Sqrt((p1.x-p2.x)*(p1.x-p2.x)+(p1.y-p2.y)*(p1.y-p2.y))<= r;
	}
	static public int GetDistanceBy2CubeCoord(CubeCoord p1,CubeCoord p2)
	{
		return (Mathf.Abs(p1.x-p2.x)+Mathf.Abs(p1.y-p2.y)+Mathf.Abs(p1.z-p2.z))/2;
	}
	static public CubeCoord GetMiddleCubeCoordBy2CubeCoord(CubeCoord first,CubeCoord last)
	{
		return null;
	}
}
