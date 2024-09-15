using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Hamster : MonoBehaviour
{
    private Rigidbody rb;

    private ScrollRect scrollRect;

    [SerializeField] private float maxNaturalForwardSpeed = 2.0f;
    [SerializeField] private float minimumForwardSpeed = 1.0f;

    [SerializeField] private float loseBelowY = 0; // If the hamster falls below this Y value, the game ends
    [SerializeField] private float loseSpeedFactor = 0.05f; // If the hamster's speed drops below this fraction of its minimum forward speed when it collides with something (i.e. it gets stuck on something), the game ends

    [SerializeField] private float accelerometerMinSpeed = -0.5f;
    [SerializeField] private float accelerometerMaxSpeed = 1.5f;

    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;

    private Vector3 previousPosition;
    private float startX;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();

        ConstantForce constantForce = gameObject.AddComponent<ConstantForce>();
        constantForce.force = new Vector3(3, 0, 0);

        startX = transform.position.x - scrollRect.viewport.position.x;
    }

    private void FixedUpdate()
    {
        float minSpeed = minimumForwardSpeed;

        debugText.text = $"{Input.acceleration}";

        float accelerometer = Input.acceleration.x;
        minSpeed += accelerometer < 0 ? Mathf.Lerp(accelerometerMinSpeed, 0, -accelerometer) : Mathf.Lerp(0, accelerometerMaxSpeed, accelerometer       );

        //Clamps rigidbody's speed up to a minimum and adds force
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
        else if (velocity.x > maxNaturalForwardSpeed)
        {
            GetComponent<ConstantForce>().force = new Vector3(0, 0, 0);
        }

        if (transform.position.y < loseBelowY)
        {
            // fell out of map, game over
            Debug.Log($"Game Over: Fell. Pos: {transform.position} Y: {transform.position.y}");
            SceneManager.LoadScene("GameOver");
        }

        // Compensate for scrolling
        // Without this, if you scroll at the correct speed to make the hamster stationary on the screen, the game will end
        Vector3 unscrolledPosition = transform.position - scrollRect.viewport.position;

        if ((unscrolledPosition - previousPosition).magnitude / Time.fixedDeltaTime < loseSpeedFactor * minimumForwardSpeed)
        {
            // got stuck on something, game over
            Debug.Log($"Game Over: Stuck. Movement: {(unscrolledPosition - previousPosition) / Time.fixedDeltaTime} Speed: {(unscrolledPosition - previousPosition).magnitude / Time.fixedDeltaTime}");
            SceneManager.LoadScene("GameOver");
        }

        previousPosition = unscrolledPosition;
        ScoreTracker.Instance.score = Mathf.RoundToInt(transform.position.x - scrollRect.viewport.position.x - startX);
        scoreText.text = $"Score: {ScoreTracker.Instance.score}";
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            ++ScoreTracker.Instance.coins;
            coinsText.text = $"Coins: {ScoreTracker.Instance.coins}";
            Destroy(other.gameObject);
        }
    }
}
