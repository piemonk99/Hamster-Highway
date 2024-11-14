using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private UIManager uiManager;

    [SerializeField] private Transform levelParent;
    [SerializeField] private GameObject startSubLevelPrefab;
    [SerializeField] private GameObject[] subLevelPrefabs;
    [SerializeField] private GameObject[] gravityInversionSubLevelPrefabs;
    private Queue<GameObject> subLevelQueue;

    [SerializeField] private int subLevelsToLoadAtOnce = 10;
    [SerializeField] private int subLevelsPerGravityInversions = 5;

    [SerializeField] private bool doGeneration = true;

    [SerializeField] private bool arMode = false;

    private bool gravityInverted;
    public int subLevelBallIsIn;
    private int currentLevelCenter;

    private bool flippingEnabled;
    private bool tiltingEnabled;

    private float difficultyBudget = 1.0f;  // Persistent difficulty budget
    private int generatedSublevelCount = 0; // Tracks the total number of sublevels generated
    private Dictionary<int, GameObject> subLevelDictionary;

    private Transform prevLevelTransform;


    // Lists for core sublevels
    private List<GameObject> coreSubLevels;
    private List<GameObject> coreGravityInversionSubLevels;

    private void Awake()
    {
        subLevelBallIsIn = 0;
        currentLevelCenter = (int)(subLevelsToLoadAtOnce / 2f);

        subLevelQueue = new Queue<GameObject>();
        subLevelQueue.Enqueue(startSubLevelPrefab);
        prevLevelTransform = startSubLevelPrefab.transform;
        subLevelDictionary = new Dictionary<int, GameObject>();

        // Initialize core sublevels lists
        coreSubLevels = new List<GameObject>();
        coreGravityInversionSubLevels = new List<GameObject>();

        // Populate core levels based on isCoreLevel flag
        foreach (var subLevel in subLevelPrefabs)
        {
            if (subLevel.GetComponent<SublevelDifficulty>()?.isCoreLevel == true)
                coreSubLevels.Add(subLevel);
        }

        foreach (var subLevel in gravityInversionSubLevelPrefabs)
        {
            if (subLevel.GetComponent<SublevelDifficulty>()?.isCoreLevel == true)
                coreGravityInversionSubLevels.Add(subLevel);
        }

        subLevelQueue.Enqueue(GameObject.Find("StartingSubLevel"));

        if (doGeneration)
        {
            for (int i = 1; i < subLevelsToLoadAtOnce - 1; i++)
            {
                var newSubLevel = SpawnNewSubLevel(i);
                subLevelQueue.Enqueue(newSubLevel);
                subLevelDictionary[i] = newSubLevel;
            }
        }

        // Read options and initialize the game state
        Options.Read();
        ScoreTracker.Read();
        RefreshOptions();
    }

    private void Update()
    {
        if (!doGeneration)
            return;

        Debug.Log($"SubLevelBallIsIn: {subLevelBallIsIn}");

        while (subLevelBallIsIn >= currentLevelCenter)
        {
            var newSubLevelNumber = currentLevelCenter + (subLevelsToLoadAtOnce / 2);
            var newSubLevel = SpawnNewSubLevel(newSubLevelNumber);

            subLevelQueue.Enqueue(newSubLevel);
            subLevelDictionary[newSubLevelNumber] = newSubLevel; // Add to the dictionary

            var oldSubLevel = subLevelQueue.Dequeue();
            Destroy(oldSubLevel);

            // Remove the dequeued sublevel from the dictionary
            foreach (var kvp in subLevelDictionary)
            {
                if (kvp.Value == oldSubLevel)
                {
                    subLevelDictionary.Remove(kvp.Key);
                    break;
                }
            }

            currentLevelCenter++;
        }
    }

    private GameObject SpawnNewSubLevel(int subLevelNumber)
    {
        // int xPosition = subLevelNumber * 16;
        Vector3 position = new Vector3(subLevelNumber * 16, 0, 0);
        Quaternion rotation = Quaternion.identity;

        if (arMode)
        {
            position = new Vector3(prevLevelTransform.localPosition.x, 0, prevLevelTransform.localPosition.z) + prevLevelTransform.localRotation * Vector3.right * 16;
            rotation = prevLevelTransform.localRotation * Quaternion.Euler(0, 180 - (subLevelsToLoadAtOnce - 2) * 180f / subLevelsToLoadAtOnce, 0);
        }

        GameObject newSubLevel;

        Debug.Log($"Budget before choosing sublevel: {difficultyBudget}");

        // Swap gravity using a gravity inversion sublevel every certain number of levels
        if (subLevelNumber % subLevelsPerGravityInversions == 0)
        {
            Debug.Log("Spawning gravity inversion sublevel...");
            newSubLevel = PickAndInstantiateSublevel(coreGravityInversionSubLevels, true);
            PositionAndInvertSubLevel(newSubLevel, position, rotation);
            gravityInverted = !gravityInverted;
        }
        else
        {
            // Pick a sublevel based on the current budget from core levels
            newSubLevel = PickAndInstantiateSublevel(coreSubLevels, false);
            PositionAndInvertSubLevel(newSubLevel, position, rotation);
        }

        // Update generatedSublevelCount and calculate the budget increment based on it
        generatedSublevelCount++;
        float budgetIncrease = 1.0f + (generatedSublevelCount * 16 / 100.0f); // Using generated count for difficulty growth
        difficultyBudget += budgetIncrease;

        // Clamp difficultyBudget between -7 and 12
        difficultyBudget = Mathf.Clamp(difficultyBudget, -7.0f, 12.0f);

        Debug.Log($"Difficulty budget gained back after spawning: {budgetIncrease}");
        Debug.Log($"Final budget after spawning: {difficultyBudget}");

        return newSubLevel;
    }


    private GameObject PickAndInstantiateSublevel(List<GameObject> coreLevels, bool isGravityInversion)
    {
        // Randomly pick a core level
        GameObject chosenCoreLevel = coreLevels[Random.Range(0, coreLevels.Count)];
        Debug.Log($"Core level chosen: {chosenCoreLevel.name}");

        // Retrieve all versions of this core level by finding subsequent entries in subLevelPrefabs
        List<GameObject> levelVariants = GetAllLevelVariants(chosenCoreLevel, isGravityInversion);

        // Find the highest difficulty version that we can afford within the budget
        GameObject chosenLevel = null;
        foreach (var variant in levelVariants)
        {
            int difficultyValue = variant.GetComponent<SublevelDifficulty>().difficultyValue;
            if (difficultyValue <= difficultyBudget)
            {
                Debug.Log($"Variant {variant.name} with difficulty {difficultyValue} - Can afford");
                chosenLevel = variant;
            }
            else
            {
                Debug.Log($"Variant {variant.name} with difficulty {difficultyValue} - Cannot afford");
                break; // Stop if we can�t afford the next harder version
            }
        }

        // If no affordable level variant was found, default to the easiest variant
        chosenLevel = chosenLevel ?? levelVariants[0];
        Debug.Log($"Chosen variant: {chosenLevel.name} with difficulty {chosenLevel.GetComponent<SublevelDifficulty>().difficultyValue}");

        // Adjust the budget by subtracting the difficulty value
        float chosenDifficultyValue = chosenLevel.GetComponent<SublevelDifficulty>().difficultyValue;
        difficultyBudget -= chosenDifficultyValue;
        Debug.Log($"Budget deducted for chosen variant: {chosenDifficultyValue}");

        // Instantiate the chosen level
        return Instantiate(chosenLevel, levelParent);
    }



    private List<GameObject> GetAllLevelVariants(GameObject coreLevel, bool isGravityInversion)
    {
        GameObject[] levelArray = isGravityInversion ? gravityInversionSubLevelPrefabs : subLevelPrefabs;
        List<GameObject> levelVariants = new List<GameObject>();

        // Extract the base name by finding the numeric part of the level name (e.g., "SubLevel5" from "SubLevel5Easy")
        string baseName = GetBaseLevelName(coreLevel.name);

        // Collect variants that start with the same base name
        foreach (var level in levelArray)
        {
            if (level.name.StartsWith(baseName))
            {
                levelVariants.Add(level);
            }
        }
        return levelVariants;
    }

    // Helper function to extract the base name of the level (e.g., "SubLevel5" from "SubLevel5Easy")
    private string GetBaseLevelName(string levelName)
    {
        for (int i = 0; i < levelName.Length; i++)
        {
            if (char.IsDigit(levelName[i]))
            {
                // Find the end of the numeric part
                int endOfNumber = i;
                while (endOfNumber < levelName.Length && char.IsDigit(levelName[endOfNumber]))
                {
                    endOfNumber++;
                }
                return levelName.Substring(0, endOfNumber); // Extract "SubLevelX"
            }
        }
        return levelName; // Fallback, although every level name should contain a number
    }


    private void PositionAndInvertSubLevel(GameObject subLevel, Vector3 position, Quaternion rotation)
    {
        subLevel.transform.SetLocalPositionAndRotation(position, rotation);
        prevLevelTransform = subLevel.transform;

        if (gravityInverted)
        {
            subLevel.transform.localScale = new Vector3(1, -1, 1);
            subLevel.transform.localPosition = new Vector3(position.x, 9, position.z);
            FlipBoxCollidersInSubLevel(subLevel.transform);
            InvertMoveables(subLevel.transform);
        }
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

                    rotatableScript.startingAngle *= -1;
                }
            }
            else if (interactable.name == "Booster Pad" || interactable.name == "Strong Booster Pad")
            {
                BoostPad boostPadScript = interactable.GetComponent<BoostPad>();
                if (boostPadScript != null)
                {
                    if (boostPadScript.direction == BoostPad.Direction.Up) boostPadScript.direction = BoostPad.Direction.Down;
                    else if (boostPadScript.direction == BoostPad.Direction.Down) boostPadScript.direction = BoostPad.Direction.Up;
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

    // Refresh game options based on saved settings
    public void RefreshOptions()
    {
        // Read the settings from Options class
        flippingEnabled = Options.Instance.invertGravityWithButton;
        tiltingEnabled = Options.Instance.useAccelerometer;

        // Call UIManager to update the gravity inversion button based on flippingEnabled
        uiManager.SetInvertGravityButtonActivity(!flippingEnabled);

        ball.GetComponent<Hamster>().flippingEnabled = flippingEnabled;
        ball.GetComponent<Hamster>().tiltingEnabled = tiltingEnabled;
    }

    public Vector3 GetRevivePosition()
    {
        subLevelBallIsIn++;

        Debug.Log($"Reviving in sublevel {GetSubLevel(subLevelBallIsIn)}");

        float xPosition = (subLevelBallIsIn * 16) + .5f; // Sublevel X coordinate
        float yPosition = IsCurrentSublevelInverted() ? 9 - 1.2f : 1.2f; // Adjust Y based on gravity
        subLevelBallIsIn--;
        return new Vector3(xPosition, yPosition, 0);
    }

    public bool IsCurrentSublevelInverted()
    {
        if (subLevelBallIsIn == 0) return false;

        int cycleLength = subLevelsPerGravityInversions * 2;
        int positionInCycle = (subLevelBallIsIn - 1) % cycleLength;
        return positionInCycle >= subLevelsPerGravityInversions;
    }

    public GameObject GetSubLevel(int subLevelNumber)
    {
        if (subLevelDictionary.TryGetValue(subLevelNumber, out var subLevel))
        {
            return subLevel;
        }

        Debug.LogWarning($"SubLevel {subLevelNumber} not found in dictionary.");
        return null;
    }

    public void IncrementSubLevelBallIsIn()
    {
        subLevelBallIsIn++;
    }
}
