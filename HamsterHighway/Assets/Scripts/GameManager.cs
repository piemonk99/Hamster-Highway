using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform ball;

    [SerializeField] private GameObject startSubLevelPrefab;
    [SerializeField] private GameObject[] subLevelPrefabs;
    [SerializeField] private GameObject[] gravityInversionSubLevelPrefabs;
    private Queue<GameObject> subLevelQueue;

    [SerializeField] private int subLevelsToLoadAtOnce = 10;
    [SerializeField] private int subLevelsPerGravityInversions = 5;

    private bool gravityInverted;
    private int subLevelBallIsIn;
    private int currentLevelCenter;

    private void Awake()
    {
        subLevelBallIsIn = 0;
        currentLevelCenter = (int)(subLevelsToLoadAtOnce / 2f);

        subLevelQueue = new Queue<GameObject>();

        for (int i = 1; i < subLevelsToLoadAtOnce; i++)
        {
            subLevelQueue.Enqueue(SpawnNewSubLevel(i));
        }

        // For starting in Unity editor
        Options.Read();
        ScoreTracker.Read();
    }

    private void Update()
    {
        //Check ball's progress and spawn more of the level every time it goes the length of a level
        subLevelBallIsIn = (int)(ball.position.x / 16f);

        while (subLevelBallIsIn >= currentLevelCenter)
        {
            subLevelQueue.Enqueue(SpawnNewSubLevel(currentLevelCenter + (subLevelsToLoadAtOnce / 2)));
            Destroy(subLevelQueue.Dequeue());

            currentLevelCenter++;
        }
    }

    private GameObject SpawnNewSubLevel(int subLevelNumber)
    {
        int xPosition = subLevelNumber * 16;

        GameObject newSubLevel;

        //Swap gravity using a gravity inversion sublevel every certain number of levels
        if (subLevelNumber % subLevelsPerGravityInversions == 0)
        {
            int randomLevelIndex = Random.Range(0, gravityInversionSubLevelPrefabs.Length);
            newSubLevel = Instantiate(gravityInversionSubLevelPrefabs[randomLevelIndex]);
            newSubLevel.transform.position = new Vector3(xPosition, 0, 0);

            if (gravityInverted)
            {
                newSubLevel.transform.localScale = new Vector3(1, -1, 1);
                newSubLevel.transform.position = new Vector3(xPosition, 9, 0);
                FlipBoxCollidersInSubLevel(newSubLevel.transform);
                InvertMoveables(newSubLevel.transform);
            }

            gravityInverted = !gravityInverted;
        }
        else
        {
            int randomLevelIndex = Random.Range(0, subLevelPrefabs.Length);
            newSubLevel = Instantiate(subLevelPrefabs[randomLevelIndex]);
            newSubLevel.transform.position = new Vector3(xPosition, 0, 0);

            if (gravityInverted)
            {
                newSubLevel.transform.localScale = new Vector3(1, -1, 1);
                newSubLevel.transform.position = new Vector3(xPosition, 9, 0);
                FlipBoxCollidersInSubLevel(newSubLevel.transform);
                InvertMoveables(newSubLevel.transform);
            }
        }


        return newSubLevel;
    }

    private void InvertMoveables(Transform subLevel)
    {
        // Find all MoveablePlatformAndTrack and MoveableWallAndTrack objects in the subLevel
        foreach (Transform interactable in subLevel.GetComponentsInChildren<Transform>())
        {
            if (interactable.name == "MoveablePlatformAndTrack" || interactable.name == "MoveableWallAndTrack")
            {
                Transform moveableChild = interactable.Find("MoveablePlatform") ?? interactable.Find("MoveableWall");

                if (moveableChild != null)
                {
                    MoveableObject moveableScript = moveableChild.GetComponent<MoveableObject>();
                    if (moveableScript != null && moveableScript.moveableType == MoveableObject.MoveableTypes.Vertical)
                    {
                        // Swap forward and backward distances
                        float tempDistance = moveableScript.forwardMaxDistance;
                        moveableScript.forwardMaxDistance = moveableScript.backwardMaxDistance;
                        moveableScript.backwardMaxDistance = tempDistance;
                    }
                }
            }
            else if (interactable.name == "RotateablePlatformAndTrack") // Handle rotational platforms
            {
                RotatableObject rotatableScript = interactable.Find("RotateablePlatform")?.GetComponent<RotatableObject>();
                if (rotatableScript != null)
                {
                    // Swap forward and backward angles
                    float tempAngle = rotatableScript.forwardMaxAngle;
                    rotatableScript.forwardMaxAngle = rotatableScript.backwardMaxAngle;
                    rotatableScript.backwardMaxAngle = tempAngle;

                    // Invert the starting angle
                    rotatableScript.startingAngle *= -1;
                    rotatableScript.currentAngle *= -1;

                }
            }
            else if (interactable.name == "Booster Pad" || interactable.name == "Strong Booster Pad")
            {
                BoostPad boostPadScript = interactable.Find("BoostPad")?.GetComponent<BoostPad>();
                if (boostPadScript != null )
                {
                    if (boostPadScript.direction == BoostPad.Direction.Up) boostPadScript.direction = BoostPad.Direction.Down;
                    if (boostPadScript.direction == BoostPad.Direction.Down) boostPadScript.direction = BoostPad.Direction.Up;
                }
            }
        }
    }

    private void FlipBoxCollidersInSubLevel(Transform subLevel)
    {
        // Get all BoxColliders in the sublevel
        BoxCollider[] colliders = subLevel.GetComponentsInChildren<BoxCollider>();

        foreach (BoxCollider collider in colliders)
        {
            // Flip the Y-scale by multiplying the size and center by -1
            Vector3 newSize = collider.size;
            Vector3 newCenter = collider.center;

            // Invert the Y component of the size and center
            newSize.y *= -1;
            newCenter.y *= -1;

            // Apply the flipped size and center to the collider
            collider.size = newSize;
            collider.center = newCenter;
        }
    }

}
