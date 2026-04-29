using UnityEngine;

public class Boss2DamageReceiver : MonoBehaviour
{
    public Boss2Health bossHealth;

    public void TakeDamage(int amount)
    {
        if (bossHealth != null)
            bossHealth.TakeDamage(amount);
    }
}