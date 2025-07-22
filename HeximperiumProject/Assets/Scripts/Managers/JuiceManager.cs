using UnityEngine;

public class JuiceManager : Singleton<JuiceManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("VFX data")]
    [SerializeField] private GameObject _resourceVFX;
    [SerializeField] private Material _goldMat;
    [SerializeField] private Material _srMat;
    [SerializeField] private Material _claimMat;
    [SerializeField] private Material _scoreMat;
    [SerializeField] private Color _gainGoldScoreColor;
    [SerializeField] private Color _gainClaimColor;
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
    [SerializeField] private GameObject _resourceVFXforUI;
    #endregion

    protected override void OnAwake()
    {
        ExplorationManager.Instance.OnScoutSpawned += scout => SpawnUnitVFX(scout.CurrentTile);
        EntertainmentManager.Instance.OnEntertainmentSpawned += ent => SpawnUnitVFX(ent.Tile);

        EntertainmentManager.Instance.OnScoreGained += (tile, value) => PlayResourceVFX(tile, value, _scoreMat, _gainGoldScoreColor);
        EntertainmentManager.Instance.OnScoreLost += (tile, value) => PlayResourceVFX(tile, value, _scoreMat, UIManager.Instance.ColorCantAfford);

        ResourcesManager.Instance.OnGoldGained += (tile, value) => ResourceGain(tile, value, Resource.Gold);
        ResourcesManager.Instance.OnGoldSpent += (value) => PlayUIResourceVFX(value, _goldMat, UIManager.Instance.VfxAnchorGold, UIManager.Instance.ColorCantAfford);
        ResourcesManager.Instance.OnSpecialResourcesGained += (tile, value) => ResourceGain(tile, value, Resource.SpecialResources);
        ResourcesManager.Instance.OnSpecialResourcesSpent += (value) => PlayUIResourceVFX(value, _srMat, UIManager.Instance.VfxAnchorSR, UIManager.Instance.ColorCantAfford);
        ResourcesManager.Instance.OnClaimGained += (tile, value) => ResourceGain(tile, value, (Resource)999);//Call with 999 to land on the default case and avoiding doing a method just for Claims
        ResourcesManager.Instance.OnClaimSpent += (value) => PlayUIResourceVFX(value, _claimMat, UIManager.Instance.VfxAnchorClaim, UIManager.Instance.ColorCantAfford);

        GameManager.Instance.OnGameFinished += EndGameVFX;

        //ExploitationManager.Instance.OnInfraBuilded += tile => DustVFX(tile); Not convinced by the visual effect of this
        //ExploitationManager.Instance.OnInfraDestroyed += tile => DustVFX(tile);
    }

    #region GAMEPLAY VFX
    private void ResourceGain(Tile tile, int value, Resource resource)
    {
        switch (resource)
        {
            case Resource.Gold:
                if (tile)
                    PlayResourceVFX(tile, value, _goldMat, _gainGoldScoreColor);
                else
                    PlayUIResourceVFX(value, _goldMat, UIManager.Instance.VfxAnchorGold, _gainGoldScoreColor);
                break;
            case Resource.SpecialResources:
                if (tile)
                    PlayResourceVFX(tile, value, _srMat, Color.white);
                else
                    PlayUIResourceVFX(value, _srMat, UIManager.Instance.VfxAnchorSR, Color.white);
                break;
            default:
                if (tile)
                    PlayResourceVFX(tile, value, _claimMat, _gainClaimColor);
                else
                    PlayUIResourceVFX(value, _claimMat, UIManager.Instance.VfxAnchorClaim, _gainClaimColor);
                break;
        }
    }

    private void SpawnUnitVFX(Tile tile)
    {
        Instantiate(_spawnUnitVFX, _spawnUnitVFX.transform.position + tile.transform.position, _spawnUnitVFX.transform.rotation);
    }

    private void PlayResourceVFX(Tile tile, int value, Material mat, Color color)
    {
        GameObject vfx = GameObject.Instantiate(_resourceVFX, _resourceVFX.transform.position + tile.transform.position, _resourceVFX.transform.rotation);

        ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();

        particleSystem.emission.SetBurst(0, new ParticleSystem.Burst(0, value));

        vfx.GetComponent<ParticleSystemRenderer>().material = mat;

        ParticleSystem.MainModule main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color);

        particleSystem.Play();
    }

    private void DustVFX(Tile tile)
    {
        Instantiate(_dustInfraVFX, _dustInfraVFX.transform.position + tile.transform.position, _dustInfraVFX.transform.rotation);
    }
    #endregion

    #region UI VFX
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

    private void PlayUIResourceVFX(int value, Material mat, RectTransform uiElement, Color color)
    {
        GameObject vfx = GameObject.Instantiate(_resourceVFXforUI, PlaceAtViewport(uiElement), _resourceVFXforUI.transform.rotation);

        ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();

        particleSystem.emission.SetBurst(0, new ParticleSystem.Burst(0, value));

        vfx.GetComponent<ParticleSystemRenderer>().material = mat;

        ParticleSystem.MainModule main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color);

        particleSystem.Play();
    }
    #endregion
}
