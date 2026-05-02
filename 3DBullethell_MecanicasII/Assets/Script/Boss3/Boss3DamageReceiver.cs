using UnityEngine;

public class Boss3DamageReceiver : MonoBehaviour
{
    public Boss3Health bossHealth;

    public void TakeDamage(int amount)
    {
        if (bossHealth != null)
            bossHealth.TakeDamage(amount);
    }
}