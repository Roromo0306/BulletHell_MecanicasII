using System.Collections;
using UnityEngine;

public class CarryObjectSpawner : MonoBehaviour
{
    public CarryThrowableObject objectPrefab;
    public float respawnTime = 5f;

    private CarryThrowableObject currentObject;
    private bool respawning;

    private void Start()
    {
        SpawnObject();
    }

    private void SpawnObject()
    {
        currentObject = Instantiate(objectPrefab, transform.position, transform.rotation);
        currentObject.SetSpawner(this);
    }

    public void StartRespawnTimer()
    {
        if (respawning) return;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        respawning = true;

        yield return new WaitForSeconds(respawnTime);

        SpawnObject();

        respawning = false;
    }
}