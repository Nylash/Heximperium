using UnityEngine;

public class InteractionButton : MonoBehaviour
{
    #region CONSTANTS
    private const string PATH_SPRITE_INTERACTION = "InteractionButtons/";
    #endregion

    #region CONFIGURATION
    [SerializeField] private GameObject _popUpClaim;
    [SerializeField] private GameObject _popUpScout;
    [SerializeField] private GameObject _popUpDestroy;
    [SerializeField] private GameObject _popUpEntertainment;
    [SerializeField] private GameObject _popUpInfra;
    [SerializeField] private Texture _textureExplo;
    [SerializeField] private Texture _textureExpand;
    [SerializeField] private Texture _textureExploit;
    [SerializeField] private Texture _textureEntertain;
    [SerializeField] private GameObject _highlightedInteractionPrefab;
    #endregion

    #region VARIABLES
    private SpriteRenderer _renderer;
    private Interaction _interaction;
    private Tile _associatedTile;
    private InfrastructureData _infraData;
    private ScoutData _scoutData;
    private EntertainmentData _entertainData;
    private Animator _animator;
    private GameObject _highlightedClone;
    #endregion

    #region ACCESSORS
    public Interaction Interaction { get => _interaction;}
    public Tile AssociatedTile { get => _associatedTile;}
    public InfrastructureData InfrastructureData { get => _infraData;}
    public EntertainmentData EntertainData { get => _entertainData;}
    public ScoutData ScoutData { get => _scoutData;}
    #endregion

    private void Awake()
    {
        switch (GameManager.Instance.CurrentPhase)
        {
            case Phase.Explore:
                GetComponent<MeshRenderer>().material.mainTexture = _textureExplo;
                break;
            case Phase.Expand:
                GetComponent<MeshRenderer>().material.mainTexture = _textureExpand;
                break;
            case Phase.Exploit:
                GetComponent<MeshRenderer>().material.mainTexture = _textureExploit;
                break;
            case Phase.Entertain:
                GetComponent<MeshRenderer>().material.mainTexture = _textureEntertain;
                break;
        }
        
    }

    public void Initialize(Tile associatedTile, Interaction action, InfrastructureData infraData = null, EntertainmentData entrainData = null)
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _associatedTile = associatedTile;
        _interaction = action;

        switch (action)
        {
            case Interaction.Claim:
                InitializeClaim();
                break;
            case Interaction.Scout:
                InitializeScout();
                break;
            case Interaction.Infrastructure:
                InitializeInfrastructure(infraData);
                break;
            case Interaction.Destroy:
                InitializeDestroy();
                break;
            case Interaction.Entertainment:
                break;
        }

        _animator = GetComponentInChildren<Animator>();
    }

    private void InitializeClaim()
    {
        if (!ResourcesManager.Instance.CanAffordClaim(_associatedTile.TileData.ClaimCost))
            _renderer.color = UIManager.Instance.ColorCantAfford;
        LoadSprite(Interaction.Claim.ToString());
    }

    private void InitializeScout()
    {
        _scoutData = ExplorationManager.Instance.ScoutData;
        if (ExplorationManager.Instance.CurrentScoutsCount >= ExplorationManager.Instance.ScoutsLimit)
            _renderer.color = UIManager.Instance.ColorCantAfford;
        LoadSprite(Interaction.Scout.ToString());
    }

    private void InitializeInfrastructure(InfrastructureData infraData)
    {
        _infraData = infraData;
        if (!ResourcesManager.Instance.CanAfford(_infraData.Costs) || !ExploitationManager.Instance.IsInfraAvailable(infraData))
            _renderer.color = UIManager.Instance.ColorCantAfford;
        LoadSprite(infraData.name);
    }

    private void InitializeDestroy()
    {
        LoadSprite(Interaction.Destroy.ToString());
    }

    private void LoadSprite(string spriteName)
    {
        Sprite sprite = Resources.Load<Sprite>(PATH_SPRITE_INTERACTION + spriteName);
        if (sprite == null)
        {
            Debug.LogError("Sprite not found at path: " + PATH_SPRITE_INTERACTION + spriteName);
            return;
        }
        _renderer.sprite = sprite;
    }

    public GameObject GetPopUpPrefab()
    {
        switch (_interaction)
        {
            case Interaction.Claim:
                return _popUpClaim;
            case Interaction.Scout:
                return _popUpScout;
            case Interaction.Infrastructure:
                return _popUpInfra;
            case Interaction.Destroy:
                return _popUpDestroy;
            case Interaction.Entertainment:
                return _popUpEntertainment;
            default:
                Debug.LogError("This interaction has no popup prefab assigned " + _interaction);
                return null;
        }
    }

    public void ShrinkAnimation(bool shrink)
    {
        if (_animator.GetBool("Shrink") != shrink)
            _animator.SetBool("Shrink", shrink);
    }

    public void FadeAnimation(bool fade)
    {
        _animator.SetBool("Fade", fade);
    }

    public void CreateHighlightedClone()
    {
        _highlightedClone = Instantiate(_highlightedInteractionPrefab, _associatedTile.transform.position + new Vector3(0, 0.2f, 0), Quaternion.identity);
        _highlightedClone.GetComponent<MeshRenderer>().material.mainTexture = GetComponent<MeshRenderer>().material.mainTexture;
        _highlightedClone.GetComponentInChildren<SpriteRenderer>().color = _renderer.color;
        _highlightedClone.GetComponentInChildren<SpriteRenderer>().sprite = _renderer.sprite;
    }

    public void DestroyHighlightedClone()
    {
        _highlightedClone.GetComponent<Animator>().SetTrigger("Destroy");
    }
}