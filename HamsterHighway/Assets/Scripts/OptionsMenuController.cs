using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private Toggle accelerometerToggle;
    [SerializeField] private Toggle gravityToggle;
    [SerializeField] private Toggle cameraFollowToggle;

    void Start()
    {
        Options.Read();

        volumeSlider.value = Options.Instance.Volume;

        accelerometerToggle.isOn = Options.Instance.useAccelerometer;
        gravityToggle.isOn = Options.Instance.usePhoneFlipping;
        cameraFollowToggle.isOn = Options.Instance.cameraFollowBall;
    }

    public void VolumeSliderSet(float value)
    {
        Options.Instance.Volume = value;
        Options.Instance.Write();
        volumeText.text = $"{(int) (value * 100)}%";
    }

    public void OnAccelerometerToggleChanged(bool isOn)
    {
        Options.Instance.useAccelerometer = isOn;
        Options.Instance.Write();
    }

    public void OnGravityToggleChanged(bool isOn)
    {
        Options.Instance.usePhoneFlipping = isOn;
        Options.Instance.Write();
    }

    public void OnCameraFollowToggleChanged(bool isOn)
    {
        Options.Instance.cameraFollowBall = isOn;
        Options.Instance.Write();
    }

    public void ResetBestScoreButtonClicked()
    {
        ScoreTracker.Instance.bestScore = 0;
        ScoreTracker.Instance.Write();
    }

    private void OnDisable()
    {
        GameManager.Instance.RefreshOptions();
    }
}
