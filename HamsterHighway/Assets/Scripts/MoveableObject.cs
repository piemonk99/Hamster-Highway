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

    private Vector3 initialPosition;
    private Vector3 lastPosition;
    private bool lerpingToLastPosition; // Flag to check if we should continue lerping after releasing the mouse

    private void Awake()
    {
        scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();

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

        lastPosition = transform.position;
    }

    public void OnMouseDrag()
    {
        if (!draggingObject)
        {
            draggingObject = true;
            scrollRect.horizontal = false;
            lerpingToLastPosition = false; // Stop automatic lerping when user drags
        }

        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = 10; // Set the z-distance for screen to world conversion

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        Vector3 newPosition = transform.position;

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

        // Get the Rigidbody component
        Rigidbody rb = GetComponent<Rigidbody>();

        // Smoothly move the Rigidbody to the new position using Lerp
        Vector3 lerpedPosition = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 8f);

        // Move the Rigidbody to the interpolated position
        rb.MovePosition(lerpedPosition);

        // Update the parent transform's position to match the Rigidbody's position
        transform.position = rb.position;

        platformTrack.UpdateConnectorPosition(lerpedPosition);

        lastPosition = lerpedPosition; // Store the last position for use after releasing the mouse
    }

    public void OnMouseUp()
    {
        if (draggingObject)
        {
            draggingObject = false;
            scrollRect.horizontal = true;
            lerpingToLastPosition = true; // Start lerping to the last position after mouse release
        }
    }

    private void Update()
    {
        // Continue lerping to the last position if the mouse is not being dragged
        if (!draggingObject && lerpingToLastPosition)
        {
            LerpToLastPosition();
        }
    }

    private void LerpToLastPosition()
    {
        // Lerp the platform towards the last position
        Vector3 lerpedPosition = Vector3.Lerp(transform.position, lastPosition, Time.deltaTime * 8f);

        // Get the Rigidbody component
        Rigidbody rb = GetComponent<Rigidbody>();

        // Move the Rigidbody to the interpolated position
        rb.MovePosition(lerpedPosition);

        // Update the parent transform's position to match the Rigidbody's position
        transform.position = rb.position;

        platformTrack.UpdateConnectorPosition(lerpedPosition);

        // Stop lerping if the platform is close enough to the last position
        if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
        {
            lerpingToLastPosition = false;
        }
    }
}
