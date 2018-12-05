using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSelection
{
    public CubeCoord first, end;
    public int count = 0;

    public BallSelection(CubeCoord first, CubeCoord end)
    {
        this.first = first;
        this.end = end;
        count = Utils.GetDistanceBy2CubeCoord(first, end) + 1;
    }

    public BallSelection Move(CubeCoord direction)
    {
        return new BallSelection(first + direction, end + direction);
    }


}