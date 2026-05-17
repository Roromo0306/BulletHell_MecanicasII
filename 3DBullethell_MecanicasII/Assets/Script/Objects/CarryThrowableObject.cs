using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ThrowableType
{
    RumBottle,
    CannonBall,
    Anchor
}

public class CarryThrowableObject : MonoBehaviour
{
    [Header("Type")]
    public ThrowableType throwableType;

    [Header("Charge")]
    public float chargeTime = 0.35f;
    public float minThrowDistance = 3f;
    public float maxThrowDistance = 8f;
    public float throwSpeed = 12f;
    public float throwArcHeight = 4f;
    public bool useGravityOnThrow = true;

    [Header("Impact Safety")]
    public float impactDelay = 0.15f;

    [Header("Boomerang Anchor")]
    public float boomerangSpeed = 9f;
    public float boomerangReturnSpeed = 11f;
    public float boomerangSideCurve = 1.2f;
    public float boomerangReturnCatchDistance = 0.6f;
    public float boomerangMaxReturnTime = 3f;

    [Header("Damage")]
    public int damage = 1;
    public float damageRadius = 2f;
    public LayerMask damageLayer;

    [Header("Impact")]
    public GameObject impactParticles;
    public float destroyAfterSeconds = 4f;

    [Header("Visual")]
    public Transform visual;

    private Rigidbody rb;
    private Collider col;
    private Collider[] objectColliders;
    private CarryObjectSpawner spawner;

    private bool isHeld;
    private bool hasBeenThrown;
    private bool canImpact;

    private Transform throwOwner;
    private Collider[] ownerColliders;

    private int facingDirection = 1;

    private readonly List<GameObject> alreadyHitBosses = new List<GameObject>();

    public bool CanBePickedUp => !isHeld && !hasBeenThrown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        objectColliders = GetComponentsInChildren<Collider>(true);

