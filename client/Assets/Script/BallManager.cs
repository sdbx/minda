using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public GameObject ballsObject;
    public ArrowsManager arrowsObject;
    public GameObject blackBallPrefab;
    public GameObject whiteBallPrefab;


    //0: 대기 1: 구슬 선택 2: 구슬 움직임 방향 선택
    public int state = 0;

    public float sizeOfBall = 5;

    private GameObject[,] ballObjects;

    private BallSelector _ballSelector;
    public BoardManager _boardManager;
    private BallSelection _ballSelection;
    private ArrowsManager _arrowsManager;
    private List<CubeCoord> movingBalls;

    private bool _canSelect;

    public void CreateBalls(BoardManager boardManager)
    {
        ballObjects = BallCreator.createBalls(sizeOfBall, ballsObject, boardManager.holeDistance, boardManager.GetBoard(), blackBallPrefab, whiteBallPrefab);
        _ballSelector = new BallSelector(this, boardManager);
        _boardManager = boardManager;
        _arrowsManager = arrowsObject.GetComponent<ArrowsManager>();
    }
    public void RemoveBalls()
    {
        foreach (var ballObject in ballObjects)
        {
            Destroy(ballObject);
        }
    }
    public GameObject[,] GetBallObjects()
    {
        return ballObjects;
    }
    
    public void SetBallObject(CubeCoord cubeCoord,GameObject ballObject)
    {
        int s = _boardManager.GetBoard().GetSide() - 1;
        ballObjects[cubeCoord.x + s, cubeCoord.y + s] = ballObject;
    }

    public GameObject GetBallObjectByCubeCoord(CubeCoord cubeCoord)
    {
        int s = _boardManager.GetBoard().GetSide() - 1;
        return ballObjects[cubeCoord.x + s, cubeCoord.y + s];
    }

    void Update()
    {
        //State Selecting balls
        if (state == 1)
        {
            _ballSelector.SelectingBalls();
            if(_ballSelector._isSelected)
            {
                _ballSelection = _ballSelector.GetBallSelection();
                state = 2;
                _ballSelector._isSelected = false;
                _arrowsManager.ballSelection = _ballSelection;
            }
        }
        //State MovingBalls
        else if(state == 2)
        {
            _arrowsManager.working = true;
            

            if(!_arrowsManager.selected)
            {
                //구슬 마우스 따라 움직임
                movingBalls = GetMovingBalls(_ballSelection,_arrowsManager.pushingArrow);
                foreach (CubeCoord ballCoord in movingBalls)
                {
                    Ball currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
                    currentBall.moving = true;
                    currentBall.direction = _arrowsManager.pushingArrow;
                    currentBall.moveDistance = _arrowsManager.pushedDistance;
                }
            }
            else
            {
                //구슬 애니매이션 마무리
                foreach (CubeCoord ballCoord in movingBalls)
                {
                    if(ballCoord==null)
                        continue;

                    Ball currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
                    MoveBall(ballCoord,_arrowsManager.selectedArrow);
                    currentBall.moving = false;
                }
                state = 1;
            }
        }
    }

    public void MoveBall(CubeCoord cubeCoord,int direction)
    {
        GameObject ballObject = GetBallObjectByCubeCoord(cubeCoord);
        if(_boardManager.GetBoard().CheckOutOfBoard(cubeCoord+Utils.GetDirectionByNum(direction)))
        {
            //주금
            Debug.Log("죽엇어");
            ballObject.GetComponent<Ball>().Move(direction);
            ballObject.GetComponent<Ball>().Die();
            SetBallObject(cubeCoord,null);
            _boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord, Hole.Ball.Empty);
        }
        else
        {
            Hole.Ball ball = ballObject.GetComponent<Ball>().GetBall();
            SetBallObject(cubeCoord, null);
            SetBallObject(cubeCoord + Utils.GetDirectionByNum(direction), ballObject);
            _boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord + Utils.GetDirectionByNum(direction), ball);
            _boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord, Hole.Ball.Empty);
            ballObject.GetComponent<Ball>().Move(direction);
        }

    }



    public List<CubeCoord> GetMovingBalls(BallSelection ballSelection, int direction)
    {
        List<CubeCoord> balls = new List<CubeCoord>(5);

        Board board = _boardManager.GetBoard();
        Hole.Ball myBallType = board.GetBallByCubeCoord(ballSelection.first);

        int sameLine = board.GetSameLine(ballSelection.first,ballSelection.end);
        CubeCoord dirCubeCoord = Utils.GetDirectionByNum(direction);

        Debug.Log(sameLine+" "+direction%3);
        if(sameLine == direction%3)
        {
            //앞으로밀기
            CubeCoord startPoint = ballSelection.GetStartPoint(direction);
            CubeCoord endPoint = ballSelection.GetEndPoint(direction);

            for (int i = 1; i <= ballSelection.count; i++)
            {
                if(_boardManager.GetBoard().CheckHoleIsEmptyOrOut(startPoint+dirCubeCoord*i))
                {
                    for (int k = i-1; k >= 1; k--)
                    {
                        balls.Add(startPoint+dirCubeCoord*k);
                    }
                    for (int n = ballSelection.count - 1; n >= 0; n--)
                    {
                        balls.Add(endPoint+dirCubeCoord*n);
                    }
                    break;
                }
            }
        }
        else
        {
            //옆으로밀기
             balls.Add(ballSelection.first);
            if(ballSelection.count>=2)
            {
                 balls.Add(ballSelection.end);
            }
            if(ballSelection.count==3)
            {
                 balls.Add(ballSelection.GetMiddleBall());
            }
        }
        return balls;
    }

}




