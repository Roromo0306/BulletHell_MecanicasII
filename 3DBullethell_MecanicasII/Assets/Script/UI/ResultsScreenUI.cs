using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultScreenUI : MonoBehaviour
{
    [Header("Panel único")]
    public GameObject resultPanel;
    public CanvasGroup canvasGroup;
    public RectTransform panelRoot;

    [Header("Textos opcionales - UI Text")]
    public Text titleText;
    public Text descriptionText;

    [Header("Textos opcionales - TextMeshPro")]
    public TextMeshProUGUI titleTMP;
    public TextMeshProUGUI descriptionTMP;

    [Header("Texto Win / Lose")]
    public string winTitle = "YOU WIN";
    public string loseTitle = "YOU LOSE";
    public string winDescription = "";
    public string loseDescription = "";

    [Header("Botones")]
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

    [Header("Animación")]
    public float showDuration = 0.35f;
    public float hideDuration = 0.2f;
    public float hiddenScale = 0.75f;
    public float visibleScale = 1f;

    [Header("Opciones")]
    public bool pauseGameOnShow = true;

    private int currentIndex;
    private float nextMoveTime;
    private bool isShowing;
    private bool inputLocked;

    private void Awake()
    {
        if (canvasGroup == null && resultPanel != null)
            canvasGroup = resultPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null && resultPanel != null)
            canvasGroup = resultPanel.AddComponent<CanvasGroup>();

        if (panelRoot == null && resultPanel != null)
            panelRoot = resultPanel.GetComponent<RectTransform>();

        HideInstant();
    }

    private void Update()
    {
        if (!isShowing) return;
        if (inputLocked) return;

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
        if (isShowing) return;

        isShowing = true;
        inputLocked = true;

        if (pauseGameOnShow)
            Time.timeScale = 0f;

        SetTexts(won);
        StartCoroutine(ShowRoutine());
    }

    private void SetTexts(bool won)
    {
        string title = won ? winTitle : loseTitle;
        string description = won ? winDescription : loseDescription;

        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        if (titleTMP != null)
            titleTMP.text = title;

        if (descriptionTMP != null)
            descriptionTMP.text = description;
    }

    private IEnumerator ShowRoutine()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panelRoot != null)
            panelRoot.localScale = Vector3.one * hiddenScale;

        float timer = 0f;

        while (timer < showDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / showDuration;
            t = EaseOutBack(t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            if (panelRoot != null)
            {
                panelRoot.localScale = Vector3.LerpUnclamped(
                    Vector3.one * hiddenScale,
                    Vector3.one * visibleScale,
                    t
                );
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (panelRoot != null)
            panelRoot.localScale = Vector3.one * visibleScale;

        currentIndex = 0;
        inputLocked = false;

        SelectCurrentButton();
    }

    private void HideInstant()
    {
        isShowing = false;
        inputLocked = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panelRoot != null)
            panelRoot.localScale = Vector3.one * hiddenScale;

        if (resultPanel != null)
            resultPanel.SetActive(false);
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

        StartCoroutine(SubmitButtonRoutine(selectedButton));
    }
    private IEnumerator SubmitButtonRoutine(Button selectedButton)
    {
        inputLocked = true;

        if (EventSystem.current != null)
        {
            ExecuteEvents.Execute<ISubmitHandler>(
                selectedButton.gameObject,
                new BaseEventData(EventSystem.current),
                ExecuteEvents.submitHandler
            );
        }

        yield return new WaitForSecondsRealtime(0.12f);

        selectedButton.onClick.Invoke();

        inputLocked = false;
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
            Debug.LogWarning("ResultScreenUI: no hay Next Scene Name asignado.");
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

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}