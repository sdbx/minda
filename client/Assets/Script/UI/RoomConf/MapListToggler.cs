using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class MapListToggler : MonoBehaviour
    {
        private enum State
        {
            UnActivated,
            Activating,
            Activated,
            UnActivating,
        }
        [SerializeField]
        private GameObject targetObject;
        private Vector3 originPosition;
        private State state = State.UnActivated;
        [SerializeField]
        private Vector3 destination;
        [SerializeField]
        private float speed = 0.3f;
        private Vector3 velocity = Vector3.zero;
        private Vector3 unActivatedScale = new Vector3(0.001f, 0.001f, 0.001f);

        void Start()
        {
            originPosition = targetObject.transform.localPosition;
            destination = transform.localPosition;
            transform.localPosition = originPosition;
            transform.localScale = unActivatedScale;
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (state == State.Activating)
            {
                if (transform.localPosition.x >= destination.x - 1)
                {
                    transform.localPosition = destination;
                    state = State.Activated;
                }
                else SmoothScaleAndMove(destination, speed);
            }
            else if (state == State.UnActivating)
            {
                if (transform.localPosition.x < originPosition.x + 10)
                {
                    transform.localPosition = originPosition;
                    transform.localScale = unActivatedScale;
                    state = State.UnActivated;
                    gameObject.SetActive(false);
                }
                else SmoothScaleAndMove(originPosition, speed * 0.5f);
            }
        }

        private void SmoothScaleAndMove(Vector3 destination, float speed)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, destination, ref velocity, speed);
            var scaleValue = (transform.localPosition.x - originPosition.x) / (this.destination.x - originPosition.x);
            transform.localScale = new Vector3(scaleValue, scaleValue, 1);
        }

        public void ToggleActivation()
        {
            if (state == State.UnActivated)
            {
                Activate();
            }
            else if (state == State.Activated)
            {
                UnActivate();
            }
        }

        public void UnActivate()
        {
            if (state == State.Activated)
            {
                state = State.UnActivating;
            }
        }

        public void Activate()
        {
            if (state == State.UnActivated)
            {
                gameObject.SetActive(true);
                state = State.Activating;
            }
        }

    }
}
