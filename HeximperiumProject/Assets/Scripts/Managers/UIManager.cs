using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] TextMeshProUGUI _currentPhaseText;
    [SerializeField] TextMeshProUGUI _confirmPhaseButtonText;
    [SerializeField] TextMeshProUGUI _turnCounterText;

    public void UpdatePhaseText()
    {
        switch (GameManager.Instance.CurrentPhase)
        {
            case Phase.Explore:
                _currentPhaseText.text = "Explore";
                break;
            case Phase.Expand:
                _currentPhaseText.text = "Expand";
                break;
            case Phase.Exploit:
                _currentPhaseText.text = "Exploit";
                break;
            case Phase.Entertain:
                _currentPhaseText.text = "Entertain";
                break;
            default:
                Debug.LogError("No matching phase.");
                break;
        }
    }

    public void UpdatePhaseButtonText() 
    {
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
        {
            _confirmPhaseButtonText.text = "End Turn";
        }
        else
        {
            _confirmPhaseButtonText.text = "End Phase";
        }
    }

    public void UpdateTurnCounterText()
    {
        _turnCounterText.text = "Turn : " + GameManager.Instance.TurnCounter;
    }
}
