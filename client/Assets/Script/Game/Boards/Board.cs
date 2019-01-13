using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Coords;
using UnityEngine;

namespace Game.Boards
{
    public class Board
    {
        private int _side;
        private Hole[,] _holes;

        public Board(int side)
        {
            _side = side;

            int s = side - 1;

            _holes = new Hole[side + s, side + s];

            for (int x = -s; x <= s; x++)
            {
                for (int y = -s; y <= s; y++)
                {
                    if (Mathf.Abs(x + y) <= s)
                        _holes[x + s, y + s] = new Hole(new CubeCoord(x, y));
                }
            }
        }


        public void SetMap(int[,] map)
        {
            int s = _side - 1;

            for (int y = 0; y <= 2 * s; y++)
            {
                for (int x = 0; x <= 2 * s; x++)
                {
                    int type = map[x, y];
                    if (type != 0)
                    {
                        Set(x - s, y - s, (HoleState)type);
                    }
                    else Set(x - s, y - s, HoleState.Empty);
                }
            }

        }

        static public int[,] GetMapFromString(string mapStr)
        {
            string[] firstArray = mapStr.Split('#');
            int[,] map = new int[firstArray.Length, firstArray.Length];
            for (int x = 0; x < firstArray.Length; x++)
            {
                string[] secondArray = firstArray[x].Split('@');
                for (int y = 0; y < firstArray.Length; y++)
                {
                    int parsedInt;
                    if (!int.TryParse(secondArray[y], out parsedInt))
                    {
                        return null;
                    }
                    map[x, y] = parsedInt;
                }
            }
            return map;
        }

        public Hole[,] GetHoles()
        {
            return _holes;
        }

        public Hole GetHole(int x, int y)
        {
            return _holes[x + _side - 1, y + _side - 1];
        }

        public Hole GetHoleByCubeCoord(CubeCoord cubeCoord)
        {
            return _holes[cubeCoord.x + _side - 1, cubeCoord.y + _side - 1];
        }

        public void SetHoleByCubeCoord(CubeCoord cubeCoord, BallType ball)
        {
            _holes[cubeCoord.x + _side - 1, cubeCoord.y + _side - 1].SetHoleState((HoleState)ball);
        }

        public void SetHoleByCubeCoord(CubeCoord cubeCoord, HoleState ball)
        {
            _holes[cubeCoord.x + _side - 1, cubeCoord.y + _side - 1].SetHoleState(ball);
        }

        public HoleState GetHoleStateByCubeCoord(CubeCoord cubeCoord)
        {
            return GetHoleByCubeCoord(cubeCoord).GetHoleState();
        }

        public int GetSide()
        {
            return _side;
        }

        public void Set(int x, int y, BallType ball)
        {
            _holes[x + (_side - 1), y + (_side - 1)].SetHoleState((HoleState)ball);
        }

        public void Set(int x, int y, HoleState ball)
        {
            var hole = GetHole(x, y);
            if (hole != null)
                hole.SetHoleState(ball);
        }

        public bool CheckMovement(BallSelection ballSelection, int direction, BallType myBallType)
        {
            int sameLine = GetSameLine(ballSelection.first, ballSelection.end);
            CubeCoord dirCubeCoord = CubeCoord.ConvertNumToDirection(direction);
            if (sameLine == direction % 3)
            {
                //앞으로밀기
                CubeCoord startPoint = ballSelection.GetStartPoint(direction);

                for (int i = 1; i < ballSelection.count + 1; i++)
                {
                    CubeCoord currentcoord = startPoint + dirCubeCoord * i;
                    if (CheckHoleIsEmptyOrOut(currentcoord))
                    {
                        return true;
                    }
                    else if (GetHoleStateByCubeCoord(currentcoord) == (HoleState)myBallType)
                    {
                        return false;
                    }
                }
                return false;
            }
            else
            {
                //옆으로밀기
                CubeCoord currentcoord = ballSelection.first + dirCubeCoord;
                if (!CheckHoleIsEmptyOrOut(currentcoord))
                {
                    return false;
                }
                if (ballSelection.count == 3)
                {
                    currentcoord = ballSelection.GetMiddleBallCubeCoord() + dirCubeCoord;
                    if (!CheckHoleIsEmptyOrOut(currentcoord))
                        return false;
                }
                if (ballSelection.count != 1)
                {
                    currentcoord = ballSelection.end + dirCubeCoord;
                    if (!CheckHoleIsEmptyOrOut(currentcoord))
                        return false;
                }

                return true;
            }
        }

        public int GetSameLine(CubeCoord a, CubeCoord b)
        {
            if (a.x == b.x)
                return 2;
            else if (a.y == b.y)
                return 1;
            else if (a.z == b.z)
                return 0;
            else return -1;
        }
        public bool CheckHoleIsEmptyOrOut(CubeCoord coord)
        {
            return CheckOutOfBoard(coord) || GetHoleStateByCubeCoord(coord) == HoleState.Empty;
        }
        public bool CheckOutOfBoard(CubeCoord cubeCoord)
        {
            return (Mathf.Abs(cubeCoord.x) >= _side ||
            Mathf.Abs(cubeCoord.y) >= _side ||
            Mathf.Abs(cubeCoord.z) >= _side);
        }

    }
}