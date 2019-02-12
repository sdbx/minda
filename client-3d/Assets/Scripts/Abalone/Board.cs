using System.Linq;
using Abalone.Data;
using UnityEngine;

namespace Abalone
{
    public class BoardSettings
    {
        public readonly int side;
        public readonly int arraySize;
        public readonly AxialCoord placementOffset;
        public readonly int cutThreshold;
        public GameContext context;

        public BoardSettings(int side)
        {
            this.side = side;
            arraySize = side * 2 - 1;
            cutThreshold = side - 1;
            placementOffset = new AxialCoord(-cutThreshold, -cutThreshold);
        }
    }

    public class Board : MonoBehaviour
    {
        public BoardSettings settings { get; private set; }
        public GameContext context;

        [SerializeField] private Transform marbleContainer;
        [SerializeField] private Marble marblePrefab;
        [SerializeField] private Transform boardContainer;
        [SerializeField] private GameObject boardInnerPartPrefab;
        [SerializeField] private GameObject boardOuterSidePrefab;
        [SerializeField] private GameObject boardOuterVertexPrefab;
        [SerializeField] private Material[] materials;
        [SerializeField] private int[] chosenMaterialIndex;

        public void Create(GameData data)
        {
            this.settings = new BoardSettings(data.boardSide);
            context = new GameContext(data);
            chosenMaterialIndex = new int[context.playerCount];
            //Select material
            chosenMaterialIndex[0] = 10;
            chosenMaterialIndex[1] = 13;

            context.marbles = new GameObject[settings.arraySize, settings.arraySize];
            context.fallenMarbles = new int[context.playerCount];

            for (var x = 0; x < settings.arraySize; x++)
            {
                for (var z = 0; z < settings.arraySize; z++)
                {
                    if (Mathf.Abs(x + settings.placementOffset.x + z + settings.placementOffset.z) > settings.cutThreshold) continue;
                    CreateInnerPart(new AxialCoord(x, z));
                }
            }

            CreateOuterParts();
            CreateMarbles(data.placement, data.players);
        }

        public bool Contains(AxialCoord arrayPosition)
        {
            if (Mathf.Abs(arrayPosition.x + settings.placementOffset.x + arrayPosition.z + settings.placementOffset.z) > settings.cutThreshold) return false;
            if (arrayPosition.x < 0 || arrayPosition.x >= settings.arraySize) return false;
            if (arrayPosition.z < 0 || arrayPosition.z >= settings.arraySize) return false;
            return true;
        }

        private void CreateInnerPart(AxialCoord coord)
        {
            var partObject = Instantiate(boardInnerPartPrefab, (coord + settings.placementOffset).ToWorld(), Quaternion.identity, boardContainer);
            partObject.name = $"Inner{coord}";
            partObject.transform.localPosition += new Vector3(0, Constants.boardOffsetY, 0);
        }

        private void CreateOuterParts()
        {
            var radius = settings.side - 1;
            var coord = CubeDirection.Left.ToCoord() * radius;

            for (int direction = 0; direction < 6; direction++)
            {
                for (int i = 0; i <= radius; i++)
                {
                    var rotation = Quaternion.Euler(0, (direction - 4) * -60, 0);
                    var prefab = boardOuterSidePrefab;

                    if (i == 0)
                    {
                        prefab = boardOuterVertexPrefab;
                    }

                    var partObject = Instantiate(prefab, coord.ToWorld(), rotation, boardContainer);
                    partObject.name = $"Outer{((AxialCoord)coord - settings.placementOffset)}";
                    partObject.transform.localPosition += new Vector3(0, Constants.boardOffsetY, 0);

                    if (i == 0) continue;
                    coord += ((CubeDirection)direction).ToCoord();
                }
            }
        }

        private void CreateMarbles(int[,] placementData, string[] playerColorCodes)
        {
            var playerColors = playerColorCodes.Select(colorCode =>
            {
                ColorUtility.TryParseHtmlString(colorCode, out var color);
                return color;
            }).ToArray();

            for (var x = 0; x < settings.arraySize; x++)
            {
                for (var z = 0; z < settings.arraySize; z++)
                {
                    var playerNumber = placementData[x, z];
                    if (playerNumber == 0) continue;

                    var playerIndex = playerNumber - 1;
                    CreateMarble(playerNumber, playerColors[playerIndex], new AxialCoord(x, z));
                }
            }
        }

        private void CreateMarble(int playerIndex, Color playerColor, AxialCoord arrayPosition)
        {
            var marbleObject = Instantiate(marblePrefab, (arrayPosition + settings.placementOffset).ToWorld(), Quaternion.identity, marbleContainer);
            marbleObject.name = arrayPosition.ToString();
            context.marbles[arrayPosition.x, arrayPosition.z] = marbleObject.gameObject;

            var marble = marbleObject.GetComponent<Marble>();
            marble.Init(settings, playerColor, materials[chosenMaterialIndex[playerIndex - 1]], arrayPosition, playerIndex, context);
        }
    }
}