using UnityEngine;

public class UI_InteractionButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    private Interaction _interaction;
    private Tile _associatedTile;
    private TileData _tileData;
    private UnitData _unitData;

    public Interaction Interaction { get => _interaction;}
    public Tile AssociatedTile { get => _associatedTile;}
    public TileData TileData { get => _tileData;}
    public UnitData UnitData { get => _unitData;}

    public void Initialize(Tile associatedTile, Interaction action)
    {
        switch (action)
        {
            case Interaction.Claim:
                _associatedTile = associatedTile;
                _interaction = Interaction.Claim;
                if (!ResourcesManager.Instance.CanAfford(Resource.Claim ,associatedTile.TileData.ClaimCost))
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
                ScoutData scoutData = (ScoutData)_unitData;
                if (!ResourcesManager.Instance.CanAfford(scoutData.Costs)
                    && ExplorationManager.Instance.FreeScouts == 0)
                    _renderer.color = UIManager.Instance.ColorCantAfford;
                _renderer.sprite = Resources.Load<Sprite>("InteractionButtons/" + action.ToString());
                break;
        }
    }
}

public enum Interaction
{
    Claim, Town, Scout
}
