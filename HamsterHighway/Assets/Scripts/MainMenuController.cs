using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [SerializeField] private Toggle ARToggle;

    void Start()
    {
        Options.Read();
        ScoreTracker.Read();
        bestScoreText.text = $"Best Score: {ScoreTracker.Instance.bestScore}";
    }

    public void StartButtonClicked()
    {
        if (ARToggle.isOn)
        {
            SceneManager.LoadScene("ARInfiniteRunner");
        }
        else
        {
            SceneManager.LoadScene("InfiniteRunner");
        }
        
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
