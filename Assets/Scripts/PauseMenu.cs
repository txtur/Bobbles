using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    private PlayerInput playerInput;
    public GameObject pauseMenuUI;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Pause"].performed += ctx => Escape();
    }

    void Escape()
    {
        if (GameIsPaused)
        {
            Resume();
        } 
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadOptions()
    {

    }
}