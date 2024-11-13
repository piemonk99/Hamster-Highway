using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public bool MayDrag = true;
    [SerializeField] private Transform ballTransform; // Reference to the ball's transform

    private Camera cam;
    private Vector3 dragPosition;
    private bool followBall; // Determines if the camera should follow the ball

    void Start()
    {
        cam = GetComponent<Camera>();

        // Load the followBall option from the saved options
        Options.Read();
        followBall = Options.Instance.cameraFollowBall;

        // Disable dragging if following the ball
        MayDrag = !followBall;
    }

    void Update()
    {
        if (followBall)
        {
            FollowBall();
        }
        else
        {
            HandleDragging();
        }
    }

    private void FollowBall()
    {
        if (ballTransform != null)
        {
            // Calculate the target position with locked Y and offset X
            Vector3 targetPosition = new Vector3(
                ballTransform.position.x + 6, // Offset by 8 on X
                4.5f,                         // Lock Y at 4.5
                transform.position.z          // Maintain current Z value
            );

            // Smoothly follow the ball's position
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5);
        }
    }

    private void HandleDragging()
    {
        if (!MayDrag || EventSystem.current.IsPointerOverGameObject() || !enabled)
            return;

        if (Input.GetMouseButtonDown(0))
            dragPosition = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Vector3 currentDragPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += new Vector3(dragPosition.x - currentDragPosition.x, 0, 0);
        }
    }
}
