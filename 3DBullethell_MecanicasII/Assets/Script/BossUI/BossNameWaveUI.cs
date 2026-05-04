using System.Collections;
using TMPro;
using UnityEngine;

public class BossNameWaveUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI bossNameText;
    public CanvasGroup canvasGroup;

    [Header("Timing")]
    public float fadeInTime = 0.25f;
    public float waveDuration = 1.6f;
    public float fadeOutTime = 0.4f;

    [Header("Letter Wave")]
    public float waveHeight = 28f;
    public float waveSpeed = 8f;
    public float characterDelay = 0.12f;

    [Header("Intro Scale")]
    public float startScale = 0.85f;
    public float finalScale = 1f;
    public float scaleInTime = 0.35f;

    private TMP_MeshInfo[] cachedMeshInfo;
    private Color originalTextColor;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (bossNameText != null)
            originalTextColor = bossNameText.color;

        HideInstant();
    }

    public IEnumerator Play(string bossName)
    {
        gameObject.SetActive(true);

        if (bossNameText == null)
        {
            Debug.LogWarning("BossNameWaveUI: falta asignar Boss Name Text.");
            yield break;
        }

        SetupText(bossName);

        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeInTime));
        yield return StartCoroutine(LetterWaveRoutine());
        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeOutTime));

        HideInstant();
    }

    private void SetupText(string bossName)
    {
        bossNameText.gameObject.SetActive(true);
        bossNameText.enabled = true;

        bossNameText.text = bossName;

        Color c = originalTextColor;
        c.a = 1f;
        bossNameText.color = c;
        bossNameText.alpha = 1f;

        bossNameText.rectTransform.localScale = Vector3.one * startScale;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        bossNameText.ForceMeshUpdate();

        cachedMeshInfo = bossNameText.textInfo.CopyMeshInfoVertexData();
    }

    private IEnumerator LetterWaveRoutine()
    {
        float timer = 0f;

        while (timer < waveDuration)
        {
            timer += Time.unscaledDeltaTime;

            float scaleT = Mathf.Clamp01(timer / scaleInTime);
            scaleT = EaseOutBack(scaleT);

            bossNameText.rectTransform.localScale = Vector3.LerpUnclamped(
                Vector3.one * startScale,
                Vector3.one * finalScale,
                scaleT
            );

            TMP_TextInfo textInfo = bossNameText.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                float delayedTime = timer - i * characterDelay;

                float appearT = Mathf.Clamp01(delayedTime / 0.25f);
                appearT = EaseOutBack(appearT);

                float wave = Mathf.Sin(delayedTime * waveSpeed) * waveHeight * appearT;

                Vector3 offset = Vector3.up * wave;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] + offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] + offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] + offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] + offset;
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                bossNameText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }

        RestoreOriginalLetters();
    }

    private void RestoreOriginalLetters()
    {
        if (bossNameText == null || cachedMeshInfo == null)
            return;

        bossNameText.ForceMeshUpdate();

        TMP_TextInfo textInfo = bossNameText.textInfo;

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            Vector3[] sourceVertices = cachedMeshInfo[i].vertices;
            Vector3[] destinationVertices = textInfo.meshInfo[i].vertices;

            for (int v = 0; v < sourceVertices.Length; v++)
            {
                destinationVertices[v] = sourceVertices[v];
            }

            textInfo.meshInfo[i].mesh.vertices = destinationVertices;
            bossNameText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }

        bossNameText.rectTransform.localScale = Vector3.one * finalScale;
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void HideInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (bossNameText != null)
        {
            bossNameText.text = "";
            bossNameText.rectTransform.localScale = Vector3.one;
        }

        gameObject.SetActive(false);
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}