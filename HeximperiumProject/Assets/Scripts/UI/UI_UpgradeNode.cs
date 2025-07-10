using UnityEngine;
using UnityEngine.UI;

public class UI_UpgradeNode : MonoBehaviour
{
    [SerializeField] private UpgradeNode _nodeData;
    
    private Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(() => UpgradesManager.Instance.UnlockNode(_nodeData));
    }

    public void UpdateVisual()
    {
        //Update the button's interactable state based on the node's cost, prerequisites and exclusivity
    }
}
