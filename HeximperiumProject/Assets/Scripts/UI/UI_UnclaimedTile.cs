using UnityEngine;
using TMPro;

public class UI_UnclaimedTile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _biomeText;
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _claimText;

    public void Initialize(string name, string biome, string effect, string gold, string claim)
    {
        _nameText.text = name;
        _biomeText.text = biome;
        _effectText.text = effect;
        _goldText.text = gold;
        _claimText.text = claim;
    }
}
