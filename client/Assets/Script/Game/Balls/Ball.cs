using System.Collections;
using System.Collections.Generic;
using Game.Boards;
using Game.Coords;
using Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Balls
{
    public class Ball : MonoBehaviour
    {

        public CubeCoord cubeCoord;
        public float movingSpeed = 0.5f;
        private SpriteRenderer _spriteRenderer;
        public enum PositionState
        {
            Origin,
            Pushing,
            Moving
        }
        [FormerlySerializedAs("_positionState")] public PositionState positionState = PositionState.Origin;
        public BallType ballType;
        [FormerlySerializedAs("_boardManager")] public BoardManager boardManager;
        public bool die = false;
        public int direction;
        public float pushedDistance;
        public float movedDistance;


        private void Awake()
        {
            boardManager = gameObject.transform.parent.GetComponent<BallManager>().boardManager;
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        // Update is called once per frame
        private void Update()
        {
            if (die)
                return;

            switch (positionState)
            {
                case PositionState.Origin:
                    {
                        gameObject.transform.position = cubeCoord.GetPixelPoint(boardManager.holeDistance);
                        break;
                    }
                case PositionState.Pushing:
                    {
                        gameObject.transform.position = GetPosition(pushedDistance, cubeCoord);
                        break;
                    }
                case PositionState.Moving:
                    {
                        if (!(movedDistance + movingSpeed > boardManager.holeDistance))
                        {
                            movedDistance += movingSpeed;
                            gameObject.transform.position = GetPosition(movedDistance, cubeCoord + CubeCoord.ConvertNumToDirection(direction) * -1);
                        }
                        else
                        {
                            direction = -1;
                            positionState = PositionState.Origin;
                            movedDistance = 0;
                        }
                        break;
                    }
            }
        }

        public Vector2 GetPosition(float distance, CubeCoord cubeCoord)
        {
            var angle = -direction * Mathf.PI / 3 + Mathf.PI / 3;
            return new Vector2(distance * Mathf.Cos(angle), distance * Mathf.Sin(angle)) + cubeCoord.GetPixelPoint(boardManager.holeDistance);
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
            positionState = PositionState.Pushing;
        }

        public void StopPushing(bool isCanceled)
        {
            if (isCanceled)
            {
                direction = -1;
                pushedDistance = 0;
                positionState = PositionState.Origin;
                return;
            }
            cubeCoord += CubeCoord.ConvertNumToDirection(direction);
            movedDistance = pushedDistance;
            pushedDistance = 0;
            positionState = PositionState.Moving;
        }

        public void Move(int direction)
        {
            cubeCoord += CubeCoord.ConvertNumToDirection(direction);
            this.direction = direction;
            movedDistance = 0;
            positionState = PositionState.Moving;
        }

        public void Die()
        {
            gameObject.AddComponent<Rigidbody2D>();
            gameObject.AddComponent<CircleCollider2D>();
            die = true;
        }

    }
}
