using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Game.Guide
{
    public class SelectGuideDisplay : MonoBehaviour
    {
        public DisplayChanger displayChanger;
        private RectTransform rectTransform;
        [SerializeField]
        private RectTransform canvasTransfrom;

        private Vector2 point1,point2;

        private void Awake()
        {
            rectTransform = gameObject.GetComponent<RectTransform>();
        }

        public void SetPoint1(Vector2 point)
        {
            point1 = point;
            refresh();
        }

        public void SetPoint2(Vector2 point)
        {
            point2 = point;
            refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void refresh()
        {
            gameObject.SetActive(true);
            rectTransform.position = point1;
            var deltaPoint = point1 - point2;
            rectTransform.sizeDelta = new Vector2(Mathf.Sqrt(deltaPoint.x * deltaPoint.x + deltaPoint.y * deltaPoint.y), rectTransform.rect.height);
            rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(deltaPoint.y, deltaPoint.x)*Mathf.Rad2Deg);
        }
    }
}