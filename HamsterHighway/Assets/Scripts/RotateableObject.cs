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

    // private Vector3 pivotPoint;
    // private Vector3 radiusVector;
    private Quaternion destination;
    private bool movingToDestination;

    private Vector3 mouseDownPosition;

    private Rigidbody rb;

    public bool inverted;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        mainCamera = Camera.main;

        //Gets pivot point and radius
        // radiusVector = new Vector3(transform.localScale.x / 2, 0, 0);
        // pivotPoint = transform.localPosition - radiusVector;
        rb.centerOfMass = Vector3.zero;

        float clampedStartingAngle = Mathf.Clamp(startingAngle, -backwardMaxAngle, forwardMaxAngle);
        destination = Quaternion.Euler(0, 0, inverted ? clampedStartingAngle - 180 : clampedStartingAngle);

        //Creates track
        platformTrack = new Track(transform.localPosition, transform.localScale.x, forwardMaxAngle, backwardMaxAngle, trackEndPrefab, trackObjectPrefab, transform.parent);

        movingToDestination = true;

        transform.localRotation = destination;
    }

    void OnMouseDrag()
    {
        if (!draggingObject)
        {
            draggingObject = true;

            if (mainCamera.GetComponent<CameraController>() != null)
                mainCamera.GetComponent<CameraController>().MayDrag = false;

            movingToDestination = false;
        }

        Vector3 mouseDirection = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1)).normalized;
        HandleRotation(transform.parent.InverseTransformPoint(ARUtil.RayIntersect(mainCamera.transform.position, mouseDirection, transform.position, transform.forward)));
    }

    private void HandleRotation(Vector3 relativePosition)
    {
        Vector3 directionFromPivot = relativePosition - transform.localPosition;
        float targetAngle = Mathf.Atan2(directionFromPivot.y, directionFromPivot.x) * Mathf.Rad2Deg;

        //Clamp the target angle within the allowed range
        float clampedTargetAngle = Mathf.Clamp(targetAngle, -backwardMaxAngle, forwardMaxAngle);

        //Move the platform smoothly along the arc
        // MoveToDestination(clampedTargetAngle);

        //Store the clamped target angle for when the user lets go
        destination = Quaternion.Euler(0, 0, inverted ? clampedTargetAngle - 180 : clampedTargetAngle);
    }

    public void OnMouseUp()
    {
        if (draggingObject)
        {
            draggingObject = false;

            if (mainCamera.GetComponent<CameraController>() != null)
                mainCamera.GetComponent<CameraController>().MayDrag = true;

            //Start lerping to the last position after releasing the mouse
            movingToDestination = true;
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
                Vector3 mouseDirection = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1)).normalized;
                Vector3 localClickPosition = transform.parent.InverseTransformPoint(ARUtil.RayIntersect(mainCamera.transform.position, mouseDirection, transform.position, transform.forward));
                Vector3 clampedPosition = transform.localPosition + (localClickPosition - transform.localPosition).normalized * transform.localScale.x;
                Vector3 directionFromPivot = localClickPosition - transform.localPosition;
                float angle = Mathf.Atan2(directionFromPivot.y, directionFromPivot.x) * Mathf.Rad2Deg;

                if (angle >= -backwardMaxAngle - 5 && angle <= forwardMaxAngle + 5 && Vector3.Distance(localClickPosition, clampedPosition) < 0.5)
                {
                    HandleRotation(clampedPosition);
                    movingToDestination = true;
                }
            }
        }

        //Continue lerping to the last target angle if the mouse is not being dragged
        if (draggingObject || movingToDestination)
        {
            MoveToDestination(destination);
        }
    }

    private void MoveToDestination(Quaternion targetRotation)
    {
        rb.constraints = RigidbodyConstraints.FreezePosition;
        // Debug.Log($"T={targetRotation.eulerAngles.z} C={transform.localRotation.eulerAngles.z} D={targetRotation.eulerAngles.z - transform.localRotation.eulerAngles.z}");
        float delta = targetRotation.eulerAngles.z - transform.localRotation.eulerAngles.z;

        if (delta > 180)
            delta = -1;
        else if (delta < -180)
            delta = 1;

        rb.angularVelocity += transform.parent.TransformDirection(new Vector3(0, 0, Mathf.Sign(delta) * acceleration * Time.deltaTime));

        //Lerp the current angle to the target angle
        // currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 8f);

        //Calculate the position based on the lerped angle to stay on the arc

        // Vector3 rotatedPosition = pivotPoint + (Quaternion.Euler(0, 0, currentAngle) * radiusVector);
        // rb.MovePosition(rotatedPosition);

        // transform.position = rb.position;

        //Rotate the platform to match the current angle
        // Quaternion targetRotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
        // transform.rotation = targetRotation;
        transform.localRotation = Quaternion.Euler(0, 0, transform.localRotation.eulerAngles.z);
        // Vector3 localAV = transform.parent.TransformDirection(rb.angularVelocity);
        // rb.angularVelocity = transform.parent.InverseTransformDirection(new Vector3(0, 0, localAV.z));

        if (rb.angularVelocity.magnitude > Mathf.PI / 2)
            rb.angularVelocity = rb.angularVelocity.normalized * Mathf.PI / 2;

        float angle = Quaternion.Angle(targetRotation, transform.localRotation);

        if (inverted)
            angle = 180 - angle;

        //Stop lerping if we are close enough to the target angle
        if (angle <= rb.angularVelocity.magnitude * Mathf.Rad2Deg * Time.fixedDeltaTime * 2)
        {
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            movingToDestination = false;
        }
    }
}
