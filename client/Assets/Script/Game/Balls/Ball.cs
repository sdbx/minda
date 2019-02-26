using System.Collections;
using System.Collections.Generic;
using Game.Boards;
using Game.Coords;
using Models;
using UnityEngine;

namespace Game.Balls
{
    public class Ball : MonoBehaviour
    {

        public CubeCoord cubeCoord;
        public float movingSpeed = 0.5f;
        private SpriteRenderer spriteRenderer;
        public enum PositionState
        {
            Origin,
            Pushing,
            Moving
        }
        public PositionState _positionState = PositionState.Origin;
        public BallType ballType;
        public BoardManager _boardManager;
        public bool die = false;
        public int direction;
        public float pushedDistance;
        public float movedDistance;


        void Awake()
        {
            _boardManager = gameObject.transform.parent.GetComponent<BallManager>().boardManager;
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        // Update is called once per frame
        void Update()
        {
            if (die)
                return;

            switch (_positionState)
            {
                case PositionState.Origin:
                    {
                        gameObject.transform.position = cubeCoord.GetPixelPoint(_boardManager.holeDistance);
                        break;
                    }
                case PositionState.Pushing:
                    {
                        gameObject.transform.position = getPosition(pushedDistance, cubeCoord);
                        break;
                    }
                case PositionState.Moving:
                    {
                        if (!(movedDistance + movingSpeed > _boardManager.holeDistance))
                        {
                            movedDistance += movingSpeed;
                            gameObject.transform.position = getPosition(movedDistance, cubeCoord + CubeCoord.ConvertNumToDirection(direction) * -1);
                        }
                        else
                        {
                            direction = -1;
                            _positionState = PositionState.Origin;
                            movedDistance = 0;
                        }
                        break;
                    }
            }
        }

        public Vector2 getPosition(float distance, CubeCoord cubeCoord)
        {
            float angle = -direction * Mathf.PI / 3 + Mathf.PI / 3;
            return new Vector2(distance * Mathf.Cos(angle), distance * Mathf.Sin(angle)) + cubeCoord.GetPixelPoint(_boardManager.holeDistance);
        }

        public void SetBall(BallType ballType)
        {
            this.ballType = ballType;
        }

        public BallType GetBall()
        {
            return ballType;
        }

        public void SetCubeCoord(CubeCoord cubeCoord)
        {
            this.cubeCoord = cubeCoord;
        }

        public CubeCoord GetCoordinates()
        {
            return cubeCoord;
        }

        public void Push(int direction, float pushedDistance)
        {
            this.direction = direction;
            this.pushedDistance = pushedDistance;
            _positionState = PositionState.Pushing;
        }

        public void StopPushing(bool isCanceled)
        {
            if (isCanceled)
            {
                direction = -1;
                pushedDistance = 0;
                _positionState = PositionState.Origin;
                return;
            }
            cubeCoord += CubeCoord.ConvertNumToDirection(direction);
            movedDistance = pushedDistance;
            pushedDistance = 0;
            _positionState = PositionState.Moving;
        }

        public void Move(int direction)
        {
            cubeCoord += CubeCoord.ConvertNumToDirection(direction);
            this.direction = direction;
            movedDistance = 0;
            _positionState = PositionState.Moving;
        }

        public void Die()
        {
            gameObject.AddComponent<Rigidbody2D>();
            gameObject.AddComponent<CircleCollider2D>();
            die = true;
        }

    }
}