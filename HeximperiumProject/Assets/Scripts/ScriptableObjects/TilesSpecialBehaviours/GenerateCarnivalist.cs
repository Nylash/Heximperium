using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/GenerateCarnivalist")]
public class GenerateCarnivalist : SpecialBehaviour
{
    [SerializeField] private int _carnivalistQuantity;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ResourcesManager.Instance.UpdateCarnivalist(_carnivalistQuantity, Transaction.Gain, behaviourTile);
        ResourcesManager.Instance.UpdateCarnivalistSource(behaviourTile.TileData, _carnivalistQuantity, Transaction.Gain);
        behaviourTile.UpdateCarnivalists(_carnivalistQuantity);
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ResourcesManager.Instance.UpdateCarnivalist(_carnivalistQuantity, Transaction.Spent, behaviourTile);
        ResourcesManager.Instance.UpdateCarnivalistSource(behaviourTile.TileData, _carnivalistQuantity, Transaction.Spent);
        behaviourTile.UpdateCarnivalists(-_carnivalistQuantity);
    }
    
    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }

    public override string GetBehaviourDescription()
    {
        return $"Recruits {_carnivalistQuantity}<sprite name=\"Carnivalist_Emoji\">";
    }
}
