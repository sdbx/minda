using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Boards;
using Game.Coords;
using Game.Guide;
using Models;
using UnityEngine;

namespace Game.Balls
{
    public class BallManager : MonoBehaviour
    {
        [SerializeField]
        private ZoomSystem zoomSystem;
        public Ball blackBallPrefab;
        public Ball whiteBallPrefab;
        public GameManager gameManager;
        public Sprite testSprite;

        public BoardManager boardManager;
        public ArrowsManager arrowsManager;
        [SerializeField]
        private SelectGuideDisplay guideDisplay;

        //0: 대기 1: 구슬 선택 2: 구슬 움직임 방향 선택
        public int state = 0;
        public float sizeOfBall = 5;

        public Ball[,] ballObjects{get;private set;}
        private BallSelector _ballSelector;
        private BallSelection _ballSelection;
        private List<CubeCoord> _movingBalls;

        private bool _canSelect;

        private float distanceBetweenBalls;

        public void CreateBalls(BoardManager boardManager)
        {
            ballObjects = BallCreator.CreateBalls(sizeOfBall, gameObject, boardManager.holeDistance, boardManager.GetBoard(), blackBallPrefab, whiteBallPrefab);
            _ballSelector = new BallSelector(this, boardManager);
            distanceBetweenBalls = boardManager.holeDistance-sizeOfBall;
            this.boardManager = boardManager;
        }

        public void SetBallsSkin(BallType ballType,Texture2D texture)
        {
            var rect = new Rect(0, 0, texture.width, texture.height);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            foreach (var ballObject in ballObjects)
            {
                if (ballObject != null && ballObject.ballType == ballType)
                    ballObject.SetSprite(sprite);
            }
        }

        public void RemoveBalls()
        {
            if (ballObjects == null)
                return;

            foreach (var ballObject in ballObjects)
            {
                Destroy(ballObject);
            }
        }
        

        public void SetBallObject(CubeCoord cubeCoord, Ball ballObject)
        {
            int s = boardManager.GetBoard().GetSide() - 1;
            ballObjects[cubeCoord.x + s, cubeCoord.y + s] = ballObject;
        }

        public Ball GetBallObjectByCubeCoord(CubeCoord cubeCoord)
        {
            int s = boardManager.GetBoard().GetSide() - 1;
            return ballObjects[cubeCoord.x + s, cubeCoord.y + s];
        }
        public Ball GetBallByCubeCoord(CubeCoord cubeCoord)
        {
            int s = boardManager.GetBoard().GetSide() - 1;
            return ballObjects[cubeCoord.x + s, cubeCoord.y + s].GetComponent<Ball>();
        }
        

        private float prevDistance;
        void Update()
        {
            //State Selecting balls
            if (state == 1)
            {
                _ballSelector.SelectingBalls(gameManager.myBallType,guideDisplay,zoomSystem);
                if (_ballSelector._isSelected)
                {
                    _ballSelection = _ballSelector.GetBallSelection();
                    state = 2;
                    _ballSelector._isSelected = false;
                    arrowsManager.ballSelection = _ballSelection;
                    _ballSelector.SelectingBalls(gameManager.myBallType,guideDisplay,zoomSystem);
                    _movingBalls = null;
                }
            }
            //State MovingBalls
            else if (state == 2)
            {
                guideDisplay.Hide();
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                {
                    arrowsManager.Reset();

                    if (arrowsManager.pushingArrow == -1)
                    {
                        foreach (CubeCoord ballCoord in _ballSelection.GetCubeCoords())
                        {
                            Ball currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
                            currentBall.StopPushing(true);
                        }
                    }
                    else
                    {
                        foreach (CubeCoord ballCoord in _movingBalls)
                        {
                            Ball currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
                            currentBall.StopPushing(true);
                        }
                    }

                    state = 1;
                    return;
                }

                arrowsManager.working = true;


                if (!arrowsManager.selected)
                {
                    if (arrowsManager.pushingArrow == -1)
                    {
                        if (_movingBalls != null)
                        {
                            foreach (var ballCoord in _movingBalls)
                             {
                                Ball currentBall = GetBallObjectByCubeCoord(ballCoord);
                                currentBall.StopPushing(true);
                            }
                        }
                        return;
                    }

                    if(_movingBalls==null)
                        _movingBalls = GetMovingBalls(_ballSelection, arrowsManager.pushingArrow);

                    int sameLine = Board.GetSameLine(_ballSelection.first, _ballSelection.end);
                    CubeCoord dirCubeCoord = CubeCoord.ConvertNumToDirection(arrowsManager.pushingArrow);

                    var distance = arrowsManager.pushedDistance;

                    if (sameLine == arrowsManager.pushingArrow % 3)
                    {
                        _movingBalls.Reverse();
                        for (int i = 0; i < _movingBalls.Count; i++)
                        {
                            var ballCoord = _movingBalls[i];
                            Ball currentBall = GetBallObjectByCubeCoord(ballCoord);
                            if (distance <= prevDistance)
                            {
                                currentBall.Push(arrowsManager.pushingArrow, distance);
                            }
                            if (i == 0 || (distanceBetweenBalls * i) <= distance)
                            {
                                currentBall.Push(arrowsManager.pushingArrow, distance - distanceBetweenBalls * i);
                            }
                        }
                        _movingBalls.Reverse();
                    }
                    else
                    {
                        foreach (CubeCoord ballCoord in _movingBalls)
                        {
                            Ball currentBall = GetBallObjectByCubeCoord(ballCoord);
                            currentBall.Push(arrowsManager.pushingArrow, distance);
                        }
                    }
                }
                else
                {
                    foreach (CubeCoord ballCoord in _movingBalls)
                    {
                        Ball currentBall = GetBallObjectByCubeCoord(ballCoord);
                        MoveBall(ballCoord, arrowsManager.selectedArrow, true);
                    }

                    gameManager.SendBallMoving(_ballSelection, arrowsManager.selectedArrow);
                    state = 0;
                }
            }
        }

