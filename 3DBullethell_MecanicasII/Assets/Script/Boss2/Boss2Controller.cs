using System.Collections;
using UnityEngine;

public class Boss2Controller : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Boss2Health bossHealth;
    public LaserBoss laserBoss;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public GameObject zigZagBulletPrefab;

    [Header("Movement - Slower")]
    public float orbitRadius = 5.5f;
    public float orbitSpeed = 0.45f;
    public float moveSpeed = 2.2f;
    public float randomOffsetStrength = 0.8f;
    public float randomOffsetChangeTime = 2f;

    [Header("Attacks")]
    public float bulletSpeedPhase1 = 4.2f;
    public float bulletSpeedPhase2 = 5.4f;
    public float timeBetweenAttacks = 1.6f;

    [Header("Pre Attack Shake")]
    public float preAttackShakeDuration = 0.28f;
    public float preAttackShakeStrength = 0.08f;

    [Header("Rage Phase")]
    public float phase2HealthPercent = 0.5f;
    public float rageDuration = 1.4f;
    public float rageShakeStrength = 0.18f;
    public float rageLiftHeight = 1.5f;

    [Header("Rage Camera Shake")]
    public CameraFollow cameraFollow;
    public float rageCameraShakeStrength = 0.18f;

    private bool phase2;
    private bool isAttacking;
    private bool isRaging;

    private float orbitAngle;
    private Vector3 randomOffset;
    private float nextRandomOffsetTime;

    private void Start()
    {
        StartCoroutine(AttackLoop());

        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();
    }

    private void Update()
    {
        if (bossHealth != null && bossHealth.IsDead) return;

        CheckPhase2();

        if (!isAttacking && !isRaging)
            Move();

        transform.rotation = Quaternion.identity;
    }

    private void Move()
    {
        if (player == null) return;

        orbitAngle += orbitSpeed * Time.deltaTime;

        if (Time.time > nextRandomOffsetTime)
        {
            randomOffset = new Vector3(
                Random.Range(-randomOffsetStrength, randomOffsetStrength),
                0f,
                Random.Range(-randomOffsetStrength, randomOffsetStrength)
            );

            nextRandomOffsetTime = Time.time + randomOffsetChangeTime;
        }

        float x = Mathf.Cos(orbitAngle) * orbitRadius;
        float z = Mathf.Sin(orbitAngle) * orbitRadius * 0.55f;

        Vector3 target = player.position + new Vector3(x, 0f, z) + randomOffset;
        target.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
    }

    private void CheckPhase2()
    {
        if (phase2) return;
        if (bossHealth == null) return;

        if (bossHealth.CurrentHealthNormalized() <= phase2HealthPercent)
            StartCoroutine(RageRoutine());
    }

    private IEnumerator RageRoutine()
    {
        phase2 = true;
        isRaging = true;
        isAttacking = true;

        if (cameraFollow != null)
            cameraFollow.StartCameraShake(rageDuration, rageCameraShakeStrength);

        if (GameSFXManager.Instance != null)
            GameSFXManager.Instance.PlayBoss2Rage();

        Vector3 startPos = transform.position;
        Vector3 liftedPos = startPos + Vector3.up * rageLiftHeight;

        float t = 0f;

        while (t < 0.35f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, liftedPos, t / 0.35f);
            yield return null;
        }

        t = 0f;

        while (t < rageDuration)
        {
            t += Time.deltaTime;

            Vector3 shake = Random.insideUnitSphere * rageShakeStrength;
            transform.position = liftedPos + shake;

            yield return null;
        }

        t = 0f;

        while (t < 0.35f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(liftedPos, startPos, t / 0.35f);
            yield return null;
        }

        transform.position = startPos;

        isRaging = false;
        isAttacking = false;
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (bossHealth != null && bossHealth.IsDead)
                yield break;

            if (!isRaging)
            {
                isAttacking = true;

                yield return StartCoroutine(PreAttackShake());

                if (!phase2)
                    yield return StartCoroutine(Phase1Attack());
                else
                    yield return StartCoroutine(Phase2Attack());

                isAttacking = false;
            }

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    private IEnumerator PreAttackShake()
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < preAttackShakeDuration)
        {
            t += Time.deltaTime;

            Vector3 shake = new Vector3(
                Random.Range(-preAttackShakeStrength, preAttackShakeStrength),
                0f,
                Random.Range(-preAttackShakeStrength, preAttackShakeStrength)
            );

            transform.position = start + shake;
            yield return null;
        }

        transform.position = start;
    }

    private IEnumerator Phase1Attack()
    {
        int r = Random.Range(0, 4);

        if (GameSFXManager.Instance != null)
            GameSFXManager.Instance.PlayBossBulletAttack();

        if (r == 0) yield return FlowerPattern();
        if (r == 1) yield return CloverPattern();
        if (r == 2) yield return ZigZagBurst();
        if (r == 3) yield return PetalWaves();
    }

    private IEnumerator Phase2Attack()
    {
        int r = Random.Range(0, 5);

        if (r != 4)
        {
            if (GameSFXManager.Instance != null)
                GameSFXManager.Instance.PlayBossBulletAttack();
        }

        if (r == 0) yield return BigFlowerPattern();
        if (r == 1) yield return CloverPatternHard();
        if (r == 2) yield return ZigZagStorm();
        if (r == 3) yield return RotatingPetalWall();
        if (r == 4 && laserBoss != null)
            yield return laserBoss.DoAttack();
    }

    // -------------------------
    // FASE 1
    // -------------------------

    private IEnumerator FlowerPattern()
    {
        int petals = 6;
        int bulletsPerPetal = 7;

        for (int p = 0; p < petals; p++)
        {
            float petalAngle = 360f / petals * p;

            for (int i = 0; i < bulletsPerPetal; i++)
            {
                float spread = Mathf.Lerp(-18f, 18f, i / (float)(bulletsPerPetal - 1));
                Spawn(petalAngle + spread, bulletSpeedPhase1);
            }

            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator CloverPattern()
    {
        int bullets = 54;

        for (int i = 0; i < bullets; i++)
        {
            float t = i / (float)bullets * Mathf.PI * 2f;

            float radius = Mathf.Sin(3f * t);
            float x = Mathf.Cos(t) * radius;
            float z = Mathf.Sin(t) * radius;

            Vector3 dir = new Vector3(x, 0f, z).normalized;

            SpawnDir(dir, bulletSpeedPhase1);

            if (i % 3 == 0)
                yield return new WaitForSeconds(0.025f);
        }

        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator ZigZagBurst()
    {
        int bullets = 8;
        float baseAngle = DirectionToAngle(DirectionToPlayer());
        float spread = 80f;

        for (int i = 0; i < bullets; i++)
        {
            float angle = baseAngle - spread / 2f + spread * i / (bullets - 1);
            SpawnZigZag(angle, bulletSpeedPhase1 + 0.8f);
            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator PetalWaves()
    {
        int waves = 4;

        for (int w = 0; w < waves; w++)
        {
            float offset = w * 18f;

            for (int i = 0; i < 12; i++)
            {
                float angle = offset + i * 30f;
                Spawn(angle, bulletSpeedPhase1);
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    // -------------------------
    // FASE 2
    // -------------------------

    private IEnumerator BigFlowerPattern()
    {
        int petals = 8;
        int bulletsPerPetal = 9;

        for (int wave = 0; wave < 3; wave++)
        {
            float waveOffset = wave * 12f;

            for (int p = 0; p < petals; p++)
            {
                float petalAngle = 360f / petals * p + waveOffset;

                for (int i = 0; i < bulletsPerPetal; i++)
                {
                    float spread = Mathf.Lerp(-20f, 20f, i / (float)(bulletsPerPetal - 1));
                    Spawn(petalAngle + spread, bulletSpeedPhase2);
                }
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator CloverPatternHard()
    {
        int bullets = 72;

        for (int i = 0; i < bullets; i++)
        {
            float t = i / (float)bullets * Mathf.PI * 2f;

            float radius = Mathf.Sin(4f * t);
            float x = Mathf.Cos(t) * radius;
            float z = Mathf.Sin(t) * radius;

            Vector3 dir = new Vector3(x, 0f, z).normalized;

            SpawnDir(dir, bulletSpeedPhase2);
            SpawnDir(-dir, bulletSpeedPhase2 * 0.9f);

            if (i % 4 == 0)
                yield return new WaitForSeconds(0.02f);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ZigZagStorm()
    {
        int waves = 5;
        float baseAngle = DirectionToAngle(DirectionToPlayer());

        for (int w = 0; w < waves; w++)
        {
            for (int i = 0; i < 9; i++)
            {
                float angle = baseAngle - 60f + i * 15f;
                SpawnZigZag(angle + w * 8f, bulletSpeedPhase2);
            }

            yield return new WaitForSeconds(0.18f);
        }
    }

    private IEnumerator RotatingPetalWall()
    {
        int waves = 12;

        for (int w = 0; w < waves; w++)
        {
            float offset = w * 13f;

            for (int i = 0; i < 5; i++)
            {
                float angle = offset + i * 72f;
                Spawn(angle, bulletSpeedPhase2);
                Spawn(angle + 10f, bulletSpeedPhase2 * 0.9f);
            }

            yield return new WaitForSeconds(0.12f);
        }
    }

    // -------------------------
    // SPAWN
    // -------------------------

    private void Spawn(float angle, float speed)
    {
        float rad = angle * Mathf.Deg2Rad;

        Vector3 dir = new Vector3(
            Mathf.Cos(rad),
            0f,
            Mathf.Sin(rad)
        ).normalized;

        SpawnDir(dir, speed);
    }

    private void SpawnDir(Vector3 dir, float speed)
    {
        if (bulletPrefab == null) return;

        GameObject b = Instantiate(
            bulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.identity
        );

        EnemyBullet eb = b.GetComponent<EnemyBullet>();

        if (eb != null)
            eb.SetDirection(dir.normalized, speed);
    }

    private void SpawnZigZag(float angle, float speed)
    {
        if (zigZagBulletPrefab == null)
        {
            Spawn(angle, speed);
            return;
        }

        float rad = angle * Mathf.Deg2Rad;

        Vector3 dir = new Vector3(
            Mathf.Cos(rad),
            0f,
            Mathf.Sin(rad)
        ).normalized;

        GameObject b = Instantiate(
            zigZagBulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.identity
        );

        Boss2ZigZagBullet zz = b.GetComponent<Boss2ZigZagBullet>();

        if (zz != null)
            zz.Init(dir, speed);
    }

    private Vector3 DirectionToPlayer()
    {
        if (player == null)
            return Vector3.forward;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return Vector3.forward;

        return dir.normalized;
    }

    private float DirectionToAngle(Vector3 direction)
    {
        return Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
    }
}