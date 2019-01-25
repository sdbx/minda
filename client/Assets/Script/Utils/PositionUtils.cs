
using UnityEngine;

namespace Utils
{
    public static class PositionUtils
    {
        public static Vector2 WorldPosToLocalRectPos(Vector2 position, RectTransform parent)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, Camera.main , out var pos);
            return pos;
        }
    }
}