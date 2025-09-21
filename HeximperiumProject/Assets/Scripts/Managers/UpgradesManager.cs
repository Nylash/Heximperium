using System.Collections.Generic;
using UnityEngine;

public class UpgradesManager : Singleton<UpgradesManager>
{
    [SerializeField] private List<UpgradeEffect> _upgrades = new List<UpgradeEffect>();

    private HashSet<UpgradeEffect> _appliedUpgrades = new HashSet<UpgradeEffect>();
    private HashSet<UpgradeEffect> _remainingUpgrades = new HashSet<UpgradeEffect>();

    public void UnlockUpgrade(UpgradeEffect upgrade)
    {
        _appliedUpgrades.Add(upgrade);
        _remainingUpgrades.Remove(upgrade);
        upgrade.ApplyEffect();

        PopUpManager.Instance.ResetPopUp(null);
        UIManager.Instance.UpgradesMenu();
    }
}