        public void PushBalls(BallSelection ballSelection, int direction)
        {
            List<CubeCoord> movingBalls = GetMovingBalls(ballSelection, direction);
            int sameLine = Board.GetSameLine(ballSelection.first, ballSelection.end);
            CubeCoord dirCubeCoord = CubeCoord.ConvertNumToDirection(direction);
            if (sameLine == direction % 3)
            {
                StartCoroutine(PushBalls(movingBalls, direction, boardManager.holeDistance / 2));
            }
            else
            {
                foreach (CubeCoord ballCoord in movingBalls)
                {
                    Ball currentBall = GetBallObjectByCubeCoord(ballCoord);
                    MoveBall(ballCoord, direction, false);
                }
            }

        }

        private IEnumerator PushBalls(List<CubeCoord> balls,int direction,float halfDistance)
        {
            balls.Reverse();
            for(float distance = 0;distance<halfDistance;distance+=0.05f)
            {
                for(int i = 0;i<balls.Count;i++)
                {
                    if ((distanceBetweenBalls * i) <= distance)
                    {
                        Ball currentBall = GetBallObjectByCubeCoord(balls[i]);
                        currentBall.Push(direction, distance - distanceBetweenBalls * i);
                    }
                }
                yield return 0;
            }
            balls.Reverse();
            foreach (CubeCoord ballCoord in balls)
            {
                Ball currentBall = GetBallObjectByCubeCoord(ballCoord);
                MoveBall(ballCoord, direction, false);
            }
        }

        public void MoveBall(CubeCoord cubeCoord, int direction, bool isPushingBall)
        {
            Ball ballObject = GetBallObjectByCubeCoord(cubeCoord);
            if (boardManager.GetBoard().CheckOutOfBoard(cubeCoord + CubeCoord.ConvertNumToDirection(direction)))
            {
                //주금
                Debug.Log("죽엇어");

                if (isPushingBall)
                    ballObject.StopPushing(false);
                else
                    ballObject.Move(direction);

                ballObject.Die();
                SetBallObject(cubeCoord, null);
                boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord, HoleState.Empty);
            }
            else
            {
                BallType ball = ballObject.GetBall();
                SetBallObject(cubeCoord, null);
                SetBallObject(cubeCoord + CubeCoord.ConvertNumToDirection(direction), ballObject);
                boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord + CubeCoord.ConvertNumToDirection(direction), ball);
                boardManager.GetBoard().SetHoleByCubeCoord(cubeCoord, HoleState.Empty);

                if (isPushingBall)
                    ballObject.StopPushing(false);
                else
                    ballObject.Move(direction);
            }

        }

        public List<CubeCoord> GetMovingBalls(BallSelection ballSelection, int direction)
        {
            List<CubeCoord> balls = new List<CubeCoord>(5);

            Board board = boardManager.GetBoard();
            BallType myBallType = (BallType)board.GetHoleStateByCubeCoord(ballSelection.first);

            int sameLine = Board.GetSameLine(ballSelection.first, ballSelection.end);
            CubeCoord dirCubeCoord = CubeCoord.ConvertNumToDirection(direction);

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
                    balls.Add(ballSelection.GetMiddleBallCubeCoord());
                }
            }
            return balls;
        }

    }
}



