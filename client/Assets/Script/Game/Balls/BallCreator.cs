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
            int side = board.GetSide();
            Ball[,] BallObjects = new Ball[side * 2 - 1, side * 2 - 1];

            foreach (Hole hole in board.GetHoles())
            {
                if (hole == null || hole.GetHoleState() == HoleState.Empty)
                {
                    continue;
                }

                CubeCoord holeCubeCoord = hole.GetCubeCoord();
                Ball BallPrefab = (hole.GetHoleState() == HoleState.Black ? blackBallPrefab : whiteBallPrefab);

                Vector2 holePosition = hole.GetPixelPoint(holeDistance);

                Ball ballObject = UnityEngine.Object.Instantiate(BallPrefab, new Vector3(holePosition.x, -holePosition.y, 8),
                Quaternion.Euler(0, 0, 0), ballsObject.transform);


                Vector3 ballSpriteSize = BallPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;
                ballObject.transform.localScale = new Vector3(sizeOfBall / ballSpriteSize.x, sizeOfBall / ballSpriteSize.y);

                Ball ballScript = ballObject.GetComponent<Ball>();
                ballScript.SetBall((BallType)hole.GetHoleState());
                ballScript.SetCubeCoord(holeCubeCoord);

                BallObjects[(int)holeCubeCoord.x + side - 1, (int)holeCubeCoord.y + side - 1] = ballObject;
            }
            return BallObjects;
        }
        
        static public GameObject[,] CreatePreviewBalls(float sizeOfBall, Transform parentTransform, float holeDistance, Board board, GameObject blackBallPrefab, GameObject whiteBallPrefab)
        {
            int side = board.GetSide();
            GameObject[,] BallObjects = new GameObject[side * 2 - 1, side * 2 - 1];

            foreach (Hole hole in board.GetHoles())
            {
                if (hole == null || hole.GetHoleState() == HoleState.Empty)
                {
                    continue;
                }

                CubeCoord holeCubeCoord = hole.GetCubeCoord();
                GameObject BallPrefab = (hole.GetHoleState() == HoleState.Black ? blackBallPrefab : whiteBallPrefab);
                Vector2 holePosition = hole.GetPixelPoint(holeDistance);

                GameObject ballObject = UnityEngine.Object.Instantiate(BallPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), parentTransform);
                ballObject.transform.localPosition = new Vector3(holePosition.x, holePosition.y, 8);
                

                float ballSpriteSize = BallPrefab.GetComponent<RawImage>().texture.width;
                ballObject.transform.localScale = new Vector3(sizeOfBall / ballSpriteSize, sizeOfBall / ballSpriteSize);

                BallObjects[(int)holeCubeCoord.x + side - 1, (int)holeCubeCoord.y + side - 1] = ballObject;
            }
            return BallObjects;
        }
    }
}