using UnityEngine;

public class UI_InteractionButton : MonoBehaviour
{
    #region CONSTANTS
    private const string PATH_DATA_INFRA = "Data/Infrastructures/";
    private const string PATH_DATA_UNIT = "Data/Units/";
    private const string PATH_SPRITE_INTERACTION = "InteractionButtons/";
    #endregion

    #region VARIABLES
    private SpriteRenderer _renderer;
    private Interaction _interaction;
    private Tile _associatedTile;
    private InfrastructureData _infraData;
    private UnitData _unitData;
    #endregion

    #region ACCESSORS
    public Interaction Interaction { get => _interaction;}
    public Tile AssociatedTile { get => _associatedTile;}
    public InfrastructureData InfrastructureData { get => _infraData;}
    public UnitData UnitData { get => _unitData;}
    #endregion

    public void Initialize(Tile associatedTile, Interaction action, InfrastructureData infraData = null, EntertainerData entrainData = null)
    {
        _renderer = GetComponent<SpriteRenderer>();
        _associatedTile = associatedTile;
        _interaction = action;

        switch (action)
        {
            case Interaction.Claim:
                InitializeClaim();
                break;
            case Interaction.Town:
                InitializeTown();
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
            case Interaction.Entertainer:
                InitializeEntertainer(entrainData);
                break;
        }
    }

    private void InitializeClaim()
    {
        if (!ResourcesManager.Instance.CanAffordClaim(_associatedTile.TileData.ClaimCost))
            _renderer.color = UIManager.Instance.ColorCantAfford;
        LoadSprite(Interaction.Claim.ToString());
    }

    private void InitializeTown()
    {
        InfrastructureData townData = Resources.Load<InfrastructureData>(PATH_DATA_INFRA + Interaction.Town.ToString());
        if (!ResourcesManager.Instance.CanAfford(townData.Costs) || ExpansionManager.Instance.AvailableTown == 0)
            _renderer.color = UIManager.Instance.ColorCantAfford;
        LoadSprite(Interaction.Town.ToString());
    }

    private void InitializeScout()
    {
        _unitData = Resources.Load<ScoutData>(PATH_DATA_UNIT + Interaction.Scout.ToString());
        if (!ResourcesManager.Instance.CanAfford(_unitData.Costs) && ExplorationManager.Instance.FreeScouts == 0)
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

    private void InitializeEntertainer(EntertainerData data)
    {
        _unitData = data;
        if (!ResourcesManager.Instance.CanAfford(_unitData.Costs))
            _renderer.color = UIManager.Instance.ColorCantAfford;
        LoadSprite(data.EntertainerType.ToString());
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
}