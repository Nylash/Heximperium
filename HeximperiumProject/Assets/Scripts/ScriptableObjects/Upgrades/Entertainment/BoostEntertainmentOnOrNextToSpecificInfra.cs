using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Entertainment/BoostEntertainmentOnOrNextToSpecificInfra")]
public class BoostEntertainmentOnOrNextToSpecificInfra : UpgradeEffect
{
    [SerializeField] private int _boostAmount = 1;
    [SerializeField] private List<InfrastructureData> _specificInfraBoostingEntertainment = new List<InfrastructureData>();

    public override void ApplyEffect()
    {
        foreach (Entertainment ent in EntertainmentManager.Instance.Entertainments)
        {
            if (ent.Tile.TileData is InfrastructureData infraData && _specificInfraBoostingEntertainment.Contains(infraData))
            {
                ent.UpdatePoints(_boostAmount, Transaction.Gain);
                continue;
            }
            if (ent.Tile.Neighbors.Any(t => t.TileData is InfrastructureData neighborInfraData && _specificInfraBoostingEntertainment.Contains(neighborInfraData)))
            {
                ent.UpdatePoints(_boostAmount, Transaction.Gain);
            }
        }

        EntertainmentManager.Instance.OnEntertainmentSpawned += CheckEntertainment;
    }

    private void CheckEntertainment(Entertainment ent)
    {
        if (ent.Tile.TileData is InfrastructureData infraData && _specificInfraBoostingEntertainment.Contains(infraData))
        {
            ent.UpdatePoints(_boostAmount, Transaction.Gain);
            return;
        }
        if (ent.Tile.Neighbors.Any(t => t.TileData is InfrastructureData neighborInfraData && _specificInfraBoostingEntertainment.Contains(neighborInfraData)))
        {
            ent.UpdatePoints(_boostAmount, Transaction.Gain);
        }
    }

    public override string GetEffectDescription()
    {
        return $"Entertainments placed on or next to {_specificInfraBoostingEntertainment.ToCustomString()} gain +{_boostAmount}<sprite name=\"Point_Emoji\">";
    }
}
