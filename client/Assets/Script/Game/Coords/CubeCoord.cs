using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Coords
{
    [Serializable]
    public class CubeCoord
    {
        public int x = 0;
        public int y = 0;
        public int z = 0;


        static private CubeCoord[] _directions
        {
            get
            {
                return new CubeCoord[]{
                new CubeCoord(1,-1),
                new CubeCoord(1,0),
                new CubeCoord(0,1),
                new CubeCoord(-1,1),
                new CubeCoord(-1,-0),
                new CubeCoord(0,-1)};
            }
        }

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
            return (x == a.x) && (y == a.y) && (z == a.z);
        }

        public Vector2 GetPixelPoint(float distance)
        {
            return new Vector2(x * distance + y * distance / 2, -y * Mathf.Sqrt(3) * distance / 2);
        }

        static public CubeCoord GetCenter(CubeCoord first, CubeCoord last)
        {
            return new CubeCoord((first.x + last.x) / 2, (first.y + last.y) / 2);
        }

        static public int ConvertDirectionToNum(CubeCoord direction)
        {
            for(int i = 0;i<6;i++)
            {
                if(direction.isSame(_directions[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        static public CubeCoord ConvertNumToDirection(int num)
        {
            return _directions[num];
        }

        public override string ToString ()
        {
            return $"{x} {y} {z}";
        }

    }
}