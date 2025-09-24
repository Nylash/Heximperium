using System.Collections.Generic;

public static class RuntimeManager
{
    private static readonly HashSet<TileData> tileDataInstances = new HashSet<TileData>();

    public static void RegisterTileDataInstance(TileData instance)
    {
        tileDataInstances.Add(instance);
    }

    public static void UnregisterTileDataInstance(TileData instance)
    {
        tileDataInstances.Remove(instance);
    }

    public static void ResetAllTileDataInstances()
    {
        foreach (var instance in tileDataInstances)
            instance.ResetRuntimeSpecialBehaviour();
    }
}
