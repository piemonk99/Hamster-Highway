using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float boostForceMultiplier = 1f;
    private Animator animator;

    private enum Direction { Up = 0, Forward = 1 }
    [SerializeField] private Direction direction = Direction.Up;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

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
                rb.AddForce(new Vector3(450f * boostForceMultiplier, 0, 0));
                break;
        }

        //Plays appropriate animation depending on boost pad type
        if (animator.HasState(0, Animator.StringToHash("Bounce"))) animator.Play("Bounce");
        else animator.Play("Strong Bounce");
    }
}
