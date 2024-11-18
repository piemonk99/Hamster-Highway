using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveableObject : MonoBehaviour
{
    private bool draggingObject;
    private Camera mainCamera;
    private Track platformTrack;

    [SerializeField] private GameObject trackEndPrefab;
    [SerializeField] private GameObject trackObjectPrefab;

    public enum MoveableTypes { Horizontal = 0, Vertical = 1 }
    [SerializeField] public MoveableTypes moveableType = MoveableTypes.Horizontal;

    [SerializeField] public float forwardMaxDistance = 2f; // Max distance in positive direction
    [SerializeField] public float backwardMaxDistance = 2f; // Max distance in negative direction

    [SerializeField] private float acceleration = 30f; // The acceleration the platform applies to move towards its destination

    private Vector3 initialPosition;
    private Vector3 destination; // Destination
    private bool movingToDestination; // Flag to check if we should continue lerping after releasing the mouse

    private Vector3 mouseDownPosition;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        mainCamera = Camera.main;
        initialPosition = transform.localPosition;

        Vector3 forwardDistanceVector, backwardDistanceVector;

        if (moveableType == (int)MoveableTypes.Horizontal)
        {
            backwardDistanceVector = new Vector3(backwardMaxDistance, 0, 0);
            forwardDistanceVector = new Vector3(forwardMaxDistance, 0, 0);
        }
        else
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
            gameObject.transform.GetChild(1).gameObject.SetActive(true);

            backwardDistanceVector = new Vector3(0, backwardMaxDistance, 0);
            forwardDistanceVector = new Vector3(0, forwardMaxDistance, 0);
        }
        platformTrack = new Track(initialPosition - backwardDistanceVector, initialPosition + forwardDistanceVector, trackEndPrefab, trackObjectPrefab, transform.parent);

        destination = initialPosition;
    }

    public void OnMouseDrag()
    {
        if (!draggingObject)
        {
            draggingObject = true;

            if (mainCamera.GetComponent<CameraController>() != null)
                mainCamera.GetComponent<CameraController>().MayDrag = false;

            movingToDestination = false; // Stop automatic lerping when user drags
        }

        if (GameManager.Instance.IsARMode())
        {
            Vector3 mouseDirection = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1)).normalized;
            destination = ClampToTrack(transform.parent.InverseTransformPoint(ARUtil.RayIntersect(mainCamera.transform.position, mouseDirection, transform.position, transform.forward)));
        }
        else
            destination = ClampToTrack(transform.parent.InverseTransformPoint(mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.WorldToScreenPoint(transform.position).z))));

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

    private Vector3 ClampToTrack(Vector3 position)
    {
        Vector3 newPosition = transform.localPosition;

        switch (moveableType)
        {
            case MoveableTypes.Horizontal:
                newPosition.x = position.x;
                newPosition.x = Mathf.Clamp(newPosition.x, initialPosition.x - backwardMaxDistance, initialPosition.x + forwardMaxDistance);
                break;

            case MoveableTypes.Vertical:
                newPosition.y = position.y;
                newPosition.y = Mathf.Clamp(newPosition.y, initialPosition.y - backwardMaxDistance, initialPosition.y + forwardMaxDistance);
                break;

            default:
                newPosition = position;
                break;
        }

        return newPosition;
    }

    public void OnMouseUp()
    {
        if (draggingObject)
        {
            draggingObject = false;

            if (mainCamera.GetComponent<CameraController>() != null)
                mainCamera.GetComponent<CameraController>().MayDrag = true;
            
            movingToDestination = true; // Continue moving to the destination after mouse release
        }
    }

    private void Update()
    {
        if (!draggingObject)
        {
            if (Input.GetMouseButtonDown(0))
                mouseDownPosition = Input.mousePosition;
            else if (Input.GetMouseButtonUp(0) && Vector3.Distance(mouseDownPosition, Input.mousePosition) < 0.1)
            {
                Vector3 localClickPosition;

                if (GameManager.Instance.IsARMode())
                {
                    Vector3 mouseDirection = (mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1)) - mainCamera.transform.position).normalized;
                    localClickPosition = transform.parent.InverseTransformPoint(ARUtil.RayIntersect(mainCamera.transform.position, mouseDirection, transform.position, transform.forward));
                }
                else
                {
                    localClickPosition = transform.parent.InverseTransformPoint(mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.WorldToScreenPoint(transform.position).z)));
                    localClickPosition.z = transform.localPosition.z;
                }

                Vector3 clampedPosition = ClampToTrack(localClickPosition);

                if (Vector3.Distance(localClickPosition, clampedPosition) < 0.5)
                {
                    destination = clampedPosition;
                    movingToDestination = true;
                }
            }
        }

        // Move to the destination
        if (draggingObject || movingToDestination)
        {
            MoveToDestination();
        }
    }

    private void MoveToDestination()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation | (moveableType == MoveableTypes.Horizontal ? RigidbodyConstraints.FreezePositionY : (RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ));
        // Accelerate smoothly
        // float actualSpeed = Mathf.Lerp(rb.velocity.magnitude, acceleration, Time.deltaTime * 8);
        rb.velocity += transform.parent.TransformDirection((destination - transform.localPosition).normalized * acceleration * Time.deltaTime);

        if (rb.velocity.magnitude > 8f)
            rb.velocity = rb.velocity.normalized * 8;

        // // Lerp the platform towards the last position
        // Vector3 lerpedPosition = Vector3.Lerp(transform.position, lastPosition, Time.deltaTime * 8f);

        // // Get the Rigidbody component
        // Rigidbody rb = GetComponent<Rigidbody>();

        // // Move the Rigidbody to the interpolated position
        // rb.MovePosition(lerpedPosition);

        // Update the parent transform's position to match the Rigidbody's position
        transform.position = rb.position;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);

        // Stop moving if the platform is close enough to the destination
        if (Vector3.Distance(transform.localPosition, destination) <= rb.velocity.magnitude * Time.fixedDeltaTime || ((destination - transform.localPosition).normalized - transform.parent.InverseTransformDirection(rb.velocity).normalized).magnitude > 0.01f)
        {
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            movingToDestination = false;
        }
    }
}
