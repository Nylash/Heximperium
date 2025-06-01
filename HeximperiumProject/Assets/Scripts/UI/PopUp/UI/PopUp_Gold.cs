using TMPro;
using UnityEngine;

public class PopUp_Gold : UI_ResourcePopUp
{
    [SerializeField] private TextMeshProUGUI _goldCount;
    [SerializeField] private TextMeshProUGUI _totalGoldIncome;
    [SerializeField] private TextMeshProUGUI _goldTileIncome;
    [SerializeField] private TextMeshProUGUI _goldInfraIncome;

    public override void InitializePopUp()
    {
        _goldCount.text += ResourcesManager.Instance.GetResourceStock(Resource.Gold);
        _totalGoldIncome.text += "+" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.Gold);
        _goldTileIncome.text += "+" + ExploitationManager.Instance.GetResourceIncomeByBasicTiles(Resource.Gold);
        _goldInfraIncome.text += "+" + ExploitationManager.Instance.GetResourceIncomeByInfra(Resource.Gold);
    }
}
