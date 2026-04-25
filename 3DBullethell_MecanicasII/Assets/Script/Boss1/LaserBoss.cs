using System.Collections;
using UnityEngine;

public class LaserBoss : MonoBehaviour
{
    [Header("References")]
    public Transform laserOrigin;
    public BossShake bossShake;
    public LineRenderer lineRenderer;

    [Header("Telegraph")]
    public float telegraphDuration = 1.2f;
    public float blinkSpeed = 8f;

    [Header("Laser Settings")]
    public float warningShakeTime = 1f;
    public float laserDuration = 1.2f;
    public float laserRange = 20f;
    public float laserWidth = 0.35f;
    public int damage = 1;
    public float damageInterval = 0.25f;
    public LayerMask playerLayer;

    private Transform player;
    private float nextDamageTime;

    private void Awake()
    {
        if (bossShake == null)
            bossShake = GetComponent<BossShake>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth;
        }
    }



    public IEnumerator DoAttack()
    {
        yield return StartCoroutine(bossShake.Shake(warningShakeTime));

        if (player == null)
            yield break;

        Vector3 direction = player.position - laserOrigin.position;
        direction.y = 0f;
        direction.Normalize();

        yield return StartCoroutine(ShowTelegraph(direction));

        yield return StartCoroutine(FireLaser(direction));
    }

    private IEnumerator ShowTelegraph(Vector3 direction)
    {
        float timer = 0f;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;

            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;

            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
        }

        while (timer < telegraphDuration)
        {
            Vector3 start = laserOrigin.position;
            Vector3 end = start + direction * laserRange;

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, start + Vector3.up * 0.3f);
                lineRenderer.SetPosition(1, end + Vector3.up * 0.3f);

                float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));

                Color blinkColor = new Color(1f, 1f, 0f, alpha);

                lineRenderer.startColor = blinkColor;
                lineRenderer.endColor = blinkColor;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator FireLaser(Vector3 direction)
    {
        float timer = 0f;
        nextDamageTime = 0f;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;

            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth;

            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }

        while (timer < laserDuration)
        {
            Vector3 start = laserOrigin.position;
            Vector3 end = start + direction * laserRange;

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, start + Vector3.up * 0.3f);
                lineRenderer.SetPosition(1, end + Vector3.up * 0.3f);
            }

            CheckLaserHit(start, direction);

            timer += Time.deltaTime;
            yield return null;
        }

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void CheckLaserHit(Vector3 start, Vector3 direction)
    {
        if (Time.time < nextDamageTime) return;

        Ray ray = new Ray(start, direction);

        if (Physics.SphereCast(ray, laserWidth, out RaycastHit hit, laserRange, playerLayer))
        {
            PlayerHealth playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }


}