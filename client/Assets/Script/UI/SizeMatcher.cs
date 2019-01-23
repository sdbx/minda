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
        targetSize = new Vector2(targetSize.x + horizontal, targetSize.y + vertical);

        Debug.Log(targetSize != rectTransform.rect.size);
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
