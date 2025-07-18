using UnityEngine;

public class JuiceManager : Singleton<JuiceManager>
{
    [SerializeField] private GameObject _resourceVFX;
    [SerializeField] private Material _goldMat;
    [SerializeField] private Material _srMat;
    [SerializeField] private Material _claimMat;
    [SerializeField] private Material _scoreMat;
    [SerializeField] private GameObject _spawnUnitVFX;

    protected override void OnAwake()
    {
        ExplorationManager.Instance.OnScoutSpawned += scout => SpawnUnitVFX(scout.CurrentTile);
        EntertainmentManager.Instance.OnEntertainmentSpawned += ent => SpawnUnitVFX(ent.Tile);

        EntertainmentManager.Instance.OnScoreGained += (tile, value) => PlayResourceVFX(tile, value, _scoreMat);
        ResourcesManager.Instance.OnGoldGained += (tile, value) => PlayResourceVFX(tile, value, _goldMat);
        ResourcesManager.Instance.OnSpecialResourcesGained += (tile, value) => PlayResourceVFX(tile, value, _srMat);
        ResourcesManager.Instance.OnClaimGained += (tile, value) => PlayResourceVFX(tile, value, _claimMat);
    }

    private void SpawnUnitVFX(Tile tile)
    {
        Instantiate(_spawnUnitVFX, _spawnUnitVFX.transform.position + tile.transform.position, _spawnUnitVFX.transform.rotation);
    }

    private void PlayResourceVFX(Tile tile, int value, Material mat)
    {
        GameObject vfx = GameObject.Instantiate(_resourceVFX, _resourceVFX.transform.position + tile.transform.position, _resourceVFX.transform.rotation);

        ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();

        particleSystem.emission.SetBurst(0, new ParticleSystem.Burst(0, value));

        vfx.GetComponent<ParticleSystemRenderer>().material = mat;

        particleSystem.Play();
    }
}
