using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private float speed = 2;
    private float AnimationAngle = 0;
    private float t = 0;

    private void Start()
    {
        //StartCoroutine(startAnimation());
    }

    private IEnumerator startAnimation()
    {
        if (t <= 0.01f) yield return new WaitForSeconds(0.2f);
        transform.RotateAround(Vector3.zero, Vector3.down, -AnimationAngle);
        t += 0.1f;
        AnimationAngle = - 30 * t * t * (t - 3);
        transform.RotateAround(Vector3.zero, Vector3.down, AnimationAngle);
        yield return null;
        if (t >= 2) yield break;
        StartCoroutine(startAnimation());
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.RotateAround(Vector3.zero, Vector3.down, speed * Input.GetAxis("Horizontal"));
        transform.RotateAround(Vector3.zero, new Vector3(-transform.localPosition.z, 0, transform.localPosition.x), speed * (1 - Mathf.Abs(Input.GetAxisRaw("Horizontal"))) * Input.GetAxis("Vertical"));
        if (transform.localPosition.y < 0 || transform.localPosition.y > 19.9f)
            transform.RotateAround(Vector3.zero, new Vector3(transform.localPosition.z, 0, -transform.localPosition.x), speed * (1 - Mathf.Abs(Input.GetAxisRaw("Horizontal"))) * Input.GetAxis("Vertical"));
    }
}