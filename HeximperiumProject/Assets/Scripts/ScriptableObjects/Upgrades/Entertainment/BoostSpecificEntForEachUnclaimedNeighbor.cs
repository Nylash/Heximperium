using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Entertainment/BoostSpecificEntForEachUnclaimedNeighbor")]
public class BoostSpecificEntForEachUnclaimedNeighbor : UpgradeEffect
{
    [SerializeField] private int _boostAmount;
    [SerializeField] private EntertainmentData _specificEntBoosted;

    public override void ApplyEffect()
    {
        foreach (Entertainment ent in EntertainmentManager.Instance.Entertainments)
            CheckEntertainment(ent);

        EntertainmentManager.Instance.OnEntertainmentSpawned += CheckEntertainment;
    }

    private void CheckEntertainment(Entertainment ent)
    {
        if (ent.Data == _specificEntBoosted)
        {
            int unclaimedNeighbors = 0;
            foreach (Tile neighbor in ent.Tile.Neighbors)
            {
                if (!neighbor)
                    continue;
                if (!neighbor.Claimed)
                    unclaimedNeighbors++;
            }
            if (unclaimedNeighbors > 0)
                ent.UpdatePoints(_boostAmount * unclaimedNeighbors, Transaction.Gain);
        }
    }

    public override string GetEffectDescription()
    {
        return $"{_specificEntBoosted.Type.ToCustomString()} gain +{_boostAmount}<sprite name=\"Point_Emoji\"> for each unrevealed or unclaimed neighbors";
    }
}
