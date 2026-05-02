using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Boss2Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public UnityEvent onHealthChanged;
    public ResultScreenUI resultScreenUI;

    [Header("Death FX")]
    public GameObject deathExplosionPrefab;
    public Transform deathExplosionPoint;

    private bool isDead;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        onHealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Boss 2 vida: " + currentHealth);

        onHealthChanged?.Invoke();

        if (currentHealth <= 0)
            Die();
    }

    public float CurrentHealthNormalized()
    {
        return (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Boss 2 muerto");

        SpawnDeathExplosion();
        DisableBossColliders();

        StartCoroutine(WinRoutine());
    }

    private void SpawnDeathExplosion()
    {
        if (deathExplosionPrefab == null) return;

        Vector3 spawnPos = deathExplosionPoint != null
            ? deathExplosionPoint.position
            : transform.position;

        Instantiate(deathExplosionPrefab, spawnPos, Quaternion.identity);
    }

    private void DisableBossColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
            col.enabled = false;
    }

    private IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (resultScreenUI != null)
            resultScreenUI.ShowWin();

        gameObject.SetActive(false);
    }
}