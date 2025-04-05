using System.Security.Claims;
using UnityEngine;

public class UI_InteractionButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    private Interaction _interaction;
    private Tile _associatedTile;

    public SpriteRenderer Renderer { get => _renderer;}
    public Interaction Interaction { get => _interaction;}
    public Tile AssociatedTile { get => _associatedTile;}

    public void Initialize(Tile associatedTile, Interaction action, bool canAfford)
    {
        switch (action)
        {
            case Interaction.Claim:
                _associatedTile = associatedTile;
                _interaction = Interaction.Claim;
                if (!canAfford)
                    _renderer.color = UIManager.Instance.ColorCantAfford;
                _renderer.sprite = Resources.Load<Sprite>("InteractionButtons/" + action.ToString());
                break;
        }
    }
}

public enum Interaction
{
    Claim, 
}
