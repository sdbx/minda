using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionSelectingArrow : MonoBehaviour
{

    public int direction = 0;
    public BallSelection _ballSelection;
    public Vector3 _originPosition = new Vector3(0, 0, -5);
    public bool working = false;
    public bool isPushing = false;
    public float originDistance = 0.1f;

    private float GetDistanceFromOrigin(Vector2 mousePosition)
    {
        float angle = GetRad();
        return mousePosition.x * Mathf.Cos(angle) + mousePosition.y * Mathf.Sin(angle);
    }

    private Vector3 GetArrowPosition(float distance)
    {
        float angle = GetRad();

        Vector3 arrowPosition = new Vector3();

        arrowPosition.x = distance * Mathf.Cos(angle);
        arrowPosition.y = distance * Mathf.Sin(angle);
        arrowPosition += _originPosition;

        return arrowPosition;
    }

    public float GetRad()
    {
        return -direction * Mathf.PI / 3;
    }
    public float GetDegree()
    {
        return -direction * 60 + 90;
    }
    void Start()
    {

    }

    void Update()
    {
        if (isPushing && Input.GetMouseButtonUp(0))
        {
            isPushing = false;
        }

        if (working)
        {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, GetDegree());
            if (isPushing)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                gameObject.transform.position = GetArrowPosition(GetDistanceFromOrigin(mousePos));
            }
            else
            {
                gameObject.transform.position = GetArrowPosition(originDistance);
            }
        }
    }


    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && !isPushing)
        {
            isPushing = true;
        }
    }

}
