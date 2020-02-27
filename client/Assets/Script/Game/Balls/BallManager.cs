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

        public Ball[,] ballObjects { get; private set; }
        private BallSelector _ballSelector;
        private BallSelection _ballSelection;
        private List<CubeCoord> _movingBalls;

        private bool _canSelect;

        private float _distanceBetweenBalls;

        public void CreateBalls(BoardManager boardManager)
        {
            ballObjects = BallCreator.CreateBalls(sizeOfBall, gameObject, boardManager.holeDistance, boardManager.GetBoard(), blackBallPrefab, whiteBallPrefab);
            _ballSelector = new BallSelector(this, boardManager);
            _distanceBetweenBalls = boardManager.holeDistance - sizeOfBall;
            this.boardManager = boardManager;
        }

        public void SetBallsSkin(BallType ballType, Texture2D texture)
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
            var s = boardManager.GetBoard().GetSide() - 1;
            ballObjects[cubeCoord.x + s, cubeCoord.y + s] = ballObject;
        }

        public Ball GetBallObjectByCubeCoord(CubeCoord cubeCoord)
        {
            var s = boardManager.GetBoard().GetSide() - 1;
            return ballObjects[cubeCoord.x + s, cubeCoord.y + s];
        }
        public Ball GetBallByCubeCoord(CubeCoord cubeCoord)
        {
            var s = boardManager.GetBoard().GetSide() - 1;
            return ballObjects[cubeCoord.x + s, cubeCoord.y + s].GetComponent<Ball>();
        }


        private float _prevDistance;

        private void Update()
        {
            //State Selecting balls
            if (state == 1)
            {
                _ballSelector.SelectingBalls(gameManager.myBallType, guideDisplay, zoomSystem);
                if (_ballSelector.IsSelected)
                {
                    _ballSelection = _ballSelector.GetBallSelection();
                    state = 2;
                    _ballSelector.IsSelected = false;
                    arrowsManager.ballSelection = _ballSelection;
                    _ballSelector.SelectingBalls(gameManager.myBallType, guideDisplay, zoomSystem);
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
                        foreach (var ballCoord in _ballSelection.GetCubeCoords())
                        {
                            var currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
                            currentBall.StopPushing(true);
                        }
                    }
                    else
                    {
                        foreach (var ballCoord in _movingBalls)
                        {
                            var currentBall = GetBallObjectByCubeCoord(ballCoord).GetComponent<Ball>();
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
                                var currentBall = GetBallObjectByCubeCoord(ballCoord);
                                currentBall.StopPushing(true);
                            }
                        }
                        return;
                    }

                    if (_movingBalls == null)
                        _movingBalls = GetMovingBalls(_ballSelection, arrowsManager.pushingArrow);

                    var sameLine = Board.GetSameLine(_ballSelection.First, _ballSelection.End);
                    var dirCubeCoord = CubeCoord.ConvertNumToDirection(arrowsManager.pushingArrow);

                    var distance = arrowsManager.pushedDistance;

                    if (sameLine == arrowsManager.pushingArrow % 3)
                    {
                        _movingBalls.Reverse();
                        for (var i = 0; i < _movingBalls.Count; i++)
                        {
                            var ballCoord = _movingBalls[i];
                            var currentBall = GetBallObjectByCubeCoord(ballCoord);
                            if (distance <= _prevDistance)
                            {
                                currentBall.Push(arrowsManager.pushingArrow, distance);
                            }
                            if (i == 0 || (_distanceBetweenBalls * i) <= distance)
                            {
                                currentBall.Push(arrowsManager.pushingArrow, distance - _distanceBetweenBalls * i);
                            }
                        }
                        _movingBalls.Reverse();
                    }
                    else
                    {
                        foreach (var ballCoord in _movingBalls)
                        {
                            var currentBall = GetBallObjectByCubeCoord(ballCoord);
                            currentBall.Push(arrowsManager.pushingArrow, distance);
                        }
                    }
                }
                else
                {
                    foreach (var ballCoord in _movingBalls)
                    {
                        var currentBall = GetBallObjectByCubeCoord(ballCoord);
                        MoveBall(ballCoord, arrowsManager.selectedArrow, true);
                    }

                    gameManager.SendBallMoving(_ballSelection, arrowsManager.selectedArrow);
                    state = 0;
                }
            }
        }

        public void PushBalls(BallSelection ballSelection, int direction)
        {
            var movingBalls = GetMovingBalls(ballSelection, direction);
            var sameLine = Board.GetSameLine(ballSelection.First, ballSelection.End);
            var dirCubeCoord = CubeCoord.ConvertNumToDirection(direction);
            if (sameLine == direction % 3)
            {
                StartCoroutine(PushBalls(movingBalls, direction, boardManager.holeDistance / 2));
            }
            else
            {
                foreach (var ballCoord in movingBalls)
                {
                    var currentBall = GetBallObjectByCubeCoord(ballCoord);
                    MoveBall(ballCoord, direction, false);
                }
            }

        }

        private IEnumerator PushBalls(List<CubeCoord> balls, int direction, float halfDistance)
        {
            balls.Reverse();
            for (float distance = 0; distance < halfDistance; distance += 0.05f)
            {
                for (var i = 0; i < balls.Count; i++)
                {
                    if ((_distanceBetweenBalls * i) <= distance)
                    {
                        var currentBall = GetBallObjectByCubeCoord(balls[i]);
                        currentBall.Push(direction, distance - _distanceBetweenBalls * i);
                    }
                }
                yield return 0;
            }
            balls.Reverse();
            foreach (var ballCoord in balls)
            {
                var currentBall = GetBallObjectByCubeCoord(ballCoord);
                MoveBall(ballCoord, direction, false);
            }
        }

        public void MoveBall(CubeCoord cubeCoord, int direction, bool isPushingBall)
        {
            var ballObject = GetBallObjectByCubeCoord(cubeCoord);
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
                var ball = ballObject.GetBall();
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
            var balls = new List<CubeCoord>(5);

            var board = boardManager.GetBoard();
            var myBallType = (BallType)board.GetHoleStateByCubeCoord(ballSelection.First);

            var sameLine = Board.GetSameLine(ballSelection.First, ballSelection.End);
            var dirCubeCoord = CubeCoord.ConvertNumToDirection(direction);

            if (sameLine == direction % 3)
            {
                //앞으로밀기
                var startPoint = ballSelection.GetStartPoint(direction);
                var endPoint = ballSelection.GetEndPoint(direction);

                for (var i = 1; i <= ballSelection.Count; i++)
                {
                    if (boardManager.GetBoard().CheckHoleIsEmptyOrOut(startPoint + dirCubeCoord * i))
                    {
                        for (var k = i - 1; k >= 1; k--)
                        {
                            balls.Add(startPoint + dirCubeCoord * k);
                        }
                        for (var n = ballSelection.Count - 1; n >= 0; n--)
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
                balls.Add(ballSelection.First);
                if (ballSelection.Count >= 2)
                {
                    balls.Add(ballSelection.End);
                }
                if (ballSelection.Count == 3)
                {
                    balls.Add(ballSelection.GetMiddleBallCubeCoord());
                }
            }
            return balls;
        }

    }
}



