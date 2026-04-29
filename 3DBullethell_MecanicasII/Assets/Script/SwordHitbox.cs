using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;

    private Collider hitboxCollider;
    private readonly List<GameObject> alreadyHit = new List<GameObject>();

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.enabled = false;
        hitboxCollider.isTrigger = true;
    }

    public void EnableHitbox()
    {
        alreadyHit.Clear();
        hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyHit.Contains(other.gameObject)) return;

        BossDamageReceiver boss = other.GetComponent<BossDamageReceiver>();

        if (boss != null)
        {
            alreadyHit.Add(other.gameObject);
            boss.TakeDamage(damage);
            return;
        }

        Boss2DamageReceiver boss2 = other.GetComponent<Boss2DamageReceiver>();

        if (boss2 != null)
        {
            alreadyHit.Add(other.gameObject);
            boss2.TakeDamage(damage);
            return;
        }

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            alreadyHit.Add(other.gameObject);
            enemy.TakeDamage(damage);
        }
    }
}