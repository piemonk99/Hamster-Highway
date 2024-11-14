using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Hamster : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [SerializeField] private GameObject UICanvas;
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

    [SerializeField] private TextMeshProUGUI debugText;
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

    private bool tripping;
    private bool rolling;

    private float gravityCooldownTimer = 0f;
    private float gravityCooldownDuration = 1.5f;
    private int gravityInversionCredits = 5;
    private int maxGravityInversionCredits = 5;
    private float previousAccelerometerY;

    private float previousYVelocity = 0;
    private Vector3 initialHamsterScale;
    private int skyboxChangeTracker;
    private int currentSkybox;

    [SerializeField] private bool invulnerable;

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

        coinsText.text = $"Coins: {ScoreTracker.Instance.coins}";
    }

    private void FixedUpdate()
    {
        if (isGameOver) return; // Stop movement updates if game over

        // Get the current sublevel's rightward facing direction
        GameObject currentSubLevel = gameManager.GetSubLevel(gameManager.subLevelBallIsIn);
        if (currentSubLevel == null)
        {
            Debug.LogError("Current sublevel not found!");
            return;
        }

        Vector3 subLevelRight = currentSubLevel.transform.right;
        float minSpeed = minimumForwardSpeed;

        debugText.text = $"{Input.acceleration}";
        Vector3 accelerometer = Input.acceleration;

        HandleGravity(accelerometer.y);
        HandleSpeed(minSpeed, accelerometer.x);

        if (Vector3.Dot(transform.position, Vector3.up) < loseBelowY || Vector3.Dot(transform.position, Vector3.up) > loseAboveY)
        {
            TriggerGameOver();
        }

        if (rb.velocity.magnitude < loseSpeedFactor * minimumForwardSpeed)
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
                    inversionsText.text = $"Inversions: {gravityInversionCredits}/{maxGravityInversionCredits}";
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
            inversionsText.text = $"Inversions: {gravityInversionCredits}/{maxGravityInversionCredits}";
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
            inversionsText.text = $"Inversions: {gravityInversionCredits}/{maxGravityInversionCredits}";
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
            Vector3 additionalForce = subLevelRight.normalized * (minSpeed - currentSpeed);
            rb.velocity += additionalForce; // Adjust horizontal velocity
        }

        // Ensure a base constant force is always applied to maintain forward momentum
        ConstantForce constantForce = GetComponent<ConstantForce>();
        constantForce.force = subLevelRight.normalized * 3;

        // Cap the horizontal velocity if exceeding max speed
        if (currentSpeed > maxNaturalForwardSpeed)
        {
            horizontalVelocity = subLevelRight.normalized * maxNaturalForwardSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z); // Preserve vertical velocity
            constantForce.force = Vector3.zero; // Stop applying extra force
        }

        Debug.Log($"HandleSpeed: CurrentSpeed: {currentSpeed}, MinSpeed: {minSpeed}, Velocity: {rb.velocity}, SubLevelRight: {subLevelRight}");
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

        // Calculate the rightward facing direction of the current sublevel
        Vector3 subLevelRight = currentSubLevel.transform.right;

        // Calculate the original horizontal speed magnitude
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float originalSpeed = horizontalVelocity.magnitude;

        // Redirect horizontal velocity
        Vector3 redirectedHorizontalVelocity = subLevelRight.normalized * originalSpeed;

        // Combine the vertical component (y) with the redirected horizontal velocity
        rb.velocity = new Vector3(redirectedHorizontalVelocity.x, rb.velocity.y, redirectedHorizontalVelocity.z);

        Debug.Log($"Redirected velocity: {rb.velocity}, SubLevelRight (unit vector): {subLevelRight}");
    }






    private void LateUpdate()
    {
        HandleRolling();
    }

    private void HandleRolling()
    {
        float angularVelocityMagnitude = rb.angularVelocity.magnitude;

        if (!tripping && !rolling && angularVelocityMagnitude > angularVelocityThreshold)
        {
            // Start rolling
            animator.SetTrigger("BeginRolling");

            tripping = true;
        }
        else if (rolling && angularVelocityMagnitude < angularVelocityRecovery && IsHamsterAlignedWithParent())
        {
            // Stop rolling
            rolling = false;
            animator.SetBool("Rolling", false);
            CorrectHamsterRotation(); // Re-enable upright correction
        }

        if (!rolling)
        {
            CorrectHamsterRotation();
        }
    }

    // Called by the trip and roll animation, allows us to stop correcting the hamster's rotation at the right time
    public void StartRolling()
    {
        // When starting to roll, we capture the parent's current rotation as the baseline
        previousRotation = transform.rotation;

        rolling = true;
        tripping = false;

        animator.SetBool("Rolling", true);
    }

    private void CorrectHamsterRotation()
    {
        transform.GetChild(0).rotation = Quaternion.Euler(0, -90, 0);
    }

    private bool IsHamsterAlignedWithParent()
    {
        // Get the parent and child Y-axis rotations
        float parentZRotation = transform.eulerAngles.z;
        float childXRotation = transform.GetChild(0).localRotation.eulerAngles.x;

        // We want the parent�s rotation to be close to the inverse of the child's locked rotation
        float rotationDifference = Mathf.Abs(((parentZRotation + 180) % 360) - childXRotation);

        // Tolerance to account for slight differences
        float tolerance = 10f;

        // Return true if the parent's Y rotation is close to the inverse of the child's Y rotation
        return rotationDifference < tolerance;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            ++ScoreTracker.Instance.coins;
            coinsText.text = $"Coins: {ScoreTracker.Instance.coins}";
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
        UICanvas.SetActive(false);
        gameOverUI.SetActive(true);
        Time.timeScale = 0; // Pause the game
    }

    public void Revive()
    {
        revivesUsed += 1;

        coinsText.text = $"Coins: {ScoreTracker.Instance.coins}";

        // Reset position based on current sublevel and gravity
        Vector3 revivePosition = gameManager.GetRevivePosition();
        transform.position = revivePosition;

        // Reset gravity and other movement settings
        invertedGravity = gameManager.IsCurrentSublevelInverted();
        transform.Find("Hamster").localScale = invertedGravity ? new Vector3(initialHamsterScale.x, -initialHamsterScale.y, initialHamsterScale.z) : initialHamsterScale;

        isGameOver = false;
        gameOverUI.SetActive(false);
        UICanvas.SetActive(true);
        rb.isKinematic = false;

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
