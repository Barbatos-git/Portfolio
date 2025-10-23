using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class PauseUIManager : MonoBehaviour
{
    public static PauseUIManager Instance;

    [Header("Pause Menu")]
    public GameObject pauseMenuUI;
    public GameObject pausePanel;         // 暂停菜单主要容器（用来设置选中框父物体）
    public GameObject settingsPanel;
    public Button firstSelectedButton;   // 暂停菜单默认选中按钮（Button的GameObject）
    public GameObject[] uiToHideWhenPaused;

    [Header("Character Info UI")]
    public GameObject combatPanel;       // 战斗UI容器（用来设置选中框父物体）
    public GameObject numericalPanel;
    public GameObject statusBarPanel;

    [Header("Post Processing")]
    public Volume postProcessVolume;

    [Header("Pause Menu Button")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainmenuButton;
    public Button quitButton; 

    private DepthOfField blurEffect;
    private bool isPaused = false;
    public static bool IsGamePaused { get; private set; }

    private GameObject currentSelectedCharacter;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false);

        //if (combatPanel != null) combatPanel.SetActive(false);
        //if (numericalPanel != null) numericalPanel.SetActive(false);
        //if (statusBarPanel != null) statusBarPanel.SetActive(false);

        if (postProcessVolume && postProcessVolume.profile.TryGet(out DepthOfField dof))
        {
            blurEffect = dof;
            blurEffect.active = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3) ||
            Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (pauseMenuUI.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Joystick1Button1))
            {
                ResumeGame();
            }
        }

        if (isPaused && settingsPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                CloseSettings();
            }
        }
    }

    // ==== Pause Functions ====
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        IsGamePaused = true;
        InputLock.IsLocked = true;

        pauseMenuUI.SetActive(true);
        if (blurEffect != null) blurEffect.active = true;

        foreach (var ui in uiToHideWhenPaused)
        {
            if (ui != null) ui.SetActive(false);
        }

        SetupNavigationLoop(new Button[] { resumeButton, settingsButton, mainmenuButton, quitButton });

        // 注册按钮鼠标悬停事件
        UISelectionManager.Instance.RegisterButton(resumeButton);
        UISelectionManager.Instance.RegisterButton(settingsButton);
        UISelectionManager.Instance.RegisterButton(mainmenuButton);
        UISelectionManager.Instance.RegisterButton(quitButton);

        // 设置选中框位置
        UISelectionManager.Instance.SetSelectionParent(pausePanel.transform as RectTransform);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);

        UISelectionManager.Instance.SetInitialSelectedDelayed(firstSelectedButton.gameObject);

        //if (UISelectionManager.Instance != null && pausePanel != null && firstSelectedButton != null)
        //{
        //    UISelectionManager.Instance.SetSelectionParent(pausePanel.transform as RectTransform);
        //    UISelectionManager.Instance.SetInitialSelected(firstSelectedButton.gameObject);
        //    EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        //}
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        IsGamePaused = false;
        InputLock.IsLocked = false;

        pauseMenuUI.SetActive(false);
        if (blurEffect != null) blurEffect.active = false;

        foreach (var ui in uiToHideWhenPaused)
        {
            if (ui != null) ui.SetActive(true);
        }

        // 选中框切换到战斗UI父物体，隐藏选中框
        if (UISelectionManager.Instance != null && combatPanel != null)
        {
            UISelectionManager.Instance.SetSelectionParent(combatPanel.transform as RectTransform);
            UISelectionManager.Instance.Hide();

            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OpenSettings()
    {
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);

        // 切换选中框父物体到设置面板
        if (UISelectionManager.Instance != null && settingsPanel != null)
        {
            UISelectionManager.Instance.SetSelectionParent(settingsPanel.transform as RectTransform);
            EventSystem.current.SetSelectedGameObject(null); // 可选：避免卡住焦点
        }
    }

    public void CloseSettings()
    {
        Time.timeScale = 0f;

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);

        // 恢复暂停菜单选中框显示
        if (UISelectionManager.Instance != null && pausePanel != null && firstSelectedButton != null)
        {
            UISelectionManager.Instance.SetSelectionParent(pausePanel.transform as RectTransform);
            UISelectionManager.Instance.SetInitialSelected(firstSelectedButton.gameObject);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        GameResetUtility.ResetGame();

        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quit Game");
    }

    // ==== Character UI Display ====
    public void ShowCharacterUI(GameObject target)
    {
        if (IsGamePaused) return;

        if (target == null || target == currentSelectedCharacter) return;

        currentSelectedCharacter = target;

        if (combatPanel != null) combatPanel.SetActive(true);
        if (numericalPanel != null) numericalPanel.SetActive(true);
        if (statusBarPanel != null) statusBarPanel.SetActive(true);

        // 战斗UI激活时，切换选中框父物体（如有默认按钮可设）
        if (UISelectionManager.Instance != null && combatPanel != null)
        {
            UISelectionManager.Instance.SetSelectionParent(combatPanel.transform as RectTransform);
            // UISelectionManager.Instance.SetInitialSelected(defaultBattleUIButtonRectTransform);
        }
    }

    public void HideCharacterUI()
    {
        if (combatPanel != null) combatPanel.SetActive(false);
        if (numericalPanel != null) numericalPanel.SetActive(false);
        if (statusBarPanel != null) statusBarPanel.SetActive(false);

        currentSelectedCharacter = null;

        if (UISelectionManager.Instance != null)
        {
            UISelectionManager.Instance.Hide();
        }
    }

    void SetupNavigationLoop(Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Navigation nav = buttons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = buttons[(i - 1 + buttons.Length) % buttons.Length];
            nav.selectOnDown = buttons[(i + 1) % buttons.Length];
            buttons[i].navigation = nav;
        }
    }
}