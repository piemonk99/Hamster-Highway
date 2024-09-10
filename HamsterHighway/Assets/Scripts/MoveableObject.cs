using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveableObject : MonoBehaviour
{
    private bool draggingObject;
    private Camera mainCamera;
    private Track platformTrack;
    private Rigidbody ballRb;

    private ScrollRect scrollRect;

    [SerializeField] private GameObject trackEndPrefab;
    [SerializeField] private GameObject trackPlatformConnectorPrefab;
    [SerializeField] private GameObject trackObjectPrefab;

    private enum MoveableTypes { Horizontal = 0, Vertical = 1, Rotational = 2 }
    [SerializeField] private MoveableTypes moveableType = MoveableTypes.Horizontal;

    [SerializeField] private float forwardMaxDistance = 2f; //Max distance in positive direction
    [SerializeField] private float backwardMaxDistance = 2f; //Max distance in negative direction

    private Vector3 initialPosition;
    private Vector3 lastPosition;


    private void Awake()
    {
        scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();
        ballRb = GameObject.Find("Hamster").GetComponent<Rigidbody>();

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
        }

        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = 10; //Set the z-distance for screen to world conversion

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

        lastPosition = lerpedPosition;
    }

    private void CheckAndApplyMomentum()
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = currentPosition - lastPosition;

        //Debug.Log($"lastPosition: {lastPosition}, currentPosition: {currentPosition}");

        if (LineIntersectsSphere(lastPosition, currentPosition, ballRb.gameObject.GetComponent<SphereCollider>()))
        {
            Debug.Log($"intersected, direction: {direction}");

            Vector3 platformVelocity = direction; // Use the direction as the momentum
            ballRb.AddForce(platformVelocity, ForceMode.VelocityChange);

            if(moveableType == MoveableTypes.Horizontal)
            {
                ballRb.transform.position = currentPosition + (new Vector3(1f * Mathf.Sign(direction.x), 0, 0) );
            }
            else if(moveableType == MoveableTypes.Vertical)
            {
                ballRb.transform.position = currentPosition + (new Vector3(0, 1f * Mathf.Sign(direction.y), 0));
            }

            
        }
    }

    private bool LineIntersectsSphere(Vector3 lineStart, Vector3 lineEnd, Collider sphereCollider)
    {
        Vector3 sphereCenter = sphereCollider.bounds.center;
        float sphereRadius = sphereCollider.bounds.extents.magnitude;

        Vector3 lineDir = (lineEnd - lineStart);
        Vector3 closestPoint = Vector3.Project(sphereCenter - lineStart, lineDir.normalized) + lineStart;

        // Ensure that the closest point lies on the line segment between lineStart and lineEnd
        float dotProduct = Vector3.Dot(closestPoint - lineStart, lineEnd - lineStart);
        if (dotProduct < 0 || dotProduct > lineDir.sqrMagnitude)
        {
            return false;
        }

        // Check if the distance from the sphere's center to the closest point is within the radius
        return Vector3.Distance(sphereCenter, closestPoint) <= sphereRadius;
    }

    public void OnMouseUp()
    {
        if (draggingObject)
        {
            draggingObject = false;
            scrollRect.horizontal = true;
        }
    }
}
