using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Boards
{
    public static class BoardCreator
    {
        public static void CreateBoard(Board board, GameObject boardObject, GameObject holePrefab, GameObject boardBottomPrefab, Vector2 boardCenter, float holeDistance)
        {
            GameObject duplicatedHole;
            var holeCount = 0;

            foreach (var hole in board.GetHoles())
            {
                if (hole == null)
                    continue;
                holeCount++;
                var holePosition = hole.GetPixelPoint(holeDistance) + boardCenter;
                duplicatedHole = UnityEngine.Object.Instantiate(holePrefab, new Vector3(holePosition.x, holePosition.y, 0), new Quaternion(0, 0, 0, 0), boardObject.transform);
                duplicatedHole.transform.position += new Vector3(0, 0, 9);
                duplicatedHole.name = "Hole" + holeCount;
            }


            var boardBottom = UnityEngine.Object.Instantiate(boardBottomPrefab, new Vector3(boardCenter.x, boardCenter.y, 10), new Quaternion(0, 0, 0, 0), boardObject.transform);
            var boardBottomSpriteSize = boardBottomPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;
            var boardDiameter = (board.GetSide() * 2) * holeDistance + 0.95f;
            boardBottom.transform.localScale = new Vector3(boardDiameter / boardBottomSpriteSize.x, (boardDiameter * Mathf.Sqrt(3) / 2) / boardBottomSpriteSize.y, 0);
        }
    }
}
