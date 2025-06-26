using UnityEngine.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : Singleton<GameManager>
{
    #region CONSTANTS
    private const int TURN_LIMIT = 20;
    #endregion

    #region CONFIGURATION
    [SerializeField] private GameObject _selectionPrefab;
    [SerializeField] private GameObject _interactionPrefab;
    #endregion

    #region VARIABLES
    private InputSystem_Actions _inputActions;

    //Interaction with tiles
    private Ray _mouseRay;
    private RaycastHit _mouseRayHit;
    private Tile _selectedTile;
    private Tile _previousSelectedTile;
    private bool _isPointerOverUI;
    private GameObject _selectionObject;

    private bool _waitingPhaseFinalization;
    private Phase _currentPhase;
    private int _turnCounter = 1;

    private bool _gamePaused;
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent<int> OnNewTurn = new UnityEvent<int>();
    [HideInInspector] public UnityEvent OnExplorationPhaseStarted = new UnityEvent();
    [HideInInspector] public UnityEvent OnExplorationPhaseEnded = new UnityEvent();
    [HideInInspector] public UnityEvent OnExpansionPhaseStarted = new UnityEvent();
    [HideInInspector] public UnityEvent OnExpansionPhaseEnded = new UnityEvent();
    [HideInInspector] public UnityEvent OnExploitationPhaseStarted = new UnityEvent();
    [HideInInspector] public UnityEvent OnExploitationPhaseEnded = new UnityEvent();
    [HideInInspector] public UnityEvent OnEntertainmentPhaseStarted = new UnityEvent();
    [HideInInspector] public UnityEvent OnEntertainmentPhaseEnded = new UnityEvent();
    [HideInInspector] public UnityEvent<Tile> OnNewTileSelected = new UnityEvent<Tile>();
    [HideInInspector] public UnityEvent OnTileUnselected = new UnityEvent();
    [HideInInspector] public UnityEvent OnGameFinished = new UnityEvent();
    #endregion

    #region ACCESSORS
    public Phase CurrentPhase { get => _currentPhase;}
    public int TurnCounter { get => _turnCounter;}
    public GameObject InteractionPrefab { get => _interactionPrefab;}
    public bool GamePaused { get => _gamePaused; set => _gamePaused = value; }
    #endregion

    private void OnEnable() => _inputActions.Player.Enable();

    private void OnDisable() => _inputActions.Player.Disable();

    protected override void OnAwake()
    {
        _inputActions = new InputSystem_Actions();

        _inputActions.Player.LeftClick.performed += ctx => LeftClickAction();
        _inputActions.Player.ConfirmPhase.performed += ctx => ConfirmPhase();
        _inputActions.Player.Menu.performed += ctx => UIManager.Instance.OpenCloseMenu();

        MapManager.Instance.OnMapGenerated.AddListener(InitializeGame);
        ExplorationManager.Instance.OnPhaseFinalized.AddListener(PhaseFinalized);
        ExpansionManager.Instance.OnPhaseFinalized.AddListener(PhaseFinalized);
        ExploitationManager.Instance.OnPhaseFinalized.AddListener(PhaseFinalized);
        EntertainmentManager.Instance.OnPhaseFinalized.AddListener(PhaseFinalized);
    }

    private void Start()
    {

    }

    private void Update()
    {
        //Check if the cursor if over UI
        _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    public void  InteractionButtonsFade(bool fade)
    {
        switch (_currentPhase)
        {
            case Phase.Explore:
                ExplorationManager.Instance.ButtonsFade(fade);
                break;
            case Phase.Expand:
                ExpansionManager.Instance.ButtonsFade(fade);
                break;
            case Phase.Exploit:
                ExploitationManager.Instance.ButtonsFade(fade);
                break;
            case Phase.Entertain:
                EntertainmentManager.Instance.ButtonsFade(fade);
                break;
        }
    }

    #region INITIALIZATION
    //Tmp until save and game setting logic
    private void InitializeGame()
    {
        Initializer();

        if (_currentPhase != Phase.Explore)
        {
            _currentPhase = Phase.Explore;
        }

        OnExplorationPhaseStarted.Invoke();

        _turnCounter = 1;
    }

    private void Initializer()
    {
        Tile centralTile;

        if (!MapManager.Instance.Tiles.TryGetValue(Vector2.zero, out centralTile))
        {
            Debug.LogError("Central tile not found.");
            return;
        }

        //Reveal the central tile and its neighbors
        centralTile.RevealTile(true);
        foreach (Tile tile in centralTile.Neighbors)
        {
            if (!tile)
                continue;
            tile.RevealTile(true);
        }

        //Give the player resources for the initial town 
        InfrastructureData townData = ExpansionManager.Instance.TownData;
        ExploitationManager.Instance.InfraAvailableModify(townData, Transaction.Gain);
        ResourcesManager.Instance.UpdateResource(townData.Costs, Transaction.Gain);
        ExpansionManager.Instance.BuildTown(centralTile);

        //Claim the tiles
        foreach (Tile tile in centralTile.Neighbors)
        {
            if (!tile)
                continue;
            //Give the player claim for the tile
            ResourcesManager.Instance.UpdateClaim(tile.TileData.ClaimCost, Transaction.Gain);
            ExpansionManager.Instance.ClaimTile(tile);
        }

        ExpansionManager.Instance.ClaimPerTurn = 4;
    }
    #endregion

    #region ACTIONS
    private void LeftClickAction()
    {
        //Specific behaviour with scouts instancing
        if (ExplorationManager.Instance.ChoosingScoutDirection)
        {
            ExplorationManager.Instance.ConfirmDirection();
            return;
        }

        //If a tile was selected we unselect it
        if (_selectedTile)
        {
            Destroy(_selectionObject);
            OnTileUnselected.Invoke();
        }

        _previousSelectedTile = _selectedTile;
        _selectedTile = null;

        //The action is performed only if the cursor is not on UI
        if (_isPointerOverUI)
            return;

        _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_mouseRay, out _mouseRayHit))
        {
            //Check if we clicked on a Tile or on a Interaction Button
            if (_mouseRayHit.collider.gameObject.GetComponent<Tile>())
            {
                SelectTile(_mouseRayHit.collider.gameObject.GetComponent<Tile>());
            }
            else if (_mouseRayHit.collider.gameObject.GetComponent<InteractionButton>())
            {
                InteractWithButton(_mouseRayHit.collider.gameObject.GetComponent<InteractionButton>());
            }
        }
    }

    private void SelectTile(Tile tile)
    {
        //We can only select revealed tiles
        if (!tile.Revealed)
            return;

        _selectedTile = tile;

        //If we click on the previous tile we just unselect it and stop the action
        if (_selectedTile == _previousSelectedTile)
        {
            _previousSelectedTile = null;
            _selectedTile = null;
            return;
        }

        //Spawn highlight and call event
        _selectionObject = Instantiate(_selectionPrefab, _selectedTile.transform.position + new Vector3(0, 0.01f, 0), Quaternion.identity);
        OnNewTileSelected.Invoke(_selectedTile);
    }

    private void InteractWithButton(InteractionButton button)
    {
        //Call the right method depending on the nature of the button
        switch (button.Interaction)
        {
            case Interaction.Claim:
                ExpansionManager.Instance.ClaimTile(button.AssociatedTile);
                break;
            case Interaction.Scout:
                ExplorationManager.Instance.SpawnScout(button.AssociatedTile);
                break;
            case Interaction.Infrastructure:
                if(_currentPhase == Phase.Expand)
                    ExpansionManager.Instance.BuildTown(button.AssociatedTile);
                else
                    ExploitationManager.Instance.BuildInfrastructure(button.AssociatedTile, button.InfrastructureData);
                break;
            case Interaction.Destroy:
                if(_currentPhase == Phase.Exploit)
                    ExploitationManager.Instance.DestroyInfrastructure(button.AssociatedTile);
                else
                    EntertainmentManager.Instance.DestroyEntertainment(button.AssociatedTile);
                    break;
            case Interaction.Entertainment:
                EntertainmentManager.Instance.SpawnEntertainment(button.AssociatedTile, button.EntertainData);
                break;
            default: 
                Debug.LogError("This interaction is not handle : " +  button.Interaction);
                break;
        }
    }
    #endregion

    #region PHASE LOGIC
    public void ConfirmPhase()
    {
        if(_gamePaused) 
            return;

        //The phase is finalizing its logic
        if (_waitingPhaseFinalization)
            return;

        //Avoid confirming phase when scout aren't properly initialized
        if (ExplorationManager.Instance.ChoosingScoutDirection)
            return;

        InvokePhaseEndEvent(_currentPhase);

        _waitingPhaseFinalization = true;
    }

    private void PhaseFinalized()
    {
        _waitingPhaseFinalization = false;

        _currentPhase = GetNextPhase(_currentPhase);

        if (_currentPhase == Phase.Entertain)
        {
            OnGameFinished.Invoke();
            return;
        }

        //New turn logic
        if (_currentPhase == Phase.Explore)
        {
            if (_turnCounter == TURN_LIMIT)
            {
                _currentPhase = Phase.Entertain;
            }
            else
            {
                _turnCounter++;
                OnNewTurn.Invoke(_turnCounter);
            }
        }
        InvokePhaseStartEvent(_currentPhase);
    }

    private Phase GetNextPhase(Phase currentPhase)
    {
        return currentPhase switch
        {
            Phase.Explore => Phase.Expand,
            Phase.Expand => Phase.Exploit,
            Phase.Exploit => Phase.Explore,
            _ => currentPhase
        };
    }

    private void InvokePhaseStartEvent(Phase phase)
    {
        switch (phase)
        {
            case Phase.Explore:
                OnExplorationPhaseStarted.Invoke();
                break;
            case Phase.Expand:
                OnExpansionPhaseStarted.Invoke();
                break;
            case Phase.Exploit:
                OnExploitationPhaseStarted.Invoke();
                break;
            case Phase.Entertain:
                OnEntertainmentPhaseStarted.Invoke();
                break;
        }
    }

    private void InvokePhaseEndEvent(Phase phase)
    {
        switch (phase)
        {
            case Phase.Explore:
                OnExplorationPhaseEnded.Invoke();
                break;
            case Phase.Expand:
                OnExpansionPhaseEnded.Invoke();
                break;
            case Phase.Exploit:
                OnExploitationPhaseEnded.Invoke();
                break;
            case Phase.Entertain:
                OnEntertainmentPhaseEnded.Invoke();
                break;
        }
    }
    #endregion
}

