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
        bestScoreText.text = $"{ScoreTracker.Instance.bestScore}";
        ARToggle.isOn = Options.Instance.arMode;
        ARToggle.onValueChanged.AddListener(OnARToggleChanged);
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

    public void QuitButtonClicked()
    {
        Application.Quit();
    }

    private void OnARToggleChanged(bool isOn)
    {
        Options.Instance.arMode = isOn;
        Options.Instance.Write();
    }
}
