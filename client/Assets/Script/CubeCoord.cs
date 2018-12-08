using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class CubeCoord
{
    public int x = 0;
    public int y = 0;
    public int z = 0;

    [JsonConstructor]
    public CubeCoord(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public CubeCoord(int x, int y)
    {
        this.x = x;
        this.y = y;
        z = -(x + y);
    }

    public bool CheckInSameLine(CubeCoord cubeCoord)
    {
        return (x == cubeCoord.x || y == cubeCoord.y || z == cubeCoord.z);
    }
    
    public static CubeCoord operator +(CubeCoord a, CubeCoord b)
    {
        return new CubeCoord(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static CubeCoord operator *(CubeCoord a, int b)
    {
        return new CubeCoord(a.x * b, a.y * b, a.z * b);
    }
    public bool isSame(CubeCoord a)
    {
        return (x==a.x)&&(y==a.y)&&(z==a.z);
    }

    public Vector2 GetPixelPoint(float distance)
    {
        return new Vector2(x * distance + y * distance / 2, -y * Mathf.Sqrt(3) * distance / 2);
    }
}
