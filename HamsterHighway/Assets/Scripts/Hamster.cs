using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Hamster : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float maxNaturalForwardSpeed = 2.0f;
    [SerializeField] private float minimumForwardSpeed = 1.0f;

    [SerializeField] private float loseBelowY = 0; // If the hamster falls below this Y value, the game ends
    [SerializeField] private float loseSpeedFactor = 0.05f; // If the hamster's speed drops below this fraction of its minimum forward speed when it collides with something (i.e. it gets stuck on something), the game ends

    [SerializeField] private float accelerometerMinSpeed = -0.5f;
    [SerializeField] private float accelerometerMaxSpeed = 1.5f;

    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [SerializeField] private AudioClip coinSound;

    private Vector3 previousPosition;
    private float startX;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        ConstantForce constantForce = gameObject.AddComponent<ConstantForce>();
        constantForce.force = new Vector3(3, 0, 0);

        startX = transform.position.x;
    }

    private void FixedUpdate()
    {
        float minSpeed = minimumForwardSpeed;

        debugText.text = $"{Input.acceleration}";

        Vector3 accelerometer = Input.acceleration;
        minSpeed += accelerometer.x < 0 ? Mathf.Lerp(accelerometerMinSpeed, 0, -accelerometer.x) : Mathf.Lerp(0, accelerometerMaxSpeed, accelerometer.x);

        // Gravity inversion, currently very exploitable
        // Needs a cooldown or something
        if (accelerometer.y > 0)
        {
            rb.useGravity = false;
            rb.velocity -= Physics.gravity * Time.fixedDeltaTime;
        }
        else
            rb.useGravity = true;

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
            GameOver();
        }

        if ((transform.position - previousPosition).magnitude / Time.fixedDeltaTime < loseSpeedFactor * minimumForwardSpeed)
        {
            // got stuck on something, game over
            Debug.Log($"Game Over: Stuck. Movement: {(transform.position - previousPosition) / Time.fixedDeltaTime} Speed: {(transform.position - previousPosition).magnitude / Time.fixedDeltaTime}");
            GameOver();
        }

        previousPosition = transform.position;
        ScoreTracker.Instance.score = Mathf.RoundToInt(transform.position.x - startX);
        scoreText.text = $"Score: {ScoreTracker.Instance.score}";
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

    private void GameOver()
    {
        if (ScoreTracker.Instance.score > ScoreTracker.Instance.bestScore)
        {
            ScoreTracker.Instance.prevBestScore = ScoreTracker.Instance.bestScore;
            ScoreTracker.Instance.bestScore = ScoreTracker.Instance.score;
        }

        // Need to write even when best score does not change to save coins
        ScoreTracker.Instance.Write();
        SceneManager.LoadScene("GameOver");
    }
}
