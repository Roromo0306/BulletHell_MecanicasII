using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    public Image vignetteImage;

    public float fadeInTime = 0.05f;
    public float fadeOutTime = 0.25f;
    public float maxAlpha = 0.45f;

    private Coroutine routine;

    private void Awake()
    {
        SetAlpha(0f);
    }

    public void Play()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(VignetteRoutine());
    }

    IEnumerator VignetteRoutine()
    {
        float timer = 0f;

        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeInTime;
            SetAlpha(Mathf.Lerp(0f, maxAlpha, t));
            yield return null;
        }

        timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutTime;
            SetAlpha(Mathf.Lerp(maxAlpha, 0f, t));
            yield return null;
        }

        SetAlpha(0f);
    }

    void SetAlpha(float alpha)
    {
        if (vignetteImage == null) return;

        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;
    }
}