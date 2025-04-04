using UnityEngine.Events;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    #region VARIABLES
    private Phase _currentPhase;
    private int _turnCounter;
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent<int> event_newTurn;
    [HideInInspector] public UnityEvent<Phase> event_newPhase;
    #endregion

    #region ACCESSORS
    public Phase CurrentPhase { get => _currentPhase; set => _currentPhase = value; }
    public int TurnCounter { get => _turnCounter; set => _turnCounter = value; }
    #endregion

    private void OnEnable()
    {
        if (event_newTurn == null)
            event_newTurn = new UnityEvent<int>();
        if (event_newPhase == null)
            event_newPhase = new UnityEvent<Phase>();
    }

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

            event_newTurn.Invoke(_turnCounter);
        }

        event_newPhase.Invoke(_currentPhase);
    }
}

public enum Phase
{
    Explore, Expand, Exploit, Entertain
}
