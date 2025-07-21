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
    [SerializeField] private GameObject _dustInfraVFX;
    [Header("_________________________________________________________")]
    [Header("UI VFX data")]
    [SerializeField] private Camera _renderTextureCam;
    [SerializeField] private GameObject _endGameConfettiVFX;
    [SerializeField] private float _endGameFirstConfettiTilt = 15f;
    [SerializeField] private GameObject _endGameFireworkVFX;
    [SerializeField] private float _endGameFireworkPosMaxOffset;
    [SerializeField] private int _endGameFireworkQuantity = 3;

    protected override void OnAwake()
    {
        ExplorationManager.Instance.OnScoutSpawned += scout => SpawnUnitVFX(scout.CurrentTile);
        EntertainmentManager.Instance.OnEntertainmentSpawned += ent => SpawnUnitVFX(ent.Tile);

        EntertainmentManager.Instance.OnScoreGained += (tile, value) => PlayResourceVFX(tile, value, _scoreMat);
        ResourcesManager.Instance.OnGoldGained += (tile, value) => PlayResourceVFX(tile, value, _goldMat);
        ResourcesManager.Instance.OnSpecialResourcesGained += (tile, value) => PlayResourceVFX(tile, value, _srMat);
        ResourcesManager.Instance.OnClaimGained += (tile, value) => PlayResourceVFX(tile, value, _claimMat);

        GameManager.Instance.OnGameFinished += EndGameVFX;

        ExploitationManager.Instance.OnInfraBuilded += tile => DustVFX(tile);
        ExploitationManager.Instance.OnInfraDestroyed += tile => DustVFX(tile);
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

    private void DustVFX(Tile tile)
    {
        Instantiate(_dustInfraVFX, _dustInfraVFX.transform.position + tile.transform.position, _dustInfraVFX.transform.rotation);
    }

    #region UI VFX
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            
        }
    }

    private void EndGameVFX()
    {
        Instantiate(_endGameConfettiVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndConfetti1), _endGameConfettiVFX.transform.rotation * Quaternion.Euler(0f, 0f, _endGameFirstConfettiTilt));
        Instantiate(_endGameConfettiVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndConfetti2), _endGameConfettiVFX.transform.rotation);

        for (int i = 0; i < _endGameFireworkQuantity; i++)
        {
            Instantiate(_endGameFireworkVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndFirework1, _endGameFireworkPosMaxOffset), _endGameFireworkVFX.transform.rotation);
            Instantiate(_endGameFireworkVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndFirework2, _endGameFireworkPosMaxOffset), _endGameFireworkVFX.transform.rotation);
        }
    }

    private Vector3 PlaceAtViewport(RectTransform uiElement, float maxOffset = 0f)
    {
        // convert UI position to viewport
        Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(null, uiElement.position);
        Vector2 targetPos = new Vector2(screenPt.x / Screen.width, screenPt.y / Screen.height);
        Vector3 vp = new Vector3(targetPos.x, targetPos.y, 1f);

        // base world‐space position
        Vector3 worldPos = _renderTextureCam.ViewportToWorldPoint(vp);

        // random offset between 0 and maxOffset on X/Y axes
        float dx = Random.Range(0f, maxOffset);
        float dy = Random.Range(0f, maxOffset);

        return worldPos + new Vector3(dx, dy, 0f);
    }
    #endregion
}
