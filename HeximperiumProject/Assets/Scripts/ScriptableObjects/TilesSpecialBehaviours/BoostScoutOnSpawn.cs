using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostScoutOnSpawn")]
public class BoostScoutOnSpawn : SpecialBehaviour
{
    [SerializeField] private int _boostSpeed;
    [SerializeField] private int _boostLifespan;
    [SerializeField] private int _boostRevealRadius;

    public int BoostSpeed { get => _boostSpeed; }
    public int BoostLifespan { get => _boostLifespan; }
    public int BoostRevealRadius { get => _boostRevealRadius; }

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.OnScoutSpawned -= behaviourTile.ListenerOnScoutSpawned_BoostScoutOnSpawn;
        ExplorationManager.Instance.OnScoutSpawned += behaviourTile.ListenerOnScoutSpawned_BoostScoutOnSpawn;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.OnScoutSpawned -= behaviourTile.ListenerOnScoutSpawned_BoostScoutOnSpawn;
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

    public override string GetBehaviourDescription()
    {
        List<string> parts = new();

        if (_boostSpeed > 0)
            parts.Add($"speed by {_boostSpeed}");
        if (_boostLifespan > 0)
            parts.Add($"lifespan by {_boostLifespan}");
        if (_boostRevealRadius > 0)
            parts.Add($"reveal radius by {_boostRevealRadius}");

        if (parts.Count == 0)
            return string.Empty;

        string joined = parts.Count switch
        {
            1 => parts[0],
            2 => $"{parts[0]} and {parts[1]}",
            _ => $"{string.Join(", ", parts.Take(parts.Count - 1))} and {parts.Last()}"
        };

        return $"Boosts the scout's {joined} when spawned on this tile";
    }
}
