using System;
using System.Collections;
using System.Collections.Generic;
using Game.Boards;
using Game.Coords;
using Models;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Balls
{
    static public class BallCreator
    {
        static public Ball[,] CreateBalls(float sizeOfBall, GameObject ballsObject, float holeDistance, Board board, Ball blackBallPrefab, Ball whiteBallPrefab)
        {
            var side = board.GetSide();
            var ballObjects = new Ball[side * 2 - 1, side * 2 - 1];

            foreach (var hole in board.GetHoles())
            {
                if (hole == null || hole.GetHoleState() == HoleState.Empty)
                {
                    continue;
                }

                var holeCubeCoord = hole.GetCubeCoord();
                var ballPrefab = (hole.GetHoleState() == HoleState.Black ? blackBallPrefab : whiteBallPrefab);

                var holePosition = hole.GetPixelPoint(holeDistance);

                var ballObject = UnityEngine.Object.Instantiate(ballPrefab, new Vector3(holePosition.x, -holePosition.y, 8),
                Quaternion.Euler(0, 0, 0), ballsObject.transform);


                var ballSpriteSize = ballPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;
                ballObject.transform.localScale = new Vector3(sizeOfBall / ballSpriteSize.x, sizeOfBall / ballSpriteSize.y);

                var ballScript = ballObject.GetComponent<Ball>();
                ballScript.SetBall((BallType)hole.GetHoleState());
                ballScript.SetCubeCoord(holeCubeCoord);

                ballObjects[(int)holeCubeCoord.x + side - 1, (int)holeCubeCoord.y + side - 1] = ballObject;
            }
            return ballObjects;
        }

        static public GameObject[,] CreatePreviewBalls(float sizeOfBall, Transform parentTransform, float holeDistance, Board board, GameObject blackBallPrefab, GameObject whiteBallPrefab)
        {
            var side = board.GetSide();
            var ballObjects = new GameObject[side * 2 - 1, side * 2 - 1];

            foreach (var hole in board.GetHoles())
            {
                if (hole == null || hole.GetHoleState() == HoleState.Empty)
                {
                    continue;
                }

                var holeCubeCoord = hole.GetCubeCoord();
                var ballPrefab = (hole.GetHoleState() == HoleState.Black ? blackBallPrefab : whiteBallPrefab);
                var holePosition = hole.GetPixelPoint(holeDistance);

                var ballObject = UnityEngine.Object.Instantiate(ballPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), parentTransform);
                ballObject.transform.localPosition = new Vector3(holePosition.x, holePosition.y, 8);


                float ballSpriteSize = ballPrefab.GetComponent<RawImage>().texture.width;
                ballObject.transform.localScale = new Vector3(sizeOfBall / ballSpriteSize, sizeOfBall / ballSpriteSize);

                ballObjects[(int)holeCubeCoord.x + side - 1, (int)holeCubeCoord.y + side - 1] = ballObject;
            }
            return ballObjects;
        }
    }
}
