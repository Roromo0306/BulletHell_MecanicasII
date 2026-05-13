using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoEndLoadScene : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Scene")]
    public string sceneToLoad = "LV1";

    [Header("Options")]
    public bool useSceneTransitionManager = true;

    private bool loading;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (loading) return;

        loading = true;
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("VideoEndLoadScene: no hay escena asignada.");
            return;
        }

        if (useSceneTransitionManager && SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(sceneToLoad);
        else
            SceneManager.LoadScene(sceneToLoad);
    }
}