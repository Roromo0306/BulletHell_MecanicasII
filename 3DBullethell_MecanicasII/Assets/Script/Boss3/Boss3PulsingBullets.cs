using UnityEngine;

public class Boss3PulsingBullet : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;

    [Header("Movement")]
    public float lifeTime = 7f;

    [Header("Heartbeat")]
    public float heartbeatPeriod = 0.85f;
    public float pulseStrength = 1.2f;
    public float visualPulseScale = 0.25f;

    private Vector3 direction;
    private float baseSpeed;
    private float timer;
    private float phaseOffset;
    private Vector3 baseScale;
    private bool initialized;

    public void Init(Vector3 dir, float speed, float newPulseStrength = 1.2f, float newPhaseOffset = 0f)
    {
        direction = dir.normalized;
        baseSpeed = speed;
        pulseStrength = newPulseStrength;
        phaseOffset = newPhaseOffset;
        initialized = true;
    }

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!initialized)
            direction = transform.forward;

        timer += Time.deltaTime;

        float heartbeat = GetHeartbeatValue(timer + phaseOffset);
        float speedMultiplier = 1f + heartbeat * pulseStrength;

        transform.position += direction * baseSpeed * speedMultiplier * Time.deltaTime;

        transform.localScale = baseScale * (1f + heartbeat * visualPulseScale);
    }

    private float GetHeartbeatValue(float time)
    {
        float t = time % heartbeatPeriod;

        float firstBeat = Mathf.Exp(-Mathf.Pow((t - 0.08f) / 0.045f, 2f));
        float secondBeat = Mathf.Exp(-Mathf.Pow((t - 0.23f) / 0.06f, 2f)) * 0.75f;

        return Mathf.Clamp01(firstBeat + secondBeat);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}