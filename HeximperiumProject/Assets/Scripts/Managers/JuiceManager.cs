using UnityEngine;

public class JuiceManager : Singleton<JuiceManager>
{
    [SerializeField] private GameObject _spawnUnitVFX;

    protected override void OnAwake()
    {
        ExplorationManager.Instance.OnScoutSpawned += ctx => SpawnUnitVFX(ctx.CurrentTile);
        EntertainmentManager.Instance.OnEntertainmentSpawned += ctx => SpawnUnitVFX(ctx.Tile);
    }

    private void SpawnUnitVFX(Tile tile)
    {
        Instantiate(_spawnUnitVFX, _spawnUnitVFX.transform.position + tile.transform.position, _spawnUnitVFX.transform.rotation);
    }
}
