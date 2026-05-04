using System.Collections;
using UnityEngine;

public class MenuPanelAnimator : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public RectTransform panelRoot;

    [Header("Animation")]
    public float animationDuration = 0.25f;
    public float hiddenScale = 0.85f;
    public float visibleScale = 1f;

    private Coroutine routine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (panelRoot == null)
            panelRoot = GetComponent<RectTransform>();

        HideInstant();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine());
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(HideRoutine());
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
            panelRoot.localScale = Vector3.one * hiddenScale;

        gameObject.SetActive(false);
    }

    private IEnumerator ShowRoutine()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / animationDuration;
            t = EaseOutBack(t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            if (panelRoot != null)
                panelRoot.localScale = Vector3.LerpUnclamped(
                    Vector3.one * hiddenScale,
                    Vector3.one * visibleScale,
                    t
                );

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

        routine = null;
    }

    private IEnumerator HideRoutine()
    {
        if (canvasGroup != null)
            canvasGroup.interactable = false;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / animationDuration;
            t = EaseInBack(t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            if (panelRoot != null)
                panelRoot.localScale = Vector3.LerpUnclamped(
                    Vector3.one * visibleScale,
                    Vector3.one * hiddenScale,
                    t
                );

            yield return null;
        }

        HideInstant();
        routine = null;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return c3 * t * t * t - c1 * t * t;
    }
}