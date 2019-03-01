using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeMatcher : MonoBehaviour
{
    [SerializeField]
    private RectTransform target;
    private RectTransform rectTransform;

    [SerializeField]
    private float vertical;
    [SerializeField]
    private float horizontal;

    //ForceSet
    public bool forceSetMode = false;
    [SerializeField]
    private Vector2 forceSetSize;

    [SerializeField]
    private Vector2 scale = new Vector2(1,1);


    void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    void Update()
    {   
        if(forceSetMode)
        {
            rectTransform.sizeDelta = forceSetSize;
            return;
        }
        var targetSize = target.rect.size;
        var sizeX = targetSize.x + horizontal;
        var sizeY = targetSize.y + vertical;
        sizeX *= scale.x;
        sizeY*=scale.y;

        if(horizontal == -1)
        {
            sizeX = rectTransform.rect.width;
        }
        if(vertical == -1)
        {
            sizeY = rectTransform.rect.height;
        }
        targetSize = new Vector2(sizeX,sizeY);

        if(targetSize != rectTransform.rect.size)
        {
            rectTransform.sizeDelta = targetSize;
        }
    }

    public void ForceSet(Vector2 size)
    {
        forceSetMode = true;
        forceSetSize = new Vector2(
            (size.x==-1?rectTransform.rect.width:size.x),
            (size.y==-1?rectTransform.rect.height:size.y)
        );
        Update();
    }
}
