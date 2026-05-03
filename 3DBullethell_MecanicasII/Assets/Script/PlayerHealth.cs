using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Damage Feedback")]
    public DamageVignettePostProcess damageVignette;
    public CameraShake cameraShake;

    [Header("Bubble Shield")]
    public BubbleShieldController bubbleShieldController;

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 0.15f;
    private float invulnerableUntilTime = 0f;
    private Coroutine invulnerabilityRoutine;

    [Header("Visual Feedback")]
    public Transform visualRoot;
    private SpriteRenderer[] spriteRenderers;

    public int maxHearts = 5;
    public int currentHearts;

    public UnityEvent onHealthChanged;
    public ResultScreenUI resultScreenUI;

    private bool isDead;

    private void Awake()
    {
        currentHearts = maxHearts;

        if (visualRoot != null)
            spriteRenderers = visualRoot.GetComponentsInChildren<SpriteRenderer>();

        if (bubbleShieldController == null)
            bubbleShieldController = GetComponentInChildren<BubbleShieldController>(true);
    }

    private void Start()
    {
        onHealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        if (bubbleShieldController != null && bubbleShieldController.IsBlockingDamage)
        {
            Debug.Log("Daño bloqueado por Bubble Shield");

            if (bubbleShieldController.IsActive)
                bubbleShieldController.PopFromHit();

            return;
        }

        if (IsInvulnerable())
        {
            Debug.Log("Daño ignorado por invulnerabilidad");
            return;
        }

        currentHearts -= amount;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        Debug.Log("Player vida: " + currentHearts);

        onHealthChanged?.Invoke();

        if (damageVignette != null)
            damageVignette.Play();

        if (cameraShake != null)
            cameraShake.Shake();

        if (currentHearts <= 0)
        {
            Die();
            return;
        }

        StartInvulnerability(invulnerabilityDuration);
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHearts += amount;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        onHealthChanged?.Invoke();
    }

    private bool IsInvulnerable()
    {
        return Time.time < invulnerableUntilTime;
    }

    public void StartInvulnerability(float duration)
    {
        if (!gameObject.activeInHierarchy) return;

        invulnerableUntilTime = Mathf.Max(invulnerableUntilTime, Time.time + duration);

        if (invulnerabilityRoutine != null)
            StopCoroutine(invulnerabilityRoutine);

        invulnerabilityRoutine = StartCoroutine(InvulnerabilityVisualRoutine());
    }

    private IEnumerator InvulnerabilityVisualRoutine()
    {
        bool toggle = false;

        while (IsInvulnerable())
        {
            toggle = !toggle;
            SetColor(toggle ? Color.red : Color.white);
            yield return new WaitForSeconds(0.08f);
        }

        SetColor(Color.white);
        invulnerabilityRoutine = null;
    }

    private void SetColor(Color color)
    {
        if (spriteRenderers == null) return;

        foreach (var sr in spriteRenderers)
            sr.color = color;
    }

    private void Die()
    {
        isDead = true;
        SetColor(Color.white);

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (resultScreenUI != null)
            resultScreenUI.ShowLose();

        Debug.Log("Player muerto");
    }
}