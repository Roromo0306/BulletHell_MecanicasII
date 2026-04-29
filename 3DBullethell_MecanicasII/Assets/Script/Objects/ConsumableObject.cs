using UnityEngine;

public class ConsumableObject : MonoBehaviour
{
    public ConsumableType consumableType;

    [Header("Visual")]
    public Transform visual;

    private Collider col;
    private Rigidbody rb;
    private ConsumableSpawner spawner;

    private bool isHeld;
    private int facingDirection = 1;

    public bool CanBePickedUp => !isHeld;

    private void Awake()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

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
            col.isTrigger = false;
    }

    public void SetSpawner(ConsumableSpawner newSpawner)
    {
        spawner = newSpawner;
    }

    public void PickUp(Transform holdPoint)
    {
        isHeld = true;

        if (col != null)
            col.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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

    public void Consume()
    {
        if (spawner != null)
            spawner.StartRespawnTimer();

        Destroy(gameObject);
    }

    public void Drop(Vector3 dropPosition)
    {
        isHeld = false;

        transform.SetParent(null);
        transform.position = dropPosition;
        transform.localRotation = Quaternion.identity;

        if (col != null)
            col.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}