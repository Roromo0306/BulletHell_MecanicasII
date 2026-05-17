using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMusicManager : MonoBehaviour
{
    public static GameMusicManager Instance { get; private set; }

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
        [Range(0f, 1f)] public float volume = 0.8f;
    }

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Scene Music")]
    public SceneMusic[] sceneMusicList;

    [Header("Fade")]
    public float fadeOutDuration = 0.7f;
    public float fadeInDuration = 0.8f;

    [Header("Pitch")]
    public float normalPitch = 1f;

    private Coroutine musicRoutine;
    private bool manualFadeOutDone;

    private float currentMusicPitch = 1f;

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

        audioSource.loop = true;
        audioSource.playOnAwake = false;

        currentMusicPitch = normalPitch;
        audioSource.pitch = currentMusicPitch;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForSceneInstant(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetMusicPitch();

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        if (manualFadeOutDone)
        {
            manualFadeOutDone = false;
            musicRoutine = StartCoroutine(FadeInSceneMusic(scene.name));
        }
        else
        {
            musicRoutine = StartCoroutine(SwitchSceneMusic(scene.name));
        }
    }

    public IEnumerator FadeOutBeforeSceneChange()
    {
        manualFadeOutDone = true;

        if (audioSource == null)
            yield break;

        if (!audioSource.isPlaying)
            yield break;

        yield return StartCoroutine(FadeVolume(audioSource.volume, 0f, fadeOutDuration));
    }

    private void PlayMusicForSceneInstant(string sceneName)
    {
        SceneMusic music = GetMusicForScene(sceneName);

        if (music == null || music.musicClip == null)
            return;

        audioSource.clip = music.musicClip;
        audioSource.volume = music.volume;
        audioSource.pitch = currentMusicPitch;
        audioSource.loop = true;
        audioSource.Play();
    }

    private IEnumerator SwitchSceneMusic(string sceneName)
    {
        SceneMusic music = GetMusicForScene(sceneName);

        if (music == null || music.musicClip == null)
        {
            if (audioSource.isPlaying)
            {
                yield return StartCoroutine(FadeVolume(audioSource.volume, 0f, fadeOutDuration));
                audioSource.Stop();
                audioSource.clip = null;
            }

            yield break;
        }

        if (audioSource.clip == music.musicClip)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            audioSource.pitch = currentMusicPitch;

            yield return StartCoroutine(FadeVolume(audioSource.volume, music.volume, fadeInDuration));
            yield break;
        }

        if (audioSource.isPlaying)
            yield return StartCoroutine(FadeVolume(audioSource.volume, 0f, fadeOutDuration));

        audioSource.Stop();
        audioSource.clip = music.musicClip;
        audioSource.volume = 0f;
        audioSource.pitch = currentMusicPitch;
        audioSource.loop = true;
        audioSource.Play();

        yield return StartCoroutine(FadeVolume(0f, music.volume, fadeInDuration));
    }

    private IEnumerator FadeInSceneMusic(string sceneName)
    {
        SceneMusic music = GetMusicForScene(sceneName);

        if (music == null || music.musicClip == null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            yield break;
        }

        audioSource.Stop();
        audioSource.clip = music.musicClip;
        audioSource.volume = 0f;
        audioSource.pitch = currentMusicPitch;
        audioSource.loop = true;
        audioSource.Play();

        yield return StartCoroutine(FadeVolume(0f, music.volume, fadeInDuration));
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (audioSource == null)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            audioSource.volume = Mathf.Lerp(from, to, t);

            yield return null;
        }

        audioSource.volume = to;
    }

    private SceneMusic GetMusicForScene(string sceneName)
    {
        if (sceneMusicList == null)
            return null;

        foreach (SceneMusic music in sceneMusicList)
        {
            if (music == null) continue;

            if (music.sceneName == sceneName)
                return music;
        }

        return null;
    }

    public void SetMusicPitch(float pitch)
    {
        currentMusicPitch = pitch;

        if (audioSource != null)
            audioSource.pitch = currentMusicPitch;
    }

    public void ResetMusicPitch()
    {
        SetMusicPitch(normalPitch);
    }

    public float GetCurrentMusicPitch()
    {
        return currentMusicPitch;
    }
}