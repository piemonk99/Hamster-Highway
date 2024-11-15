using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAnchor : MonoBehaviour
{
    [SerializeField] bool mouseDrag;

    Vector3 lastMousePosition;

    void Update()
    {
        if (!mouseDrag)
            transform.Rotate(new Vector3(-Input.gyro.rotationRate.x, -Input.gyro.rotationRate.y, Input.gyro.rotationRate.z) * 180 / MathF.PI * Time.deltaTime);
        else if (Input.GetMouseButton(0))
            transform.rotation *= Quaternion.Euler((Input.mousePosition.y - lastMousePosition.y) * 10 * Time.deltaTime, 0, 0) * Quaternion.Euler(0, (Input.mousePosition.x - lastMousePosition.x) * 10 * Time.deltaTime, 0);

        lastMousePosition = Input.mousePosition;
    }
}
