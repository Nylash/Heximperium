using UnityEngine;

public class UI_InteractionButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    private Interaction _interaction;
    private Tile _associatedTile;
    private InfrastructureData _infraData;
    private UnitData _unitData;

    public Interaction Interaction { get => _interaction;}
    public Tile AssociatedTile { get => _associatedTile;}
    public InfrastructureData InfrastructureData { get => _infraData;}
    public UnitData UnitData { get => _unitData;}

    public void Initialize(Tile associatedTile, Interaction action, InfrastructureData infraData = null)
    {
        switch (action)
        {
            case Interaction.Claim:
                _associatedTile = associatedTile;
                _interaction = Interaction.Claim;
                if (!ResourcesManager.Instance.CanAffordClaim(associatedTile.TileData.ClaimCost))
                    _renderer.color = UIManager.Instance.ColorCantAfford;
                _renderer.sprite = Resources.Load<Sprite>("InteractionButtons/" + action.ToString());
                break;
            case Interaction.Town:
                _associatedTile = associatedTile;
                _interaction = Interaction.Town;
                if (!ResourcesManager.Instance.CanAfford(Resources.Load<InfrastructureData>("Data/Infrastructures/" + action.ToString()).Costs) 
                    || ExpansionManager.Instance.AvailableTown == 0)
                    _renderer.color = UIManager.Instance.ColorCantAfford;
                _renderer.sprite = Resources.Load<Sprite>("InteractionButtons/" + action.ToString());
                break;
            case Interaction.Scout:
                _associatedTile = associatedTile;
                _interaction = Interaction.Scout;
                _unitData = Resources.Load<ScoutData>("Data/Units/" + action.ToString());
                if (!ResourcesManager.Instance.CanAfford(_unitData.Costs)
                    && ExplorationManager.Instance.FreeScouts == 0)
                    _renderer.color = UIManager.Instance.ColorCantAfford;
                _renderer.sprite = Resources.Load<Sprite>("InteractionButtons/" + action.ToString());
                break;
            case Interaction.Infrastructure:
                _associatedTile = associatedTile;
                _interaction = Interaction.Infrastructure;
                _infraData = infraData;
                if(!ResourcesManager.Instance.CanAfford(_infraData.Costs) || !ExploitationManager.Instance.IsInfraAvailable(infraData))
                    _renderer.color = UIManager.Instance.ColorCantAfford;
                _renderer.sprite = Resources.Load<Sprite>("InteractionButtons/" + infraData.name);
                break;
            case Interaction.Destroy:
                _associatedTile = associatedTile;
                _interaction = Interaction.Destroy;
                _renderer.sprite = Resources.Load<Sprite>("InteractionButtons/" + action.ToString());
                break;
        }
    }
}

public enum Interaction
{
    Claim, Town, Scout, Infrastructure, Destroy
}
