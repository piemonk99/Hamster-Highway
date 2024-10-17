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

    [SerializeField] private Toggle accelerometerToggle;
    [SerializeField] private Toggle gravityToggle;

    void Start()
    {
        Options.Read();

        volumeSlider.value = Options.Instance.Volume;

        accelerometerToggle.isOn = Options.Instance.useAccelerometer;
        gravityToggle.isOn = Options.Instance.invertGravityWithButton;

        // Add listeners for when toggles are changed
        accelerometerToggle.onValueChanged.AddListener(OnAccelerometerToggleChanged);
        gravityToggle.onValueChanged.AddListener(OnGravityToggleChanged);
    }

    public void VolumeSliderSet(float value)
    {
        Options.Instance.Volume = value;
        Options.Instance.Write();
        volumeText.text = $"Volume: {(int) (value * 100)}%";
    }

    private void OnAccelerometerToggleChanged(bool isOn)
    {
        Options.Instance.useAccelerometer = isOn;
        Options.Instance.Write();
    }

    private void OnGravityToggleChanged(bool isOn)
    {
        Options.Instance.invertGravityWithButton = isOn;
        Options.Instance.Write();
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
