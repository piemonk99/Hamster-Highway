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

    private void Start()
    {
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

        float progress = transform.localPosition.x - startX;
        float minSpeed = minimumForwardSpeed/* + progress * extraSpeedPerScore*/;

        debugText.text = $"{Input.acceleration}";
        Vector3 accelerometer = Input.acceleration;

        

        HandleGravity(accelerometer.y);

        HandleSpeed(minSpeed, progress, accelerometer.x);

        if (transform.localPosition.y < loseBelowY || transform.localPosition.y > loseAboveY)
        {
            TriggerGameOver();
        }

        if ((transform.localPosition - previousPosition).magnitude / Time.fixedDeltaTime < loseSpeedFactor * minimumForwardSpeed)
        {
            TriggerGameOver();
        }

        previousPosition = transform.localPosition;

        ScoreTracker.Instance.score = Mathf.RoundToInt(progress);
        scoreText.text = $"Score: {ScoreTracker.Instance.score}";


        PlayCorrectAnimation();

        if (progress - skyboxChangeTracker * skyboxChangeInterval > skyboxChangeInterval)
        {
            ++skyboxChangeTracker;
            int temp = currentSkybox;
            do
            {
                currentSkybox = Random.Range(0, skyboxes.Length);
            } while (currentSkybox == temp);

            RenderSettings.skybox = skyboxes[currentSkybox];
            DynamicGI.UpdateEnvironment();
        }
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

    private void HandleSpeed(float minSpeed, float progress, float accelerometerX)
    {
        if (tiltingEnabled)
        {
            rb.AddForce(new Vector3(Input.gyro.rotationRate.y * yawForce, 0, 0));
            minSpeed += accelerometerX < 0 ? Mathf.Lerp(accelerometerMinSpeed, 0, -accelerometerX) : Mathf.Lerp(0, accelerometerMaxSpeed, accelerometerX);
        }

        Vector3 velocity = rb.velocity;

        // Ensure a base constant force is always applied to maintain forward momentum
        ConstantForce constantForce = GetComponent<ConstantForce>();
        constantForce.force = new Vector3(3, 0, 0);

        // Adjust the hamster's velocity if it's below minSpeed or exceeding maxNaturalForwardSpeed
        if (velocity.x < minSpeed)
        {
            velocity.x = minSpeed;
            rb.velocity = velocity;
        }
        else if (velocity.x > maxNaturalForwardSpeed)
        {
            // Cap the forward speed at maxNaturalForwardSpeed and temporarily reduce force
            constantForce.force = Vector3.zero;
        }
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
