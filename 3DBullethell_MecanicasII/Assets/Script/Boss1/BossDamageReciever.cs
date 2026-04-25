using UnityEngine;

public class BossDamageReceiver : MonoBehaviour
{
    public BossHealth bossHealth;

    public void TakeDamage(int amount)
    {
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(amount);
        }
    }
}
