public class GameManager : Singleton<GameManager>
{
    private Phase _currentPhase;
    private int _turnCounter;

    public Phase CurrentPhase { get => _currentPhase; set => _currentPhase = value; }
    public int TurnCounter { get => _turnCounter; set => _turnCounter = value; }

    private void Start()
    {
        //Replace it by save logic
        if (_currentPhase != Phase.Explore)
        {
            _currentPhase = Phase.Explore;
        }

        _turnCounter = 1;
    }

    public void ConfirmPhase()
    {
        if (_currentPhase != Phase.Entertain) 
        {
            _currentPhase++;
        }
        else
        {
            _currentPhase = Phase.Explore;
            _turnCounter++;
            UIManager.Instance.UpdateTurnCounterText();
        }

        switch (_currentPhase)
        {
            case Phase.Explore:
                ExplorationManager.Instance.StartPhase();
                break;
            case Phase.Expand:
                ExpansionManager.Instance.StartPhase();
                break;
            case Phase.Exploit:
                ExploitationManager.Instance.StartPhase();
                break;
            case Phase.Entertain:
                EntertainementManager.Instance.StartPhase();
                break;
            default:
                break;
        }

        UIManager.Instance.UpdatePhaseButtonText();
        UIManager.Instance.UpdatePhaseText();
    }
}

public enum Phase
{
    Explore, Expand, Exploit, Entertain
}
