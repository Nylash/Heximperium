using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] TextMeshProUGUI _currentPhaseText;
    [SerializeField] TextMeshProUGUI _confirmPhaseButtonText;
    [SerializeField] TextMeshProUGUI _turnCounterText;
    [SerializeField] float _durationHoverForUI = 2.0f;

    private GameObject _objectUnderMouse;
    private float _hoverTimer;

    public void HoverUIPopupCheck(GameObject obj)
    {
        if (obj == _objectUnderMouse) 
        {
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= _durationHoverForUI) 
            {
                print(obj.name);
            }
        }
        else
        {
            _objectUnderMouse = obj;
            _hoverTimer = 0.0f;
        }
    }

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
