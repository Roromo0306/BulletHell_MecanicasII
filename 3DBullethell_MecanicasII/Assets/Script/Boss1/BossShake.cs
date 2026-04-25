using System.Collections;
using UnityEngine;

public class BossShake : MonoBehaviour
{
    public float shakeAmount = 0.12f;
    public float shakeSpeed = 40f;

    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public IEnumerator Shake(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float z = Mathf.Cos(Time.time * shakeSpeed) * shakeAmount;

            transform.localPosition = originalPosition + new Vector3(x, 0f, z);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}