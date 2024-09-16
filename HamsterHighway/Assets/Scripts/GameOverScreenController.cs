using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI newBestScoreText;

    [SerializeField] private AudioSource gameOverAudio;

    [SerializeField] private AudioClip newBestScoreClip;

    [SerializeField] private float newBestScoreTextFlashSpeed = 1;

    void Start()
    {
        scoreText.text = $"Score: {ScoreTracker.Instance.score}";

        if (ScoreTracker.Instance.score > ScoreTracker.Instance.prevBestScore)
        {
            newBestScoreText.gameObject.SetActive(true);
            gameOverAudio.clip = newBestScoreClip;
        }

        gameOverAudio.volume = Options.Instance.Volume;
        gameOverAudio.Play();
    }

    void Update()
    {
        newBestScoreText.color = Color.Lerp(Color.white, Color.yellow, (Mathf.Sin(Time.time * 2 * Mathf.PI * newBestScoreTextFlashSpeed) + 1) / 2);
    }   

    public void MainMenuButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitButtonClicked()
    {
        Application.Quit();
    }
}
