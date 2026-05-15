using System.Collections.Generic;
using UnityEngine;

public class BossDitherRevealController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera targetCamera;

    [Tooltip("Arrastra aquí el root visual del boss. En tu caso: Fish (1).")]
    public Transform visualRoot;

    [Header("Shader")]
    public Shader ditherShader;

    [Header("Detection")]
    public float activationScreenDistance = 0.22f;
    public float depthMargin = 0.03f;
    public bool invertBehindCheck = false;

    [Header("Reveal Effect")]
    public Vector3 playerRevealOffset = new Vector3(0f, 0.75f, 0f);
    public float revealRadius = 0.14f;
    public float revealSoftness = 0.04f;

    [Range(0.05f, 1f)]
    public float minVisibleAmount = 0.35f;

    [Tooltip("1 = puntitos pequeños. 2 o 3 = puntitos más grandes.")]
    public float ditherPixelScale = 1f;

    public float fadeSpeed = 8f;

    [Header("Safety")]
    [Tooltip("Si está activo, cuando no hay efecto vuelve SIEMPRE a los materiales originales.")]
    public bool restoreOriginalWhenInactive = true;

    private class RendererMaterialData
    {
        public Renderer renderer;
        public Material[] originalMaterials;
        public Material[] ditherMaterials;
    }

    private readonly List<RendererMaterialData> rendererData = new List<RendererMaterialData>();
    private readonly List<Material> ditherMaterials = new List<Material>();

    private float currentReveal;
    private bool usingDitherMaterials;

    private static readonly int RevealScreenPosID = Shader.PropertyToID("_RevealScreenPos");
    private static readonly int RevealRadiusID = Shader.PropertyToID("_RevealRadius");
    private static readonly int RevealSoftnessID = Shader.PropertyToID("_RevealSoftness");
    private static readonly int MinAlphaID = Shader.PropertyToID("_MinAlpha");
    private static readonly int DitherPixelScaleID = Shader.PropertyToID("_DitherPixelScale");

    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");

    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

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

        if (visualRoot == null)
            visualRoot = transform;

        if (ditherShader == null)
            ditherShader = Shader.Find("Custom/BossDitherReveal_Mesh");

        CacheOriginalAndCreateDitherMaterials();

        currentReveal = 0f;
        UseOriginalMaterials();
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

        if (currentReveal > 0.001f)
        {
            if (!usingDitherMaterials)
                UseDitherMaterials();

            UpdateDitherShaderValues();
        }
        else
        {
            if (restoreOriginalWhenInactive && usingDitherMaterials)
                UseOriginalMaterials();
        }
    }

    private void CacheOriginalAndCreateDitherMaterials()
    {
        rendererData.Clear();
        ditherMaterials.Clear();

        if (ditherShader == null)
        {
            Debug.LogWarning("BossDitherRevealController: no se encontró Custom/BossDitherReveal_Mesh.");
            return;
        }

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning("BossDitherRevealController: no se encontraron Renderers en " + visualRoot.name);
            return;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            Material[] originals = renderer.sharedMaterials;
            Material[] ditherCopies = new Material[originals.Length];

            for (int i = 0; i < originals.Length; i++)
            {
                Material original = originals[i];

                if (original == null)
                {
                    ditherCopies[i] = null;
                    continue;
                }

                Material ditherCopy = new Material(ditherShader);
                ditherCopy.name = original.name + "_DitherCopy";

                CopyBasicLook(original, ditherCopy);

                ditherCopy.SetFloat(RevealRadiusID, 0f);
                ditherCopy.SetFloat(RevealSoftnessID, revealSoftness);
                ditherCopy.SetFloat(MinAlphaID, minVisibleAmount);
                ditherCopy.SetFloat(DitherPixelScaleID, ditherPixelScale);

                ditherCopies[i] = ditherCopy;
                ditherMaterials.Add(ditherCopy);
            }

            RendererMaterialData data = new RendererMaterialData
            {
                renderer = renderer,
                originalMaterials = originals,
                ditherMaterials = ditherCopies
            };

            rendererData.Add(data);
        }

        Debug.Log("BossDitherRevealController: Renderers detectados: " + rendererData.Count);
        Debug.Log("BossDitherRevealController: Dither materials creados: " + ditherMaterials.Count);
    }

    private void CopyBasicLook(Material original, Material target)
    {
        Texture texture = null;
        Vector2 textureScale = Vector2.one;
        Vector2 textureOffset = Vector2.zero;
        Color color = Color.white;

        if (original.HasProperty(BaseColorID))
            color = original.GetColor(BaseColorID);
        else if (original.HasProperty(ColorID))
            color = original.GetColor(ColorID);

        if (original.HasProperty(BaseMapID) && original.GetTexture(BaseMapID) != null)
        {
            texture = original.GetTexture(BaseMapID);
            textureScale = original.GetTextureScale(BaseMapID);
            textureOffset = original.GetTextureOffset(BaseMapID);
        }
        else if (original.HasProperty(MainTexID) && original.GetTexture(MainTexID) != null)
        {
            texture = original.GetTexture(MainTexID);
            textureScale = original.GetTextureScale(MainTexID);
            textureOffset = original.GetTextureOffset(MainTexID);
        }

        if (target.HasProperty(MainTexID))
        {
            target.SetTexture(MainTexID, texture);
            target.SetTextureScale(MainTexID, textureScale);
            target.SetTextureOffset(MainTexID, textureOffset);
        }

        if (target.HasProperty(ColorID))
            target.SetColor(ColorID, color);
    }

    private void UseOriginalMaterials()
    {
        foreach (RendererMaterialData data in rendererData)
        {
            if (data == null || data.renderer == null) continue;

            data.renderer.sharedMaterials = data.originalMaterials;
        }

        usingDitherMaterials = false;
    }

    private void UseDitherMaterials()
    {
        foreach (RendererMaterialData data in rendererData)
        {
            if (data == null || data.renderer == null) continue;

            data.renderer.sharedMaterials = data.ditherMaterials;
        }

        usingDitherMaterials = true;
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

    private void UpdateDitherShaderValues()
    {
        Vector3 playerWorldPos = player.position + playerRevealOffset;
        Vector3 viewport = targetCamera.WorldToViewportPoint(playerWorldPos);

        Vector4 screenPos = new Vector4(viewport.x, viewport.y, 0f, 0f);

        float finalRadius = revealRadius * currentReveal;

        foreach (Material mat in ditherMaterials)
        {
            if (mat == null) continue;

            mat.SetVector(RevealScreenPosID, screenPos);
            mat.SetFloat(RevealRadiusID, finalRadius);
            mat.SetFloat(RevealSoftnessID, revealSoftness);
            mat.SetFloat(MinAlphaID, minVisibleAmount);
            mat.SetFloat(DitherPixelScaleID, ditherPixelScale);
        }
    }

    private void OnDisable()
    {
        UseOriginalMaterials();
    }

    private void OnDestroy()
    {
        UseOriginalMaterials();
    }
}