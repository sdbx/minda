using System;
using System.Linq;

namespace Abalone.Data
{
    public class GameData
    {
        public string name { get; internal set; }
        public int boardSide { get; internal set; }
        public string[] players { get; internal set; }
        public int[,] placement { get; internal set; }

        public int GetAt(AxialCoord position)
        {
            return placement[position.x, position.z];
        }

        public void SetAt(AxialCoord position, int playerIndex)
        {
            placement[position.x, position.z] = playerIndex;
        }
    }
}