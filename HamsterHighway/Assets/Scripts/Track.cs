/*using UnityEngine;

public class Track
{
    private Transform trackEndA; // Pivot point
    private Transform trackEndB; // Moving end
    private Transform trackPlatformConnector;
    private Transform trackObject;

    private bool isRotational;

    // Constructor for linear movement (existing)
    

    // Constructor for rotational platforms
    public Track(Vector3 pivotPosition, Vector3 movingEndPosition, GameObject trackEndPrefab, GameObject trackPlatformConnectorPrefab, GameObject trackObjectPrefab, Transform parent, bool isRotational)
    {
        Initialize(pivotPosition, movingEndPosition, trackEndPrefab, trackPlatformConnectorPrefab, trackObjectPrefab, parent, isRotational);
    }

    private void Initialize(Vector3 endAPosition, Vector3 endBPosition, GameObject trackEndPrefab, GameObject trackPlatformConnectorPrefab, GameObject trackObjectPrefab, Transform parent, bool isRotationalPlatform)
    {
        

        isRotational = isRotationalPlatform;
    }

    private void UpdateTrackObject()
    {
        if (isRotational)
        {
            // For rotational platforms, you might want to draw an arc between trackEndA and trackEndB
            // Generate points along the arc and position/scale objects accordingly
            UpdateArcTrackObject();
        }
        else
        {
            // Calculate direction and distance between TrackEnds for linear
            Vector3 direction = trackEndB.position - trackEndA.position;
            float distance = direction.magnitude;

            // Position the trackObject at the midpoint
            trackObject.position = (trackEndA.position + trackEndB.position) / 2f;

            // Rotate the trackObject to align with the direction between TrackEnds
            trackObject.rotation = Quaternion.LookRotation(direction);

            // Scale the trackObject to match the distance between the TrackEnds
            trackObject.localScale = new Vector3(trackObject.localScale.x, trackObject.localScale.y, distance - 0.45f);
        }
    }

    private void UpdateArcTrackObject()
    {
        // Calculate the distance between trackEndA and trackEndB
        Vector3 direction = trackEndB.position - trackEndA.position;
        float distance = direction.magnitude;

        // Calculate the midpoint between trackEndA and trackEndB
        Vector3 midpoint = (trackEndA.position + trackEndB.position) / 2f;

        // Set the position of the trackObject (arc) at the midpoint
        trackObject.position = midpoint;

        // Determine the angle of rotation based on the positions of trackEndA and trackEndB
        float angle = Vector3.SignedAngle(Vector3.right, direction, Vector3.forward);

        // Set the rotation of the trackObject to match the direction of the arc
        trackObject.rotation = Quaternion.Euler(0, 0, angle);

        // Set the scale of the arc object to represent the curvature (distance between ends)
        // This assumes the arc object scales along its Z-axis and represents part of a circle
        float arcLength = distance; // The arc length can be modified if you need more curvature
        trackObject.localScale = new Vector3(trackObject.localScale.x, trackObject.localScale.y, arcLength);
    }

    public void UpdateConnectorPosition(Vector3 newPosition)
    {
        // Update the TrackPlatformConnector's position
        trackPlatformConnector.position = newPosition + new Vector3(0, 0, -1);
    }
}*/

using UnityEngine;

public class Track
{
    private Transform trackEndA;
    private Transform trackEndB;
    private Transform trackPlatformConnector;
    private Transform trackObject;

    private float arcRadius;
    private float arcAngle;


    public Track(Vector3 endAPosition, Vector3 endBPosition, GameObject trackEndPrefab, GameObject trackPlatformConnectorPrefab, GameObject trackObjectPrefab, Transform parent)
    {
        //Instantiate the TrackEnd objects
        trackEndA = GameObject.Instantiate(trackEndPrefab, parent).transform;
        trackEndA.position = endAPosition;

        trackEndB = GameObject.Instantiate(trackEndPrefab, parent).transform;
        trackEndB.position = endBPosition;

        //Instantiate the TrackPlatformConnector at TrackEndA position
        trackPlatformConnector = GameObject.Instantiate(trackPlatformConnectorPrefab, parent).transform;
        trackPlatformConnector.position = endAPosition;

        //Instantiate the TrackObject and place it between the TrackEnds
        trackObject = GameObject.Instantiate(trackObjectPrefab, parent).transform;
        UpdateTrackObject();
    }

    public Track(Vector3 pivotPoint, float radius, float forwardDegrees, float backwardDegrees, GameObject trackEndPrefab, GameObject trackPlatformConnectorPrefab, GameObject trackObjectPrefab, Transform parent)
    {
        arcRadius = radius;

        //Calculate the positions of the track ends along the arc
        trackEndA = GameObject.Instantiate(trackEndPrefab, parent).transform;
        trackEndA.position = CalculateArcPoint(pivotPoint, radius, -backwardDegrees);

        trackEndB = GameObject.Instantiate(trackEndPrefab, parent).transform;
        trackEndB.position = CalculateArcPoint(pivotPoint, radius, forwardDegrees);

        //Instantiate the TrackPlatformConnector at the pivot point
        trackPlatformConnector = GameObject.Instantiate(trackPlatformConnectorPrefab, parent).transform;
        trackPlatformConnector.position = pivotPoint;

        //Instantiate the TrackObject and generate the curved shape
        trackObject = GameObject.Instantiate(trackObjectPrefab, parent).transform;
        UpdateArcTrackObject();
    }

    private Vector3 CalculateArcPoint(Vector3 pivotPoint, float radius, float angleInDegrees)
    {
        //Convert the angle to radians
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        //Calculate the position of the point on the arc
        float x = pivotPoint.x + radius * Mathf.Cos(angleInRadians);
        float y = pivotPoint.y + radius * Mathf.Sin(angleInRadians);

        return new Vector3(x, y, pivotPoint.z);
    }

    private void UpdateTrackObject()
    {
        //Calculate direction and distance between TrackEnds for linear
        Vector3 direction = trackEndB.position - trackEndA.position;
        float distance = direction.magnitude;

        //Position the trackObject at the midpoint, rotate it to align with the trackEnds, and scale it to meet both trackEnds
        trackObject.position = (trackEndA.position + trackEndB.position) / 2f;
        trackObject.rotation = Quaternion.LookRotation(direction);
        trackObject.localScale = new Vector3(trackObject.localScale.x, trackObject.localScale.y, distance - 0.45f);
    }

    private void UpdateArcTrackObject()
    {
        LineRenderer lineRenderer = trackObject.gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = trackObject.gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 50; //Number of segments in the arc
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float t = i / (float)(lineRenderer.positionCount - 1);
            float angle = Mathf.Lerp(-arcAngle / 2, arcAngle / 2, t); //Covers the full arc
            Vector3 arcPosition = CalculateArcPoint(trackPlatformConnector.position, arcRadius, angle);
            lineRenderer.SetPosition(i, arcPosition);
        }
    }

    //Update the position of the connector
    public void UpdateConnectorPosition(Vector3 newPosition)
    {
        trackPlatformConnector.position = newPosition - Vector3.forward;
    }

    public void UpdateConnectorPosition(Vector3 pivotPoint, float arcAngle)
    {
        trackPlatformConnector.position = CalculateArcPoint(pivotPoint, arcRadius, arcAngle) - Vector3.forward;
    }
}

