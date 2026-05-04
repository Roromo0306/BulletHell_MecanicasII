using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeOutDuration = 0.35f;
    public float fadeInDuration = 0.45f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        FadeIn();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FadeIn();
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeOutDuration));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
            yield return null;

        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeInDuration));

        isTransitioning = false;
    }

    public void FadeIn()
    {
        if (fadeCanvasGroup == null) return;

        StopCoroutine(nameof(FadeInRoutine));
        StartCoroutine(nameof(FadeInRoutine));
    }

    private IEnumerator FadeInRoutine()
    {
        yield return FadeRoutine(1f, 0f, fadeInDuration);
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        fadeCanvasGroup.alpha = to;

        if (to <= 0f)
            fadeCanvasGroup.blocksRaycasts = false;
    }
}