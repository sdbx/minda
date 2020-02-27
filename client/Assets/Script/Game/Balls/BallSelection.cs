using System.Collections;
using System.Collections.Generic;
using Game.Coords;
using UnityEngine;

namespace Game.Balls
{
    public class BallSelection
    {
        public CubeCoord First, End;
        public int Count = 0;

        public BallSelection(CubeCoord first, CubeCoord end)
        {
            this.First = first;
            this.End = end;
            Count = Utils.GetDistanceBy2CubeCoord(first, end) + 1;
        }

        public Vector2 GetMiddlePoint(float distance)
        {
            var firstPosition = First.GetPixelPoint(distance);
            var endPosition = End.GetPixelPoint(distance);
            return new Vector2((firstPosition.x + endPosition.x) / 2, (firstPosition.y + endPosition.y) / 2);
        }

        public CubeCoord GetMiddleBallCubeCoord()
        {
            if (Count <= 2)
            {
                return null;
            }

            return CubeCoord.GetCenter(First, End);
        }

        public BallSelection Move(CubeCoord direction)
        {
            return new BallSelection(First + direction, End + direction);
        }

        public CubeCoord GetStartPoint(int dirNum)
        {
            CubeCoord startPoint;

            if ((First + CubeCoord.ConvertNumToDirection(dirNum) * (Count - 1)).IsSame(End))
            {
                startPoint = End;
            }
            else
            {
                startPoint = First;
            }

            return startPoint;
        }

        public CubeCoord GetEndPoint(int dirNum)
        {
            CubeCoord endPoint;

            if ((First + CubeCoord.ConvertNumToDirection(dirNum) * (Count - 1)).IsSame(End))
            {
                endPoint = First;
            }
            else
            {
                endPoint = End;
            }

            return endPoint;
        }

        public List<CubeCoord> GetCubeCoords()
        {
            var coords = new List<CubeCoord>();
            coords.Add(First);
            if (Count > 1)
            {
                coords.Add(End);
            }
            if (Count == 3)
            {
                coords.Add(GetMiddleBallCubeCoord());
            }
            return coords;
        }

    }
}
