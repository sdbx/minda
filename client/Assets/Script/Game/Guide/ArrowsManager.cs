using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Boards;
using UnityEngine;

namespace Game.Guide
{
    public class ArrowsManager : MonoBehaviour
    {
        public ZoomSystem zoomSystem;
        [SerializeField]
        private BoardManager boardManager;
        [SerializeField]
        private GameManager gameManager;
        [SerializeField]
        private GameObject arrowPrefab;

        public float arrowDistance;
        public BallSelection ballSelection;
        public int pushingArrow = -1;
        public int selectedArrow = -1;
        public float pushedDistance = 0;
        public bool selected = false;
        public bool working = false;

        private Arrow[] _arrows = new Arrow[6];


        public void Reset()
        {
            pushingArrow = -1;
            selectedArrow = -1;
            pushedDistance = 0;
            selected = false;
            working = false;
        }

        private void Start()
        {
            for (var i = 0; i < 6; i++)
            {
                var current = Instantiate(arrowPrefab, new Vector3(), new Quaternion(0, 0, 0, 0), gameObject.transform).GetComponent<Arrow>();
                current.SetDirection(i);
                current.originDistance = arrowDistance;
                current.maxDistance = boardManager.holeDistance;
                _arrows[i] = current;
            }
        }

        private void Update()
        {
            if (!working)
            {
                Reset();
                for (var i = 0; i < 6; i++)
                {
                    _arrows[i].Stop();
                }
                return;
            }

            if (selected)
            {
                working = false;
                return;
            }

            gameObject.transform.position = ballSelection.GetMiddlePoint(boardManager.holeDistance);

            if (pushingArrow != -1)
            {
                for (var i = 0; i < 6; i++)
                {
                    if (i == pushingArrow)
                        continue;
                    _arrows[i].gameObject.SetActive(false);
                }
            }

            else
            {
                for (var i = 0; i < 6; i++)
                {

                    if (boardManager.GetBoard().CheckMovement(ballSelection, i, gameManager.myBallType))
                        _arrows[i].gameObject.SetActive(true);
                    else
                        _arrows[i].gameObject.SetActive(false);

                }
            }
        }

    }
}
