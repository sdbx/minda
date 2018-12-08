using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class BallCreator
{
    static public GameObject[,] createBalls(float sizeOfBall, GameObject ballsObject, float holeDistance, Board board, GameObject blackBallPrefab, GameObject whiteBallPrefab)
    {
        int side = board.GetSide();
        GameObject[,] BallObjects = new GameObject[side * 2 - 1, side * 2 - 1];

        foreach (Hole hole in board.GetHoles())
        {
            if (hole == null || hole.GetBall() == Hole.Ball.Empty)
            {
                continue;
            }

            CubeCoord holeCubeCoord = hole.GetCubeCoord();
            GameObject BallPrefab = (hole.GetBall() == Hole.Ball.Black ? blackBallPrefab : whiteBallPrefab);

            Vector2 holePosition = hole.GetPixelPoint(holeDistance);

            GameObject ballObject = UnityEngine.Object.Instantiate(BallPrefab, new Vector3(holePosition.x, holePosition.y, 8),
            new Quaternion(0, 0, 0, 0), ballsObject.transform);


            Vector3 ballSpriteSize = BallPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;
            ballObject.transform.localScale = new Vector3(sizeOfBall / ballSpriteSize.x, sizeOfBall / ballSpriteSize.y);

            Ball ballScript = ballObject.GetComponent<Ball>();
            ballScript.SetBall(hole.GetBall());
            ballScript.SetCubeCoord(holeCubeCoord);

            BallObjects[(int)holeCubeCoord.x+side-1, (int)holeCubeCoord.y+side-1] = ballObject;
        }
        return BallObjects;
    }
}
