using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Boss2Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public ParticleSystem deadParticles;
    public UnityEvent onHealthChanged;
    public ResultScreenUI resultScreenUI;

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
        deadParticles.Play();

        StartCoroutine(WinRoutine());
    }

    private IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (resultScreenUI != null)
            resultScreenUI.ShowWin();

        gameObject.SetActive(false);
    }
}