using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ResultScreenUI : MonoBehaviour
{
    [Header("Main References")]
    public GameObject resultPanel;
    public CanvasGroup panelCanvasGroup;
    public TextMeshProUGUI resultTitle;

    [Header("Buttons")]
    public GameObject nextButton;
    public GameObject retryButton;
    public GameObject mainMenuButton;

    [Header("Animation")]
    public float fadeDuration = 0.35f;
    public float titlePopDuration = 0.45f;
    public float buttonPopDuration = 0.28f;
    public float delayBetweenButtons = 0.18f;

    [Header("Scenes")]
    public string nextSceneName = "NextLevel";
    public string mainMenuSceneName = "MainMenu";

    private bool isShowing;

    private void Awake()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        HideButtonInstant(nextButton);
        HideButtonInstant(retryButton);
        HideButtonInstant(mainMenuButton);
    }

    public void ShowWin()
    {
        if (isShowing) return;

        isShowing = true;
        StartCoroutine(ShowResultRoutine("YOU WIN", true));
    }

    public void ShowLose()
    {
        if (isShowing) return;

        isShowing = true;
        StartCoroutine(ShowResultRoutine("YOU LOSE", false));
    }

    private IEnumerator ShowResultRoutine(string titleText, bool win)
    {
        Time.timeScale = 0f;

        resultPanel.SetActive(true);

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }

        resultTitle.text = titleText;
        resultTitle.transform.localScale = Vector3.zero;

        HideButtonInstant(nextButton);
        HideButtonInstant(retryButton);
        HideButtonInstant(mainMenuButton);

        yield return StartCoroutine(FadePanel());

        yield return StartCoroutine(PopTransform(resultTitle.transform, titlePopDuration, 1.25f));

        yield return new WaitForSecondsRealtime(0.2f);

        if (win)
        {
            yield return StartCoroutine(ShowButton(nextButton));
            SelectButton(nextButton);
        }
        else
        {
            yield return StartCoroutine(ShowButton(retryButton));
            SelectButton(retryButton);

            yield return new WaitForSecondsRealtime(delayBetweenButtons);

            yield return StartCoroutine(ShowButton(mainMenuButton));
        }
    }

    private IEnumerator FadePanel()
    {
        if (panelCanvasGroup == null)
            yield break;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeDuration;

            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, Smooth(t));

            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
    }

    private IEnumerator ShowButton(GameObject button)
    {
        if (button == null) yield break;

        button.SetActive(true);
        button.transform.localScale = Vector3.zero;

        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = button.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        float timer = 0f;

        while (timer < buttonPopDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / buttonPopDuration;
            float smooth = Smooth(t);

            canvasGroup.alpha = smooth;

            float scale = PopValue(t, 1.15f);
            button.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        canvasGroup.alpha = 1f;
        button.transform.localScale = Vector3.one;
    }

    private IEnumerator PopTransform(Transform target, float duration, float overshoot)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;

            float scale = PopValue(t, overshoot);
            target.localScale = Vector3.one * scale;

            yield return null;
        }

        target.localScale = Vector3.one;
    }

    private float PopValue(float t, float overshoot)
    {
        if (t < 0.7f)
        {
            float p = t / 0.7f;
            return Mathf.Lerp(0f, overshoot, Smooth(p));
        }
        else
        {
            float p = (t - 0.7f) / 0.3f;
            return Mathf.Lerp(overshoot, 1f, Smooth(p));
        }
    }

    private float Smooth(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void HideButtonInstant(GameObject button)
    {
        if (button == null) return;

        button.SetActive(false);
        button.transform.localScale = Vector3.zero;

        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void SelectButton(GameObject button)
    {
        if (button == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Next()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}