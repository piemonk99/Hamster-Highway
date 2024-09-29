using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Hamster : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [SerializeField] private float maxNaturalForwardSpeed = 2.0f;
    [SerializeField] private float minimumForwardSpeed = 1.0f;
    [SerializeField] private float extraSpeedPerScore = 0.005f;

    [SerializeField] private float loseBelowY = 0;
    [SerializeField] private float loseAboveY = 10;
    [SerializeField] private float loseSpeedFactor = 0.05f;

    [SerializeField] private float accelerometerMinSpeed = -0.5f;
    [SerializeField] private float accelerometerMaxSpeed = 1.5f;
    [SerializeField] private float yawForce = 1;

    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText; 

    [SerializeField] private AudioClip coinSound;
    [SerializeField] private Material[] skyboxes;

    [SerializeField] private float skyboxChangeInterval = 100;
    private float angularVelocityThreshold = 5.0f; // Threshold for rolling
    private float angularVelocityRecovery = 4.5f; // Speed to recover and stop rolling

    private Vector3 previousPosition;
    private Quaternion previousRotation;

    private float startX;
    private bool invertedGravity;
    private bool tripping;
    private bool rolling;

    private float previousYVelocity = 0;
    private Vector3 initialHamsterScale;
    private int skyboxChangeTracker;
    private int currentSkybox;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        ConstantForce constantForce = gameObject.AddComponent<ConstantForce>();
        constantForce.force = new Vector3(3, 0, 0);

        startX = transform.position.x;
        initialHamsterScale = transform.Find("Hamster").localScale;
        Input.gyro.enabled = true;

        previousRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        float progress = transform.position.x - startX;
        float minSpeed = minimumForwardSpeed + progress * extraSpeedPerScore;

        debugText.text = $"{Input.acceleration}";
        Vector3 accelerometer = Input.acceleration;
        minSpeed += accelerometer.x < 0 ? Mathf.Lerp(accelerometerMinSpeed, 0, -accelerometer.x) : Mathf.Lerp(0, accelerometerMaxSpeed, accelerometer.x);

        if (accelerometer.y > 0.5)
        {
            invertedGravity = true;
            transform.Find("Hamster").localScale = new Vector3(initialHamsterScale.x, -initialHamsterScale.y, initialHamsterScale.z);
        }
        else if (accelerometer.y < -0.5)
        {
            invertedGravity = false;
            transform.Find("Hamster").localScale = initialHamsterScale;
        }

        // Gravity inversion
        if (invertedGravity)
        {
            rb.useGravity = false;
            rb.velocity -= Physics.gravity * Time.fixedDeltaTime;
        }
        else
            rb.useGravity = true;

        // Speed and movement control
        Vector3 velocity = rb.velocity;
        if (velocity.x < minSpeed)
        {
            velocity.x = minSpeed;
            rb.velocity = velocity;
        }
        else if (velocity.x > minSpeed && velocity.x < maxNaturalForwardSpeed)
        {
            GetComponent<ConstantForce>().force = new Vector3(3, 0, 0);
        }
        else if (velocity.x > maxNaturalForwardSpeed + progress * extraSpeedPerScore)
        {
            GetComponent<ConstantForce>().force = new Vector3(0, 0, 0);
        }

        rb.AddForce(new Vector3(Input.gyro.rotationRate.y * yawForce, 0, 0));

        if (transform.position.y < loseBelowY || transform.position.y > loseAboveY)
        {
            GameOver();
        }

        if ((transform.position - previousPosition).magnitude / Time.fixedDeltaTime < loseSpeedFactor * minimumForwardSpeed)
        {
            GameOver();
        }

        previousPosition = transform.position;
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

        if (rolling)
        {
            // Smoothly inherit the changes in the parent's rotation
            RotateWithParent();
        }
        else
        {
            // Correct the hamster's upright rotation
            CorrectHamsterRotation();
        }
    }

    //Called by the trip and roll animation, allows us to stop correcting the hamster's rotation at the right time
    public void StartRolling()
    {
        // When starting to roll, we capture the parent's current rotation as the baseline
        previousRotation = transform.rotation;

        Debug.Log("Started rolling");
        rolling = true;
        tripping = false;

        animator.SetBool("Rolling", true);
    }

    private void RotateWithParent()
    {
        /*// Get the parent's current rotation
        Quaternion currentParentRotation = transform.rotation;

        // Calculate the difference in rotation from the last frame (how much the parent has rotated)
        Quaternion rotationDifference = Quaternion.Inverse(previousRotation) * currentParentRotation;

        // Update previous parent rotation for the next frame
        previousRotation = currentParentRotation;

        // Apply the rotation difference to the hamster's body, but maintain the initial offset
        Transform hamsterBody = transform.GetChild(0);
        //hamsterBody.localRotation = Quaternion.Euler(0, -90, 0) * rotationDifference; // Maintain the forward facing

        // Optional: Log the local rotation to help debug
        Debug.Log($"Hamster Body Local Rotation: {hamsterBody.localRotation.eulerAngles}");*/
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

        // We want the parent’s rotation to be close to the inverse of the child's locked rotation
        float rotationDifference = Mathf.Abs(((parentZRotation + 180) % 360) - childXRotation);

        // Tolerance to account for slight differences
        float tolerance = 10f;

        Debug.Log($"Is hamster aligned with parent? {rotationDifference < tolerance}. parentZRotation = {parentZRotation}, childXRotation = {childXRotation} \n" +
                  $"We are checking if {parentZRotation + 180} minus {childXRotation}, which is {rotationDifference}, is less than {tolerance}.");

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
            GameOver();
        }
    }

    private void GameOver()
    {
        ScoreTracker.Instance.prevBestScore = ScoreTracker.Instance.bestScore;

        if (ScoreTracker.Instance.score > ScoreTracker.Instance.bestScore)
            ScoreTracker.Instance.bestScore = ScoreTracker.Instance.score;

        ScoreTracker.Instance.Write();
        SceneManager.LoadScene("GameOver");
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
}
