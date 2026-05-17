using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlowManager : MonoBehaviour
{
    [Header("Time")]
    public float slowTimeScale = 0.45f;
    public float slowDuration = 4f;

    [Header("Music")]
    public bool useGameMusicManager = true;
    public AudioSource fallbackMusicSource;
    public float slowMusicPitch = 0.65f;

    [Header("Blue Overlay")]
    public Image blueOverlay;
    public float overlayAlpha = 0.28f;
    public float fadeTime = 0.25f;

    private Coroutine routine;
    private float originalFixedDeltaTime;
    private float originalMusicPitch = 1f;
    private bool isSlowMotionActive;

    private void Awake()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;

        if (fallbackMusicSource != null)
            originalMusicPitch = fallbackMusicSource.pitch;

        SetOverlayAlpha(0f);
    }

    private void OnDisable()
    {
        if (isSlowMotionActive)
            ResetSlowMotionInstant();
    }

    public void PlaySlowMotion()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(SlowMotionRoutine());
    }

    private IEnumerator SlowMotionRoutine()
    {
        isSlowMotionActive = true;

        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * slowTimeScale;

        ApplySlowMusicPitch();

        yield return StartCoroutine(FadeOverlay(GetOverlayAlpha(), overlayAlpha));

        float timer = 0f;

        while (timer < slowDuration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return StartCoroutine(FadeOverlay(GetOverlayAlpha(), 0f));

        ResetSlowMotionInstant();

        routine = null;
    }

    private void ApplySlowMusicPitch()
    {
        if (useGameMusicManager && GameMusicManager.Instance != null)
        {
            GameMusicManager.Instance.SetMusicPitch(slowMusicPitch);
            return;
        }

        if (fallbackMusicSource != null)
            fallbackMusicSource.pitch = slowMusicPitch;
    }

    private void ResetMusicPitch()
    {
        if (useGameMusicManager && GameMusicManager.Instance != null)
        {
            GameMusicManager.Instance.ResetMusicPitch();
            return;
        }

        if (fallbackMusicSource != null)
            fallbackMusicSource.pitch = originalMusicPitch;
    }

    private void ResetSlowMotionInstant()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        ResetMusicPitch();
        SetOverlayAlpha(0f);

        isSlowMotionActive = false;
    }

    private IEnumerator FadeOverlay(float from, float to)
    {
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / fadeTime;
            SetOverlayAlpha(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetOverlayAlpha(to);
    }

    private float GetOverlayAlpha()
    {
        if (blueOverlay == null)
            return 0f;

        return blueOverlay.color.a;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (blueOverlay == null) return;

        Color c = blueOverlay.color;
        c.a = alpha;
        blueOverlay.color = c;
    }
}