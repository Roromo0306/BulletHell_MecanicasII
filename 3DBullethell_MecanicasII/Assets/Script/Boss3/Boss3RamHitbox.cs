using System.Collections.Generic;
using UnityEngine;

public class Boss3RamHitbox : MonoBehaviour
{
    public int damage = 1;

    private bool active;
    private readonly List<PlayerHealth> alreadyHit = new List<PlayerHealth>();

    private void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    public void SetActiveHitbox(bool value)
    {
        active = value;

        if (value)
            alreadyHit.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (!active) return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null) return;
        if (alreadyHit.Contains(player)) return;

        alreadyHit.Add(player);
        player.TakeDamage(damage);
    }
}