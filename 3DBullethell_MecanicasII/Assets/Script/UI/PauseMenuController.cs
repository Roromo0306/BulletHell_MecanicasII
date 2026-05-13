using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject pausePanel;
    public CanvasGroup canvasGroup;
    public RectTransform panelRoot;

    [Header("Buttons")]
    public Button resumeButton;
    public Button resetButton;
    public Button mainMenuButton;
    public Button closeButton;

    [Header("Navigation Order")]
    public Button[] buttons;

    [Header("Input")]
    public KeyCode pauseKeyboard = KeyCode.Escape;
    public KeyCode pauseGamepad = KeyCode.JoystickButton9; // Options normalmente

    [Header("Navigation")]
    public string verticalAxis = "Vertical";
    public float deadZone = 0.45f;
    public float moveCooldown = 0.22f;

    [Header("Submit / Cancel")]
    public KeyCode submitButton = KeyCode.JoystickButton1;
    public KeyCode submitKeyboard = KeyCode.Return;
    public KeyCode cancelButton = KeyCode.JoystickButton0;
    public KeyCode cancelKeyboard = KeyCode.Backspace;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Animation")]
    public float showDuration = 0.25f;
    public float hideDuration = 0.18f;
    public float hiddenScale = 0.8f;
    public float visibleScale = 1f;

    private readonly List<Button> activeButtons = new List<Button>();

    private int currentIndex;
    private float nextMoveTime;
    private bool isPaused;
    private bool inputLocked;
    private Vector3 originalPanelScale = Vector3.one;

    private void Awake()
    {
        if (canvasGroup == null && pausePanel != null)
            canvasGroup = pausePanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null && pausePanel != null)
            canvasGroup = pausePanel.AddComponent<CanvasGroup>();

        if (panelRoot == null && pausePanel != null)
            panelRoot = pausePanel.GetComponent<RectTransform>();

        if (panelRoot != null)
            originalPanelScale = panelRoot.localScale;

        BuildButtonList();
        HideInstant();
    }

    private void Update()
    {
        HandlePauseInput();

        if (!isPaused) return;
        if (inputLocked) return;

        HandleNavigation();
        HandleSubmit();
        HandleCancel();
    }

    private void BuildButtonList()
    {
        activeButtons.Clear();

        if (buttons != null && buttons.Length > 0)
        {
            foreach (Button button in buttons)
            {
                if (button != null)
                    activeButtons.Add(button);
            }

            return;
        }

        AddButton(resumeButton);
        AddButton(resetButton);
        AddButton(mainMenuButton);
        AddButton(closeButton);
    }

    private void AddButton(Button button)
    {
        if (button == null) return;

        activeButtons.Add(button);
    }

    private void HandlePauseInput()
    {
        if (inputLocked) return;

        if (Input.GetKeyDown(pauseKeyboard) || Input.GetKeyDown(pauseGamepad))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        inputLocked = true;

        Time.timeScale = 0f;

        BuildButtonList();
        StartCoroutine(ShowRoutine());
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        StartCoroutine(HideRoutine());
    }

    public void ClosePauseMenu()
    {
        ResumeGame();
    }

    public void ResetScene()
    {
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(currentScene);
        else
            SceneManager.LoadScene(currentScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator ShowRoutine()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        Vector3 startScale = originalPanelScale * hiddenScale;
        Vector3 endScale = originalPanelScale * visibleScale;

        if (panelRoot != null)
            panelRoot.localScale = startScale;

        float timer = 0f;

        while (timer < showDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / showDuration;
            t = EaseOutBack(t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            if (panelRoot != null)
                panelRoot.localScale = Vector3.LerpUnclamped(startScale, endScale, t);

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (panelRoot != null)
            panelRoot.localScale = endScale;

        RefreshButtonsVisual();

        currentIndex = 0;
        inputLocked = false;

        SelectCurrentButton();
    }

    private IEnumerator HideRoutine()
    {
        inputLocked = true;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        Vector3 startScale = originalPanelScale * visibleScale;
        Vector3 endScale = originalPanelScale * hiddenScale;

        float timer = 0f;

        while (timer < hideDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / hideDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            if (panelRoot != null)
                panelRoot.localScale = Vector3.LerpUnclamped(startScale, endScale, t);

            yield return null;
        }

        HideInstant();

        Time.timeScale = 1f;

        isPaused = false;
        inputLocked = false;
    }

    private void HideInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panelRoot != null)
            panelRoot.localScale = originalPanelScale * hiddenScale;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void HandleNavigation()
    {
        if (activeButtons.Count == 0) return;

        float input = Input.GetAxisRaw(verticalAxis);

        if (Mathf.Abs(input) < deadZone)
            return;

        if (Time.unscaledTime < nextMoveTime)
            return;

        if (input > 0f)
            MoveSelection(-1);
        else if (input < 0f)
            MoveSelection(1);

        nextMoveTime = Time.unscaledTime + moveCooldown;
    }

    private void MoveSelection(int direction)
    {
        if (activeButtons.Count == 0) return;

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = activeButtons.Count - 1;

        if (currentIndex >= activeButtons.Count)
            currentIndex = 0;

        SelectCurrentButton();
    }

    private void SelectCurrentButton()
    {
        if (activeButtons.Count == 0) return;

        Button selectedButton = activeButtons[currentIndex];

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

        if (activeButtons.Count == 0) return;

        Button selectedButton = activeButtons[currentIndex];

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

        inputLocked = false;

        selectedButton.onClick.Invoke();
    }

    private void HandleCancel()
    {
        if (!Input.GetKeyDown(cancelButton) && !Input.GetKeyDown(cancelKeyboard))
            return;

        ResumeGame();
    }

    private void RefreshButtonsVisual()
    {
        foreach (Button button in activeButtons)
        {
            if (button == null) continue;

            button.interactable = true;

            ColorBlock colors = button.colors;
            button.colors = colors;

            if (button.targetGraphic != null)
                button.targetGraphic.CrossFadeColor(colors.normalColor, 0f, true, true);
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}