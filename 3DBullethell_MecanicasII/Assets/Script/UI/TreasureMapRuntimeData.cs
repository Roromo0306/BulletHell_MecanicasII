public static class TreasureMapRuntimeData
{
    public static bool HasData { get; private set; }
    public static int MapStage { get; private set; }
    public static string NextBattleSceneName { get; private set; }

    public static void SetMapData(int mapStage, string nextBattleSceneName)
    {
        HasData = true;
        MapStage = mapStage;
        NextBattleSceneName = nextBattleSceneName;
    }

    public static void Clear()
    {
        HasData = false;
        MapStage = 0;
        NextBattleSceneName = "";
    }
}