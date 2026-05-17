using System.Collections;
using UnityEngine;

public class Boss3Controller : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Boss3Health bossHealth;
    public Transform bulletSpawnPoint;
    public GameObject normalBulletPrefab;
    public GameObject pulsingBulletPrefab;
    public Boss3RamHitbox ramHitbox;

    [Header("Movement")]
    public float orbitRadius = 5.5f;
    public float orbitSpeed = 0.35f;
    public float moveSpeed = 1.9f;
    public float randomOffsetStrength = 0.8f;
    public float randomOffsetChangeTime = 2f;

    [Header("Bullet Speeds")]
    public float normalBulletSpeedPhase1 = 3.6f;
    public float normalBulletSpeedPhase3 = 4.4f;
    public float pulsingBulletSpeedPhase2 = 2.8f;
    public float pulsingBulletSpeedPhase3 = 3.4f;

    [Header("Attack Timing")]
    public float timeBetweenAttacks = 1.6f;

    [Header("Pre Attack Shake")]
    public float preAttackShakeDuration = 0.28f;
    public float preAttackShakeStrength = 0.08f;

    [Header("Phases")]
    public float phase2HealthPercent = 0.66f;
    public float phase3HealthPercent = 0.33f;

    [Header("Rage")]
    public float rageDuration = 1.35f;
    public float rageShakeStrength = 0.18f;
    public float rageLiftHeight = 1.4f;

    [Header("Ram Attack")]
    public float ramTriggerDistance = 8f;
    public float ramCooldown = 5f;
    public float ramWindupTime = 0.55f;
    public float ramSpeed = 11f;
    public float ramMaxDistance = 9f;
    public float ramRecoveryTime = 0.45f;

    [Header("Spike Attack")]
    public Boss3SpikeTrap spikePrefab;
    public float spikeGroundYOffset = 0f;
    public float spikeDelayBetweenSpawns = 0.3f;

    [Header("Rage Camera Shake")]
    public CameraFollow cameraFollow;
    public float rageCameraShakeStrength = 0.22f;

    private int currentPhase = 1;

    private bool isAttacking;
    private bool isRaging;
    private bool isRamming;
    private bool phaseChanging;

    private float orbitAngle;
    private Vector3 randomOffset;
    private float nextRandomOffsetTime;
    private float nextRamTime;

    private Quaternion startRotation;

    private void Awake()
    {
        startRotation = transform.rotation;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        if (bossHealth == null)
            bossHealth = GetComponent<Boss3Health>();

        if (bulletSpawnPoint == null)
            bulletSpawnPoint = transform;

        if (ramHitbox != null)
            ramHitbox.SetActiveHitbox(false);

        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();
    }

    private void Start()
    {
        StartCoroutine(AttackLoop());
    }

    private void Update()
    {
        if (bossHealth != null && bossHealth.IsDead) return;

        CheckPhaseChanges();

        if (!isAttacking && !isRaging && !isRamming)
        {
            MoveAroundPlayer();
            CheckRam();
        }

        transform.rotation = startRotation;
    }

    private void MoveAroundPlayer()
    {
        if (player == null) return;

        orbitAngle += orbitSpeed * Time.deltaTime;

        if (Time.time >= nextRandomOffsetTime)
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

    private void CheckPhaseChanges()
    {
        if (phaseChanging) return;
        if (bossHealth == null) return;

        float hp = bossHealth.CurrentHealthNormalized();

        if (currentPhase == 1 && hp <= phase2HealthPercent)
        {
            StartCoroutine(ChangePhaseRoutine(2));
        }
        else if (currentPhase == 2 && hp <= phase3HealthPercent)
        {
            StartCoroutine(ChangePhaseRoutine(3));
        }
    }

    private IEnumerator ChangePhaseRoutine(int newPhase)
    {
        phaseChanging = true;
        currentPhase = newPhase;

        isRaging = true;
        isAttacking = true;
        isRamming = false;

        if (cameraFollow != null)
            cameraFollow.StartCameraShake(rageDuration, rageCameraShakeStrength);

        if (GameSFXManager.Instance != null)
            GameSFXManager.Instance.PlayBoss3Rage();

        if (ramHitbox != null)
            ramHitbox.SetActiveHitbox(false);

        Vector3 startPos = transform.position;
        Vector3 liftedPos = startPos + Vector3.up * rageLiftHeight;

        float liftTime = 0.35f;
        float timer = 0f;

        while (timer < liftTime)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, liftedPos, timer / liftTime);
            yield return null;
        }

        timer = 0f;

        while (timer < rageDuration)
        {
            timer += Time.deltaTime;

            Vector3 shake = Random.insideUnitSphere * rageShakeStrength;
            transform.position = liftedPos + shake;

            yield return null;
        }

        timer = 0f;

        while (timer < liftTime)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(liftedPos, startPos, timer / liftTime);
            yield return null;
        }

        transform.position = startPos;

        isRaging = false;
        isAttacking = false;
        phaseChanging = false;

        Debug.Log("Boss 3 entra en fase " + currentPhase);
    }

    private void CheckRam()
    {
        if (player == null) return;
        if (Time.time < nextRamTime) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance >= ramTriggerDistance)
        {
            StartCoroutine(RamRoutine());
        }
    }

    private IEnumerator RamRoutine()
    {
        isRamming = true;
        isAttacking = true;

        nextRamTime = Time.time + ramCooldown;

        Vector3 startPos = transform.position;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector3.forward;

        direction.Normalize();

        float windupTimer = 0f;

        while (windupTimer < ramWindupTime)
        {
            windupTimer += Time.deltaTime;

            Vector3 shake = new Vector3(
                Random.Range(-preAttackShakeStrength, preAttackShakeStrength),
                0f,
                Random.Range(-preAttackShakeStrength, preAttackShakeStrength)
            );

            transform.position = startPos + shake;

            yield return null;
        }

        transform.position = startPos;

        if (GameSFXManager.Instance != null)
            GameSFXManager.Instance.PlayBossDash();

        if (ramHitbox != null)
            ramHitbox.SetActiveHitbox(true);

        float distanceToPlayer = Vector3.Distance(startPos, player.position);
        float ramDistance = Mathf.Min(distanceToPlayer + 2f, ramMaxDistance);

        Vector3 endPos = startPos + direction * ramDistance;
        endPos.y = startPos.y;

        float ramTimer = 0f;
        float maxRamTime = ramDistance / ramSpeed;

        while (ramTimer < maxRamTime)
        {
            ramTimer += Time.deltaTime;

            transform.position = Vector3.MoveTowards(
                transform.position,
                endPos,
                ramSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, endPos) < 0.05f)
                break;

            yield return null;
        }

        if (ramHitbox != null)
            ramHitbox.SetActiveHitbox(false);

        yield return new WaitForSeconds(ramRecoveryTime);

        isRamming = false;
        isAttacking = false;
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (bossHealth != null && bossHealth.IsDead)
                yield break;

            if (!isRaging && !isRamming && !phaseChanging)
            {
                isAttacking = true;

                yield return StartCoroutine(PreAttackShake());

                if (currentPhase == 1)
                    yield return StartCoroutine(Phase1Attack());
                else if (currentPhase == 2)
                    yield return StartCoroutine(Phase2Attack());
                else
                    yield return StartCoroutine(Phase3Attack());

                isAttacking = false;
            }

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    private IEnumerator PreAttackShake()
    {
        Vector3 start = transform.position;
        float timer = 0f;

        while (timer < preAttackShakeDuration)
        {
            timer += Time.deltaTime;

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
        int r = Random.Range(0, 7);

        if (r != 6)
        {
            if (GameSFXManager.Instance != null)
                GameSFXManager.Instance.PlayBossBulletAttack();
        }

        switch (r)
        {
            case 0:
                yield return StartCoroutine(Normal_BloomFan());
                break;

            case 1:
                yield return StartCoroutine(Normal_RingWithSafeGap());
                break;

            case 2:
                yield return StartCoroutine(Normal_DoubleSpiral());
                break;

            case 3:
                yield return StartCoroutine(Normal_ArteryCurtain());
                break;

            case 4:
                yield return StartCoroutine(Normal_SlowWideSpiral());
                break;

            case 5:
                yield return StartCoroutine(Normal_BrokenSpiral());
                break;

            case 6:
                yield return StartCoroutine(Spike_PlayerPositionAttack(1));
                break;
        }
    }

    private IEnumerator Phase2Attack()
    {
        int r = Random.Range(0, 5);

        if (r != 4)
        {
            if (GameSFXManager.Instance != null)
                GameSFXManager.Instance.PlayBossBulletAttack();
        }

        switch (r)
        {
            case 0:
                yield return StartCoroutine(Pulse_HeartbeatRings());
                break;

            case 1:
                yield return StartCoroutine(Pulse_AimedArteries());
                break;

            case 2:
                yield return StartCoroutine(Pulse_HeartbeatSpiral());
                break;

            case 3:
                yield return StartCoroutine(Pulse_DoubleHeartbeatSpiral());
                break;

            case 4:
                yield return StartCoroutine(Spike_PlayerPositionAttack(2));
                break;
        }
    }

    private IEnumerator Phase3Attack()
    {
        int r = Random.Range(0, 8);

        if (r != 6)
        {
            if (GameSFXManager.Instance != null)
                GameSFXManager.Instance.PlayBossBulletAttack();
        }

        switch (r)
        {
            case 0:
                yield return StartCoroutine(Combo_BloomHeartbeat());
                break;

            case 1:
                yield return StartCoroutine(Combo_GapRingArteries());
                break;

            case 2:
                yield return StartCoroutine(Combo_SpiralPulseNest());
                break;

            case 3:
                yield return StartCoroutine(Combo_CurtainHeartbeats());
                break;

            case 4:
                yield return StartCoroutine(Combo_HelixHeartbeat());
                break;

            case 5:
                yield return StartCoroutine(Combo_ClosingSpiral());
                break;

            case 6:
                yield return StartCoroutine(Spike_PlayerPositionAttack(3));
                break;

            case 7:
                yield return StartCoroutine(Combo_SpikesAndHeartbeats());
                break;
        }
    }

    // -------------------------
    // FASE 1 - NORMALES
    // -------------------------

    private IEnumerator Normal_BloomFan()
    {
        float baseAngle = DirectionToAngle(DirectionToPlayer());

        int waves = 3;
        int bullets = 11;
        float spread = 85f;

        for (int w = 0; w < waves; w++)
        {
            float waveOffset = w % 2 == 0 ? 0f : 8f;

            for (int i = 0; i < bullets; i++)
            {
                float angle = baseAngle - spread / 2f + spread * i / (bullets - 1) + waveOffset;
                SpawnNormal(angle, normalBulletSpeedPhase1);
            }

            yield return new WaitForSeconds(0.22f);
        }
    }

    private IEnumerator Normal_RingWithSafeGap()
    {
        int rings = 4;
        int bulletsPerRing = 30;
        float gapSize = 45f;

        for (int r = 0; r < rings; r++)
        {
            float gapAngle = DirectionToAngle(DirectionToPlayer());
            float offset = r * 7f;

            for (int i = 0; i < bulletsPerRing; i++)
            {
                float angle = offset + 360f * i / bulletsPerRing;

                if (Mathf.Abs(Mathf.DeltaAngle(angle, gapAngle)) < gapSize * 0.5f)
                    continue;

                SpawnNormal(angle, normalBulletSpeedPhase1);
            }

            yield return new WaitForSeconds(0.28f);
        }
    }

    private IEnumerator Normal_DoubleSpiral()
    {
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < 42; i++)
        {
            angle += 17f;

            SpawnNormal(angle, normalBulletSpeedPhase1);
            SpawnNormal(angle + 180f, normalBulletSpeedPhase1);

            yield return new WaitForSeconds(0.045f);
        }
    }

    private IEnumerator Normal_ArteryCurtain()
    {
        float baseAngle = DirectionToAngle(DirectionToPlayer());

        int waves = 6;

        for (int w = 0; w < waves; w++)
        {
            float offset = w % 2 == 0 ? 0f : 12f;

            SpawnNormal(baseAngle - 105f + offset, normalBulletSpeedPhase1);
            SpawnNormal(baseAngle - 75f + offset, normalBulletSpeedPhase1);
            SpawnNormal(baseAngle - 45f + offset, normalBulletSpeedPhase1);

            SpawnNormal(baseAngle + 45f - offset, normalBulletSpeedPhase1);
            SpawnNormal(baseAngle + 75f - offset, normalBulletSpeedPhase1);
            SpawnNormal(baseAngle + 105f - offset, normalBulletSpeedPhase1);

            yield return new WaitForSeconds(0.18f);
        }
    }

    private IEnumerator Normal_SlowWideSpiral()
    {
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < 55; i++)
        {
            angle += 13f;

            SpawnNormal(angle, normalBulletSpeedPhase1);
            SpawnNormal(angle + 160f, normalBulletSpeedPhase1 * 0.95f);

            yield return new WaitForSeconds(0.045f);
        }
    }

    private IEnumerator Normal_BrokenSpiral()
    {
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < 72; i++)
        {
            angle += 11f;

            bool skip = i % 7 == 0 || i % 7 == 1;

            if (!skip)
            {
                SpawnNormal(angle, normalBulletSpeedPhase1);
                SpawnNormal(angle + 180f, normalBulletSpeedPhase1);
            }

            yield return new WaitForSeconds(0.035f);
        }
    }

    // -------------------------
    // FASE 2 - PULSANTES
    // -------------------------

    private IEnumerator Pulse_HeartbeatRings()
    {
        int beats = 3;
        int bullets = 18;

        for (int b = 0; b < beats; b++)
        {
            for (int i = 0; i < bullets; i++)
            {
                float angle = 360f * i / bullets;
                SpawnPulse(angle, pulsingBulletSpeedPhase2, 1.3f, i * 0.02f);
            }

            yield return new WaitForSeconds(0.16f);

            for (int i = 0; i < bullets; i++)
            {
                float angle = 10f + 360f * i / bullets;
                SpawnPulse(angle, pulsingBulletSpeedPhase2 * 1.05f, 1.1f, i * 0.025f);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator Pulse_AimedArteries()
    {
        float baseAngle = DirectionToAngle(DirectionToPlayer());

        int beats = 6;

        for (int b = 0; b < beats; b++)
        {
            float side = b % 2 == 0 ? -1f : 1f;

            SpawnPulse(baseAngle, pulsingBulletSpeedPhase2, 1.4f, 0f);
            SpawnPulse(baseAngle + side * 18f, pulsingBulletSpeedPhase2, 1.2f, 0.08f);
            SpawnPulse(baseAngle - side * 32f, pulsingBulletSpeedPhase2 * 0.9f, 1.1f, 0.14f);

            yield return new WaitForSeconds(0.28f);
        }
    }

    private IEnumerator Pulse_HeartbeatSpiral()
    {
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < 48; i++)
        {
            angle += 16f;

            float phaseOffset = i * 0.035f;

            SpawnPulse(angle, pulsingBulletSpeedPhase2, 1.45f, phaseOffset);

            yield return new WaitForSeconds(0.055f);
        }
    }

    private IEnumerator Pulse_DoubleHeartbeatSpiral()
    {
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < 54; i++)
        {
            angle += 14f;

            float phaseOffsetA = i * 0.025f;
            float phaseOffsetB = phaseOffsetA + 0.12f;

            SpawnPulse(angle, pulsingBulletSpeedPhase2, 1.35f, phaseOffsetA);
            SpawnPulse(angle + 180f, pulsingBulletSpeedPhase2, 1.35f, phaseOffsetB);

            yield return new WaitForSeconds(0.045f);
        }
    }

    // -------------------------
    // FASE 3 - COMBOS
    // -------------------------

    private IEnumerator Combo_BloomHeartbeat()
    {
        float baseAngle = DirectionToAngle(DirectionToPlayer());

        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0; i < 9; i++)
            {
                float angle = baseAngle - 70f / 2f + 70f * i / 8f;
                SpawnNormal(angle + wave * 10f, normalBulletSpeedPhase3);
            }

            yield return new WaitForSeconds(0.12f);

            for (int i = 0; i < 14; i++)
            {
                float angle = wave * 15f + 360f * i / 14f;
                SpawnPulse(angle, pulsingBulletSpeedPhase3, 1.2f, i * 0.02f);
            }

            yield return new WaitForSeconds(0.28f);
        }
    }

    private IEnumerator Combo_GapRingArteries()
    {
        int bullets = 32;

        for (int wave = 0; wave < 3; wave++)
        {
            float gapAngle = DirectionToAngle(DirectionToPlayer());

            for (int i = 0; i < bullets; i++)
            {
                float angle = wave * 8f + 360f * i / bullets;

                if (Mathf.Abs(Mathf.DeltaAngle(angle, gapAngle)) < 38f)
                    continue;

                SpawnNormal(angle, normalBulletSpeedPhase3);
            }

            yield return new WaitForSeconds(0.15f);

            SpawnPulse(gapAngle - 20f, pulsingBulletSpeedPhase3, 1.3f, 0f);
            SpawnPulse(gapAngle + 20f, pulsingBulletSpeedPhase3, 1.3f, 0.1f);

            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator Combo_SpiralPulseNest()
    {
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < 46; i++)
        {
            angle += 15f;

            SpawnNormal(angle, normalBulletSpeedPhase3);

            if (i % 4 == 0)
            {
                SpawnPulse(angle + 90f, pulsingBulletSpeedPhase3, 1.4f, 0f);
                SpawnPulse(angle - 90f, pulsingBulletSpeedPhase3, 1.4f, 0.12f);
            }

            yield return new WaitForSeconds(0.04f);
        }
    }

    private IEnumerator Combo_CurtainHeartbeats()
    {
        float baseAngle = DirectionToAngle(DirectionToPlayer());

        for (int wave = 0; wave < 6; wave++)
        {
            float offset = wave * 8f;

            SpawnNormal(baseAngle - 95f + offset, normalBulletSpeedPhase3);
            SpawnNormal(baseAngle - 65f + offset, normalBulletSpeedPhase3);
            SpawnNormal(baseAngle - 35f + offset, normalBulletSpeedPhase3);

            SpawnNormal(baseAngle + 35f - offset, normalBulletSpeedPhase3);
            SpawnNormal(baseAngle + 65f - offset, normalBulletSpeedPhase3);
            SpawnNormal(baseAngle + 95f - offset, normalBulletSpeedPhase3);

            if (wave % 2 == 0)
            {
                SpawnPulse(baseAngle, pulsingBulletSpeedPhase3, 1.5f, 0f);
                SpawnPulse(baseAngle + 14f, pulsingBulletSpeedPhase3, 1.3f, 0.08f);
                SpawnPulse(baseAngle - 14f, pulsingBulletSpeedPhase3, 1.3f, 0.16f);
            }

            yield return new WaitForSeconds(0.17f);
        }
    }

    private IEnumerator Combo_HelixHeartbeat()
    {
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < 64; i++)
        {
            angle += 12f;

            SpawnNormal(angle, normalBulletSpeedPhase3);

            if (i % 2 == 0)
            {
                SpawnPulse(angle + 120f, pulsingBulletSpeedPhase3, 1.25f, i * 0.02f);
                SpawnPulse(angle - 120f, pulsingBulletSpeedPhase3, 1.25f, i * 0.025f);
            }

            yield return new WaitForSeconds(0.04f);
        }
    }

    private IEnumerator Combo_ClosingSpiral()
    {
        float baseAngle = DirectionToAngle(DirectionToPlayer());
        float spiralAngle = baseAngle;

        for (int wave = 0; wave < 4; wave++)
        {
            for (int i = 0; i < 18; i++)
            {
                spiralAngle += 18f;

                float pullToPlayer = Mathf.Lerp(0f, 45f, i / 17f);

                SpawnNormal(spiralAngle + pullToPlayer, normalBulletSpeedPhase3);
                SpawnNormal(spiralAngle + 180f - pullToPlayer, normalBulletSpeedPhase3);

                if (i % 5 == 0)
                {
                    SpawnPulse(baseAngle + Random.Range(-25f, 25f), pulsingBulletSpeedPhase3, 1.4f, i * 0.03f);
                }

                yield return new WaitForSeconds(0.035f);
            }

            yield return new WaitForSeconds(0.18f);
        }
    }

    // -------------------------
    // SPIKE ATTACKS
    // -------------------------

    private IEnumerator Spike_PlayerPositionAttack(int spikeCount)
    {
        if (spikePrefab == null || player == null)
            yield break;

        for (int i = 0; i < spikeCount; i++)
        {
            Vector3 spawnPos = player.position;
            spawnPos.y += spikeGroundYOffset;

            Boss3SpikeTrap spike = Instantiate(
                spikePrefab,
                spawnPos,
                Quaternion.identity
            );

            spike.Begin();

            yield return new WaitForSeconds(spikeDelayBetweenSpawns);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator Combo_SpikesAndHeartbeats()
    {
        if (player == null)
            yield break;

        for (int wave = 0; wave < 3; wave++)
        {
            Vector3 spawnPos = player.position;
            spawnPos.y += spikeGroundYOffset;

            if (spikePrefab != null)
            {
                Boss3SpikeTrap spike = Instantiate(
                    spikePrefab,
                    spawnPos,
                    Quaternion.identity
                );

                spike.Begin();
            }

            float baseAngle = DirectionToAngle(DirectionToPlayer());

            SpawnPulse(baseAngle - 25f, pulsingBulletSpeedPhase3, 1.3f, 0f);
            SpawnPulse(baseAngle, pulsingBulletSpeedPhase3, 1.5f, 0.08f);
            SpawnPulse(baseAngle + 25f, pulsingBulletSpeedPhase3, 1.3f, 0.16f);

            yield return new WaitForSeconds(0.45f);
        }

        yield return new WaitForSeconds(0.4f);
    }

    // -------------------------
    // SPAWN HELPERS
    // -------------------------

    private void SpawnNormal(float angle, float speed)
    {
        SpawnNormalDir(AngleToDirection(angle), speed);
    }

    private void SpawnNormalDir(Vector3 direction, float speed)
    {
        if (normalBulletPrefab == null) return;

        GameObject bulletObj = Instantiate(
            normalBulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.identity
        );

        EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();

        if (bullet != null)
            bullet.SetDirection(direction.normalized, speed);
    }

    private void SpawnPulse(float angle, float speed, float pulseStrength, float phaseOffset)
    {
        SpawnPulseDir(AngleToDirection(angle), speed, pulseStrength, phaseOffset);
    }

    private void SpawnPulseDir(Vector3 direction, float speed, float pulseStrength, float phaseOffset)
    {
        if (pulsingBulletPrefab == null) return;

        GameObject bulletObj = Instantiate(
            pulsingBulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.identity
        );

        Boss3PulsingBullet bullet = bulletObj.GetComponent<Boss3PulsingBullet>();

        if (bullet != null)
            bullet.Init(direction.normalized, speed, pulseStrength, phaseOffset);
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

    private Vector3 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        return new Vector3(
            Mathf.Cos(rad),
            0f,
            Mathf.Sin(rad)
        ).normalized;
    }
}