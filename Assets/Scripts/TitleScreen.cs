using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    private PlayerInput playerInput;
    
    [Header("Menu Screens")]
    public GameObject TitleScreenUI;
    public GameObject OptionsUI;
    public GameObject KeyboardBindsUI;

    [Header("Selected Buttons")]
    public Button TitleScreenStartButton;
    public Button OptionsStartButton;
    public Button KeybindsStartButton;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.SwitchCurrentActionMap("Menus");

        playerInput.actions["Cancel"].performed += ctx => Cancel();
    }
    
    public void Cancel()
    {   
        if (KeyboardBindsUI.activeSelf == true)
        {
            Options();
            Debug.Log("exit keybinds");
        }

        if (OptionsUI.activeSelf == true)
        {
            MainMenu();
            Debug.Log("exit options");
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void MainMenu()
    {
        TitleScreenUI.SetActive(true);
        OptionsUI.SetActive(false);

        TitleScreenStartButton.Select();
    }

    public void Options()
    {
        TitleScreenUI.SetActive(false);
        OptionsUI.SetActive(true);
        KeyboardBindsUI.SetActive(false);

        OptionsStartButton.Select();
    }

    public void KeyboardBinds()
    {
        OptionsUI.SetActive(false);
        KeyboardBindsUI.SetActive(true);

        KeybindsStartButton.Select();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting...");
    }
}
