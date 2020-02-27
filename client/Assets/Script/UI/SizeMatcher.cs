using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeMatcher : MonoBehaviour
{
    [SerializeField]
    private RectTransform target;
    private RectTransform _rectTransform;

    [SerializeField]
    private float vertical;
    [SerializeField]
    private float horizontal;

    //ForceSet
    public bool forceSetMode = false;
    [SerializeField]
    private Vector2 forceSetSize;

    [SerializeField]
    private Vector2 scale = new Vector2(1, 1);


    private void Awake()
    {
        _rectTransform = gameObject.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (forceSetMode)
        {
            _rectTransform.sizeDelta = forceSetSize;
            return;
        }
        var targetSize = target.rect.size;
        var sizeX = targetSize.x + horizontal;
        var sizeY = targetSize.y + vertical;
        sizeX *= scale.x;
        sizeY *= scale.y;

        if (horizontal == -1)
        {
            sizeX = _rectTransform.rect.width;
        }
        if (vertical == -1)
        {
            sizeY = _rectTransform.rect.height;
        }
        targetSize = new Vector2(sizeX, sizeY);

        if (targetSize != _rectTransform.rect.size)
        {
            _rectTransform.sizeDelta = targetSize;
        }
    }

    public void ForceSet(Vector2 size)
    {
        forceSetMode = true;
        forceSetSize = new Vector2(
            (size.x == -1 ? _rectTransform.rect.width : size.x),
            (size.y == -1 ? _rectTransform.rect.height : size.y)
        );
        Update();
    }
}
