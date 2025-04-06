using UnityEngine;

public class UI_InteractionButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    private Interaction _interaction;
    private Tile _associatedTile;

    public Interaction Interaction { get => _interaction;}
    public Tile AssociatedTile { get => _associatedTile;}

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
        }
    }
}

public enum Interaction
{
    Claim, Town,
}
