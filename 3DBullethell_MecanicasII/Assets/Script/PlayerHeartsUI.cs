using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeartsUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Image[] hearts;

    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Heart Shake")]
    public float shakeDuration = 0.25f;
    public float shakeStrength = 8f;

    private int previousHearts;
    private Coroutine[] shakeRoutines;

    private void Start()
    {
        shakeRoutines = new Coroutine[hearts.Length];

        if (playerHealth != null)
        {
            previousHearts = playerHealth.currentHearts;

            UpdateHearts();

            playerHealth.onHealthChanged.AddListener(UpdateHearts);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(UpdateHearts);
    }

    public void UpdateHearts()
    {
        if (playerHealth == null) return;

        int current = playerHealth.currentHearts;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            hearts[i].sprite = i < current ? fullHeart : emptyHeart;
            hearts[i].enabled = i < playerHealth.maxHearts;
        }

        if (current < previousHearts)
        {
            for (int i = current; i < previousHearts; i++)
            {
                if (i >= 0 && i < hearts.Length)
                    ShakeHeart(i);
            }
        }

        previousHearts = current;
    }

    private void ShakeHeart(int index)
    {
        if (hearts[index] == null) return;

        if (shakeRoutines[index] != null)
            StopCoroutine(shakeRoutines[index]);

        shakeRoutines[index] = StartCoroutine(ShakeHeartRoutine(index));
    }

    private IEnumerator ShakeHeartRoutine(int index)
    {
        RectTransform rect = hearts[index].GetComponent<RectTransform>();

        // Importante: usa la posición actual, no una guardada al inicio.
        Vector2 startPosition = rect.anchoredPosition;

        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.unscaledDeltaTime;

            Vector2 randomOffset = Random.insideUnitCircle * shakeStrength;
            rect.anchoredPosition = startPosition + randomOffset;

            yield return null;
        }

        rect.anchoredPosition = startPosition;
        shakeRoutines[index] = null;
    }
}