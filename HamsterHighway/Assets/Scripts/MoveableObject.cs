using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveableObject : MonoBehaviour
{
    private bool draggingObject;
    private Camera mainCamera;
    private Track platformTrack;

    private ScrollRect scrollRect;

    [SerializeField] private GameObject trackEndPrefab;
    [SerializeField] private GameObject trackPlatformConnectorPrefab;
    [SerializeField] private GameObject trackObjectPrefab;

    private enum MoveableTypes { Horizontal = 0, Vertical = 1 }
    [SerializeField] private MoveableTypes moveableType = MoveableTypes.Horizontal;

    [SerializeField] private float forwardMaxDistance = 2f; // Max distance in positive direction
    [SerializeField] private float backwardMaxDistance = 2f; // Max distance in negative direction

    [SerializeField] private float acceleration = 30f; // The acceleration the platform applies to move towards its destination

    private Vector3 initialPosition;
    private Vector3 destination; // Destination
    private bool movingToDestination; // Flag to check if we should continue lerping after releasing the mouse

    private Rigidbody rb;

    private void Start()
    {
        scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();

        rb = GetComponent<Rigidbody>();

        mainCamera = Camera.main;
        initialPosition = transform.position;

        Vector3 forwardDistanceVector, backwardDistanceVector;

        if (moveableType == (int)MoveableTypes.Horizontal)
        {
            backwardDistanceVector = new Vector3(backwardMaxDistance, 0, 0);
            forwardDistanceVector = new Vector3(forwardMaxDistance, 0, 0);
        }
        else
        {
            backwardDistanceVector = new Vector3(0, backwardMaxDistance, 0);
            forwardDistanceVector = new Vector3(0, forwardMaxDistance, 0);
        }
        platformTrack = new Track(initialPosition - backwardDistanceVector, initialPosition + forwardDistanceVector, trackEndPrefab, trackPlatformConnectorPrefab, trackObjectPrefab, transform.parent);
        platformTrack.UpdateConnectorPosition(transform.position);

        destination = transform.position;
    }

    public void OnMouseDrag()
    {
        if (!draggingObject)
        {
            draggingObject = true;
            scrollRect.horizontal = false;
            movingToDestination = false; // Stop automatic lerping when user drags
        }

        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = 10; // Set the z-distance for screen to world conversion

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        Vector3 newPosition = rb.position;

        switch (moveableType)
        {
            case MoveableTypes.Horizontal:
                newPosition.x = worldPosition.x;
                newPosition.x = Mathf.Clamp(newPosition.x, initialPosition.x - backwardMaxDistance + scrollRect.viewport.position.x, initialPosition.x + forwardMaxDistance + scrollRect.viewport.position.x);
                break;

            case MoveableTypes.Vertical:
                newPosition.y = worldPosition.y;
                newPosition.y = Mathf.Clamp(newPosition.y, initialPosition.y - backwardMaxDistance, initialPosition.y + forwardMaxDistance);
                break;

            default:
                newPosition = worldPosition;
                break;
        }

        destination = newPosition;

        // // Get the Rigidbody component
        // Rigidbody rb = GetComponent<Rigidbody>();

        // // Smoothly move the Rigidbody to the new position using Lerp
        // Vector3 lerpedPosition = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 8f);

        // // Move the Rigidbody to the interpolated position
        // rb.MovePosition(lerpedPosition);

        // // Update the parent transform's position to match the Rigidbody's position
        // transform.position = rb.position;

        // platformTrack.UpdateConnectorPosition(lerpedPosition);

        // lastPosition = newPosition; // Store the last position for use after releasing the mouse
    }

    public void OnMouseUp()
    {
        if (draggingObject)
        {
            draggingObject = false;
            scrollRect.horizontal = true;
            movingToDestination = true; // Continue moving to the destination after mouse release
        }
    }

    private void Update()
    {
        // Move to the destination
        if (draggingObject || movingToDestination)
        {
            MoveToDestination();
        }
    }

    private void MoveToDestination()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation | (moveableType == MoveableTypes.Horizontal ? RigidbodyConstraints.FreezePositionY : RigidbodyConstraints.FreezePositionX) | RigidbodyConstraints.FreezePositionZ;     
        // Accelerate smoothly
        // float actualSpeed = Mathf.Lerp(rb.velocity.magnitude, acceleration, Time.deltaTime * 8);
        rb.velocity += (destination - transform.position).normalized * acceleration * Time.deltaTime;

        // // Lerp the platform towards the last position
        // Vector3 lerpedPosition = Vector3.Lerp(transform.position, lastPosition, Time.deltaTime * 8f);

        // // Get the Rigidbody component
        // Rigidbody rb = GetComponent<Rigidbody>();

        // // Move the Rigidbody to the interpolated position
        // rb.MovePosition(lerpedPosition);

        // Update the parent transform's position to match the Rigidbody's position
        transform.position = rb.position;

        platformTrack.UpdateConnectorPosition(transform.position);

        // Stop moving if the platform is close enough to the destination
        if (Vector3.Distance(transform.position, destination) <= rb.velocity.magnitude * Time.fixedDeltaTime || ((destination - transform.position).normalized - rb.velocity.normalized).magnitude > 0.01f)
        {
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            movingToDestination = false;
        }
    }
}
