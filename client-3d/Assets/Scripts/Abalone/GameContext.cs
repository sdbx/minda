using Abalone.Data;
using UnityEngine;

namespace Abalone
{
    // Holds match's information and utility functions
    public class GameContext
    {
        public readonly Board board;
        public int currentPlayerIndex { get; private set; } = 1;
        public string playerContext = "Choose";
        public int playerCount = 2;
        public GameObject[,] marbles;
        public int[] fallenMarbles;
        public bool CanPaintOver = true;

        // TODO : exterminated marble numbers
        public GameContext(GameData gameData)
        {
        }

        public void NextTurn()
        {
            currentPlayerIndex++;

            if (currentPlayerIndex > playerCount)
            {
                currentPlayerIndex = 1;
            }
        }

        public void MoveData(AxialCoord before, AxialCoord after)
        {
            marbles[after.x, after.z] = marbles[before.x, before.z];
            marbles[before.x, before.z] = null;
        }
    }
}