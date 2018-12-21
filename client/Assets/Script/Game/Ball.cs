using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Ball : MonoBehaviour
    {

        public CubeCoord _cubeCoord;
        public float movingSpeed = 0.5f;

        public enum PositionState
        {
            Origin,
            Pushing,
            Moving
        }
        public PositionState _positionState = PositionState.Origin;
        public BallType _ballType;
        public BoardManager _boardManager;
        public bool _die = false;
        public int _direction;
        public float _pushedDistance;
        public float _movedDistance;



        void Start()
        {
            _boardManager = gameObject.transform.parent.GetComponent<BallManager>().boardManager;
        }

        // Update is called once per frame
        void Update()
        {
            if (_die)
                return;


            switch (_positionState)
            {
                case PositionState.Origin:
                    {
                        gameObject.transform.position = _cubeCoord.GetPixelPoint(_boardManager.holeDistance);
                        break;
                    }
                case PositionState.Pushing:
                    {
                        gameObject.transform.position = getPosition(_pushedDistance, _cubeCoord);
                        break;
                    }
                case PositionState.Moving:
                    {
                        if (!(_movedDistance + movingSpeed > _boardManager.holeDistance))
                        {
                            _movedDistance += movingSpeed;
                            gameObject.transform.position = getPosition(_movedDistance, _cubeCoord + CubeCoord.ConvertNumToDirection(_direction) * -1);
                        }
                        else
                        {
                            _direction = -1;
                            _positionState = PositionState.Origin;
                            _movedDistance = 0;
                        }
                        break;
                    }
            }
        }

        public Vector2 getPosition(float distance, CubeCoord cubeCoord)
        {
            float angle = -_direction * Mathf.PI / 3 + Mathf.PI / 3;
            return new Vector2(distance * Mathf.Cos(angle), distance * Mathf.Sin(angle)) + cubeCoord.GetPixelPoint(_boardManager.holeDistance);
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



        public void Push(int direction, float pushedDistance)
        {
            _direction = direction;
            _pushedDistance = pushedDistance;
            _positionState = PositionState.Pushing;
        }

        public void StopPushing(bool isCanceled)
        {
            if (isCanceled)
            {
                _direction = -1;
                _pushedDistance = 0;
                _positionState = PositionState.Origin;
                return;
            }
            _cubeCoord += CubeCoord.ConvertNumToDirection(_direction);
            _movedDistance = _pushedDistance;
            _pushedDistance = 0;
            _positionState = PositionState.Moving;
        }

        public void Move(int direction)
        {
            _cubeCoord += CubeCoord.ConvertNumToDirection(direction);
            _direction = direction;
            _movedDistance = 0;
            _positionState = PositionState.Moving;
        }

        public void Die()
        {
            gameObject.AddComponent<Rigidbody2D>();
            gameObject.AddComponent<CircleCollider2D>();
            _die = true;
        }

    }
}