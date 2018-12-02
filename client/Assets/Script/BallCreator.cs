using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class BallCreator
{
    static public GameObject[,] createBalls(float sizeOfBall,GameObject ballsObject, float holeDistance,Board board, GameObject blackBallPrefab, GameObject whiteBallPrefab)
    {
        int side = board.GetSide();
        GameObject[,] BallObjects = new GameObject[side * 2 - 1, side * 2 - 1];

        foreach (Hole hole in board.GetHoles())
        {
            if (hole == null || hole.GetBall() == Hole.Ball.Empty)
            {
                continue;
            }

            Vector3 holePoint = hole.GetPoint();
            GameObject BallPrefab = (hole.GetBall() == Hole.Ball.Black ? blackBallPrefab : whiteBallPrefab);

			Vector2 holePosition = hole.GetPixelPoint(holeDistance);
            BallObjects[(int)holePoint.x, (int)holePoint.y] = UnityEngine.Object.Instantiate(BallPrefab, new Vector3(holePosition.x,holePosition.y,-3),
            new Quaternion(0, 0, 0, 0), ballsObject.transform);

            Vector3 ballSpriteSize = BallPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;
			BallObjects[(int)holePoint.x, (int)holePoint.y].transform.localScale = new Vector3(sizeOfBall/ballSpriteSize.x,sizeOfBall/ballSpriteSize.y);
        }
		return BallObjects;
    }
}
