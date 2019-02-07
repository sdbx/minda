using System.Text.RegularExpressions;
using UnityEngine;

public struct AxialCoord
{
    private static Regex marbleNotation = new Regex("([a-zA-Z])([0-9]+)");

    public int x;
    public int z;

    public AxialCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public Vector3 ToWorld()
    {
        var worldX = Constants.gridScale * (3f / 2 * z);
        var worldZ = Constants.gridScale * (Mathf.Sqrt(3) / 2 * z + Mathf.Sqrt(3) * x);
        return new Vector3(worldX, 0, worldZ);
    }

    public override string ToString()
    {
        return $"({x}, {z})";
    }

    public static AxialCoord FromNotation(string notation, int arraySize)
    {
        var match = marbleNotation.Match(notation);
        // Handles one alphabet character
        var x = arraySize - (match.Groups[1].Value.ToLower()[0] - 'a') - 1;
        var z = int.Parse(match.Groups[2].Value) - 1;
        return new AxialCoord(x, z);
    }

    public static implicit operator AxialCoord(CubeCoord cube)
    {
        return new AxialCoord(cube.x, cube.z);
    }

    public static AxialCoord operator +(AxialCoord a, AxialCoord b)
    {
        return new AxialCoord(a.x + b.x, a.z + b.z);
    }

    public static AxialCoord operator -(AxialCoord a, AxialCoord b)
    {
        return new AxialCoord(a.x - b.x, a.z - b.z);
    }

    public static AxialCoord operator *(AxialCoord a, int b)
    {
        return new AxialCoord(a.x * b, a.z * b);
    }

    public static bool operator ==(AxialCoord a, AxialCoord b)
    {
        return (a.x == b.x) && (a.z == b.z);
    }

    public static bool operator !=(AxialCoord a, AxialCoord b)
    {
        return (a.x != b.x) || (a.z != b.z);
    }
}
