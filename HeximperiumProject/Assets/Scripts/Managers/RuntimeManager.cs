using System.Collections.Generic;

public static class RuntimeManager
{
    private static readonly HashSet<TileData> tileDataInstances = new HashSet<TileData>();
    private static readonly HashSet<EntertainmentData> entertainmentDataInstances = new HashSet<EntertainmentData>();

    public static void RegisterDataInstance(TileData instance)
    {
        tileDataInstances.Add(instance);
    }

    public static void RegisterDataInstance(EntertainmentData instance)
    {
        entertainmentDataInstances.Add(instance);
    }

    public static void UnregisterDataInstance(TileData instance)
    {
        tileDataInstances.Remove(instance);
    }

    public static void UnregisterDataInstance(EntertainmentData instance)
    {
        entertainmentDataInstances.Remove(instance);
    }

    public static void ResetAllDataInstances()
    {
        foreach (TileData instance in tileDataInstances)
            instance.ResetRuntimeValues();
        foreach (EntertainmentData instance in entertainmentDataInstances)
            instance.ResetRuntimeSpecialEffects();
    }
}
