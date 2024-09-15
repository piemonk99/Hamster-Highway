using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    void Start()
    {
        Options.Read();
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
