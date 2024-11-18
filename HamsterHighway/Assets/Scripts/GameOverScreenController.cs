using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreenController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI newBestScoreText;

    [SerializeField] private Button reviveButton;
    [SerializeField] private TextMeshProUGUI reviveCostText;

    [SerializeField] private Hamster hamster; // Reference to Hamster script

    [SerializeField] private AudioSource gameOverAudio;

    [SerializeField] private AudioClip newBestScoreClip;

    [SerializeField] private float newBestScoreTextFlashSpeed = 1;

    void OnEnable()
    {
        scoreText.text = $"Score: {ScoreTracker.Instance.score}";

        if (ScoreTracker.Instance.score > ScoreTracker.Instance.prevBestScore)
        {
            newBestScoreText.gameObject.SetActive(true);
            gameOverAudio.clip = newBestScoreClip;
        }

        ConfigureReviveButton();

        gameOverAudio.volume = Options.Instance.Volume;
        gameOverAudio.Play();
    }

    void Update()
    {
        newBestScoreText.color = Color.Lerp(Color.white, Color.yellow, (Mathf.Sin(Time.time * 2 * Mathf.PI * newBestScoreTextFlashSpeed) + 1) / 2);
    }   

    public void ConfigureReviveButton()
    {
        int reviveCost = 50 + (50 * hamster.GetRevivesUsed());
        reviveCostText.text = reviveCost.ToString();
        reviveButton.interactable = ScoreTracker.Instance.coins >= reviveCost;
    }

    public void MainMenuButtonClicked()
    {
        Time.timeScale = 1; // Ensure game is unpaused if returning to main menu
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitButtonClicked()
    {
        Application.Quit();
    }

    public void OnReviveButtonClicked()
    {
        ScoreTracker.Instance.coins -= 50 + (50 * hamster.GetRevivesUsed());
        hamster.Revive();
    }
}
