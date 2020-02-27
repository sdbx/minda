using UnityEngine;
using DG.Tweening;
using Game.Balls;

namespace Game
{
    public class ZoomSystem : MonoBehaviour
    {
        [SerializeField]
        private BallManager ballmanager;
        [SerializeField]
        private Camera cam;
        [SerializeField]
        private float minSize;
        [SerializeField]
        private float maxSize;
        [SerializeField]
        private float sensitivity;

        private Vector3 _dragOrigin;

        private Vector3 _resetCamera;
        private Vector3 _diference;
        private bool _drag = false;
        [SerializeField]
        private Vector2 dragLimit;

        [SerializeField]
        private float duration = 0.5f;
        public bool isLocked;

        private bool _resetVeiw;
        private bool _canResetView;

        private bool _mouseRightPrevState;

        private void Start()
        {
            _resetCamera = Camera.main.transform.position;
        }

        private void Update()
        {
            if (isLocked)
            {
                _drag = false;
                return;
            }
            if (!_mouseRightPrevState && Input.GetMouseButton(1) && ballmanager.state != 2)
            {
                cam.transform.DOMove(_resetCamera, duration);
                cam.DOOrthoSize(maxSize, duration);
                dragLimit = new Vector2(0, 0);
                _resetVeiw = false;
                return;
            }
            _mouseRightPrevState = Input.GetMouseButton(1);
            var axis = Input.GetAxis("Mouse ScrollWheel");

            if (axis == 0)
                return;

            var isZoom = axis > 0;

            var camSize = cam.orthographicSize;
            if ((isZoom && camSize <= minSize) || (!isZoom && camSize >= maxSize))
            {
                return;
            }

            var camPosition = cam.transform.position;
            camSize -= axis * sensitivity;
            camSize = Mathf.Clamp(camSize, minSize, maxSize);
            cam.orthographicSize = camSize;
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (axis < 0)
            {
                cam.transform.position = new Vector3((1 - (camSize / maxSize)) * (camPosition.x) * 2, (1 - (camSize / maxSize)) * (camPosition.y) * 2, -10);
            }

            var value = (1 - (camSize / maxSize)) * 3;
            dragLimit = new Vector2(value, value);

        }

        //drag

        public void OnMouseDown()
        {
            _drag = true;
        }

        private Vector3 _prevPos;
        private void LateUpdate()
        {
            if (isLocked)
            {
                _drag = false;
                return;
            }

            var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            if (!Input.GetMouseButton(0))
            {
                _drag = false;
                _prevPos = Vector3.zero;
            }

            if (_drag == true)
            {
                if (_prevPos == Vector3.zero)
                {
                    _prevPos = mousePos;
                }
                var deltaPos = _prevPos - mousePos;
                var pos = cam.transform.position + deltaPos;

                if (-dragLimit.x > pos.x || pos.x > dragLimit.x)
                {
                    deltaPos = new Vector2(0, deltaPos.y);
                }
                if (-dragLimit.y > pos.y || pos.y > dragLimit.y)
                {
                    deltaPos = new Vector2(deltaPos.x, 0);
                }
                pos = new Vector3(Mathf.Clamp(pos.x, -dragLimit.x, dragLimit.x), Mathf.Clamp(pos.y, -dragLimit.y, dragLimit.y), -10);


                cam.transform.position = pos;
                _prevPos = mousePos + deltaPos;
            }

        }
    }
}

