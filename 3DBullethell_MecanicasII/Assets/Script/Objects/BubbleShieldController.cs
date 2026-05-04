using System.Collections;
using UnityEngine;

public class BubbleShieldController : MonoBehaviour
{
    [Header("Shield")]
    public Transform visualRoot;
    public Collider shieldCollider;
    public float duration = 5f;

    [Header("Protection")]
    public float damageGraceAfterPop = 0.12f;

    [Header("Blink")]
    public float warningBlinkTime = 1f;
    public float hitBlinkTime = 0.45f;
    public float blinkInterval = 0.08f;

    [Header("Optional UI")]
    public ChargeFillUI timerFillUI;

    [Header("Optional FX")]
    public GameObject popParticles;

    private Renderer[] renderers;
    private ParticleSystem[] particleSystems;
    private Coroutine routine;

    private bool isActive;
    private float blockDamageUntilTime;

    public bool IsActive => isActive;

    public bool IsBlockingDamage
    {
        get
        {
            return isActive || Time.unscaledTime < blockDamageUntilTime;
        }
    }

    private void Awake()
    {
        if (shieldCollider == null)
            shieldCollider = GetComponent<Collider>();

        if (visualRoot == null)
            visualRoot = transform;

        renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        particleSystems = visualRoot.GetComponentsInChildren<ParticleSystem>(true);

        DisableShieldImmediate();
    }

    private void OnDisable()
    {
        ForceStopShield();
    }

    public void Activate()
    {
        Debug.Log("BubbleShieldController Activate()");

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        isActive = true;
        blockDamageUntilTime = 0f;

        if (visualRoot != null)
            visualRoot.gameObject.SetActive(true);

        SetRenderersVisible(true);
        PlayParticles();

        if (shieldCollider != null)
            shieldCollider.enabled = true;
        else
            Debug.LogWarning("BubbleShieldController: falta Shield Collider");

        if (timerFillUI != null)
        {
            timerFillUI.Show();
            timerFillUI.SetFill(1f);
        }

        float elapsed = 0f;
        float nextBlinkTime = 0f;
        bool visible = true;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float remaining = duration - elapsed;
            float fill = Mathf.Clamp01(remaining / duration);

            if (timerFillUI != null)
                timerFillUI.SetFill(fill);

            if (remaining <= warningBlinkTime)
            {
                if (Time.unscaledTime >= nextBlinkTime)
                {
                    visible = !visible;
                    SetRenderersVisible(visible);
                    nextBlinkTime = Time.unscaledTime + blinkInterval;
                }
            }
            else
            {
                if (!visible)
                {
                    visible = true;
                    SetRenderersVisible(true);
                }
            }

            yield return null;
        }

        DisableShieldImmediate();
        routine = null;
    }

    public void PopFromHit()
    {
        if (!isActive) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        isActive = false;

        // Evita que la misma bala haga daño justo al romper la pompa.
        blockDamageUntilTime = Time.unscaledTime + damageGraceAfterPop;

        if (shieldCollider != null)
            shieldCollider.enabled = false;

        if (popParticles != null)
            Instantiate(popParticles, transform.position, Quaternion.identity);

        if (visualRoot != null)
            visualRoot.gameObject.SetActive(true);

        float elapsed = 0f;
        float nextBlinkTime = 0f;
        bool visible = true;

        while (elapsed < hitBlinkTime)
        {
            elapsed += Time.unscaledDeltaTime;

            if (Time.unscaledTime >= nextBlinkTime)
            {
                visible = !visible;
                SetRenderersVisible(visible);
                nextBlinkTime = Time.unscaledTime + blinkInterval;
            }

            yield return null;
        }

        DisableShieldImmediate();
        routine = null;
    }

    private void DisableShieldImmediate()
    {
        isActive = false;

        if (shieldCollider != null)
            shieldCollider.enabled = false;

        SetRenderersVisible(false);
        StopParticles();

        if (visualRoot != null && visualRoot != transform)
            visualRoot.gameObject.SetActive(false);

        if (timerFillUI != null)
            timerFillUI.Hide();
    }

    private void ForceStopShield()
    {
        isActive = false;

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (shieldCollider != null)
            shieldCollider.enabled = false;

        SetRenderersVisible(false);
        StopParticles();

        if (timerFillUI != null)
            timerFillUI.Hide();
    }

    private void SetRenderersVisible(bool visible)
    {
        if (renderers == null) return;

        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    private void PlayParticles()
    {
        if (particleSystems == null) return;

        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
                ps.Play(true);
        }
    }

    private void StopParticles()
    {
        if (particleSystems == null) return;

        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (TryDestroyBullet(other))
        {
            Debug.Log("Bubble Shield bloqueó una bala");
            PopFromHit();
        }
    }

    private bool TryDestroyBullet(Collider other)
    {
        EnemyBullet enemyBullet = other.GetComponentInParent<EnemyBullet>();

        if (enemyBullet != null)
        {
            Destroy(enemyBullet.gameObject);
            return true;
        }

        Boss2ZigZagBullet zigZagBullet = other.GetComponentInParent<Boss2ZigZagBullet>();

        if (zigZagBullet != null)
        {
            Destroy(zigZagBullet.gameObject);
            return true;
        }

        Boss3PulsingBullet pulsingBullet = other.GetComponentInParent<Boss3PulsingBullet>();

        if (pulsingBullet != null)
        {
            Destroy(pulsingBullet.gameObject);
            return true;
        }

        if (other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject);
            return true;
        }

        return false;
    }
}