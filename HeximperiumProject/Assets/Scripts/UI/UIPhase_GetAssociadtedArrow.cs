using UnityEngine;
using UnityEngine.UI;

public class UIPhase_GetAssociadtedArrow : MonoBehaviour
{
    [SerializeField] private Image _associatedArrow;

    public Image AssociatedArrow { get => _associatedArrow; }
}
