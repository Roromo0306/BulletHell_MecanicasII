using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    public BossController bossController;

    [Header("Boss Parts")]
    public GameObject bulletBoss;
    public GameObject laserBoss;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public float destroyExplosionAfter = 2f;

    [Header("UI")]
    public Slider healthSlider;
    public ResultScreenUI resultScreenUI;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log("Boss recibió daño. Vida: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (bossController != null)
            bossController.StopEncounter();

        ExplodeBossPart(bulletBoss);
        ExplodeBossPart(laserBoss);

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        if (bulletBoss != null)
            Destroy(bulletBoss);

        if (laserBoss != null)
            Destroy(laserBoss);

        if (resultScreenUI != null)
            resultScreenUI.ShowWin();

        Debug.Log("Boss derrotado");
    }

    private void ExplodeBossPart(GameObject bossPart)
    {
        if (bossPart == null) return;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                bossPart.transform.position,
                Quaternion.identity
            );

            Destroy(explosion, destroyExplosionAfter);
        }
    }
}