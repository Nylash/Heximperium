using NUnit;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Entertainment/BoostSpecificEntIfEnoughIdenticalNeighbors")]
public class BoostSpecificEntIfEnoughIdenticalNeighbors : UpgradeEffect
{
    [SerializeField] private int _boostAmount;
    [SerializeField] private EntertainmentData _entData;
    [SerializeField] private int _requiredIdenticalNeighbors;

    public override void ApplyEffect()
    {
        foreach (Entertainment ent in EntertainmentManager.Instance.Entertainments)
            CheckEntertainment(ent);

        EntertainmentManager.Instance.OnEntertainmentSpawned += CheckEntertainment;
        EntertainmentManager.Instance.OnEntertainmentRemoved += CheckEntertainmentRemoved;
    }

    private void CheckEntertainment(Entertainment ent)
    {
        if (ent.Data != _entData) 
            return;

        // 1) Check spawned ent (guard against double-boost)
        if (!ent.BoostedByIdenticalNeighbors &&
            CountIdenticalNeighbors(ent) >= _requiredIdenticalNeighbors)
        {
            ent.UpdatePoints(_boostAmount, Transaction.Gain);
            ent.BoostedByIdenticalNeighbors = true;
        }

        // 2) Check neighbors of same type (their counts may have changed)
        foreach (Tile neighborTile in ent.Tile.Neighbors)
        {
            if (!neighborTile)
                continue;
            Entertainment nbr = neighborTile?.Entertainment;
            if (nbr == null || nbr.Data != _entData) continue;

            if (!nbr.BoostedByIdenticalNeighbors &&
                CountIdenticalNeighbors(nbr) >= _requiredIdenticalNeighbors)
            {
                nbr.UpdatePoints(_boostAmount, Transaction.Gain);
                nbr.BoostedByIdenticalNeighbors = true;
            }
        }
    }

    private void CheckEntertainmentRemoved(EntertainmentData data, Tile tileOfRemovedEnt)
    {
        if (data != _entData)
            return;

        // Check neighbors of same type (their counts may have changed)
        foreach (Tile neighborTile in tileOfRemovedEnt.Neighbors)
        {
            if (!neighborTile) 
                continue;
            Entertainment nbr = neighborTile?.Entertainment;
            if (nbr == null || nbr.Data != _entData) continue;

            if (nbr.BoostedByIdenticalNeighbors &&
                CountIdenticalNeighbors(nbr) < _requiredIdenticalNeighbors)
            {
                nbr.UpdatePoints(_boostAmount, Transaction.Spent);
                nbr.BoostedByIdenticalNeighbors = false;
            }
        }
    }

    private int CountIdenticalNeighbors(Entertainment e)
    {
        int count = 0;
        foreach (Tile t in e.Tile.Neighbors)
        {
            if (!t)
                continue;
            if (t?.Entertainment != null && t.Entertainment.Data == _entData)
            {
                count++;
                if (count >= _requiredIdenticalNeighbors) break; // early exit
            }
        }
        return count;
    }

    public override string GetEffectDescription()
    {
        return $"{_entData.Type.ToCustomString()} gain +{_boostAmount}<sprite name=\"Point_Emoji\"> if it has at least {_requiredIdenticalNeighbors} other {_entData.Type.ToCustomString()} adjacent";
    }
}
