using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    static public bool CheckCircleAndPointCollision(Vector2 p1, Vector3 p2, float r)
    {
        return Mathf.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y)) <= r;
    }
    static public int GetDistanceBy2CubeCoord(CubeCoord p1, CubeCoord p2)
    {
        return (Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) + Mathf.Abs(p1.z - p2.z)) / 2;
    }
    static public CubeCoord GetMiddleCubeCoordBy2CubeCoord(CubeCoord first, CubeCoord last)
    {
        return new CubeCoord((first.x + last.x) / 2, (first.y + last.y) / 2);
    }
    static public int GetNumberDirection(CubeCoord direction)
    {
        if(direction.x == 1 && direction.y == -1)
        {
            return 0;
        }
        else if(direction.x == 1 && direction.y == 0)
        {
            return 1;
        }
        else if(direction.x == 0 && direction.y == 1)
        {
            return 2;
        }
        else if(direction.x == -1 && direction.y == 1)
        {
            return 3;
        }
        else if(direction.x == -1 && direction.y == 0)
        {
            return 4;
        }
        else if(direction.x == 0 && direction.y == -1)
        {
            return 5;
        }
        else
        {
            return -1;
        }
    }
}
