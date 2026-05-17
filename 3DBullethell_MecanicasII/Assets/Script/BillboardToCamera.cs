using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public enum BillboardMode
    {
        CopyCameraRotation,
        LookAtCamera
    }

    [Header("Camera")]
    public Camera targetCamera;

    [Header("Billboard")]
    public BillboardMode billboardMode = BillboardMode.CopyCameraRotation;

    [Tooltip("Actívalo si el sprite se ve al revés.")]
    public bool flipForward = false;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
                return;
        }

        if (billboardMode == BillboardMode.CopyCameraRotation)
        {
            transform.rotation = targetCamera.transform.rotation;

            if (flipForward)
                transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            Vector3 directionToCamera = transform.position - targetCamera.transform.position;

            if (directionToCamera.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(directionToCamera.normalized, targetCamera.transform.up);

            if (flipForward)
                transform.Rotate(0f, 180f, 0f);
        }
    }
}