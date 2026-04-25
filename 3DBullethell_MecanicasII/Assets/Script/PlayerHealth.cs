using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 5;
    public int currentHearts;

    public UnityEvent onHealthChanged;
    public ResultScreenUI resultScreenUI;
    private bool isDead;

    private void Awake()
    {
        currentHearts = maxHearts;
    }

    private void Start()
    {
        onHealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHearts -= amount;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        Debug.Log("Player vida: " + currentHearts);

        onHealthChanged?.Invoke();

        if (currentHearts <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHearts += amount;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        onHealthChanged?.Invoke();
    }

    private void Die()
    {
        isDead = true;

        if (resultScreenUI != null)
            resultScreenUI.ShowLose();

        Debug.Log("Player muerto");
    }
}