using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Boards;
using Game.Coords;
using Models;
using UnityEngine;

namespace Game.Guide
{
    public class BallSelector
    {
        public bool _isSelected = false;

        private BallManager _ballManager;
        private BoardManager _boardManager;

        private bool _isFirstBallpicked = false;
        private CubeCoord _firstBallSelection, _lastBallSelection;

        //임시 효과 주기용
        private List<GameObject> _selectingBalls = new List<GameObject>();

        public BallSelector(BallManager ballManager, BoardManager boardManager)
        {
            _ballManager = ballManager;
            _boardManager = boardManager;
        }

        public BallSelection GetBallSelection()
        {
            return new BallSelection(_firstBallSelection, _lastBallSelection);
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

        public void SelectingBalls(BallType ballType)
        {
            if (_isSelected)
            {
                return;
            }

            foreach (GameObject selectingBall in _selectingBalls)
            {
                selectingBall.GetComponent<SpriteRenderer>().color = Color.white;
            }
            _selectingBalls.Clear();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            CubeCoord ballCubeCoord = GetBallCubeCoordByMouseXY(mousePos);

            if (ballCubeCoord == null)
            {
                UnSelectable();
                return;
            }
            GameObject ballObject = _ballManager.GetBallObjectByCubeCoord(ballCubeCoord);
            if (ballObject.GetComponent<Ball>().GetBall() != ballType)
                return;

            int s = _boardManager.GetBoard().GetSide() - 1;



            if (!_isFirstBallpicked)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _firstBallSelection = ballCubeCoord;
                    _isFirstBallpicked = true;
                }
            }
            else if (_isFirstBallpicked)
            {
                if (ballCubeCoord.CheckInSameLine(_firstBallSelection))
                {
                    int distance = Utils.GetDistanceBy2CubeCoord(ballCubeCoord, _firstBallSelection);

                    if (distance == 0)
                    {
                        _lastBallSelection = ballCubeCoord;
                        Selectable(distance);
                        return;
                    }
                    else if (distance == 1)
                    {
                        _lastBallSelection = ballCubeCoord;
                        Selectable(distance);
                        return;
                    }
                    else if (distance == 2)
                    {
                        GameObject middleBall = _ballManager.GetBallObjectByCubeCoord(
                            CubeCoord.GetCenter(ballCubeCoord, _firstBallSelection));

                        if (middleBall != null && _boardManager.CheckBallObjectIsMine(middleBall))
                        {
                            _lastBallSelection = ballCubeCoord;
                            Selectable(distance);
                            return;
                        }
                    }
                }
            }
            UnSelectable();
        }

        private void UnSelectable()
        {
            if (Input.GetMouseButtonUp(0))
            {
                _isFirstBallpicked = false;
            }
        }

        private void Selectable(int count)
        {
            if (Input.GetMouseButtonUp(0))
            {
                _isSelected = true;
                _isFirstBallpicked = false;
                SetBallsColor(count, Color.cyan);
            }
            else
            {
                SetBallsColor(count, Color.green);
            }
        }

        private void SetBallsColor(int count, Color color)
        {
            GameObject first = _ballManager.GetBallObjectByCubeCoord(_firstBallSelection);
            first.GetComponent<SpriteRenderer>().color = color;

            _selectingBalls.Add(first);

            if (count >= 1)
            {
                GameObject last = _ballManager.GetBallObjectByCubeCoord(_lastBallSelection);
                last.GetComponent<SpriteRenderer>().color = color;

                _selectingBalls.Add(last);
            }
            if (count == 2)
            {
                GameObject middleBall = _ballManager.GetBallObjectByCubeCoord(
                       CubeCoord.GetCenter(_firstBallSelection, _lastBallSelection));
                middleBall.GetComponent<SpriteRenderer>().color = color;

                _selectingBalls.Add(middleBall);
            }
        }

    }

}