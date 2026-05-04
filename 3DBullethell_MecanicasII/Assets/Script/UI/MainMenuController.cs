using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    public string cinematicSceneName = "IntroCinematic";

    [Header("Main Menu")]
    public CanvasGroup mainMenuCanvasGroup;
    public RectTransform title;
    public RectTransform buttonsRoot;

    [Header("Main Buttons Order")]
    public Button[] mainButtons;
    // Orden recomendado:
    // 0 Play
    // 1 Options
    // 2 Credits
    // 3 Quit

    [Header("Panels")]
    public MenuPanelAnimator creditsPanel;
    public MenuPanelAnimator optionsPanel;

    [Header("Panel Buttons")]
    public Button creditsCloseButton;
    public Button optionsCloseButton;

    [Header("Intro Animation")]
    public float introDuration = 0.6f;

    [Header("Gamepad Navigation")]
    public string verticalAxis = "Vertical";
    public float deadZone = 0.45f;
    public float moveCooldown = 0.22f;

    [Header("Gamepad Buttons")]
    public KeyCode submitButton = KeyCode.JoystickButton0; // A / X
    public KeyCode cancelButton = KeyCode.JoystickButton1; // B / Circle

    [Header("Keyboard Fallback")]
    public KeyCode submitKeyboard = KeyCode.Return;
    public KeyCode cancelKeyboard = KeyCode.Escape;

    private int currentIndex;
    private float nextMoveTime;
    private bool inputLocked;

    private MenuState currentState = MenuState.Main;

    private enum MenuState
    {
        Main,
        Credits,
        Options
    }

    private void Start()
    {
        ClosePanelsInstant();
        StartCoroutine(IntroRoutine());
    }

    private void Update()
    {
        if (inputLocked) return;

        HandleNavigation();
        HandleSubmit();
        HandleCancel();
    }

    private IEnumerator IntroRoutine()
    {
        inputLocked = true;

        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
        }

        if (title != null)
            title.localScale = Vector3.one * 0.75f;

        if (buttonsRoot != null)
            buttonsRoot.localScale = Vector3.one * 0.85f;

        float timer = 0f;

        while (timer < introDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / introDuration;
            t = EaseOutBack(t);

            if (mainMenuCanvasGroup != null)
                mainMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            if (title != null)
                title.localScale = Vector3.LerpUnclamped(Vector3.one * 0.75f, Vector3.one, t);

            if (buttonsRoot != null)
                buttonsRoot.localScale = Vector3.LerpUnclamped(Vector3.one * 0.85f, Vector3.one, t);

            yield return null;
        }

        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
        }

        inputLocked = false;

        currentState = MenuState.Main;
        currentIndex = 0;
        SelectCurrentButton();
    }

    private void HandleNavigation()
    {
        if (currentState != MenuState.Main)
            return;

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
        if (mainButtons == null || mainButtons.Length == 0)
            return;

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = mainButtons.Length - 1;

        if (currentIndex >= mainButtons.Length)
            currentIndex = 0;

        SelectCurrentButton();
    }

    private void SelectCurrentButton()
    {
        if (mainButtons == null || mainButtons.Length == 0)
            return;

        Button selectedButton = mainButtons[currentIndex];

        if (selectedButton == null)
            return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectedButton.gameObject);
        }

        selectedButton.Select();
    }

    private void SelectButton(Button button)
    {
        if (button == null)
            return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        button.Select();
    }

    private void HandleSubmit()
    {
        if (!Input.GetKeyDown(submitButton) && !Input.GetKeyDown(submitKeyboard))
            return;

        if (currentState == MenuState.Main)
        {
            if (mainButtons == null || mainButtons.Length == 0)
                return;

            Button selectedButton = mainButtons[currentIndex];

            if (selectedButton != null)
                selectedButton.onClick.Invoke();

            return;
        }

        if (currentState == MenuState.Credits)
        {
            if (creditsCloseButton != null)
                creditsCloseButton.onClick.Invoke();

            return;
        }

        if (currentState == MenuState.Options)
        {
            if (optionsCloseButton != null)
                optionsCloseButton.onClick.Invoke();

            return;
        }
    }

    private void HandleCancel()
    {
        if (!Input.GetKeyDown(cancelButton) && !Input.GetKeyDown(cancelKeyboard))
            return;

        if (currentState == MenuState.Credits)
        {
            CloseCredits();
            return;
        }

        if (currentState == MenuState.Options)
        {
            CloseOptions();
            return;
        }
    }

    public void Play()
    {
        if (inputLocked) return;

        StartCoroutine(LoadSceneRoutine(cinematicSceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        inputLocked = true;

        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
        }

        yield return new WaitForSecondsRealtime(0.15f);

        SceneManager.LoadScene(sceneName);
    }

    public void OpenCredits()
    {
        if (inputLocked) return;

        currentState = MenuState.Credits;

        if (optionsPanel != null)
            optionsPanel.Hide();

        if (creditsPanel != null)
            creditsPanel.Show();

        SelectButton(creditsCloseButton);
    }

    public void CloseCredits()
    {
        currentState = MenuState.Main;

        if (creditsPanel != null)
            creditsPanel.Hide();

        SelectCurrentButton();
    }

    public void OpenOptions()
    {
        if (inputLocked) return;

        currentState = MenuState.Options;

        if (creditsPanel != null)
            creditsPanel.Hide();

        if (optionsPanel != null)
            optionsPanel.Show();

        SelectButton(optionsCloseButton);
    }

    public void CloseOptions()
    {
        currentState = MenuState.Main;

        if (optionsPanel != null)
            optionsPanel.Hide();

        SelectCurrentButton();
    }

    public void QuitGame()
    {
        if (inputLocked) return;

        Debug.Log("Cerrar juego");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ClosePanelsInstant()
    {
        if (creditsPanel != null)
            creditsPanel.gameObject.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.gameObject.SetActive(false);
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}