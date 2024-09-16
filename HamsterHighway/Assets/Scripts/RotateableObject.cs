using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RotatableObject : MonoBehaviour
{
    private bool draggingObject;
    private Camera mainCamera;
    private Track platformTrack;

    [SerializeField] private GameObject trackEndPrefab;
    [SerializeField] private GameObject trackObjectPrefab;

    [SerializeField] public float forwardMaxAngle = 90f;
    [SerializeField] public float backwardMaxAngle = 90f;
    [SerializeField] public float startingAngle = 0f;

    [SerializeField] private float acceleration = 30f; // The angular acceleration the platform applies to move towards its destination

    private Vector3 initialPivotPoint;
    private Vector3 pivotPoint;
    private Vector3 radiusVector;
    private Vector3 destination;
    private bool movingToDestination;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        mainCamera = Camera.main;

        //Gets pivot point and radius
        radiusVector = new Vector3(transform.localScale.x / 2, 0, 0);
        initialPivotPoint = -radiusVector;

        destination = Quaternion.Euler(0, 0, Mathf.Clamp(startingAngle, -backwardMaxAngle, forwardMaxAngle)) * Vector3.right;

        //Creates track
        platformTrack = new Track(transform.position + initialPivotPoint, transform.localScale.x, forwardMaxAngle, backwardMaxAngle, trackEndPrefab, trackObjectPrefab, transform.parent);

        movingToDestination = true;
    }

    public void OnMouseDrag()
    {
        if (!draggingObject)
        {
            draggingObject = true;
            mainCamera.GetComponent<CameraController>().MayDrag = false;
            movingToDestination = false;
        }

        debugging = true;
        HandleRotation();
    }

    private void HandleRotation()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        Vector3 directionFromPivot = mouseWorldPosition - rb.worldCenterOfMass;
        float targetAngle = Mathf.Atan2(directionFromPivot.y, directionFromPivot.x) * Mathf.Rad2Deg;

        //Clamp the target angle within the allowed range
        float clampedTargetAngle = Mathf.Clamp(targetAngle, -backwardMaxAngle, forwardMaxAngle);

        //Move the platform smoothly along the arc
        // MoveToDestination(clampedTargetAngle);

        //Store the clamped target angle for when the user lets go
        destination = Quaternion.Euler(0, 0, clampedTargetAngle) * Vector3.right;
    }

    public void OnMouseUp()
    {
        if (draggingObject)
        {
            draggingObject = false;
            mainCamera.GetComponent<CameraController>().MayDrag = true;

            //Start lerping to the last position after releasing the mouse
            movingToDestination = true;
        }
    }

    private void Update()
    {
        pivotPoint = initialPivotPoint;

        //Continue lerping to the last target angle if the mouse is not being dragged
        if (draggingObject || movingToDestination)
        {
            MoveToDestination(destination);
        }
    }

    private void MoveToDestination(Vector3 targetDirection)
    {
        rb.constraints = RigidbodyConstraints.FreezePosition;
        rb.centerOfMass = pivotPoint;
        rb.angularVelocity += new Vector3(0, 0, Mathf.Sign(Vector3.SignedAngle(transform.right, targetDirection, Vector3.forward)) * acceleration * Time.deltaTime);

        if (rb.angularVelocity.magnitude > Mathf.PI / 4)
        {
            rb.angularVelocity = new Vector3(0, 0, Mathf.Clamp(rb.angularVelocity.z, -Mathf.PI / 4, Mathf.PI / 4));
        }

        //Lerp the current angle to the target angle
        // currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 8f);

        //Calculate the position based on the lerped angle to stay on the arc
        
        // Vector3 rotatedPosition = pivotPoint + (Quaternion.Euler(0, 0, currentAngle) * radiusVector);
        // rb.MovePosition(rotatedPosition);

        // transform.position = rb.position;

        //Rotate the platform to match the current angle
        // Quaternion targetRotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
        // transform.rotation = targetRotation;

        //Stop lerping if we are close enough to the last target angle
        if (Vector3.Angle(targetDirection, transform.right) <= rb.angularVelocity.z * Mathf.Rad2Deg * Time.fixedDeltaTime)
        {
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            movingToDestination = false;
        }
    }
}
