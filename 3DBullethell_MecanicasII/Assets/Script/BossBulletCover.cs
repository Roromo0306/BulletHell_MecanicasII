using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossBulletCover : MonoBehaviour
{
    [Header("Cover Settings")]
    public bool destroyBossBullets = true;

    [Tooltip("Si está activado, este collider se pone como Trigger automáticamente para no bloquear al player.")]
    public bool forceTriggerCollider = true;

    [Tooltip("Añade un Rigidbody kinematic automáticamente para que los triggers funcionen bien.")]
    public bool addKinematicRigidbodyIfMissing = true;

    [Header("Optional Feedback")]
    public GameObject blockVFXPrefab;
    public AudioClip blockSFX;
    [Range(0f, 1f)] public float blockSFXVolume = 0.8f;

    private AudioSource audioSource;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null && forceTriggerCollider)
            col.isTrigger = true;

        if (addKinematicRigidbodyIfMissing)
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (blockSFX != null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!destroyBossBullets)
            return;

        TryBlockBossBullet(other);
    }

    private void TryBlockBossBullet(Collider other)
    {
        if (other == null)
            return;

        EnemyBullet enemyBullet = other.GetComponentInParent<EnemyBullet>();

        if (enemyBullet != null)
        {
            BlockBullet(enemyBullet.gameObject);
            return;
        }

        Boss2ZigZagBullet boss2ZigZagBullet = other.GetComponentInParent<Boss2ZigZagBullet>();

        if (boss2ZigZagBullet != null)
        {
            BlockBullet(boss2ZigZagBullet.gameObject);
            return;
        }

        Boss3PulsingBullet boss3PulsingBullet = other.GetComponentInParent<Boss3PulsingBullet>();

        if (boss3PulsingBullet != null)
        {
            BlockBullet(boss3PulsingBullet.gameObject);
            return;
        }
    }

    private void BlockBullet(GameObject bulletObject)
    {
        if (bulletObject == null)
            return;

        if (blockVFXPrefab != null)
            Instantiate(blockVFXPrefab, bulletObject.transform.position, Quaternion.identity);

        if (blockSFX != null && audioSource != null)
            audioSource.PlayOneShot(blockSFX, blockSFXVolume);

        Destroy(bulletObject);
    }
}