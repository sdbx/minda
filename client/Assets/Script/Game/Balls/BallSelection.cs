using System.Collections;
using System.Collections.Generic;
using Game.Coords;
using UnityEngine;

namespace Game.Balls
{
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

        public Vector2 GetMiddlePoint(float distance)
        {
            Vector2 firstPosition = first.GetPixelPoint(distance);
            Vector2 endPosition = end.GetPixelPoint(distance);
            return new Vector2((firstPosition.x + endPosition.x) / 2, (firstPosition.y + endPosition.y) / 2);
        }

        public CubeCoord GetMiddleBall()
        {
            if (count <= 2)
            {
                return null;
            }

            return CubeCoord.GetCenter(first, end);
        }

        public BallSelection Move(CubeCoord direction)
        {
            return new BallSelection(first + direction, end + direction);
        }

        public CubeCoord GetStartPoint(int dirNum)
        {
            CubeCoord startPoint;

            if ((first + CubeCoord.ConvertNumToDirection(dirNum) * (count - 1)).isSame(end))
            {
                startPoint = end;
            }
            else
            {
                startPoint = first;
            }

            return startPoint;
        }

        public CubeCoord GetEndPoint(int dirNum)
        {
            CubeCoord endPoint;

            if ((first + CubeCoord.ConvertNumToDirection(dirNum) * (count - 1)).isSame(end))
            {
                endPoint = first;
            }
            else
            {
                endPoint = end;
            }

            return endPoint;
        }

    }
}