using UnityEngine;

public class JuiceManager : Singleton<JuiceManager>
{
    [Header("_________________________________________________________")]
    [Header("VFX data")]
    [SerializeField] private GameObject _resourceVFX;
    [SerializeField] private Material _goldMat;
    [SerializeField] private Material _srMat;
    [SerializeField] private Material _claimMat;
    [SerializeField] private Material _scoreMat;
    [SerializeField] private GameObject _spawnUnitVFX;
    [Header("_________________________________________________________")]
    [Header("UI VFX data")]
    [SerializeField] private Camera _renderTextureCam;
    [SerializeField] private GameObject _protoUIVFX;

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

    #region UI VFX
    private void Update()
    {
        if ( Input.GetKey(KeyCode.J))
        {
            Instantiate(_protoUIVFX, PlaceAtViewport(UIManager.Instance.GoldText.transform as RectTransform), Quaternion.identity);
        }
    }

    private Vector3 PlaceAtViewport(RectTransform uiElement)
    {
        Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(null, uiElement.position);
        Vector2 targetPos = new Vector2(screenPt.x / Screen.width, screenPt.y / Screen.height);
        Vector3 vp = new Vector3(targetPos.x, targetPos.y, 1);
        return _renderTextureCam.ViewportToWorldPoint(vp);
    }
    #endregion
}
