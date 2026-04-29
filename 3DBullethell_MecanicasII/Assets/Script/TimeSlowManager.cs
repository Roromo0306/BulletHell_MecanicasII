using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlowManager : MonoBehaviour
{
    [Header("Time")]
    public float slowTimeScale = 0.45f;
    public float slowDuration = 4f;

    [Header("Music")]
    public AudioSource bossMusic;
    public float slowMusicPitch = 0.65f;

    [Header("Blue Overlay")]
    public Image blueOverlay;
    public float overlayAlpha = 0.28f;
    public float fadeTime = 0.25f;

    private Coroutine routine;
    private float originalFixedDeltaTime;
    private float originalMusicPitch = 1f;

    private void Awake()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;

        if (bossMusic != null)
            originalMusicPitch = bossMusic.pitch;

        SetOverlayAlpha(0f);
    }

    public void PlaySlowMotion()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(SlowMotionRoutine());
    }

    private IEnumerator SlowMotionRoutine()
    {
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * slowTimeScale;

        if (bossMusic != null)
            bossMusic.pitch = slowMusicPitch;

        yield return StartCoroutine(FadeOverlay(0f, overlayAlpha));

        float timer = 0f;

        while (timer < slowDuration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return StartCoroutine(FadeOverlay(overlayAlpha, 0f));

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        if (bossMusic != null)
            bossMusic.pitch = originalMusicPitch;

        routine = null;
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

    private void SetOverlayAlpha(float alpha)
    {
        if (blueOverlay == null) return;

        Color c = blueOverlay.color;
        c.a = alpha;
        blueOverlay.color = c;
    }
}