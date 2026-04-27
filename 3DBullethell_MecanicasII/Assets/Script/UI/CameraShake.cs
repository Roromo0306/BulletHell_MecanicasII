using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float duration = 0.12f;
    public float strength = 0.08f;

    private Coroutine routine;
    private Vector3 startLocalPosition;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    public void Shake()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            Vector2 offset = Random.insideUnitCircle * strength;

            transform.localPosition = startLocalPosition + new Vector3(
                offset.x,
                offset.y,
                0f
            );

            yield return null;
        }

        transform.localPosition = startLocalPosition;
        routine = null;
    }
}