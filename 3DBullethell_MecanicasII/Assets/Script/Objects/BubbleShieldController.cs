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

        SetVisualVisible(true);

        if (shieldCollider != null)
            shieldCollider.enabled = true;
        else
            Debug.LogWarning("BubbleShieldController: falta Shield Collider");

        if (timerFillUI != null)
        {
            timerFillUI.Show();
            timerFillUI.SetFill(1f);
        }

        float timer = 0f;
        bool visible = true;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float remaining = duration - timer;
            float fill = Mathf.Clamp01(remaining / duration);

            if (timerFillUI != null)
                timerFillUI.SetFill(fill);

            if (remaining <= warningBlinkTime)
            {
                visible = !visible;
                SetVisualVisible(visible);

                yield return new WaitForSecondsRealtime(blinkInterval);
            }
            else
            {
                yield return null;
            }
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

        // Esto evita que una bala haga daño en el mismo frame en el que rompe la pompa.
        blockDamageUntilTime = Time.unscaledTime + damageGraceAfterPop;

        if (shieldCollider != null)
            shieldCollider.enabled = false;

        if (popParticles != null)
            Instantiate(popParticles, transform.position, Quaternion.identity);

        float timer = 0f;
        bool visible = true;

        while (timer < hitBlinkTime)
        {
            timer += Time.unscaledDeltaTime;

            visible = !visible;
            SetVisualVisible(visible);

            yield return new WaitForSecondsRealtime(blinkInterval);
        }

        DisableShieldImmediate();
        routine = null;
    }

    private void DisableShieldImmediate()
    {
        isActive = false;

        if (shieldCollider != null)
            shieldCollider.enabled = false;

        SetVisualVisible(false);

        if (timerFillUI != null)
            timerFillUI.Hide();
    }

    private void SetVisualVisible(bool visible)
    {
        if (visualRoot != null && visualRoot != transform)
            visualRoot.gameObject.SetActive(visible);

        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = visible;
            }
        }

        if (particleSystems != null)
        {
            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps == null) continue;

                if (visible)
                    ps.Play(true);
                else
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
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