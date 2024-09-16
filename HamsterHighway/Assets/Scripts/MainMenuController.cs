using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bestScoreText;

    void Start()
    {
        Options.Read();
        ScoreTracker.Read();
        bestScoreText.text = $"Best Score: {ScoreTracker.Instance.bestScore}";
    }

    public void StartButtonClicked()
    {
        SceneManager.LoadScene("InfiniteRunner");
    }

    public void OptionsButtonClicked()
    {
        SceneManager.LoadScene("OptionsMenu");
    }

    public void QuitButtonClicked()
    {
        Application.Quit();
    }
}
