using System.Collections;
using UnityEngine;

public class TreasureMapPoint : MonoBehaviour
{
    [Header("Progress")]
    public int revealStage = 2;

    [Header("References")]
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;

    [Header("Pop Animation")]
    public float popDuration = 0.3f;
    public float startScaleMultiplier = 0f;
    public float overshootScaleMultiplier = 1.2f;

    private Vector3 originalScale;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalScale = rectTransform != null ? rectTransform.localScale : transform.localScale;
    }

    public void HideInstant()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        SetScale(originalScale * startScaleMultiplier);
    }

    public void ShowInstant()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        SetScale(originalScale);
    }

    public IEnumerator PlayPop()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        Vector3 startScale = originalScale * startScaleMultiplier;
        Vector3 overshootScale = originalScale * overshootScaleMultiplier;
        Vector3 endScale = originalScale;

        SetScale(startScale);

        float timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / popDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            Vector3 currentScale;

            if (t < 0.65f)
            {
                float t1 = t / 0.65f;
                currentScale = Vector3.LerpUnclamped(startScale, overshootScale, EaseOutBack(t1));
            }
            else
            {
                float t2 = (t - 0.65f) / 0.35f;
                currentScale = Vector3.LerpUnclamped(overshootScale, endScale, Mathf.SmoothStep(0f, 1f, t2));
            }

            SetScale(currentScale);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        SetScale(endScale);
    }

    private void SetScale(Vector3 scale)
    {
        if (rectTransform != null)
            rectTransform.localScale = scale;
        else
            transform.localScale = scale;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}