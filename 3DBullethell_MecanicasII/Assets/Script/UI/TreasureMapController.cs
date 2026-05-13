using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureMapController : MonoBehaviour
{
    [Header("Map Points / Route Parts")]
    public TreasureMapPoint[] mapPoints;

    [Header("Fallback For Testing")]
    public int fallbackMapStage = 2;
    public string fallbackNextBattleSceneName = "LV2";

    [Header("Timing")]
    public float startDelay = 0.5f;
    public float delayBetweenPops = 0.18f;
    public float delayAfterAllPops = 1.2f;

    private int currentMapStage;
    private string nextBattleSceneName;

    private void Awake()
    {
        ReadRuntimeData();
        PrepareMap();
    }

    private void Start()
    {
        StartCoroutine(MapSequenceRoutine());
    }

    private void ReadRuntimeData()
    {
        if (TreasureMapRuntimeData.HasData)
        {
            currentMapStage = TreasureMapRuntimeData.MapStage;
            nextBattleSceneName = TreasureMapRuntimeData.NextBattleSceneName;
        }
        else
        {
            currentMapStage = fallbackMapStage;
            nextBattleSceneName = fallbackNextBattleSceneName;
        }
    }

    private void PrepareMap()
    {
        if (mapPoints == null) return;

        foreach (TreasureMapPoint point in mapPoints)
        {
            if (point == null) continue;

            if (point.revealStage < currentMapStage)
            {
                point.ShowInstant();
            }
            else
            {
                point.HideInstant();
            }
        }
    }

    private IEnumerator MapSequenceRoutine()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        if (mapPoints != null)
        {
            foreach (TreasureMapPoint point in mapPoints)
            {
                if (point == null) continue;

                if (point.revealStage == currentMapStage)
                {
                    yield return StartCoroutine(point.PlayPop());
                    yield return new WaitForSecondsRealtime(delayBetweenPops);
                }
            }
        }

        yield return new WaitForSecondsRealtime(delayAfterAllPops);

        LoadNextBattle();
    }

    private void LoadNextBattle()
    {
        if (string.IsNullOrEmpty(nextBattleSceneName))
        {
            Debug.LogWarning("TreasureMapController: falta Next Battle Scene Name.");
            return;
        }

        TreasureMapRuntimeData.Clear();
        Time.timeScale = 1f;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(nextBattleSceneName);
        else
            SceneManager.LoadScene(nextBattleSceneName);
    }
}