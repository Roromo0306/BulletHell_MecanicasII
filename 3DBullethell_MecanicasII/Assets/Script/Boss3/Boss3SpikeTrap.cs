using System.Collections;
using UnityEngine;

public class Boss3SpikeTrap : MonoBehaviour
{
    [Header("References")]
    public Transform spikeVisual;
    public GameObject darkTelegraphVisual;
    public Collider damageCollider;

    [Header("Damage")]
    public int damage = 1;

    [Header("Timing")]
    public float telegraphTime = 0.75f;
    public float riseTime = 0.12f;
    public float stayTime = 0.25f;
    public float sinkTime = 0.18f;

    [Header("Movement")]
    public float hiddenDepth = 2f;

    private bool canDamage;
    private bool alreadyHitPlayer;

    private void Awake()
    {
        if (damageCollider == null)
            damageCollider = GetComponent<Collider>();

        if (damageCollider != null)
        {
            damageCollider.isTrigger = true;
            damageCollider.enabled = false;
        }

        if (spikeVisual != null)
            spikeVisual.localPosition = Vector3.down * hiddenDepth;

        if (darkTelegraphVisual != null)
            darkTelegraphVisual.SetActive(false);
    }

    public void Begin()
    {
        StartCoroutine(SpikeRoutine());
    }

    private IEnumerator SpikeRoutine()
    {
        alreadyHitPlayer = false;
        canDamage = false;

        if (spikeVisual != null)
            spikeVisual.localPosition = Vector3.down * hiddenDepth;

        if (darkTelegraphVisual != null)
            darkTelegraphVisual.SetActive(true);

        yield return new WaitForSeconds(telegraphTime);

        if (darkTelegraphVisual != null)
            darkTelegraphVisual.SetActive(false);

        if (damageCollider != null)
            damageCollider.enabled = true;

        canDamage = true;

        yield return StartCoroutine(MoveSpike(Vector3.down * hiddenDepth, Vector3.zero, riseTime));

        yield return new WaitForSeconds(stayTime);

        canDamage = false;

        yield return StartCoroutine(MoveSpike(Vector3.zero, Vector3.down * hiddenDepth, sinkTime));

        if (damageCollider != null)
            damageCollider.enabled = false;

        Destroy(gameObject);
    }

    private IEnumerator MoveSpike(Vector3 from, Vector3 to, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            if (spikeVisual != null)
                spikeVisual.localPosition = Vector3.Lerp(from, to, t);

            yield return null;
        }

        if (spikeVisual != null)
            spikeVisual.localPosition = to;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider other)
    {
        if (!canDamage) return;
        if (alreadyHitPlayer) return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null) return;

        alreadyHitPlayer = true;
        playerHealth.TakeDamage(damage);
    }
}