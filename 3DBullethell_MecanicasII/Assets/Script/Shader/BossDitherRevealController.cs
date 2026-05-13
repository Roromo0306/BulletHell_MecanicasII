using System.Collections.Generic;
using UnityEngine;

public class BossDitherRevealController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera targetCamera;

    [Tooltip("Si está vacío, busca automáticamente todos los renderers hijos.")]
    public Renderer[] bossRenderers;

    [Header("Detection")]
    public float activationScreenDistance = 0.18f;
    public float depthMargin = 0.05f;
    public bool invertBehindCheck = false;

    [Header("Reveal Effect")]
    public Vector3 playerRevealOffset = new Vector3(0f, 0.75f, 0f);
    public float revealRadius = 0.12f;
    public float revealSoftness = 0.035f;
    [Range(0.05f, 1f)]
    public float minVisibleAmount = 0.35f;
    public float fadeSpeed = 8f;

    private readonly List<Material> bossMaterials = new List<Material>();
    private float currentReveal;

    private static readonly int RevealScreenPosID = Shader.PropertyToID("_RevealScreenPos");
    private static readonly int RevealRadiusID = Shader.PropertyToID("_RevealRadius");
    private static readonly int RevealSoftnessID = Shader.PropertyToID("_RevealSoftness");
    private static readonly int MinAlphaID = Shader.PropertyToID("_MinAlpha");

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                player = playerObj.transform;
        }

        if (bossRenderers == null || bossRenderers.Length == 0)
            bossRenderers = GetComponentsInChildren<Renderer>(true);

        CacheMaterials();

        SetRevealAmount(0f);
    }

    private void LateUpdate()
    {
        if (player == null || targetCamera == null)
            return;

        bool shouldReveal = ShouldReveal();

        float targetReveal = shouldReveal ? 1f : 0f;

        currentReveal = Mathf.MoveTowards(
            currentReveal,
            targetReveal,
            fadeSpeed * Time.deltaTime
        );

        UpdateShader();
    }

    private void CacheMaterials()
    {
        bossMaterials.Clear();

        if (bossRenderers == null) return;

        foreach (Renderer renderer in bossRenderers)
        {
            if (renderer == null) continue;

            Material[] mats = renderer.materials;

            foreach (Material mat in mats)
            {
                if (mat == null) continue;

                if (!bossMaterials.Contains(mat))
                    bossMaterials.Add(mat);
            }
        }
    }

    private bool ShouldReveal()
    {
        Vector3 playerWorldPos = player.position + playerRevealOffset;

        Vector3 playerViewport = targetCamera.WorldToViewportPoint(playerWorldPos);
        Vector3 bossViewport = targetCamera.WorldToViewportPoint(transform.position);

        if (playerViewport.z < 0f)
            return false;

        float depthDifference = playerViewport.z - bossViewport.z;

        bool playerBehindBoss = invertBehindCheck
            ? depthDifference < -depthMargin
            : depthDifference > depthMargin;

        Vector2 playerScreen = new Vector2(playerViewport.x, playerViewport.y);
        Vector2 bossScreen = new Vector2(bossViewport.x, bossViewport.y);

        float screenDistance = Vector2.Distance(playerScreen, bossScreen);

        bool closeOnScreen = screenDistance <= activationScreenDistance;

        return playerBehindBoss && closeOnScreen;
    }

    private void UpdateShader()
    {
        Vector3 playerWorldPos = player.position + playerRevealOffset;
        Vector3 viewport = targetCamera.WorldToViewportPoint(playerWorldPos);

        Vector4 screenPos = new Vector4(viewport.x, viewport.y, 0f, 0f);

        float finalRadius = revealRadius * currentReveal;

        foreach (Material mat in bossMaterials)
        {
            if (mat == null) continue;

            mat.SetVector(RevealScreenPosID, screenPos);
            mat.SetFloat(RevealRadiusID, finalRadius);
            mat.SetFloat(RevealSoftnessID, revealSoftness);
            mat.SetFloat(MinAlphaID, minVisibleAmount);
        }
    }

    private void SetRevealAmount(float amount)
    {
        currentReveal = amount;
        UpdateShader();
    }
}