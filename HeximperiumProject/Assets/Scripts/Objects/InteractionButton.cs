using UnityEngine;

public class InteractionButton : MonoBehaviour
{
    #region CONSTANTS
    private const string PATH_SPRITES_INTERACTION = "Sprites/InteractionButtons/";
    #endregion

    #region CONFIGURATION
    [SerializeField]
    private SpriteRenderer _iconRenderer;
    [SerializeField]
    private SpriteRenderer _phaseColorRenderer;
    #endregion

    #region VARIABLES

    private Interaction _interaction;
    private Tile _associatedTile;
    private InfrastructureData _infraData;
    private ScoutData _scoutData;
    private EntertainmentData _entertainData;
    private Animator _animator;
    private Scout _associatedScout;
    #endregion

    #region ACCESSORS
    public Interaction Interaction { get => _interaction;}
    public Tile AssociatedTile { get => _associatedTile;}
    public InfrastructureData InfrastructureData { get => _infraData;}
    public EntertainmentData EntertainData { get => _entertainData;}
    public ScoutData ScoutData { get => _scoutData;}
    public Scout AssociatedScout { get => _associatedScout; }
    #endregion

    public void Initialize(Tile associatedTile, Interaction action, InfrastructureData infraData = null, EntertainmentData entertainData = null, Scout scout = null)
    {
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
                InitializeEntertainment(entertainData);
                break;
            case Interaction.RedirectScout:
                InitializeRedirectScout(scout);
                break;
            case Interaction.RevealAnywhere:
                InitializeRevealAnywhere();
                break;
        }

        _animator = GetComponentInChildren<Animator>();
    }

    private void InitializeClaim()
    {
        if (!ResourcesManager.Instance.CanAffordClaim(_associatedTile.TileData.ClaimCost))
            _iconRenderer.color = UIManager.Instance.ColorCantAfford;
        _phaseColorRenderer.color = UIManager.Instance.ColorExpand;
        LoadSprite(Interaction.Claim.ToString());
    }

    private void InitializeScout()
    {
        _scoutData = ExplorationManager.Instance.ScoutData;
        if (ExplorationManager.Instance.CurrentScoutsCount >= ExplorationManager.Instance.ScoutsLimit)
            _iconRenderer.color = UIManager.Instance.ColorCantAfford;
        _phaseColorRenderer.color = UIManager.Instance.ColorExplo;
        LoadSprite(Interaction.Scout.ToString());
    }

    private void InitializeInfrastructure(InfrastructureData infraData)
    {
        _infraData = infraData;
        if (!ResourcesManager.Instance.CanAfford(_infraData.Costs) 
            || !ResourcesManager.Instance.CanAffordClaim(_infraData.ClaimCost)
            || !ExploitationManager.Instance.IsInfraAvailable(infraData))
            _iconRenderer.color = UIManager.Instance.ColorCantAfford;
        switch (infraData.AssociatedPhase)
        {
            case Phase.Explore:
                _phaseColorRenderer.color = UIManager.Instance.ColorExplo;
                break;
            case Phase.Expand:
                _phaseColorRenderer.color = UIManager.Instance.ColorExpand;
                break;
            case Phase.Exploit:
                _phaseColorRenderer.color = UIManager.Instance.ColorExploit;
                break;
            case Phase.Entertain:
                _phaseColorRenderer.color = UIManager.Instance.ColorEntertain;
                break;
            default:
                break;
        }
        LoadSprite(infraData.name);
    }

    private void InitializeDestroy()
    {
        _iconRenderer.color = UIManager.Instance.ColorIvory;
        _phaseColorRenderer.color = UIManager.Instance.ColorCantAfford;
        LoadSprite(Interaction.Destroy.ToString());
    }

    private void InitializeEntertainment(EntertainmentData data)
    {
        _entertainData = data;
        if (!ResourcesManager.Instance.CanAffordCarnivalist(_entertainData.GetActualCarnivalistCost(_associatedTile)))
            _iconRenderer.color = UIManager.Instance.ColorCantAfford;
        _phaseColorRenderer.color = UIManager.Instance.ColorEntertain;
        LoadSprite(_entertainData.name);
    }

    private void InitializeRedirectScout(Scout scout)
    {
        _phaseColorRenderer.color = UIManager.Instance.ColorExplo;
        _associatedScout = scout;
        LoadSprite(Interaction.RedirectScout.ToString());
    }

    private void InitializeRevealAnywhere()
    {
        _phaseColorRenderer.color = UIManager.Instance.ColorExplo;
        LoadSprite(Interaction.RevealAnywhere.ToString());
    }

    private void LoadSprite(string spriteName)
    {
        Sprite sprite = Resources.Load<Sprite>(PATH_SPRITES_INTERACTION + spriteName);
        if (sprite == null)
        {
            Debug.LogError("Sprite not found at path: " + PATH_SPRITES_INTERACTION + spriteName);
            return;
        }
        _iconRenderer.sprite = sprite;
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

    public void DestroyInteractionButton()
    {
        _animator.SetTrigger("Death");
    }
}