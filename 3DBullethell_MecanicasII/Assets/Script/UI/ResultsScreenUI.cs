using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ResultScreenUI : MonoBehaviour
{
    [Header("Panel único")]
    public GameObject resultPanel;

    [Header("Textos opcionales")]
    public Text titleText;
    public Text descriptionText;

    [Header("Texto Win / Lose")]
    public string winTitle = "YOU WIN";
    public string loseTitle = "YOU LOSE";
    public string winDescription = "";
    public string loseDescription = "";

    [Header("Botones del panel")]
    public Button[] buttons;

    [Header("Navegación mando")]
    public string verticalAxis = "Vertical";
    public float deadZone = 0.45f;
    public float moveCooldown = 0.22f;

    [Header("Confirmar")]
    public KeyCode submitButton = KeyCode.JoystickButton1;
    public KeyCode submitKeyboard = KeyCode.Return;

    [Header("Escenas")]
    public string nextSceneName = "LV2";
    public string mainMenuSceneName = "MainMenu";

    [Header("Opciones")]
    public bool pauseGameOnShow = true;

    private int currentIndex;
    private float nextMoveTime;
    private bool isShowing;
    private bool playerWon;

    private void Awake()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        isShowing = false;
    }

    private void Update()
    {
        if (!isShowing) return;

        HandleNavigation();
        HandleSubmit();
    }

    public void ShowWin()
    {
        ShowResult(true);
    }

    public void ShowLose()
    {
        ShowResult(false);
    }

    private void ShowResult(bool won)
    {
        playerWon = won;
        isShowing = true;

        if (pauseGameOnShow)
            Time.timeScale = 0f;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (titleText != null)
            titleText.text = won ? winTitle : loseTitle;

        if (descriptionText != null)
            descriptionText.text = won ? winDescription : loseDescription;

        currentIndex = 0;
        SelectCurrentButton();
    }

    private void HandleNavigation()
    {
        if (buttons == null || buttons.Length == 0) return;

        float vertical = Input.GetAxisRaw(verticalAxis);

        if (Mathf.Abs(vertical) < deadZone)
            return;

        if (Time.unscaledTime < nextMoveTime)
            return;

        if (vertical > 0f)
            MoveSelection(-1);
        else if (vertical < 0f)
            MoveSelection(1);

        nextMoveTime = Time.unscaledTime + moveCooldown;
    }

    private void MoveSelection(int direction)
    {
        if (buttons == null || buttons.Length == 0) return;

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = buttons.Length - 1;

        if (currentIndex >= buttons.Length)
            currentIndex = 0;

        SelectCurrentButton();
    }

    private void SelectCurrentButton()
    {
        if (buttons == null || buttons.Length == 0) return;

        Button selectedButton = buttons[currentIndex];

        if (selectedButton == null) return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectedButton.gameObject);
        }

        selectedButton.Select();
    }

    private void HandleSubmit()
    {
        if (!Input.GetKeyDown(submitButton) && !Input.GetKeyDown(submitKeyboard))
            return;

        if (buttons == null || buttons.Length == 0) return;

        Button selectedButton = buttons[currentIndex];

        if (selectedButton == null) return;

        if (EventSystem.current != null)
        {
            ExecuteEvents.Execute<ISubmitHandler>(
                selectedButton.gameObject,
                new BaseEventData(EventSystem.current),
                ExecuteEvents.submitHandler
            );
        }
        else
        {
            selectedButton.onClick.Invoke();
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }

    public void Restart()
    {
        Retry();
    }

    public void GoToNextScene()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("No hay Next Scene Name asignado en ResultScreenUI.");
            return;
        }

        LoadScene(nextSceneName);
    }

    public void NextLevel()
    {
        GoToNextScene();
    }

    public void Continue()
    {
        GoToNextScene();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        LoadScene(mainMenuSceneName);
    }

    public void GoToMenu()
    {
        GoToMainMenu();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Debug.Log("Cerrar juego");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadScene(string sceneName)
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}