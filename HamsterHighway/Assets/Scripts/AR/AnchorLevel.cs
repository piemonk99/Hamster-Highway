using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AnchorLevel : MonoBehaviour
{
    public GameObject levelObject;   // The object to be placed on the plane
    public GameObject gameManagerObject; // Optional GameManager reference
    private ARPlaneManager planeManager; // Manages plane detection
    private bool isLevelPlaced = false;  // Ensures we only place the object once

    void Start()
    {
        // Get reference to ARPlaneManager
        planeManager = FindObjectOfType<ARPlaneManager>();

        // Register for the planesChanged event to detect new planes
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDestroy()
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }

    // Callback for when AR planes are updated
    private void OnPlanesChanged(ARPlanesChangedEventArgs planes)
    {
        if (!isLevelPlaced && planes.added.Count > 0)
        {
            // Get the first detected plane
            ARPlane firstDetectedPlane = planes.added[0];

            // Place the level object on top of the plane
            PlaceLevelOnPlane(firstDetectedPlane);
            isLevelPlaced = true; // Set flag to prevent placing again

            // Disable plane detection after the object is placed
            planeManager.enabled = false;
        }
    }

    // Function to place the level on the plane
    private void PlaceLevelOnPlane(ARPlane plane)
    {
        // Get the center position of the plane
        Vector3 planeCenter = plane.center;

        // Adjust the position of the level object to sit slightly above the plane
        levelObject.transform.position = new Vector3(planeCenter.x, planeCenter.y + 0.1f, planeCenter.z);
        levelObject.transform.rotation = Quaternion.identity;  // Reset rotation to avoid tilting
        levelObject.transform.localScale = Vector3.one;        // Ensure scale is set to 1

        // Activate the level object and game manager (if applicable)
        levelObject.SetActive(true);
        if (gameManagerObject != null)
        {
            gameManagerObject.SetActive(true);
        }
    }

}
