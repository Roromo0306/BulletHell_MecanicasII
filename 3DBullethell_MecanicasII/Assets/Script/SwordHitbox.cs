using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;

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

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            alreadyHit.Add(other.gameObject);
            enemy.TakeDamage(damage);
        }
    }
}