using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hamster : MonoBehaviour
{
    private Rigidbody rigidbody;

    private float currentMaxForwardSpeed;
    private float maxForwardSpeed = 2.0f;
    private float maxDownhillForwardSpeed = 10f;
    private float minimumForwardSpeed = 1.0f;

    private float previousYPosition;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        

        ConstantForce constantForce = gameObject.AddComponent<ConstantForce>();
        constantForce.force = new Vector3(1, 0, 0);

        
    }

    private void Start()
    {
        previousYPosition = transform.position.y;
    }

    private void FixedUpdate()
    {

        //Sets current maximum forward speed depending on conditions
        float currentYPosition = transform.position.y;
         if (currentYPosition < previousYPosition) //Going downhill
        {
            currentMaxForwardSpeed = maxDownhillForwardSpeed;

        }
        else //Flat or going uphill
        {
            currentMaxForwardSpeed = Mathf.Lerp(rigidbody.velocity.x, maxForwardSpeed, 0.1f);
        }
        previousYPosition = currentYPosition;


        //Clamps rigidbody's speed between minimum and maximum
        Vector3 velocity = rigidbody.velocity;
        if (velocity.x < minimumForwardSpeed)
        {
            velocity.x = minimumForwardSpeed;
            rigidbody.velocity = velocity;
        }
        else if (velocity.x > currentMaxForwardSpeed)
        {
            velocity.x = currentMaxForwardSpeed;
            rigidbody.velocity = velocity;
        }
    }
}
