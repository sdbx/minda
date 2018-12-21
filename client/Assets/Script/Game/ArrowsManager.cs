using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ArrowsManager : MonoBehaviour
    {
        public GameObject boardObject;
        public GameObject arrowPrefab;

        public float arrowDistance;
        public BallSelection ballSelection;
        public int pushingArrow = -1;
        public int selectedArrow = -1;
        public float pushedDistance = 0;
        public bool selected = false;
        public bool working = false;

        private Arrow[] _arrows = new Arrow[6];
        private BoardManager _boardManger;

        public void Reset()
        {
            pushingArrow = -1;
            selectedArrow = -1;
            pushedDistance = 0;
            selected = false;
            working = false;
        }

        void Start()
        {
            _boardManger = boardObject.GetComponent<BoardManager>();

            for (int i = 0; i < 6; i++)
            {
                Arrow current;
                current = Instantiate(arrowPrefab, new Vector3(), new Quaternion(0, 0, 0, 0), gameObject.transform).GetComponent<Arrow>();
                current.SetDirection(i);
                current.originDistance = arrowDistance;
                current.maxDistance = _boardManger.holeDistance;
                _arrows[i] = current;
            }
        }

        void Update()
        {
            if (!working)
            {
                Reset();
                for (int i = 0; i < 6; i++)
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

            gameObject.transform.position = ballSelection.GetMiddlePoint(_boardManger.holeDistance);

            if (pushingArrow != -1)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (i == pushingArrow)
                        continue;
                    _arrows[i].gameObject.SetActive(false);
                }
            }

            else
            {
                for (int i = 0; i < 6; i++)
                {

                    if (_boardManger.GetBoard().CheckMovement(ballSelection, i, _boardManger.GetMyBallType()))
                        _arrows[i].gameObject.SetActive(true);
                    else
                        _arrows[i].gameObject.SetActive(false);

                }
            }
        }

    }
}