using System.Collections.Generic;
using UnityEngine;

public class UpgradesManager : Singleton<UpgradesManager>
{
    [SerializeField] private List<int> _turnsForUpgradesChoice = new List<int>();
    [SerializeField] private List<UpgradeEffect> _exploUpgrades = new List<UpgradeEffect>();
    [SerializeField] private List<UpgradeEffect> _expandUpgrades = new List<UpgradeEffect>();
    [SerializeField] private List<UpgradeEffect> _exploitUpgrades = new List<UpgradeEffect>();
    [SerializeField] private List<UpgradeEffect> _entertainUpgrades = new List<UpgradeEffect>();

    private List<UpgradeEffect> _appliedUpgrades = new List<UpgradeEffect>();
    private List<UpgradeEffect> _remainingExploUpgrades = new List<UpgradeEffect>();
    private List<UpgradeEffect> _remainingExpandUpgrades = new List<UpgradeEffect>();
    private List<UpgradeEffect> _remainingExploitpgrades = new List<UpgradeEffect>();
    private List<UpgradeEffect> _remainingEntertainUpgrades = new List<UpgradeEffect>();
    private UpgradeEffect _currentExploUpgrade;
    private UpgradeEffect _currentExpandUpgrade;
    private UpgradeEffect _currentExploitUpgrade;
    private UpgradeEffect _currentEntertainUpgrade;

    protected override void OnAwake()
    {
        GameManager.Instance.OnNewTurn += CheckNewTurnValue;
    }

    private void Start()
    {
        _remainingExploUpgrades.AddRange(_exploUpgrades);
        _remainingExpandUpgrades.AddRange(_expandUpgrades);
        _remainingExploitpgrades.AddRange(_exploitUpgrades);
        _remainingEntertainUpgrades.AddRange(_entertainUpgrades);

        StartUpgradesChoice();
    }

    private void CheckNewTurnValue(int newTurn)
    {
        if (_turnsForUpgradesChoice.Contains(newTurn))
            StartUpgradesChoice();
    }

    private void UnlockUpgrade(UpgradeEffect upgrade)
    {
        _appliedUpgrades.Add(upgrade);
        upgrade.ApplyEffect();

        PopUpManager.Instance.ResetPopUp(null);
        UIManager.Instance.UpgradesChoiceMenu();

        UIManager.Instance.FillUpgradesList(_appliedUpgrades.Count, _appliedUpgrades[_appliedUpgrades.Count - 1]);
    }

    private void StartUpgradesChoice()
    {
        _currentExploUpgrade = _remainingExploUpgrades[Random.Range(0, _remainingExploUpgrades.Count)];
        UIManager.Instance.ExploChoiceTitle.text = _currentExploUpgrade.EffectName;
        UIManager.Instance.ExploChoiceDetail.text = _currentExploUpgrade.GetEffectDescription();
        _currentExpandUpgrade = _remainingExpandUpgrades[Random.Range(0, _remainingExpandUpgrades.Count)];
        UIManager.Instance.ExpandChoiceTitle.text = _currentExpandUpgrade.EffectName;
        UIManager.Instance.ExpandChoiceDetail.text = _currentExpandUpgrade.GetEffectDescription();
        _currentExploitUpgrade = _remainingExploitpgrades[Random.Range(0, _remainingExploitpgrades.Count)];
        UIManager.Instance.ExploitChoiceTitle.text = _currentExploitUpgrade.EffectName;
        UIManager.Instance.ExploitChoiceDetail.text = _currentExploitUpgrade.GetEffectDescription();
        _currentEntertainUpgrade = _remainingEntertainUpgrades[Random.Range(0, _remainingEntertainUpgrades.Count)];
        UIManager.Instance.EntertainChoiceTitle.text = _currentEntertainUpgrade.EffectName;
        UIManager.Instance.EntertainChoiceDetail.text = _currentEntertainUpgrade.GetEffectDescription();

        UIManager.Instance.UpgradesChoiceMenu();
    }

