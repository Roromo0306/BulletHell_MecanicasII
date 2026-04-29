using UnityEngine;

public class Boss2ZigZagBullet : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 6f;
    public float zigZagAmplitude = 1.2f;
    public float zigZagFrequency = 8f;

    private Vector3 direction;
    private Vector3 sideDirection;
    private float speed;
    private float timer;

    public void Init(Vector3 dir, float moveSpeed)
    {
        direction = dir.normalized;
        speed = moveSpeed;
        sideDirection = new Vector3(-direction.z, 0f, direction.x);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        Vector3 forwardMove = direction * speed;
        Vector3 zigMove = sideDirection * Mathf.Sin(timer * zigZagFrequency) * zigZagAmplitude;

        transform.position += (forwardMove + zigMove) * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}