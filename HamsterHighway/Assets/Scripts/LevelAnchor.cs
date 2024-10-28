using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAnchor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(-Input.gyro.rotationRate.x, -Input.gyro.rotationRate.y, Input.gyro.rotationRate.z) * 180 / MathF.PI * Time.deltaTime);
    }
}
