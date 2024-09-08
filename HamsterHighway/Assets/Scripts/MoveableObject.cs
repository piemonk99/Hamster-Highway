using UnityEngine;
using UnityEngine.UI;

public class MoveableObject : MonoBehaviour
{
    private bool draggingObject;
    private Camera mainCamera;
    private Track platformTrack;

    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private GameObject trackEndPrefab;
    [SerializeField] private GameObject trackPlatformConnectorPrefab;
    [SerializeField] private GameObject trackObjectPrefab;

    private enum MoveableTypes { Horizontal = 0, Vertical = 1, Rotational = 2 }
    [SerializeField] private MoveableTypes moveableType = MoveableTypes.Horizontal;

    [SerializeField] private float forwardMaxDistance = 2f; //Max distance in positive direction
    [SerializeField] private float backwardMaxDistance = 2f; //Max distance in negative direction

    private Vector3 initialPosition;

    private void Awake()
    {
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
    }

    private void Update()
    {
        Debug.Log(scrollRect.viewport.position.x);
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

        transform.position = newPosition;

        platformTrack.UpdateConnectorPosition(newPosition);
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
