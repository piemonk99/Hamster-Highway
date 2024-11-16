using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float boostForceMultiplier = 1f;
    private Animator animator;
    private AudioSource audioSource;

    public enum Direction { Up = 0, Forward = 1, Down = 2 }
    [SerializeField] public Direction direction = Direction.Up;

    private bool used;

    public bool inverted;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + (inverted ? -transform.up : transform.up) * 5);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used)
            return;

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
                rb.AddForce((inverted ? -transform.up : transform.up) * 450f * boostForceMultiplier);
                break;
            case Direction.Down:
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(new Vector3(0, -450f * boostForceMultiplier, 0));
                break;
        }

        //Plays appropriate animation depending on boost pad type
        if (animator.HasState(0, Animator.StringToHash("Bounce"))) animator.Play("Bounce");
        else animator.Play("Strong Bounce");

        audioSource.volume = Options.Instance.Volume;
        audioSource.Play();

        used = true;
    }
}
