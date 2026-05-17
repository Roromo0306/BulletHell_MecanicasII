using System.Collections;
using UnityEngine;

public class BossEncounterTrigger : MonoBehaviour
{
    [Header("Boss Info")]
    public string bossName = "BOSS NAME";

    [Header("Boss")]
    public GameObject bossRoot;
    public bool hideBossUntilIntroEnds = false;

    [Tooltip("Aquí metes los scripts que arrancan la batalla: BossController, Boss2Controller o Boss3Controller.")]
    public Behaviour[] bossBehavioursToEnable;

    [Header("Battle Colliders")]
    public Collider[] collidersToEnableWhenBattleStarts;
    public bool disableBattleCollidersOnAwake = true;

    [Header("Camera")]
    public CameraFollow cameraFollow;
    public bool setCameraToExplorationBoundsOnAwake = true;
    public bool setCameraToBattleBoundsWhenBattleStarts = true;

    [Header("UI Intro")]
    public BossNameWaveUI bossNameWaveUI;

    [Header("Health Bar")]
    public GameObject healthBarRoot;
    public CanvasGroup healthBarCanvasGroup;
    public float healthBarFadeTime = 0.35f;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public bool lockPlayerDuringIntro = true;

    [Header("Timing")]
    public float delayBeforeName = 0.25f;
    public float delayAfterName = 0.2f;

    private bool triggered;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();

        if (cameraFollow != null && setCameraToExplorationBoundsOnAwake)
            cameraFollow.UseExplorationBounds();

        PrepareBossInactive();
        HideHealthBarInstant();
        PrepareBattleColliders();
    }

    private void PrepareBossInactive()
    {
        if (bossRoot != null && hideBossUntilIntroEnds)
            bossRoot.SetActive(false);

        SetBossBehaviours(false);
    }

    private void PrepareBattleColliders()
    {
        if (!disableBattleCollidersOnAwake)
            return;

        SetBattleColliders(false);
    }

    private void SetBattleColliders(bool value)
    {
        if (collidersToEnableWhenBattleStarts == null)
            return;

        foreach (Collider battleCollider in collidersToEnableWhenBattleStarts)
        {
            if (battleCollider != null)
                battleCollider.enabled = value;
        }
    }

    private void SetBossBehaviours(bool value)
    {
        if (bossBehavioursToEnable == null) return;

        foreach (Behaviour behaviour in bossBehavioursToEnable)
        {
            if (behaviour != null)
                behaviour.enabled = value;
        }
    }

    private void HideHealthBarInstant()
    {
        if (healthBarRoot != null)
            healthBarRoot.SetActive(false);

        if (healthBarCanvasGroup == null && healthBarRoot != null)
            healthBarCanvasGroup = healthBarRoot.GetComponent<CanvasGroup>();

        if (healthBarCanvasGroup != null)
        {
            healthBarCanvasGroup.alpha = 0f;
            healthBarCanvasGroup.interactable = false;
            healthBarCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null) return;

        triggered = true;

        if (playerMovement == null)
            playerMovement = player;

        StartCoroutine(StartEncounterRoutine());
    }

    private IEnumerator StartEncounterRoutine()
    {
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        if (lockPlayerDuringIntro && playerMovement != null)
            playerMovement.CanMove = false;

        if (bossRoot != null)
        {
            bossRoot.SetActive(true);

            // Por seguridad, aunque se active el bossRoot, mantenemos los scripts de combate apagados.
            SetBossBehaviours(false);
        }

        yield return new WaitForSecondsRealtime(delayBeforeName);

        if (bossNameWaveUI != null)
            yield return StartCoroutine(bossNameWaveUI.Play(bossName));

        yield return new WaitForSecondsRealtime(delayAfterName);

        yield return StartCoroutine(ShowHealthBarRoutine());

        if (lockPlayerDuringIntro && playerMovement != null)
            playerMovement.CanMove = true;

        if (cameraFollow != null && setCameraToBattleBoundsWhenBattleStarts)
            cameraFollow.UseBattleBounds();

        SetBattleColliders(true);
        SetBossBehaviours(true);
    }

    private IEnumerator ShowHealthBarRoutine()
    {
        if (healthBarRoot == null)
            yield break;

        healthBarRoot.SetActive(true);

        if (healthBarCanvasGroup == null)
            healthBarCanvasGroup = healthBarRoot.GetComponent<CanvasGroup>();

        if (healthBarCanvasGroup == null)
            healthBarCanvasGroup = healthBarRoot.AddComponent<CanvasGroup>();

        healthBarCanvasGroup.interactable = false;
        healthBarCanvasGroup.blocksRaycasts = false;
        healthBarCanvasGroup.alpha = 0f;

        float timer = 0f;

        while (timer < healthBarFadeTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / healthBarFadeTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            healthBarCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        healthBarCanvasGroup.alpha = 1f;
    }
}