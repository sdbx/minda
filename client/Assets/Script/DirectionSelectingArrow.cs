using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionSelectingArrow : MonoBehaviour
{
    
    public BallSelection _ballSelection;
    public Vector2 originPosition = new Vector2(); 
    public bool isPushing = false;
    public float originDistance = 0.1f;
    public float maxDistance;
    private int _direction = 0;
    public float pushingDistance = 0;
    private ArrowsManager _arrowsManager;

    public void SetDirection(int direction)
    {
        _direction = direction;
    }

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
        arrowPosition.z = 7;
        return arrowPosition;
    }

    public float GetRad()
    {
        return -_direction * Mathf.PI / 3;
    }
    public float GetDegree()
    {
        return -_direction * 60 + 90;
    }
    void Start()
    {
        _arrowsManager = gameObject.transform.parent.GetComponent<ArrowsManager>();
    }

    void Update()
    {
        Vector3 parentPostion = gameObject.transform.parent.position;
        if (isPushing && Input.GetMouseButtonUp(0))
        {
            isPushing = false;
            _arrowsManager.pushingArrow = -1;
        }


        gameObject.transform.rotation = Quaternion.Euler(0, 0, GetDegree());

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = GetDistanceFromOrigin(mousePos);

        Debug.Log("거리:" + distance);

        if (isPushing && distance <= originDistance)
        {
            _arrowsManager.pushingArrow = _direction;
            gameObject.transform.position = GetArrowPosition(distance) + parentPostion;
            pushingDistance = distance;
        }
        else
        {
            gameObject.transform.position = GetArrowPosition(originDistance) + parentPostion;
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
