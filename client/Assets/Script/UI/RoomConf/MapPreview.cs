using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class MapPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private float maxScale = 1.1f;
        [SerializeField]
        private float speed = 0.01f;
        private bool isMouseEnter;

        [SerializeField]
        private MapList mapList;

        void Start()
        {

        }

        void Update()
        {
            if (isMouseEnter && gameObject.transform.localScale.x < maxScale)
            {
                var transform = gameObject.transform;
                float value = transform.localScale.x + speed;
                transform.localScale = new Vector3(value, value, 1);
            }
            else if (!isMouseEnter && gameObject.transform.localScale.x > 1)
            {
                var transform = gameObject.transform;
                float value = transform.localScale.x - speed;
                transform.localScale = new Vector3(value, value, 1);
            }

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseEnter = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseEnter = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            mapList.ToggleActivation();
        }

    }

}