    public void RerollUpgradesChoice(Phase phase)
    {
        switch (phase)
        {
            case Phase.Explore:
                UpgradeEffect newExploEffect = _remainingExploUpgrades[Random.Range(0, _remainingExploUpgrades.Count)];
                if (_remainingExploUpgrades.Count > 1)
                {
                    while (newExploEffect == _currentExploUpgrade)
                        newExploEffect = _remainingExploUpgrades[Random.Range(0, _remainingExploUpgrades.Count)];
                }
                _currentExploUpgrade = newExploEffect;
                UIManager.Instance.ExploChoiceTitle.text = _currentExploUpgrade.EffectName;
                UIManager.Instance.ExploChoiceDetail.text = _currentExploUpgrade.GetEffectDescription();
                break;
            case Phase.Expand:
                UpgradeEffect newExpandEffect = _remainingExpandUpgrades[Random.Range(0, _remainingExpandUpgrades.Count)];
                if (_remainingExpandUpgrades.Count > 1)
                {
                    while (newExpandEffect == _currentExpandUpgrade)
                        newExpandEffect = _remainingExpandUpgrades[Random.Range(0, _remainingExpandUpgrades.Count)];
                }
                _currentExpandUpgrade = newExpandEffect;
                UIManager.Instance.ExpandChoiceTitle.text = _currentExpandUpgrade.EffectName;
                UIManager.Instance.ExpandChoiceDetail.text = _currentExpandUpgrade.GetEffectDescription();
                break;
            case Phase.Exploit:
                UpgradeEffect newExploitEffect = _remainingExploitpgrades[Random.Range(0, _remainingExploitpgrades.Count)];
                if (_remainingExploitpgrades.Count > 1)
                {
                    while (newExploitEffect == _currentExploitUpgrade)
                        newExploitEffect = _remainingExploitpgrades[Random.Range(0, _remainingExploitpgrades.Count)];
                }
                _currentExploitUpgrade = newExploitEffect;
                UIManager.Instance.ExploitChoiceTitle.text = _currentExploitUpgrade.EffectName;
                UIManager.Instance.ExploitChoiceDetail.text = _currentExploitUpgrade.GetEffectDescription();
                break;
            case Phase.Entertain:
                UpgradeEffect newEntertainEffect = _remainingEntertainUpgrades[Random.Range(0, _remainingEntertainUpgrades.Count)];
                if (_remainingEntertainUpgrades.Count > 1)
                {
                    while (newEntertainEffect == _currentEntertainUpgrade)
                        newEntertainEffect = _remainingEntertainUpgrades[Random.Range(0, _remainingEntertainUpgrades.Count)];
                }
                _currentEntertainUpgrade = newEntertainEffect;
                UIManager.Instance.EntertainChoiceTitle.text = _currentEntertainUpgrade.EffectName;
                UIManager.Instance.EntertainChoiceDetail.text = _currentEntertainUpgrade.GetEffectDescription();
                break;
        }
    }

    public void ConfirmUpgrade(Phase phase)
    {
        switch (phase)
        {
            case Phase.Explore:
                UnlockUpgrade(_currentExploUpgrade);
                _remainingExploUpgrades.Remove(_currentExploUpgrade);
                break;
            case Phase.Expand:
                UnlockUpgrade(_currentExpandUpgrade);
                _remainingExpandUpgrades.Remove(_currentExpandUpgrade);
                break;
            case Phase.Exploit:
                UnlockUpgrade(_currentExploitUpgrade);
                _remainingExploitpgrades.Remove(_currentExploitUpgrade);
                break;
            case Phase.Entertain:
                UnlockUpgrade(_currentEntertainUpgrade);
                _remainingEntertainUpgrades.Remove(_currentEntertainUpgrade);
                break;
        }
    }

    public int GetNextUpgradeTurn()
    {
        if (_appliedUpgrades.Count >= _turnsForUpgradesChoice.Count)
            return -1;
        if (_turnsForUpgradesChoice[_appliedUpgrades.Count] > GameManager.Instance.TurnLimit)
            return -1;
        return _turnsForUpgradesChoice[_appliedUpgrades.Count];
    }
}
