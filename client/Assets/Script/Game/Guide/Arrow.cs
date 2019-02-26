using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using UnityEngine;

namespace Game.Guide
{
    public class Arrow : MonoBehaviour
    {
        public float originDistance = 0.1f;
        public float maxDistance;

        private BallSelection _ballSelection;
        private bool _isPushing = false;
        private float _pushingDistance = 0;
        private ArrowsManager _arrowsManager;
        private int _direction = 0;
        [SerializeField]
        private Color unSelected;
        [SerializeField]
        private Color selected;
        [SerializeField]
        private Color overHalf;
        public void SetDirection(int direction)
        {
            _direction = direction;
        }

        public float GetRad()
        {
            return -_direction * Mathf.PI / 3 + Mathf.PI / 3 - Mathf.PI;
        }

        public float GetArrowDegree()
        {
            return -_direction * 60 - 30;
        }

        public void Stop()
        {
            _isPushing = false;
            _pushingDistance = 0;
            gameObject.SetActive(false);
        }

        void Start()
        {
            gameObject.GetComponent<SpriteRenderer>().color = unSelected;
            _arrowsManager = gameObject.transform.parent.GetComponent<ArrowsManager>();
        }

        void Update()
        {
            Vector3 parentPostion = gameObject.transform.parent.position;
            if (_isPushing && Input.GetMouseButtonUp(0))
            {
                if (CheckDistanceOverHalf())
                {
                    _arrowsManager.selected = true;
                    _arrowsManager.selectedArrow = _direction;
                }
                _isPushing = false;
                _arrowsManager.pushingArrow = -1;
                SetArrowDisplayMode(0);
                gameObject.SetActive(false);
            }


            gameObject.transform.rotation = Quaternion.Euler(0, 0, GetArrowDegree());

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float distance = GetDistanceFromOrigin(mousePos);

            if (_isPushing)
            {
                _arrowsManager.zoomSystem.isLocked = true;
                if (distance <= maxDistance - originDistance)
                {
                    gameObject.transform.position = GetArrowPosition(maxDistance - originDistance) + parentPostion;
                    _arrowsManager.pushedDistance = maxDistance;
                }
                else if (distance >= originDistance)
                {
                    gameObject.transform.position = GetArrowPosition(originDistance) + parentPostion;
                    _arrowsManager.pushedDistance = 0;
                }
                else
                {
                    _arrowsManager.pushingArrow = _direction;
                    gameObject.transform.position = GetArrowPosition(distance) + parentPostion;
                    _arrowsManager.pushedDistance = maxDistance - distance;
                }

                if (CheckDistanceOverHalf())
                    SetArrowDisplayMode(2);
                else
                    SetArrowDisplayMode(1);
            }
            else
            {
                _arrowsManager.zoomSystem.isLocked = false;
                gameObject.transform.position = GetArrowPosition(originDistance) + parentPostion;
            }

        }

        private bool CheckDistanceOverHalf()
        {
            return _arrowsManager.pushedDistance >= maxDistance / 2;
        }

        void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0) && !_isPushing)
            {
                _isPushing = true;
            }
            else
            {
                SetArrowDisplayMode(1);
            }
        }

        void OnMouseExit()
        {
            if (!_isPushing)
                SetArrowDisplayMode(0);
        }

        private float GetDistanceFromOrigin(Vector2 mousePosition)
        {
            float angle = GetRad();
            Vector2 parentPosition = gameObject.transform.parent.position;
            return (mousePosition.x - parentPosition.x) * Mathf.Cos(angle) + (mousePosition.y - parentPosition.y) * Mathf.Sin(angle);
        }

        private Vector3 GetArrowPosition(float distance)
        {
            float angle = GetRad();

            Vector3 arrowPosition = new Vector3();

            arrowPosition.x = distance * Mathf.Cos(angle);
            arrowPosition.y = distance * Mathf.Sin(angle);
            arrowPosition.z = 7;
            return arrowPosition;
        }

        void SetArrowDisplayMode(int type)
        {
            switch (type)
            {
                //unslected
                case 0:
                    {
                        gameObject.GetComponent<SpriteRenderer>().color = unSelected;
                        break;
                    }
                //selected
                case 1:
                    {
                        gameObject.GetComponent<SpriteRenderer>().color = selected;
                        break;
                    }
                //AfterHalf
                case 2:
                    {
                        gameObject.GetComponent<SpriteRenderer>().color = overHalf;
                        break;
                    }

            }
        }
    }
}

