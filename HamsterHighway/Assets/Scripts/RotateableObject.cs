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

    private Vector3 initialPivotPoint;
    private Vector3 pivotPoint;
    private Vector3 radiusVector;
    public float currentAngle;
    private float lastTargetAngle;
    private bool movingToDestination;

    private void Start()
    {
        mainCamera = Camera.main;

        //Gets pivot point and radius
        radiusVector = new Vector3(transform.localScale.x / 2, 0, 0);
        initialPivotPoint = transform.position - radiusVector;

        currentAngle = Mathf.Clamp(startingAngle, -backwardMaxAngle, forwardMaxAngle);
        lastTargetAngle = currentAngle;

        //Creates track
        platformTrack = new Track(initialPivotPoint, transform.localScale.x, forwardMaxAngle, backwardMaxAngle, trackEndPrefab, trackObjectPrefab, transform.parent);

        movingToDestination = true;
    }

    public void OnMouseDrag()
    {
        if (!draggingObject)
        {
            draggingObject = true;
            mainCamera.GetComponent<CameraController>().MayDrag = false;
            movingToDestination = false; //Stop automatic lerping when user drags
        }

        HandleRotation();
    }

    private void HandleRotation()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        Vector3 directionFromPivot = mouseWorldPosition - pivotPoint;
        float targetAngle = Mathf.Atan2(directionFromPivot.y, directionFromPivot.x) * Mathf.Rad2Deg;

        //Clamp the target angle within the allowed range
        float clampedTargetAngle = Mathf.Clamp(targetAngle, -backwardMaxAngle, forwardMaxAngle);

        //Move the platform smoothly along the arc
        MoveToDestination(clampedTargetAngle);

        //Store the clamped target angle for when the user lets go
        lastTargetAngle = clampedTargetAngle;
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
        if (!draggingObject && movingToDestination)
        {
            MoveToDestination(lastTargetAngle);
        }
    }

    private void MoveToDestination(float targetAngle)
    {
        //Lerp the current angle to the target angle
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 8f);

        //Calculate the position based on the lerped angle to stay on the arc
        
        Vector3 rotatedPosition = pivotPoint + (Quaternion.Euler(0, 0, currentAngle) * radiusVector);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.MovePosition(rotatedPosition);

        transform.position = rb.position;

        //Rotate the platform to match the current angle
        Quaternion targetRotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
        transform.rotation = targetRotation;

        //Stop lerping if we are close enough to the last target angle
        if (Mathf.Abs(currentAngle - lastTargetAngle) < 0.1f)
        {
            movingToDestination = false;
        }
    }
}
