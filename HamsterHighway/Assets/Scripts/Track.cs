using UnityEngine;

public class Track
{
    private Transform trackEndA;
    private Transform trackEndB;
    private Transform trackPlatformConnector;
    private Transform trackObject;

    public Track(Vector3 endAPosition, Vector3 endBPosition, GameObject trackEndPrefab, GameObject trackPlatformConnectorPrefab, GameObject trackObjectPrefab, Transform parent)
    {
        // Instantiate the TrackEnd objects
        trackEndA = GameObject.Instantiate(trackEndPrefab, parent).transform;
        trackEndA.position = endAPosition;

        trackEndB = GameObject.Instantiate(trackEndPrefab, parent).transform;
        trackEndB.position = endBPosition;

        // Instantiate the TrackPlatformConnector at TrackEndA position
        trackPlatformConnector = GameObject.Instantiate(trackPlatformConnectorPrefab, parent).transform;
        trackPlatformConnector.position = endAPosition;

        // Instantiate the TrackObject (cylinder or cube) and place it between the TrackEnds
        trackObject = GameObject.Instantiate(trackObjectPrefab, parent).transform;
        UpdateTrackObject();
    }

    private void UpdateTrackObject()
    {
        // Calculate direction and distance between TrackEnds
        Vector3 direction = trackEndB.position - trackEndA.position;
        float distance = direction.magnitude;

        // Position the trackObject at the midpoint
        trackObject.position = (trackEndA.position + trackEndB.position) / 2f;

        // Rotate the trackObject to align with the direction between TrackEnds
        trackObject.rotation = Quaternion.LookRotation(direction);

        // Scale the trackObject to match the distance between the TrackEnds
        trackObject.localScale = new Vector3(trackObject.localScale.x, trackObject.localScale.y, distance - .45f);
    }

    public void UpdateConnectorPosition(Vector3 newPosition)
    {
        // Update the TrackPlatformConnector's position
        trackPlatformConnector.position = newPosition + new Vector3(0, 0, -1);
    }
}
