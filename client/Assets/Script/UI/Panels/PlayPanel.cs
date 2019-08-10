using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace UI
{
    public class PlayPanel : MonoBehaviour
    {
        private RectTransform[] buttons;

        private void Awake()
        {
            buttons = GetComponentsInChildren<RectTransform>().Where(rectTransform => rectTransform.parent == transform).ToArray();

            var i = 5;
            var sequence = DOTween.Sequence();
            foreach (var button in buttons)
            {
                button.anchoredPosition = button.anchoredPosition + new Vector2(0, -Screen.height * 0.5f);
                var delay = i * 0.05f;
                sequence.Insert(delay, button.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutCirc));
                i++;
            }

            sequence.Play();
        }
    }
}