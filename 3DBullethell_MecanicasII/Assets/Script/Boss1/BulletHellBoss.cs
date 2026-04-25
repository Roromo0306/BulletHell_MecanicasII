using System.Collections;
using UnityEngine;

public class BulletHellBoss : MonoBehaviour
{
    [Header("References")]
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public BossShake bossShake;

    [Header("General")]
    public float warningShakeTime = 0.8f;

    [Header("Pattern 1 - Circle")]
    public int circleBulletCount = 24;
    public float circleBulletSpeed = 6f;

    [Header("Pattern 2 - Spiral")]
    public int spiralBulletCount = 48;
    public float spiralDelay = 0.04f;
    public float spiralBulletSpeed = 5.5f;

    [Header("Pattern 3 - Aimed Burst")]
    public int burstWaves = 5;
    public int bulletsPerBurst = 5;
    public float burstDelay = 0.18f;
    public float burstSpreadAngle = 45f;
    public float burstBulletSpeed = 7f;

    private Transform player;

    private void Awake()
    {
        if (bossShake == null)
            bossShake = GetComponent<BossShake>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    public IEnumerator DoAttack()
    {
        yield return StartCoroutine(bossShake.Shake(warningShakeTime));

        int pattern = Random.Range(0, 3);

        if (pattern == 0)
            yield return StartCoroutine(CirclePattern());
        else if (pattern == 1)
            yield return StartCoroutine(SpiralPattern());
        else
            yield return StartCoroutine(AimedBurstPattern());
    }

    private IEnumerator CirclePattern()
    {
        float angleStep = 360f / circleBulletCount;

        for (int i = 0; i < circleBulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = DirectionFromAngle(angle);
            SpawnBullet(direction, circleBulletSpeed);
        }

        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator SpiralPattern()
    {
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < spiralBulletCount; i++)
        {
            Vector3 direction = DirectionFromAngle(angle);
            SpawnBullet(direction, spiralBulletSpeed);

            angle += 18f;

            yield return new WaitForSeconds(spiralDelay);
        }

        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator AimedBurstPattern()
    {
        if (player == null)
            yield break;

        for (int wave = 0; wave < burstWaves; wave++)
        {
            Vector3 directionToPlayer = player.position - bulletSpawnPoint.position;
            directionToPlayer.y = 0f;
            directionToPlayer.Normalize();

            float baseAngle = Mathf.Atan2(directionToPlayer.z, directionToPlayer.x) * Mathf.Rad2Deg;

            float startAngle = baseAngle - burstSpreadAngle / 2f;
            float angleStep = burstSpreadAngle / (bulletsPerBurst - 1);

            for (int i = 0; i < bulletsPerBurst; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 direction = DirectionFromAngle(angle);
                SpawnBullet(direction, burstBulletSpeed);
            }

            yield return new WaitForSeconds(burstDelay);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void SpawnBullet(Vector3 direction, float speed)
    {
        GameObject bulletObj = Instantiate(
            bulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.identity
        );

        EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();
        bullet.SetDirection(direction, speed);
    }

    private Vector3 DirectionFromAngle(float angleDegrees)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)).normalized;
    }
}