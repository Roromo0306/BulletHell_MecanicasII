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
        GameObject hitObject = other.gameObject;

        if (alreadyHit.Contains(hitObject)) return;

        BossDamageReceiver boss1 = other.GetComponentInParent<BossDamageReceiver>();

        if (boss1 != null)
        {
            alreadyHit.Add(hitObject);
            boss1.TakeDamage(damage);
            return;
        }

        Boss2DamageReceiver boss2 = other.GetComponentInParent<Boss2DamageReceiver>();

        if (boss2 != null)
        {
            alreadyHit.Add(hitObject);
            boss2.TakeDamage(damage);
            return;
        }

        Boss3DamageReceiver boss3 = other.GetComponentInParent<Boss3DamageReceiver>();

        if (boss3 != null)
        {
            alreadyHit.Add(hitObject);
            boss3.TakeDamage(damage);
            return;
        }

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            alreadyHit.Add(hitObject);
            enemy.TakeDamage(damage);
        }
    }
}