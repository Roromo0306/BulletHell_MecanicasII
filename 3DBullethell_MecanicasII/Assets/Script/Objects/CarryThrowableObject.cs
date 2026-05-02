using UnityEngine;

public enum ThrowableType
{
    RumBottle,
    CannonBall
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
    public float impactDelay = 0.15f;
    private bool canImpact;

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
    private CarryObjectSpawner spawner;
    private bool isHeld;
    private bool hasBeenThrown;
    private int facingDirection = 1;

    public bool CanBePickedUp => !isHeld && !hasBeenThrown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (visual == null)
            visual = transform;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
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

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
        {
            col.enabled = false;
        }

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

    public void Throw(Vector3 direction, float charge01)
    {
        isHeld = false;
        hasBeenThrown = true;
        canImpact = false;

        transform.SetParent(null);

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 horizontalVelocity = direction.normalized * throwSpeed * Mathf.Lerp(0.5f, 1f, charge01);
            Vector3 verticalVelocity = Vector3.up * throwArcHeight;

            rb.velocity = horizontalVelocity + verticalVelocity;
            rb.angularVelocity = Vector3.zero;
        }

        Invoke(nameof(EnableImpact), impactDelay);
        Invoke(nameof(Impact), destroyAfterSeconds);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) return;
        if (!canImpact) return;

        Impact();
    }

    private void Impact()
    {
        if (!hasBeenThrown) return;

        hasBeenThrown = false;

        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, damageLayer);

        foreach (Collider hit in hits)
        {
            BossDamageReceiver boss = hit.GetComponentInParent<BossDamageReceiver>();

            if (boss != null)
                boss.TakeDamage(damage);

            Boss2DamageReceiver boss2 = hit.GetComponentInParent<Boss2DamageReceiver>();

            if (boss2 != null)
            {
                boss2.TakeDamage(damage);
                continue;
            }

            Boss3DamageReceiver boss3 = hit.GetComponentInParent<Boss3DamageReceiver>();

            if (boss3 != null)
            {
                boss3.TakeDamage(damage);
                continue;
            }
        }

        

        if (impactParticles != null)
            Instantiate(impactParticles, transform.position, Quaternion.identity);

        if (spawner != null)
            spawner.StartRespawnTimer();

        Destroy(gameObject);
    }

    private void EnableImpact()
    {
        canImpact = true;
    }
}