using UnityEngine;

public class ARUtil
{
    public static Vector3 RayIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 planeCenter, Vector3 planeNormal)
    {
        float distance = -Vector3.Dot(rayOrigin - planeCenter, planeNormal) / Vector3.Dot(rayDirection, planeNormal);
        // Debug.Log($"{-Vector3.Dot(rayOrigin - planeCenter, planeNormal)} / {Vector3.Dot(rayDirection, planeNormal)} = {distance}");
        return rayOrigin + rayDirection * distance;
    }
}
