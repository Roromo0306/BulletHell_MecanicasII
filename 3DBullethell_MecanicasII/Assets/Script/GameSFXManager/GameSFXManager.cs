using UnityEngine;

public class GameSFXManager : MonoBehaviour
{
    public static GameSFXManager Instance { get; private set; }

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("General")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    [Header("Resultado")]
    public AudioClip levelCompletedClip;
    [Range(0f, 1f)] public float levelCompletedVolume = 1f;

    public AudioClip loseClip;
    [Range(0f, 1f)] public float loseVolume = 1f;

    [Header("Player")]
    public AudioClip playerDamageClip;
    [Range(0f, 1f)] public float playerDamageVolume = 1f;

    public AudioClip dashClip;
    [Range(0f, 1f)] public float dashVolume = 1f;

    public AudioClip swordClip;
    [Range(0f, 1f)] public float swordVolume = 1f;

    [Header("Objetos lanzables")]
    public AudioClip bottleImpactClip;
    [Range(0f, 1f)] public float bottleImpactVolume = 1f;

    public AudioClip cannonBallImpactClip;
    [Range(0f, 1f)] public float cannonBallImpactVolume = 1f;

    public AudioClip anchorClip;
    [Range(0f, 1f)] public float anchorVolume = 1f;

    [Header("Consumibles")]
    public AudioClip heartConsumableClip;
    [Range(0f, 1f)] public float heartConsumableVolume = 1f;

    public AudioClip hourglassConsumableClip;
    [Range(0f, 1f)] public float hourglassConsumableVolume = 1f;

    public AudioClip bubbleConsumableClip;
    [Range(0f, 1f)] public float bubbleConsumableVolume = 1f;

    [Header("Bosses")]
    public AudioClip boss3SpikeClip;
    [Range(0f, 1f)] public float boss3SpikeVolume = 1f;

    public AudioClip bossBulletClip;
    [Range(0f, 1f)] public float bossBulletVolume = 1f;
    public float bossBulletMinTimeBetweenSounds = 0.04f;

    public AudioClip laserChargeClip;
    [Range(0f, 1f)] public float laserChargeVolume = 1f;

    public AudioClip laserFireClip;
    [Range(0f, 1f)] public float laserFireVolume = 1f;

    [Header("Boss Rage")]
    public AudioClip boss2RageClip;
    [Range(0f, 1f)] public float boss2RageVolume = 1f;

    public AudioClip boss3RageClip;
    [Range(0f, 1f)] public float boss3RageVolume = 1f;

    [Header("Boss Dash")]
    public AudioClip bossDashClip;
    [Range(0f, 1f)] public float bossDashVolume = 1f;

    [Header("Boss Damage")]
    public AudioClip bossDamageClip;
    [Range(0f, 1f)] public float bossDamageVolume = 1f;
    public float bossDamageMinTimeBetweenSounds = 0.08f;

    private float nextBossDamageSoundTime;

    [Header("UI")]
    public AudioClip retryButtonClip;
    [Range(0f, 1f)] public float retryButtonVolume = 1f;

    public AudioClip continueButtonClip;
    [Range(0f, 1f)] public float continueButtonVolume = 1f;

    private float nextBossBulletSoundTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
    public void PlayBossDamage()
    {
        if (Time.time < nextBossDamageSoundTime)
            return;

        nextBossDamageSoundTime = Time.time + bossDamageMinTimeBetweenSounds;
        PlaySound(bossDamageClip, bossDamageVolume);
    }
    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null) return;
        if (audioSource == null) return;

        audioSource.PlayOneShot(clip, volume * masterVolume);
    }

    public void PlayBossDash()
    {
        PlaySound(bossDashClip, bossDashVolume);
    }

    public void PlayLevelCompleted()
    {
        PlaySound(levelCompletedClip, levelCompletedVolume);
    }

    public void PlayLose()
    {
        PlaySound(loseClip, loseVolume);
    }

    public void PlayPlayerDamage()
    {
        PlaySound(playerDamageClip, playerDamageVolume);
    }

    public void PlayBottleImpact()
    {
        PlaySound(bottleImpactClip, bottleImpactVolume);
    }

    public void PlayCannonBallImpact()
    {
        PlaySound(cannonBallImpactClip, cannonBallImpactVolume);
    }

    public void PlayHeartConsumable()
    {
        PlaySound(heartConsumableClip, heartConsumableVolume);
    }

    public void PlayHourglassConsumable()
    {
        PlaySound(hourglassConsumableClip, hourglassConsumableVolume);
    }

    public void PlayBubbleConsumable()
    {
        PlaySound(bubbleConsumableClip, bubbleConsumableVolume);
    }

    public void PlayAnchor()
    {
        PlaySound(anchorClip, anchorVolume);
    }

    public void PlayBoss3Spike()
    {
        PlaySound(boss3SpikeClip, boss3SpikeVolume);
    }

    public void PlayBossBullet()
    {
        if (Time.time < nextBossBulletSoundTime)
            return;

        nextBossBulletSoundTime = Time.time + bossBulletMinTimeBetweenSounds;
        PlaySound(bossBulletClip, bossBulletVolume);
    }

    public void PlayLaserCharge()
    {
        PlaySound(laserChargeClip, laserChargeVolume);
    }

    public void PlayLaserFire()
    {
        PlaySound(laserFireClip, laserFireVolume);
    }

    public void PlayRetryButton()
    {
        PlaySound(retryButtonClip, retryButtonVolume);
    }

    public void PlayContinueButton()
    {
        PlaySound(continueButtonClip, continueButtonVolume);
    }

    public void PlayDash()
    {
        PlaySound(dashClip, dashVolume);
    }

    public void PlaySword()
    {
        PlaySound(swordClip, swordVolume);
    }

    public void PlayBossBulletAttack()
    {
        PlayBossBullet();
    }

    public void PlayBoss2Rage()
    {
        PlaySound(boss2RageClip, boss2RageVolume);
    }

    public void PlayBoss3Rage()
    {
        PlaySound(boss3RageClip, boss3RageVolume);
    }
}