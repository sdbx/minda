using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject ballsObject;
    public NetworkManager networkManager;
    public ArrowsManager arrowsManager;
    public GameObject blackBallPrefab;
    public GameObject whiteBallPrefab;
    public BoardManager boardManager;

    //0: 대기 1: 구슬 선택 2: 구슬 움직임 방향 선택
    public int state = 0;
    public float sizeOfBall = 5;

    private GameObject[,] ballObjects;
    private BallSelector _ballSelector;
    private BallSelection _ballSelection;
    private List<CubeCoord> _movingBalls;

    private bool _canSelect;

    public void CreateBalls(BoardManager boardManager)
    {
        ballObjects = BallCreator.createBalls(sizeOfBall, ballsObject, boardManager.holeDistance, boardManager.GetBoard(), blackBallPrefab, whiteBallPrefab);
        _ballSelector = new BallSelector(this, boardManager);
        this.boardManager = boardManager;
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

    public void SetBallObject(CubeCoord cubeCoord, GameObject ballObject)
    {
        int s = boardManager.GetBoard().GetSide() - 1;
        ballObjects[cubeCoord.x + s, cubeCoord.y + s] = ballObject;
    }

    public GameObject GetBallObjectByCubeCoord(CubeCoord cubeCoord)
    {
        int s = boardManager.GetBoard().GetSide() - 1;
        return ballObjects[cubeCoord.x + s, cubeCoord.y + s];
    }

    void Update()
    {
        //State Selecting balls
        if (state == 1)
        {
            _ballSelector.SelectingBalls();
            if (_ballSelector._isSelected)
            {
                _ballSelection = _ballSelector.GetBallSelection();
                state = 2;
                _ballSelector._isSelected = false;
                arrowsManager.ballSelection = _ballSelection;
                _ballSelector.SelectingBalls();
            }
        }
        //State MovingBalls
        else if (state == 2)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                arrowsManager.Reset();
                state = 1;
                return;
            }

            arrowsManager.working = true;


            if (!arrowsManager.selected)
            {
                _movingBalls = GetMovingBalls(_ballSelection, arrowsManager.pushingArrow);
                foreach (CubeCoord ballCoord in _movingBalls)
                {
                    Ball currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
                    currentBall.moving = true;
                    currentBall.direction = arrowsManager.pushingArrow;
                    currentBall.moveDistance = arrowsManager.pushedDistance;
                }
            }
            else
            {
                foreach (CubeCoord ballCoord in _movingBalls)
                {
                    Ball currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
                    MoveBall(ballCoord, arrowsManager.selectedArrow);
                    currentBall.moving = false;
                }

                gameManager.SendBallMoving(_ballSelection,arrowsManager.selectedArrow);
                state = 0;
            }
        }
    }

    public void PushBalls(BallSelection ballSelection, int direction)
    {
        List<CubeCoord> movingBalls = GetMovingBalls(ballSelection, direction);

        foreach (CubeCoord ballCoord in movingBalls)
        {
            Ball currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
            MoveBall(ballCoord, arrowsManager.selectedArrow);
        }
    }

    public void MoveBall(CubeCoord cubeCoord, int direction)
    {
        GameObject ballObject = GetBallObjectByCubeCoord(cubeCoord);
        if (boardManager.GetBoard().CheckOutOfBoard(cubeCoord + Utils.GetDirectionByNum(direction)))
        {
            //주금
            Debug.Log("죽엇어");
            ballObject.GetComponent<Ball>().Move(direction);
            ballObject.GetComponent<Ball>().Die();
            SetBallObject(cubeCoord, null);
            boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord, BallType.Empty);
        }
        else
        {
            BallType ball = ballObject.GetComponent<Ball>().GetBall();
            SetBallObject(cubeCoord, null);
            SetBallObject(cubeCoord + Utils.GetDirectionByNum(direction), ballObject);
            boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord + Utils.GetDirectionByNum(direction), ball);
            boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord, BallType.Empty);
            ballObject.GetComponent<Ball>().Move(direction);
        }

    }



    public List<CubeCoord> GetMovingBalls(BallSelection ballSelection, int direction)
    {
        List<CubeCoord> balls = new List<CubeCoord>(5);

        Board board = boardManager.GetBoard();
        BallType myBallType = board.GetBallByCubeCoord(ballSelection.first);

        int sameLine = board.GetSameLine(ballSelection.first, ballSelection.end);
        CubeCoord dirCubeCoord = Utils.GetDirectionByNum(direction);

        if (sameLine == direction % 3)
        {
            //앞으로밀기
            CubeCoord startPoint = ballSelection.GetStartPoint(direction);
            CubeCoord endPoint = ballSelection.GetEndPoint(direction);

            for (int i = 1; i <= ballSelection.count; i++)
            {
                if (boardManager.GetBoard().CheckHoleIsEmptyOrOut(startPoint + dirCubeCoord * i))
                {
                    for (int k = i - 1; k >= 1; k--)
                    {
                        balls.Add(startPoint + dirCubeCoord * k);
                    }
                    for (int n = ballSelection.count - 1; n >= 0; n--)
                    {
                        balls.Add(endPoint + dirCubeCoord * n);
                    }
                    break;
                }
            }
        }
        else
        {
            //옆으로밀기
            balls.Add(ballSelection.first);
            if (ballSelection.count >= 2)
            {
                balls.Add(ballSelection.end);
            }
            if (ballSelection.count == 3)
            {
                balls.Add(ballSelection.GetMiddleBall());
            }
        }
        return balls;
    }

}




