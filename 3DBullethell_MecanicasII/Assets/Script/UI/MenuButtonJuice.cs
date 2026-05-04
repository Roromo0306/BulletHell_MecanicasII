using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonJuice : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler,
    ISubmitHandler
{
    [Header("Scale")]
    public float normalScale = 1f;
    public float hoverScale = 1.12f;
    public float clickScale = 0.92f;
    public float tweenSpeed = 14f;

    [Header("Optional")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private RectTransform rectTransform;
    private Coroutine scaleRoutine;
    private bool selectedOrHovered;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one * normalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        selectedOrHovered = true;
        PlaySound(hoverSound);
        ScaleTo(hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selectedOrHovered = false;
        ScaleTo(normalScale);
    }

    public void OnSelect(BaseEventData eventData)
    {
        selectedOrHovered = true;
        PlaySound(hoverSound);
        ScaleTo(hoverScale);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selectedOrHovered = false;
        ScaleTo(normalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlaySound(clickSound);
        ScaleTo(clickScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ScaleTo(selectedOrHovered ? hoverScale : normalScale);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlaySound(clickSound);

        if (gameObject.activeInHierarchy)
            StartCoroutine(SubmitPunchRoutine());
    }

    private IEnumerator SubmitPunchRoutine()
    {
        ScaleTo(clickScale);

        yield return new WaitForSecondsRealtime(0.08f);

        ScaleTo(hoverScale);
    }

    private void ScaleTo(float targetScale)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(float targetScale)
    {
        Vector3 start = rectTransform.localScale;
        Vector3 target = Vector3.one * targetScale;

        while (Vector3.Distance(rectTransform.localScale, target) > 0.01f)
        {
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                target,
                Time.unscaledDeltaTime * tweenSpeed
            );

            yield return null;
        }

        rectTransform.localScale = target;
        scaleRoutine = null;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null) return;
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
    }
}