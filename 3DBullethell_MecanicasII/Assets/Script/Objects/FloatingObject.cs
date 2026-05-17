using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Floating Movement")]
    public bool useFloating = true;

    [Tooltip("Altura del movimiento arriba/abajo.")]
    public float floatAmount = 0.25f;

    [Tooltip("Velocidad del movimiento.")]
    public float floatSpeed = 1.5f;

    [Tooltip("Hace que cada objeto empiece en un punto distinto de la animación.")]
    public bool randomStartOffset = true;

    [Header("Space")]
    [Tooltip("Si está activado, mueve el objeto en localPosition. Si está desactivado, usa position.")]
    public bool useLocalPosition = true;

    private Vector3 startPosition;
    private float randomOffset;

    private void Awake()
    {
        if (useLocalPosition)
            startPosition = transform.localPosition;
        else
            startPosition = transform.position;

        if (randomStartOffset)
            randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (!useFloating)
            return;

        float yOffset = Mathf.Sin((Time.time * floatSpeed) + randomOffset) * floatAmount;

        Vector3 targetPosition = startPosition + new Vector3(0f, yOffset, 0f);

        if (useLocalPosition)
            transform.localPosition = targetPosition;
        else
            transform.position = targetPosition;
    }

    public void ResetStartPosition()
    {
        if (useLocalPosition)
            startPosition = transform.localPosition;
        else
            startPosition = transform.position;
    }
}