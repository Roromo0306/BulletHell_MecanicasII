using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public BulletHellBoss bulletBoss;
    public LaserBoss laserBoss;

    public float timeBetweenAttacks = 1.5f;

    private bool encounterActive = true;

    private void Start()
    {
        StartCoroutine(BossLoop());
    }

    public void StopEncounter()
    {
        encounterActive = false;
        StopAllCoroutines();
    }

    private IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(1f);

        while (encounterActive)
        {
            if (bulletBoss == null || laserBoss == null)
                yield break;

            int chosenBoss = Random.Range(0, 2);

            if (chosenBoss == 0)
                yield return StartCoroutine(bulletBoss.DoAttack());
            else
                yield return StartCoroutine(laserBoss.DoAttack());

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }
}