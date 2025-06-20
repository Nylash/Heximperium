using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostScoutOnSpawn")]
public class BoostScoutOnSpawn : SpecialBehaviour
{
    [SerializeField] private int _boostSpeed;
    [SerializeField] private int _boostLifespan;
    [SerializeField] private int _boostRevealRadius;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.OnScoutSpawned.RemoveListener(behaviourTile.ListenerOnScoutSpawned);
        ExplorationManager.Instance.OnScoutSpawned.AddListener(behaviourTile.ListenerOnScoutSpawned);
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.OnScoutSpawned.RemoveListener(behaviourTile.ListenerOnScoutSpawned);
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }

    public void CheckScoutSpawned(Tile behaviourTile, Scout scout) 
    {
        if (scout.CurrentTile == behaviourTile) 
        {
            scout.Speed += _boostSpeed;
            scout.Lifespan += _boostLifespan;
            scout.RevealRadius += _boostRevealRadius;
        }
    }
}
