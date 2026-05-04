using System.Collections;
using UnityEngine;

public class ButtonSelectionVisual : MonoBehaviour
{
    [Header("Target")]
    public RectTransform target;

    [Header("Scale")]
    public float normalScale = 1f;
    public float selectedScale = 1.12f;
    public float pressedScale = 0.92f;
    public float scaleSpeed = 16f;

    [Header("Old Border - Optional")]
    public GameObject borderVisual;

    private Coroutine scaleRoutine;
    private bool selected;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (borderVisual != null)
            borderVisual.SetActive(false);

        if (target != null)
            target.localScale = Vector3.one * normalScale;
    }

    public void SetSelected(bool value)
    {
        selected = value;

        // Apagamos siempre el reborde viejo
        if (borderVisual != null)
            borderVisual.SetActive(false);

        ScaleTo(selected ? selectedScale : normalScale);
    }

    public void PlayPressedFeedback()
    {
        if (!gameObject.activeInHierarchy) return;

        StartCoroutine(PressedRoutine());
    }

    private IEnumerator PressedRoutine()
    {
        ScaleTo(pressedScale);

        yield return new WaitForSecondsRealtime(0.08f);

        ScaleTo(selected ? selectedScale : normalScale);
    }

    private void ScaleTo(float scale)
    {
        if (target == null) return;

        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleRoutine(scale));
    }

    private IEnumerator ScaleRoutine(float scale)
    {
        Vector3 targetScale = Vector3.one * scale;

        while (Vector3.Distance(target.localScale, targetScale) > 0.01f)
        {
            target.localScale = Vector3.Lerp(
                target.localScale,
                targetScale,
                Time.unscaledDeltaTime * scaleSpeed
            );

            yield return null;
        }

        target.localScale = targetScale;
        scaleRoutine = null;
    }
}