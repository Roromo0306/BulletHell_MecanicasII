using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DamageVignettePostProcess : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;

    private Vignette vignette;

    [Header("Damage Vignette")]
    public float maxIntensity = 0.45f;
    public float fadeInTime = 0.05f;
    public float fadeOutTime = 0.25f;

    private Coroutine routine;

    private void Awake()
    {
        if (postProcessVolume == null)
            postProcessVolume = GetComponent<PostProcessVolume>();

        if (postProcessVolume == null)
        {
            Debug.LogWarning("No hay PostProcessVolume asignado.");
            return;
        }

        if (postProcessVolume.profile == null)
        {
            Debug.LogWarning("El PostProcessVolume no tiene Profile.");
            return;
        }

        if (!postProcessVolume.profile.TryGetSettings(out vignette))
        {
            Debug.LogWarning("El Profile no tiene Vignette añadido.");
            return;
        }

        vignette.enabled.value = true;
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0f;
    }

    public void Play()
    {
        Debug.Log("Play vignette");

        if (vignette == null)
        {
            Debug.LogWarning("Vignette es null. Revisa el PostProcessVolume/Profile.");
            return;
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(VignetteRoutine());
    }

    private IEnumerator VignetteRoutine()
    {
        float timer = 0f;

        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(0f, maxIntensity, timer / fadeInTime);
            yield return null;
        }

        timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(maxIntensity, 0f, timer / fadeOutTime);
            yield return null;
        }

        vignette.intensity.value = 0f;
        routine = null;
    }
}