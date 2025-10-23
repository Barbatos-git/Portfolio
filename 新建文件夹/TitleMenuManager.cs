using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class TitleMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuUI;
    public GameObject settingsMenuUI;

    [Header("First Selected Buttons")]
    public Button mainFirstSelectedButton;
    public Button settingsFirstSelectedButton;

    public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    void Start()
    {
        OpenMainMenu();

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);

        UISelectionManager.Instance.RegisterButton(startButton);
        UISelectionManager.Instance.RegisterButton(settingsButton);
        UISelectionManager.Instance.RegisterButton(quitButton);

        UISelectionManager.Instance.SetInitialSelectedDelayed(mainFirstSelectedButton.gameObject);

        //startButton.onClick.AddListener(StartGame);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            CloseSettings();
        }
    }

    public void OpenMainMenu()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);

        SetSelectedButton(mainFirstSelectedButton.gameObject);
    }

    public void OpenSettingsMenu()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
    }

    private void SetSelectedButton(GameObject buttonGO)
    {
        if (buttonGO == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonGO);
    }

    public void StartGame()
    {
        //Debug.Log("StartGame called");
        SceneManager.LoadScene("ControllerScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}