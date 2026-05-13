using System.Collections;
using System.Collections.Generic;
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

    [Header("Botones por estado")]
    public Button nextButton;
    public Button retryButton;
    public Button mainMenuButton;

    [Header("Navegación mando")]
    public string verticalAxis = "Horizontal";
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

    private readonly List<Button> activeButtons = new List<Button>();

    private int currentIndex;
    private float nextMoveTime;
    private bool isShowing;
    private bool inputLocked;

    private Vector3 originalPanelScale = Vector3.one;

    private void Awake()
    {
        if (canvasGroup == null && resultPanel != null)
            canvasGroup = resultPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null && resultPanel != null)
            canvasGroup = resultPanel.AddComponent<CanvasGroup>();

        if (panelRoot == null && resultPanel != null)
            panelRoot = resultPanel.GetComponent<RectTransform>();

        if (panelRoot != null)
            originalPanelScale = panelRoot.localScale;

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
        SetButtonsForResult(won);

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

    private void SetButtonsForResult(bool won)
    {
        activeButtons.Clear();

        SetButtonVisible(nextButton, won);
        SetButtonVisible(retryButton, !won);
        SetButtonVisible(mainMenuButton, true);

        if (won)
        {
            AddActiveButton(nextButton);
            AddActiveButton(mainMenuButton);
        }
        else
        {
            AddActiveButton(retryButton);
            AddActiveButton(mainMenuButton);
        }

        currentIndex = 0;
    }

    private void SetButtonVisible(Button button, bool visible)
    {
        if (button == null) return;

        button.gameObject.SetActive(visible);
        button.interactable = visible;

        if (visible)
            RefreshButtonVisual(button);
    }

    private void AddActiveButton(Button button)
    {
        if (button == null) return;
        if (!button.gameObject.activeSelf) return;

        activeButtons.Add(button);
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

        RefreshActiveButtonsVisual();

        inputLocked = false;

        SelectCurrentButton();
    }

    private void HideInstant()
    {
        isShowing = false;
        inputLocked = false;

        activeButtons.Clear();

        SetButtonVisible(nextButton, false);
        SetButtonVisible(retryButton, false);
        SetButtonVisible(mainMenuButton, false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panelRoot != null)
            panelRoot.localScale = originalPanelScale * hiddenScale;

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void RefreshActiveButtonsVisual()
    {
        foreach (Button button in activeButtons)
        {
            if (button == null) continue;

            button.interactable = true;
            RefreshButtonVisual(button);
        }
    }

    private void RefreshButtonVisual(Button button)
    {
        if (button == null) return;

        ColorBlock colors = button.colors;
        button.colors = colors;

        if (button.targetGraphic != null)
            button.targetGraphic.CrossFadeColor(colors.normalColor, 0f, true, true);
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