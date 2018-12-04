using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSelector
{
    private BallManager _ballManager;
    private BoardManager _boardManager;

    public BallSelector(BallManager ballManager, BoardManager boardManager)
    {
        _ballManager = ballManager;
        _boardManager = boardManager;
    }

    public CubeCoord GetBallCubeCoordByMouseXY(Vector2 mouseXY)
    {
        int s = _boardManager.GetBoard().GetSide() - 1;
        for (int x = 0; x <= 2 * s; x++)
        {
            for (int y = 0; y <= 2 * s; y++)
            {
                GameObject ballObject = _ballManager.GetBallObjects()[x, y];
                if (ballObject == null)
                    continue;

                if (Utils.CheckCircleAndPointCollision(mouseXY, ballObject.transform.position, _ballManager.sizeOfBall / 2))
                {
                    x = x - s;
                    y = y - s;
                    return new CubeCoord(x, y, -(x + y));
                }

            }
        }
        return null;
    }
}
