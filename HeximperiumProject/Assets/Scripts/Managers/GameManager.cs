using UnityEngine.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : Singleton<GameManager>
{
    private InputSystem_Actions _inputActions;

    //Interaction with tiles
    private Ray _mouseRay;
    private RaycastHit _mouseRayHit;
    private Tile _selectedTile;
    private Tile _previousSelectedTile;
    private bool _isPointerOverUI;
    private GameObject _highlightObject;
    [SerializeField] GameObject _highlighPrefab;

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
        _inputActions.Player.Enable();

        if (event_newTurn == null)
            event_newTurn = new UnityEvent<int>();
        if (event_newPhase == null)
            event_newPhase = new UnityEvent<Phase>();
    }

    private void OnDisable() => _inputActions.Player.Disable();

    protected override void OnAwake()
    {
        _inputActions = new InputSystem_Actions();

        _inputActions.Player.LeftClick.performed += ctx => LeftClickAction();
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

    private void Update()
    {
        _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    private void LeftClickAction()
    {
        _previousSelectedTile = _selectedTile;
        _selectedTile = null;
        if (_highlightObject)
            Destroy(_highlightObject);

        if (!_isPointerOverUI)
        {
            _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_mouseRay, out _mouseRayHit))
            {
                if (_mouseRayHit.collider.gameObject.GetComponent<Tile>())
                {
                    _selectedTile = _mouseRayHit.collider.gameObject.GetComponent<Tile>();
                    if (_selectedTile == _previousSelectedTile)
                    {
                        _previousSelectedTile = null;
                        _selectedTile = null;
                        return;
                    }
                        
                    _highlightObject = Instantiate(_highlighPrefab, _selectedTile.transform.position + new Vector3(0, 0.01f, 0), Quaternion.identity);
                }
            }
        }
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
