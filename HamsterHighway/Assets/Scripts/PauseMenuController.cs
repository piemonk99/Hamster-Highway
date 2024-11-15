using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject gameHUD;

    public void PauseButtonClicked()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
        gameHUD.SetActive(false);
    }
    public void ResumeButtonClicked()
    {
        Time.timeScale = 1;
        gameHUD.SetActive(true);
        gameObject.SetActive(false);
    }
    public void OptionsButtonClicked()
    {
        optionsMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    public void MainMenuButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
