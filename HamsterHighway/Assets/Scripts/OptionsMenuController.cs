using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenuController   : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        volumeSlider.value = Options.Instance.Volume;
    }

    public void VolumeSliderSet(float value)
    {
        Options.Instance.Volume = value;
        Options.Instance.Write();
        volumeText.text = $"Volume: {(int) (value * 100)}%";
    }

    public void ResetBestScoreButtonClicked()
    {
        ScoreTracker.Instance.bestScore = 0;
        ScoreTracker.Instance.Write();
    }

    public void BackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
