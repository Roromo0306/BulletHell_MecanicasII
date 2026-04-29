using System.Collections;
using UnityEngine;

public class ConsumableSpawner : MonoBehaviour
{
    public ConsumableObject consumablePrefab;
    public float respawnTime = 8f;

    private ConsumableObject currentConsumable;
    private bool respawning;

    private void Start()
    {
        SpawnConsumable();
    }

    private void SpawnConsumable()
    {
        if (consumablePrefab == null)
        {
            Debug.LogWarning("No hay consumablePrefab asignado en " + name);
            return;
        }

        if (currentConsumable != null)
            return;

        currentConsumable = Instantiate(consumablePrefab, transform.position, transform.rotation);
        currentConsumable.SetSpawner(this);
    }

    public void StartRespawnTimer()
    {
        if (respawning) return;

        currentConsumable = null;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        respawning = true;

        yield return new WaitForSeconds(respawnTime);

        SpawnConsumable();

        respawning = false;
    }
}