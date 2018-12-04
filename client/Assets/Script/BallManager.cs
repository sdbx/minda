using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public GameObject ballsObject;
    public GameObject blackBallPrefab;
    public GameObject whiteBallPrefab;


    //0: 대기 1: 구슬 선택 2: 구슬 움직임 방향 선택
    public int state = 0;

    public float sizeOfBall = 5;

    private GameObject[,] ballObjects;

    private BallSelector _ballSelector;
    private BoardManager _boardManager;
    private BallSelection _ballSelection;

    private bool _canSelect;

    public void CreateBalls(BoardManager boardManager)
    {
        ballObjects = BallCreator.createBalls(sizeOfBall, ballsObject, boardManager.holeDistance, boardManager.GetBoard(), blackBallPrefab, whiteBallPrefab);
        _ballSelector = new BallSelector(this, boardManager);
        _boardManager = boardManager;
    }

    public GameObject[,] GetBallObjects()
    {
        return ballObjects;
    }
    
    public GameObject GetBallObjectByCubeCoord(CubeCoord cubeCoord)
    {
        int s = _boardManager.GetBoard().GetSide() - 1;
        return ballObjects[cubeCoord.x + s, cubeCoord.y + s];
    }

    void Update()
    {

        if (state == 1)
        {
            Debug.Log("구슬 선택 중");
            
            _ballSelector.SelectingBalls();
            if(_ballSelector._isSelected)
            {
                _ballSelection = _ballSelector.GetBallSelection();
                state = 2;
                _ballSelector._isSelected = false;
            }
        }
        else if(state == 2)
        {
            Debug.Log("방향 선택 중");
        }


    }

}




