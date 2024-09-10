using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hamster : MonoBehaviour
{
    private Rigidbody rigidbody;

    private float maxNaturalForwardSpeed = 2.0f;
    private float minimumForwardSpeed = 1.0f;

    private float previousYPosition;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        ConstantForce constantForce = gameObject.AddComponent<ConstantForce>();
        constantForce.force = new Vector3(1, 0, 0);

        
    }

    private void Start()
    {
        previousYPosition = transform.position.y;
    }

    private void FixedUpdate()
    {


        //Clamps rigidbody's speed up to a minimum and adds force
        Vector3 velocity = rigidbody.velocity;
        if (velocity.x < minimumForwardSpeed)
        {
            velocity.x = minimumForwardSpeed;
            rigidbody.velocity = velocity;
        }
        else if (velocity.x > maxNaturalForwardSpeed)
        {
            GetComponent<ConstantForce>().force = new Vector3(1, 0, 0);
        }
    }
}