        if (visual == null)
            visual = transform;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
            col.isTrigger = true;
    }

    public void SetSpawner(CarryObjectSpawner newSpawner)
    {
        spawner = newSpawner;
    }

    public float GetCharge01(float chargeTimer)
    {
        return Mathf.Clamp01(chargeTimer / chargeTime);
    }

    public float GetThrowDistance(float charge01)
    {
        return Mathf.Lerp(minThrowDistance, maxThrowDistance, charge01);
    }

    public void PickUp(Transform holdPoint)
    {
        isHeld = true;
        hasBeenThrown = false;
        canImpact = false;
        alreadyHitBosses.Clear();

        CancelInvoke();
        StopAllCoroutines();

        ClearOwnerCollisionIgnore();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
            col.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void SetFacing(int direction)
    {
        facingDirection = direction;

        if (visual == null) return;

        Vector3 scale = visual.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        visual.localScale = scale;
    }

    public void Throw(Vector3 direction, float charge01, Transform returnTarget = null)
    {
        throwOwner = returnTarget;

        if (throwOwner == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                throwOwner = playerObj.transform;
        }

        SetupOwnerCollisionIgnore();

        if (throwableType == ThrowableType.Anchor)
        {
            

            ThrowAnchorBoomerang(direction, charge01, throwOwner);
            return;
        }

        ThrowNormal(direction, charge01);
    }

    private void ThrowNormal(Vector3 direction, float charge01)
    {
        isHeld = false;
        hasBeenThrown = true;
        canImpact = false;
        alreadyHitBosses.Clear();

        transform.SetParent(null);

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = useGravityOnThrow;

            Vector3 horizontalVelocity = direction.normalized * throwSpeed * Mathf.Lerp(0.5f, 1f, charge01);
            Vector3 verticalVelocity = Vector3.up * throwArcHeight;

            rb.velocity = horizontalVelocity + verticalVelocity;
            rb.angularVelocity = Vector3.zero;
        }

        StartCoroutine(EnableImpactAfterDelay());
        Invoke(nameof(Impact), destroyAfterSeconds);
    }

    private void ThrowAnchorBoomerang(Vector3 direction, float charge01, Transform returnTarget)
    {
        isHeld = false;
        hasBeenThrown = true;
        canImpact = false;
        alreadyHitBosses.Clear();

        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }

        StartCoroutine(EnableImpactAfterDelay());
        StartCoroutine(AnchorBoomerangRoutine(direction.normalized, charge01, returnTarget));
    }

    private IEnumerator EnableImpactAfterDelay()
    {
        yield return new WaitForSeconds(impactDelay);
        canImpact = true;
    }

    private IEnumerator AnchorBoomerangRoutine(Vector3 direction, float charge01, Transform returnTarget)
    {
        float distance = GetThrowDistance(charge01);

        Vector3 start = transform.position;
        Vector3 end = start + direction * distance;

        Vector3 side = new Vector3(-direction.z, 0f, direction.x);

        float outDuration = Mathf.Max(0.15f, distance / boomerangSpeed);
        float timer = 0f;

        while (timer < outDuration)
        {
            timer += Time.deltaTime;
            float t = timer / outDuration;

            Vector3 pos = Vector3.Lerp(start, end, t);
            pos += side * Mathf.Sin(t * Mathf.PI) * boomerangSideCurve;
            pos.y = start.y;

            transform.position = pos;

            yield return null;
        }

        timer = 0f;

        while (timer < boomerangMaxReturnTime)
        {
            timer += Time.deltaTime;

            Vector3 targetPos;

            if (returnTarget != null)
                targetPos = returnTarget.position + Vector3.up * 0.7f;
            else
                targetPos = start;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                boomerangReturnSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) <= boomerangReturnCatchDistance)
                break;

            yield return null;
        }

        FinishThrownObject();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) return;
        if (throwableType == ThrowableType.Anchor) return;

        if (IsPlayerCollider(collision.collider))
            return;

        if (!canImpact)
            return;

        Impact();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasBeenThrown) return;
        if (throwableType != ThrowableType.Anchor) return;

        if (IsPlayerCollider(other))
            return;

        if (!canImpact)
            return;

        DamageBossFromCollider(other);
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null) return false;

        if (other.GetComponentInParent<PlayerHealth>() != null)
            return true;

        if (other.GetComponentInParent<PlayerMovement>() != null)
            return true;

        return false;
    }

    private void Impact()
    {
        if (!hasBeenThrown) return;

        if (GameSFXManager.Instance != null)
        {
            if (throwableType == ThrowableType.RumBottle)
                GameSFXManager.Instance.PlayBottleImpact();
            else if (throwableType == ThrowableType.CannonBall)
                GameSFXManager.Instance.PlayCannonBallImpact();
        }

        DamageBossesInRadius();

        if (impactParticles != null)
            Instantiate(impactParticles, transform.position, Quaternion.identity);

        FinishThrownObject();
    }

    private void DamageBossesInRadius()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, damageLayer);

        foreach (Collider hit in hits)
        {
            DamageBossFromCollider(hit);
        }
    }

    private void DamageBossFromCollider(Collider hit)
    {
        GameObject rootObject = hit.transform.root.gameObject;

        if (alreadyHitBosses.Contains(rootObject))
            return;

        BossDamageReceiver boss1 = hit.GetComponentInParent<BossDamageReceiver>();

        if (boss1 != null)
        {
            alreadyHitBosses.Add(rootObject);
            boss1.TakeDamage(damage);

            if (throwableType == ThrowableType.Anchor && GameSFXManager.Instance != null)
                GameSFXManager.Instance.PlayAnchor();

            return;
        }

        Boss2DamageReceiver boss2 = hit.GetComponentInParent<Boss2DamageReceiver>();

        if (boss2 != null)
        {
            alreadyHitBosses.Add(rootObject);
            boss2.TakeDamage(damage);

            if (throwableType == ThrowableType.Anchor && GameSFXManager.Instance != null)
                GameSFXManager.Instance.PlayAnchor();

            return;
        }

        Boss3DamageReceiver boss3 = hit.GetComponentInParent<Boss3DamageReceiver>();

        if (boss3 != null)
        {
            alreadyHitBosses.Add(rootObject);
            boss3.TakeDamage(damage);

            if (throwableType == ThrowableType.Anchor && GameSFXManager.Instance != null)
                GameSFXManager.Instance.PlayAnchor();

            return;
        }
    }

    private void FinishThrownObject()
    {
        if (!hasBeenThrown) return;

        hasBeenThrown = false;
        canImpact = false;

        CancelInvoke();
        ClearOwnerCollisionIgnore();

        if (spawner != null)
            spawner.StartRespawnTimer();

        Destroy(gameObject);
    }

    private void SetupOwnerCollisionIgnore()
    {
        if (throwOwner == null)
            return;

        ownerColliders = throwOwner.GetComponentsInChildren<Collider>();

        if (ownerColliders == null || objectColliders == null)
            return;

        foreach (Collider objectCollider in objectColliders)
        {
            if (objectCollider == null) continue;

            foreach (Collider ownerCollider in ownerColliders)
            {
                if (ownerCollider == null) continue;

                Physics.IgnoreCollision(objectCollider, ownerCollider, true);
            }
        }
    }

    private void ClearOwnerCollisionIgnore()
    {
        if (ownerColliders == null || objectColliders == null)
            return;

        foreach (Collider objectCollider in objectColliders)
        {
            if (objectCollider == null) continue;

            foreach (Collider ownerCollider in ownerColliders)
            {
                if (ownerCollider == null) continue;

                Physics.IgnoreCollision(objectCollider, ownerCollider, false);
            }
        }

        ownerColliders = null;
    }
}