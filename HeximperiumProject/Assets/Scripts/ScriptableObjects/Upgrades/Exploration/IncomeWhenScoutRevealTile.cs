using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Exploration/IncomeWhenScoutRevealTile")]
public class IncomeWhenScoutRevealTile : UpgradeEffect
{
    [SerializeField] private List<ResourceToIntMap> _income = new List<ResourceToIntMap>();

    public override void ApplyEffect()
    {
        ExplorationManager.Instance.OnScoutSpawned += ScoutSpawned;

        foreach (Scout scout in ExplorationManager.Instance.Scouts)
        {
            ScoutSpawned(scout);
        }
    }

    private void ScoutSpawned(Scout scout)
    {
        scout.OnScoutRevealingTile += TileRevealed;
    }

    public void TileRevealed(Tile revealedTile)
    {
        ResourcesManager.Instance.UpdateResource(_income, Transaction.Gain, revealedTile);
    }

    public override string GetEffectDescription()
    {
        return $"Gains {_income.IncomeToString()} when a scout<sprite name=\"Scout_Emoji\"> reveals a tile";

    }
}
