using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float boostForceMultiplier = 1f;
    private enum Direction { Up = 0, Forward = 1 }
    [SerializeField] private Direction direction = Direction.Up;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

        if (rb == null)
            return;

        switch (direction)
        {
            case Direction.Up:
                rb.velocity = new Vector3 (rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(new Vector3(0, 450f * boostForceMultiplier, 0));
                break;
            case Direction.Forward:
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(new Vector3(450f * boostForceMultiplier, 0, 0));
                break;
        }
    }
}
