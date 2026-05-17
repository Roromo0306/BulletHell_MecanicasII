using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public enum CameraBoundsMode
    {
        Exploration,
        Battle
    }

    [System.Serializable]
    public class CameraBounds
    {
        public bool useLimits = true;

        [Header("Horizontal")]
        public float minX = -20f;
        public float maxX = 20f;

        [Header("Depth / Vertical del mapa")]
        public float minZ = -20f;
        public float maxZ = 20f;
    }

    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -8f);
    public float smoothSpeed = 8f;

    [Header("Bounds Mode")]
    public CameraBoundsMode currentBoundsMode = CameraBoundsMode.Exploration;

    [Header("Exploration Bounds")]
    public CameraBounds explorationBounds = new CameraBounds();

    [Header("Battle Bounds")]
    public CameraBounds battleBounds = new CameraBounds();

    [Header("Camera Size Clamp")]
    public bool includeOrthographicCameraSize = true;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
            cam = GetComponentInChildren<Camera>();

        if (cam == null && Camera.main != null)
            cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;

        CameraBounds activeBounds = GetCurrentBounds();

        if (activeBounds != null && activeBounds.useLimits)
            targetPosition = ClampPositionToBounds(targetPosition, activeBounds);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    private CameraBounds GetCurrentBounds()
    {
        if (currentBoundsMode == CameraBoundsMode.Battle)
            return battleBounds;

        return explorationBounds;
    }

    private Vector3 ClampPositionToBounds(Vector3 position, CameraBounds bounds)
    {
        float minX = bounds.minX;
        float maxX = bounds.maxX;
        float minZ = bounds.minZ;
        float maxZ = bounds.maxZ;

        if (includeOrthographicCameraSize && cam != null && cam.orthographic)
        {
            float cameraHalfHeight = cam.orthographicSize;
            float cameraHalfWidth = cameraHalfHeight * cam.aspect;

            minX += cameraHalfWidth;
            maxX -= cameraHalfWidth;

            minZ += cameraHalfHeight;
            maxZ -= cameraHalfHeight;
        }

        if (maxX < minX)
        {
            float centerX = (bounds.minX + bounds.maxX) * 0.5f;
            minX = centerX;
            maxX = centerX;
        }

        if (maxZ < minZ)
        {
            float centerZ = (bounds.minZ + bounds.maxZ) * 0.5f;
            minZ = centerZ;
            maxZ = centerZ;
        }

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        return position;
    }

    public void UseExplorationBounds()
    {
        currentBoundsMode = CameraBoundsMode.Exploration;
    }

    public void UseBattleBounds()
    {
        currentBoundsMode = CameraBoundsMode.Battle;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void OnDrawGizmosSelected()
    {
        DrawBoundsGizmo(explorationBounds, Color.green);
        DrawBoundsGizmo(battleBounds, Color.red);
    }

    private void DrawBoundsGizmo(CameraBounds bounds, Color color)
    {
        if (bounds == null || !bounds.useLimits)
            return;

        Gizmos.color = color;

        Vector3 center = new Vector3(
            (bounds.minX + bounds.maxX) * 0.5f,
            transform.position.y,
            (bounds.minZ + bounds.maxZ) * 0.5f
        );

        Vector3 size = new Vector3(
            Mathf.Abs(bounds.maxX - bounds.minX),
            0.05f,
            Mathf.Abs(bounds.maxZ - bounds.minZ)
        );

        Gizmos.DrawWireCube(center, size);
    }
}