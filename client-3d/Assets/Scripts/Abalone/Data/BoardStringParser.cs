using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Abalone.Data
{
    public class BoardStringParser
    {
        private static Regex playerMarbleNotation = new Regex("([0-9]+)([a-zA-Z][0-9]+)");

        // TODO : handle invalid string
        public static GameData Parse(string boardString)
        {
            var gameData = new GameData();

            var lines = boardString.Split('\n');
            var arraySize = 0;

            foreach (var line in lines)
            {
                if (line.Contains("="))
                {
                    // metadata
                    var splitted = line.Split('=');
                    var left = splitted[0].Trim().ToLower();
                    var right = splitted[1].Trim();

                    switch (left)
                    {
                        case "name":
                            gameData.name = right;
                            break;
                        case "size":
                            var side = int.Parse(right);
                            arraySize = side * 2 - 1;
                            gameData.boardSide = side;
                            gameData.placement = new int[arraySize, arraySize];
                            break;
                        case "players":
                            gameData.players = right.Split(',');
                            break;
                    }
                }
                else
                {
                    // marble placement
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var splitted = line.Split(' ');
                    foreach (var marble in splitted)
                    {
                        var trimmed = marble.Trim();
                        var match = playerMarbleNotation.Match(trimmed);
                        var player = int.Parse(match.Groups[1].Value);
                        var position = AxialCoord.FromNotation(match.Groups[2].Value, arraySize);
                        Debug.Log(position);
                        gameData.SetAt(position, player);
                    }
                }
            }

            return gameData;
        }
    }
}