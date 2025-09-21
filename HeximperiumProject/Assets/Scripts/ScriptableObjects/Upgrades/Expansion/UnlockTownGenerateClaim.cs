using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UnlockTownGenerateClaim")]
public class UnlockTownGenerateClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData infraData && infraData.IsTown)
            {
                tile.ClaimIncome += 1;
            }
        }
        ExploitationManager.Instance.OnInfraBuilded += (Tile tile) =>
        {
            if (tile.TileData == ExpansionManager.Instance.NewTownData)
            {
                tile.ClaimIncome += 1;
            }
        };
    }

    public override string GetEffectDescription()
    {
        return "Each Town generates +1<sprite name=\"Claim_Emoji\"> per turn";
    }
}
