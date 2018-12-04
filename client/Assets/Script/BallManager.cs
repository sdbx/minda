using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public GameObject ballsObject;
    public GameObject blackBallPrefab;
    public GameObject whiteBallPrefab;

    //private로 변경할거
    public int state = 0;

    public float sizeOfBall = 5;

    private GameObject[,] ballObjects;

    private BallSelector _ballSelector;
    private BoardManager _boardManager;

    private bool _canSelect;
    private CubeCoord _firstBallSelection, _lastBallSelection;

    //임시 효과 주기용
    private List<GameObject> _selectingBalls = new List<GameObject>();

    public void createBalls(BoardManager boardManager)
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

        if (state == 0)
        {
            return;
        }

        BallSelecting();
    }

    private void BallSelecting()
    {
        foreach (GameObject selectingBall in _selectingBalls)
        {
            selectingBall.GetComponent<SpriteRenderer>().color = Color.white;
        }
        _selectingBalls.Clear();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        CubeCoord ballCubeCoord = _ballSelector.GetBallCubeCoordByMouseXY(mousePos);
        if (ballCubeCoord == null)
        {
            UnSelectable();
            return;
        }
        GameObject ballObject = GetBallObjectByCubeCoord(ballCubeCoord);
        int s = _boardManager.GetBoard().GetSide() - 1;



        if (state == 1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _firstBallSelection = ballCubeCoord;
                state = 2;
            }
        }
        else if (state == 2)
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
                    GameObject middleBall = GetBallObjectByCubeCoord(
                        Utils.GetMiddleCubeCoordBy2CubeCoord(ballCubeCoord, _firstBallSelection));

                    if (middleBall != null && CheckBallObjectIsMine(middleBall))
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
            state = 1;
        }
    }

    private void Selectable(int count)
    {
        if (Input.GetMouseButtonUp(0))
        {
            state = 3;
            SetBallsColor(count, Color.cyan);
        }
        else
        {
            SetBallsColor(count, Color.green);
        }
    }

    private void SetBallsColor(int count, Color color)
    {
        GameObject first = GetBallObjectByCubeCoord(_firstBallSelection);
        first.GetComponent<SpriteRenderer>().color = color;

        if (state == 2)
        {
            _selectingBalls.Add(first);
        }

        if (count >= 1)
        {
            GameObject last = GetBallObjectByCubeCoord(_lastBallSelection);
            last.GetComponent<SpriteRenderer>().color = color;

            if (state == 2)
            {
                _selectingBalls.Add(last);
            }
        }

        if (count == 2)
        {
            GameObject middleBall = GetBallObjectByCubeCoord(
                   Utils.GetMiddleCubeCoordBy2CubeCoord(_firstBallSelection, _lastBallSelection));
            middleBall.GetComponent<SpriteRenderer>().color = color;

            if (state == 2)
            {
                _selectingBalls.Add(middleBall);
            }
        }
    }

    private bool CheckBallObjectIsMine(GameObject ballObject)
    {
        return ballObject.GetComponent<Ball>().GetBall() == _boardManager.GetMyBallType();
    }

}




