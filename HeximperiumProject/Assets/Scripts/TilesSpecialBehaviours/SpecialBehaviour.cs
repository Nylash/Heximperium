using UnityEngine;

public abstract class SpecialBehaviour : ScriptableObject
{
    protected Tile _tile;

    public Tile Tile { get => _tile; set => _tile = value; }

    //Realize special behaviour as the tile with the behaviour
    public abstract void RealizeSpecialBehaviour();

    //Realize special behaviour toward a specific tile (e.g. when this specific tile change)
    public abstract void RealizeSpecialBehaviour(Tile specificTile);
    public abstract void RollbackSpecialBehaviour();
}
