using System;
using UnityEngine;

public struct CubeCoord
{
    public int x;
    public int y;
    public int z;

    public CubeCoord(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public int Distance(CubeCoord other)
    {
        return (Math.Abs(x - other.x) + Math.Abs(y - other.y) + Math.Abs(z - other.z)) / 2;
    }

    public Vector3 ToWorld()
    {
        return ((AxialCoord)this).ToWorld();
    }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }

    public static implicit operator CubeCoord(AxialCoord axial)
    {
        return new CubeCoord(axial.x, -axial.x - axial.z, axial.z);
    }

    public static CubeCoord operator +(CubeCoord a, CubeCoord b)
    {
        return new CubeCoord(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static CubeCoord operator *(CubeCoord a, int b)
    {
        return new CubeCoord(a.x * b, a.y * b, a.z * b);
    }

    public static CubeCoord operator -(CubeCoord a, CubeCoord b)
    {
        return new CubeCoord(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static bool operator ==(CubeCoord a, CubeCoord b)
    {
        return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
    }

    public static bool operator !=(CubeCoord a, CubeCoord b)
    {
        return (a.x != b.x) || (a.y != b.y) || (a.z != b.z);
    }
}