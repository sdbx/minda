using UnityEngine;

public static class MouseUtil
{
    private static Plane floorPlane = new Plane(Vector3.up, Vector3.zero);

    public static Vector3 GetWorld(Camera camera)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        floorPlane.Raycast(ray, out var distance);
        return ray.GetPoint(distance);
    }
}