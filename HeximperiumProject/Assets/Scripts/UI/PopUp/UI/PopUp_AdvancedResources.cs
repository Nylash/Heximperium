using TMPro;
using UnityEngine;

public class PopUp_AdvancedResources : UI_ResourcePopUp
{
    [SerializeField] private TextMeshProUGUI _resourceCount;
    [SerializeField] private TextMeshProUGUI _resourceIncome;

    private Resource _resource;

    public Resource Resource { get => _resource; set => _resource = value; }

    public override void InitializePopUp()
    {
        _resourceCount.text = _resource.ToCustomString() + " : " + ResourcesManager.Instance.GetResourceStock(_resource);
        _resourceIncome.text += "+" + ExploitationManager.Instance.GetResourceIncomeByInfra(_resource);
    }
}
