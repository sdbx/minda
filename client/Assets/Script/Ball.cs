using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public CubeCoord _cubeCoord;
    public bool moving = false;
    public int direction;
    public float moveDistance;

    private BallType _ballType;
    private BoardManager _boardManager;
    private bool _die = false;

    void Start()
    {
        _boardManager = gameObject.transform.parent.GetComponent<BallManager>().boardManager;
    }

    // Update is called once per frame
    void Update()
    {
        if(_die)
            return;
        if (moving)
        {
            float angle = GetRad();
            gameObject.transform.position = new Vector2(moveDistance/1.5f * Mathf.Cos(angle), moveDistance/1.5f * Mathf.Sin(angle)) + _cubeCoord.GetPixelPoint(_boardManager.holeDistance);
        }
        else
        {
            RefreshPosition();
        }
    }

    public float GetRad()
    {
        return -direction * Mathf.PI / 3 + Mathf.PI / 3;
    }

    public void SetBall(BallType ballType)
    {
        _ballType = ballType;
    }

    public BallType GetBall()
    {
        return _ballType;
    }

    public void SetCubeCoord(CubeCoord cubeCoord)
    {
        _cubeCoord = cubeCoord;
    }

    public CubeCoord GetCoordinates()
    {
        return _cubeCoord;
    }

    public void Move(int dirNum)
    {
        _cubeCoord += Utils.GetDirectionByNum(dirNum);
        RefreshPosition();
    }

    public void Die()
    {
        gameObject.AddComponent<Rigidbody2D>();
        _die = true;
    }
    
    public void RefreshPosition()
    {
        gameObject.transform.position = _cubeCoord.GetPixelPoint(_boardManager.holeDistance);
    }
}
