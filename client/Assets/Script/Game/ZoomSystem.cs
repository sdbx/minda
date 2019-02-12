using UnityEngine;
using DG.Tweening;

namespace Game
{
    public class ZoomSystem : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;
        [SerializeField]
        private float minSize;
        [SerializeField]
        private float maxSize;
        [SerializeField]
        private float sensitivity;

        private Vector3 dragOrigin;

        private Vector3 ResetCamera;
        private Vector3 Diference;
        private bool Drag = false;
        [SerializeField]
        private Vector2 dragLimit;

        [SerializeField]
        private float duration = 0.5f;

        void Start()
        {
            ResetCamera = Camera.main.transform.position;
        }

        private void Update()
        {
            var axis = Input.GetAxis("Mouse ScrollWheel");

            if(axis==0)
                return;

            bool isZoom = axis > 0;

            float camSize = cam.orthographicSize;
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
                cam.transform.position = new Vector3((1 - (camSize / maxSize)) * (camPosition.x) * 2, (1 - (camSize / maxSize)) * (camPosition.y) * 2,-10);
            }

            var value = (1 - (camSize / maxSize))*3;
            dragLimit = new Vector2(value, value);
        }

        //drag

        private Vector3 prevPos;
        void LateUpdate()
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButton(0))
            {
                if (Drag == false)
                {
                    prevPos = mousePos;
                    Drag = true;
                }
            }
            else
            {
                Drag = false;
            }
            if (Drag == true)
            {
                var deltaPos = prevPos - mousePos;
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
                prevPos = mousePos+deltaPos;
            }
            if (Input.GetMouseButton(1))
            {
                cam.transform.DOMove(ResetCamera,duration);
                cam.DOOrthoSize(maxSize,duration);
                dragLimit = new Vector2(0,0);
            }
        }
    }
}
                
