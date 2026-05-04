using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneSceneLoader : MonoBehaviour
{
    [Header("Scene")]
    public string levelSceneName = "LV1";

    [Header("Timeline Optional")]
    public PlayableDirector playableDirector;

    [Header("Fallback Timer")]
    public bool useTimerIfNoDirector = true;
    public float cutsceneDuration = 5f;

    [Header("Skip")]
    public bool allowSkip = true;
    public KeyCode skipKeyboard = KeyCode.Space;
    public KeyCode skipGamepad = KeyCode.JoystickButton0;

    private bool loading;

    private void Start()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped += OnTimelineStopped;
            playableDirector.Play();
        }
        else if (useTimerIfNoDirector)
        {
            StartCoroutine(TimerRoutine());
        }
    }

    private void Update()
    {
        if (!allowSkip) return;
        if (loading) return;

        if (Input.GetKeyDown(skipKeyboard) || Input.GetKeyDown(skipGamepad))
        {
            LoadLevel();
        }
    }

    private IEnumerator TimerRoutine()
    {
        yield return new WaitForSeconds(cutsceneDuration);

        LoadLevel();
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        LoadLevel();
    }

    private void LoadLevel()
    {
        if (loading) return;

        loading = true;
        SceneManager.LoadScene(levelSceneName);
    }

    private void OnDestroy()
    {
        if (playableDirector != null)
            playableDirector.stopped -= OnTimelineStopped;
    }
}