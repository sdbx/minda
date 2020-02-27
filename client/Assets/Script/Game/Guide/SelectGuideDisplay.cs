using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Game.Guide
{
    public class SelectGuideDisplay : MonoBehaviour
    {
        public DisplayChanger displayChanger;
        private RectTransform _rectTransform;
        [SerializeField]
        private RectTransform canvasTransfrom;

        private Vector2 _point1, _point2;

        private void Awake()
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
        }

        public void SetPoint1(Vector2 point)
        {
            _point1 = point;
            Refresh();
        }

        public void SetPoint2(Vector2 point)
        {
            _point2 = point;
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            gameObject.SetActive(true);
            _rectTransform.position = _point1;
            var deltaPoint = _point1 - _point2;
            _rectTransform.sizeDelta = new Vector2(Mathf.Sqrt(deltaPoint.x * deltaPoint.x + deltaPoint.y * deltaPoint.y), _rectTransform.rect.height);
            _rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(deltaPoint.y, deltaPoint.x) * Mathf.Rad2Deg);
        }
    }
}
