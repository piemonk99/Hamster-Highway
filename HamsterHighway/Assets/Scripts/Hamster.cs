using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Hamster : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject gameOverUI;
    private bool isGameOver = false;
    private int revivesUsed = 0;

    // Reference to GameManager for accessing current sublevel details
    private GameManager gameManager;

    [SerializeField] private float maxNaturalForwardSpeed = 2.0f;
    [SerializeField] private float minimumForwardSpeed = 1.0f;

    [SerializeField] private float loseBelowY = 0;
    [SerializeField] private float loseAboveY = 10;
    [SerializeField] private float loseSpeedFactor = 0.05f;

    [SerializeField] private float accelerometerMinSpeed = -0.5f;
    [SerializeField] private float accelerometerMaxSpeed = 1.5f;
    [SerializeField] private float yawForce = 1;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI inversionsText;

    [SerializeField] private AudioClip coinSound;
    [SerializeField] private Material[] skyboxes;

    [SerializeField] private float skyboxChangeInterval = 100;
    private float angularVelocityThreshold = 5.0f; // Threshold for rolling
    private float angularVelocityRecovery = 4.5f; // Speed to recover and stop rolling

    private Vector3 previousPosition;
    private Quaternion previousRotation;

    private float startX;
    private bool invertedGravity;

    public bool flippingEnabled;
    public bool tiltingEnabled;

    private bool rolling;

    private float gravityCooldownTimer = 0;
    private float gravityCooldownDuration = 3;
    private int gravityInversionCredits = 5;
    private int maxGravityInversionCredits = 5;
    private float previousAccelerometerY;

    private float previousYVelocity = 0;
    private Vector3 initialHamsterScale;
    // private int skyboxChangeTracker;
    // private int currentSkybox;

    [SerializeField] private bool invulnerable;

    private float reviveGracePeriod = 1.5f;
    private float reviveGraceTimer;

    private void Start()
    {
        ScoreTracker.Instance.coins = 1500;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        gameManager = FindObjectOfType<GameManager>();

        ConstantForce constantForce = gameObject.AddComponent<ConstantForce>();
        constantForce.force = new Vector3(3, 0, 0);

        startX = transform.localPosition.x;
        initialHamsterScale = transform.Find("Hamster").localScale;
        Input.gyro.enabled = true;

        previousRotation = transform.rotation;

        coinsText.text = $"{ScoreTracker.Instance.coins}";
    }

    private void FixedUpdate()
    {
        if (isGameOver) return; // Stop movement updates if game over

        if (rb.isKinematic)
        {
            reviveGraceTimer -= Time.fixedDeltaTime;

            if (reviveGraceTimer <= 0)
                rb.isKinematic = false;

            return;
        }

        // Get the current sublevel's rightward facing direction
        GameObject currentSubLevel = gameManager.GetSubLevel(gameManager.subLevelBallIsIn);
        if (currentSubLevel == null)
        {
            Debug.LogError("Current sublevel not found!");
            return;
        }

        Vector3 subLevelRight = currentSubLevel.transform.right;
        float minSpeed = minimumForwardSpeed;

        Vector3 accelerometer = Input.acceleration;

        HandleGravity(accelerometer.y);
        HandleSpeed(minSpeed, accelerometer.x);

        if (transform.localPosition.y < loseBelowY || transform.localPosition.y > loseAboveY)
        {
            TriggerGameOver();
        }

        // The hamster is going backwards if the dot product is less than 0
        if ((transform.localPosition - previousPosition).magnitude / Time.fixedDeltaTime < loseSpeedFactor * minimumForwardSpeed || Vector3.Dot(rb.velocity, subLevelRight) < 0)
        {
            TriggerGameOver();
        }

        previousPosition = transform.localPosition;

        PlayCorrectAnimation();
    }


    private void HandleGravity(float accelerometerY)
    {
        if (flippingEnabled)
        {
            // Check if the accelerometer Y value has changed from positive to negative or vice versa
            if ((previousAccelerometerY < 0 && accelerometerY >= 0) || (previousAccelerometerY >= 0 && accelerometerY < 0))
            {
                if (gravityInversionCredits > 0)
                {
                    // Flip gravity, adjust hamster's scale to reflect gravity inversion, and decrement the credits when gravity is inverted
                    invertedGravity = accelerometerY >= 0;
                    transform.Find("Hamster").localScale = invertedGravity ? new Vector3(initialHamsterScale.x, -initialHamsterScale.y, initialHamsterScale.z) : initialHamsterScale;
                    gravityInversionCredits--;
                    inversionsText.text = $"{gravityInversionCredits}/{maxGravityInversionCredits}";
                }
            }

            // Update previous accelerometer Y value for the next frame
            previousAccelerometerY = accelerometerY;
        }
        

        // Gravity inversion mechanics
        if (invertedGravity)
        {
            rb.useGravity = false;
            rb.velocity -= Physics.gravity * Time.fixedDeltaTime;
        }
        else
        {
            rb.useGravity = true;
        }

        // Timer to replenish gravity inversion credits
        gravityCooldownTimer += Time.fixedDeltaTime;
        if (gravityCooldownTimer >= gravityCooldownDuration && gravityInversionCredits < maxGravityInversionCredits)
        {
            gravityInversionCredits++;
            gravityCooldownTimer = 0f;
            inversionsText.text = $"{gravityInversionCredits}/{maxGravityInversionCredits}";
        }
    }

    public void FlipGravityButtonPressed()
    {
        if (gravityInversionCredits > 0)
        {
            // Flip gravity, adjust hamster's scale to reflect gravity inversion, and decrement the credits when gravity is inverted
            invertedGravity = !invertedGravity;
            transform.Find("Hamster").localScale = invertedGravity ? new Vector3(initialHamsterScale.x, -initialHamsterScale.y, initialHamsterScale.z) : initialHamsterScale;
            gravityInversionCredits--;
            inversionsText.text = $"{gravityInversionCredits}/{maxGravityInversionCredits}";
        }
    }

    private void HandleSpeed(float minSpeed, float accelerometerX)
    {
        // Get the current sublevel's rightward facing direction
        GameObject currentSubLevel = gameManager.GetSubLevel(gameManager.subLevelBallIsIn);
        if (currentSubLevel == null)
        {
            Debug.LogError("Current sublevel not found!");
            return;
        }

        Vector3 subLevelRight = currentSubLevel.transform.right;

        // Extract horizontal velocity (x and z components only)
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Calculate the current horizontal speed in the direction of the sublevel's rightward facing
        float currentSpeed = Vector3.Dot(horizontalVelocity, subLevelRight);

        // Adjust minSpeed based on accelerometer input if tilting is enabled
        if (tiltingEnabled)
        {
            rb.AddForce(subLevelRight * Input.gyro.rotationRate.y * yawForce, ForceMode.Force);
            minSpeed += accelerometerX < 0
                ? Mathf.Lerp(accelerometerMinSpeed, 0, -accelerometerX)
                : Mathf.Lerp(0, accelerometerMaxSpeed, accelerometerX);
        }

        // Ensure the hamster's horizontal velocity is at least minSpeed
        if (currentSpeed < minSpeed)
        {
            Vector3 additionalForce = subLevelRight.normalized * (minSpeed - currentSpeed) * 2; // Boost for sharp turns
            rb.velocity += additionalForce; // Adjust horizontal velocity
        }

        // Ensure a base constant force is always applied to maintain forward momentum
        ConstantForce constantForce = GetComponent<ConstantForce>();
        constantForce.force = subLevelRight.normalized * 3;

        // Cap the horizontal velocity if exceeding max speed
        if (currentSpeed > maxNaturalForwardSpeed)
        {
            constantForce.force = Vector3.zero; // Stop applying extra force
        }
    }

    public void RedirectVelocity()
    {
        // Get the current sublevel GameObject
        GameObject currentSubLevel = gameManager.GetSubLevel(gameManager.subLevelBallIsIn);
        if (currentSubLevel == null)
        {
            Debug.LogError("Current sublevel not found!");
            return;
        }

        Transform hamsterModel = transform.Find("Hamster");

        // Align the hamster's rotation with the current sublevel
        float targetYRotation = currentSubLevel.transform.eulerAngles.y - 90; // Offset as needed
        Quaternion targetRotation = Quaternion.Euler(hamsterModel.eulerAngles.x, targetYRotation, hamsterModel.eulerAngles.z);

        hamsterModel.rotation = targetRotation;


        // Calculate the rightward facing direction of the current sublevel
        Vector3 subLevelRight = currentSubLevel.transform.right;

        // Calculate the original horizontal speed magnitude
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float originalSpeed = horizontalVelocity.magnitude;

        // Redirect horizontal velocity
        Vector3 redirectedHorizontalVelocity = subLevelRight.normalized * originalSpeed;

        // Combine the vertical component (y) with the redirected horizontal velocity
        rb.velocity = new Vector3(redirectedHorizontalVelocity.x, rb.velocity.y, redirectedHorizontalVelocity.z);
    }

    private void LockToPlane()
    {
        // Get the current sublevel GameObject
        GameObject currentSubLevel = gameManager.GetSubLevel(gameManager.subLevelBallIsIn);
        if (currentSubLevel == null)
        {
            Debug.LogError("LockToPlane: Current sublevel not found!");
            return;
        }

        // Get the sublevel's transform
        Transform subLevelTransform = currentSubLevel.transform;

        // Transform the hamster's position into the sublevel's local space
        Vector3 localPosition = subLevelTransform.InverseTransformPoint(transform.position);

        // Reset only the local X and Z coordinates, keeping Y unchanged
        localPosition.y = Mathf.Clamp(localPosition.y, -8f, 8f); // Optional: Clamp within level bounds
        localPosition.z = 0; // Align to the center plane (local Z)

        // Convert the position back to world space
        Vector3 correctedPosition = subLevelTransform.TransformPoint(localPosition);

        // Apply the corrected position without affecting the Y value
        rb.MovePosition(new Vector3(correctedPosition.x, transform.position.y, correctedPosition.z));

        // Debug logs and visual aids
        ///Debug.Log($"LockToPlane: LocalPosition: {localPosition}, CorrectedPosition: {correctedPosition}, SubLevel: {currentSubLevel.name}");
        ///Debug.DrawLine(correctedPosition, correctedPosition + Vector3.up * 5, Color.green, 0.1f);
    }

    private float hamsterRollingXRotation; // Store initial hamster x-rotation when rolling starts


    private void LateUpdate()
    {
        HandleRolling();
        LockToPlane(); // Ensure the hamster stays on the plane of the current sublevel
    }

    private void HandleRolling()
    {
        float angularVelocityMagnitude = rb.angularVelocity.magnitude;

        if (!rolling && angularVelocityMagnitude > angularVelocityThreshold)
        {
            // Start rolling
            animator.SetTrigger("BeginRolling");

            CorrectHamsterRotation();
            StartRolling();
        }
        else if (rolling && angularVelocityMagnitude < angularVelocityRecovery && IsHamsterAlignedWithFloor())
        {
            // Stop rolling
            rolling = false;
            animator.SetBool("Rolling", false);
        }

        if (!rolling)
        {
            CorrectHamsterRotation();
        }
        else
        {
            RollWithParent();
        }
    }

    private void StartRolling()
    {
        hamsterRollingXRotation = 180 - transform.GetChild(0).localRotation.eulerAngles.x;

        rolling = true;

        animator.SetBool("Rolling", true);
    }

    private void CorrectHamsterRotation()
    {
        Transform hamsterModel = transform.Find("Hamster");
        GameObject currentSubLevel = gameManager.GetSubLevel(gameManager.subLevelBallIsIn);

        hamsterModel.rotation = Quaternion.Euler(0, -90 + currentSubLevel.transform.eulerAngles.y, 0);

        Debug.Log($"Hamster model's local x angle is {180 - transform.GetChild(0).localRotation.eulerAngles.x} after corrected");
    }
    private void RollWithParent()
    {
        Transform hamsterModel = transform.Find("Hamster");
        GameObject currentSubLevel = gameManager.GetSubLevel(gameManager.subLevelBallIsIn);

        //hamsterRollingXRotation += transform.eulerAngles.x - previousHamsterXRotation;

        Debug.Log($"RollWithParent is setting local hamster rotation to ({hamsterRollingXRotation}, {-90 + currentSubLevel.transform.eulerAngles.y}, {0})");

        hamsterModel.localRotation = Quaternion.Euler(hamsterRollingXRotation, -90 + currentSubLevel.transform.eulerAngles.y, 0);
    }

    private bool IsHamsterAlignedWithFloor()
    {
        Transform hamsterModel = transform.Find("Hamster");
        float parentZRotation = transform.rotation.eulerAngles.z;
        if (parentZRotation > 180)
        {
            parentZRotation -= 360; // Convert to -180 to 180 range
        }

        Debug.Log($"ChildXRotation: {parentZRotation} hamsterRollingXRotation: {hamsterRollingXRotation}");

        float rotationTotal = parentZRotation + hamsterRollingXRotation;

        // Tolerance to account for slight differences
        float lowerLimit, upperLimit;

        lowerLimit = -10;
        upperLimit = 10;

        Debug.Log($"Checking if {rotationTotal} < {upperLimit} && {rotationTotal} > {lowerLimit}");


        // Return true if the parent's Y rotation is close to the inverse of the child's Y rotation
        return (rotationTotal > lowerLimit && rotationTotal < upperLimit);
    }

    float ConvertToRange180(float angle)
    {
        // Normalize angle to the range -180 to 180
        float newAngle = (angle + 180) % 360;
        if (newAngle > 180) newAngle -= 360;
        return newAngle;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            ++ScoreTracker.Instance.coins;
            coinsText.text = $"{ScoreTracker.Instance.coins}";
            AudioClipPlayer.PlayClipAtPoint(coinSound, other.gameObject.transform.position);
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (invulnerable)
            return;

        ScoreTracker.Instance.prevBestScore = ScoreTracker.Instance.bestScore;

        if (ScoreTracker.Instance.score > ScoreTracker.Instance.bestScore)
            ScoreTracker.Instance.bestScore = ScoreTracker.Instance.score;

        ScoreTracker.Instance.Write();

        isGameOver = true;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true; // Freeze movement
        playerHUD.SetActive(false);
        gameOverUI.SetActive(true);
        Time.timeScale = 0; // Pause the game
    }

    public void Revive()
    {
        revivesUsed += 1;

        coinsText.text = $"{ScoreTracker.Instance.coins}";

        // Reset position based on current sublevel and gravity
        Vector3 revivePosition = gameManager.GetRevivePosition();
        transform.position = revivePosition;

        reviveGraceTimer = reviveGracePeriod;

        // Reset gravity and other movement settings
        invertedGravity = gameManager.IsCurrentSublevelInverted();
        transform.Find("Hamster").localScale = invertedGravity ? new Vector3(initialHamsterScale.x, -initialHamsterScale.y, initialHamsterScale.z) : initialHamsterScale;

        isGameOver = false;
        gameOverUI.SetActive(false);
        playerHUD.SetActive(true);

        // Focus the camera on the ball once
        if (Camera.main.GetComponent<CameraController>() != null)
        {
            Camera.main.GetComponent<CameraController>().SnapToBall();
        }

        Time.timeScale = 1; // Resume game
    }

    private void PlayCorrectAnimation()
    {
        if ((!invertedGravity && previousYVelocity < 1 && rb.velocity.y > 1) ||
            (invertedGravity && previousYVelocity > -1 && rb.velocity.y < -1))
        {
            animator.SetTrigger("Jumped");
        }

        previousYVelocity = rb.velocity.y;

        Vector3 rayDirection = invertedGravity ? Vector3.up : Vector3.down;
        bool isGrounded = Physics.Raycast(transform.position, rayDirection, out RaycastHit hitInfo, 1f);
        animator.SetBool("Grounded", isGrounded);
    }

    public int GetRevivesUsed()
    {
        return revivesUsed;
    }
}
