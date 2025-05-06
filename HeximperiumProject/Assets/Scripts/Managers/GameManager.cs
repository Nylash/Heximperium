using UnityEngine.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : Singleton<GameManager>
{
    #region VARIABLES
    private InputSystem_Actions _inputActions;

    //Interaction with tiles
    private Ray _mouseRay;
    private RaycastHit _mouseRayHit;
    private Tile _selectedTile;
    private Tile _previousSelectedTile;
    private bool _isPointerOverUI;
    private GameObject _highlightObject;
    [SerializeField] private GameObject _highlighPrefab;
    [SerializeField] private GameObject _interactionPrefab;

    private bool _waitingPhaseFinalization;
    private Phase _currentPhase;
    private int _turnCounter;
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent<int> EventNewTurn;
    [HideInInspector] public UnityEvent EventStartExplorationPhase;
    [HideInInspector] public UnityEvent EventEndExplorationPhase;
    [HideInInspector] public UnityEvent EventStartExpansionPhase;
    [HideInInspector] public UnityEvent EventEndExpansionPhase;
    [HideInInspector] public UnityEvent EventStartExploitationPhase;
    [HideInInspector] public UnityEvent EventEndExploitationPhase;
    [HideInInspector] public UnityEvent EventStartEntertainementPhase;
    [HideInInspector] public UnityEvent EventEndEntertainementPhase;
    [HideInInspector] public UnityEvent<Tile> event_newTileSelected;
    [HideInInspector] public UnityEvent event_tileUnselected;
    #endregion

    #region ACCESSORS
    public Phase CurrentPhase { get => _currentPhase;}
    public int TurnCounter { get => _turnCounter;}
    public GameObject InteractionPrefab { get => _interactionPrefab;}
    #endregion

    private void OnEnable()
    {
        _inputActions.Player.Enable();

        if (EventNewTurn == null)
            EventNewTurn = new UnityEvent<int>();
        if (EventStartExplorationPhase == null)
            EventStartExplorationPhase = new UnityEvent();
        if (EventEndExplorationPhase == null)
            EventEndExplorationPhase = new UnityEvent();
        if (EventStartExpansionPhase == null)
            EventStartExpansionPhase = new UnityEvent();
        if (EventEndExpansionPhase == null)
            EventEndExpansionPhase = new UnityEvent();
        if (EventStartExploitationPhase == null)
            EventStartExploitationPhase = new UnityEvent();
        if (EventEndExploitationPhase == null)
            EventEndExploitationPhase = new UnityEvent();
        if (EventStartEntertainementPhase == null)
            EventStartEntertainementPhase = new UnityEvent();
        if (EventEndEntertainementPhase == null)
            EventEndEntertainementPhase = new UnityEvent();
        if (event_newTileSelected == null)
            event_newTileSelected = new UnityEvent<Tile>();
        if (event_tileUnselected == null)
            event_tileUnselected = new UnityEvent();
    }

    private void OnDisable() => _inputActions.Player.Disable();

    protected override void OnAwake()
    {
        _inputActions = new InputSystem_Actions();

        _inputActions.Player.LeftClick.performed += ctx => LeftClickAction();

        MapManager.Instance.event_mapGenerated.AddListener(InitializeGame);
        ExplorationManager.Instance.EventScoutsMovementDone.AddListener(PhaseFinalized);
    }

    private void Start()
    {
        //Replace it by save logic
        if (_currentPhase != Phase.Explore)
        {
            _currentPhase = Phase.Explore;
        }

        EventStartExplorationPhase.Invoke();

        _turnCounter = 1;
    }

    private void Update()
    {
        _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    private void InitializeGame()
    {
        if (_turnCounter == 1)
        {
            Initializer(1);
        }
    }

    //Depth will be replace by game settings
    private void Initializer(int depth)
    {
        Tile centralTile;
        MapManager.Instance.Tiles.TryGetValue(Vector2.zero, out centralTile);

        //Reveal the central tile and its neighbors
        centralTile.RevealTile(true);
        foreach (Tile tile in centralTile.Neighbors)
            tile.RevealTile(true);

        //Give the player resources for the initial town 
        ExpansionManager.Instance.AvailableTown += 1;
        InfrastructureData townData = Resources.Load<InfrastructureData>("Data/Infrastructures/Town");
        foreach (ResourceValue cost in townData.Costs)
            ResourcesManager.Instance.UpdateResource(cost.resource, cost.value, Transaction.Gain);
        ExpansionManager.Instance.BuildTown(centralTile);

        if (depth == 0)
        {
            ExplorationManager.Instance.FreeScouts = 1;
            ExpansionManager.Instance.BaseClaimPerTurn = 2;
        }
        if(depth == 1)
        {
            ExplorationManager.Instance.FreeScouts = 3;
            ExpansionManager.Instance.BaseClaimPerTurn = 4;
            foreach (Tile tile in centralTile.Neighbors)
            {
                foreach (Tile t in tile.Neighbors)
                    t.RevealTile(true);
            }
            foreach (Tile tile in centralTile.Neighbors)
            {
                //Give the player claim for the tile
                ResourcesManager.Instance.UpdateClaim(tile.TileData.ClaimCost, Transaction.Gain);
                ExpansionManager.Instance.ClaimTile(tile);
            }
        }
        if (depth == 2) 
        {
            ExplorationManager.Instance.FreeScouts = 5;
            ExpansionManager.Instance.BaseClaimPerTurn = 6;
            foreach (Tile tile in centralTile.Neighbors)
            {
                foreach (Tile t in tile.Neighbors)
                {
                    t.RevealTile(true);
                    foreach (Tile item in t.Neighbors)
                        item.RevealTile(true);
                }
            }
            foreach (Tile firstRingTile in centralTile.Neighbors)
            {
                if (!firstRingTile.Claimed)
                {
                    //Give the player claim for the tile
                    ResourcesManager.Instance.UpdateClaim(firstRingTile.TileData.ClaimCost, Transaction.Gain);
                    ExpansionManager.Instance.ClaimTile(firstRingTile);
                }
                foreach (Tile secondRingTile in firstRingTile.Neighbors)
                {
                    if (!secondRingTile.Claimed)
                    {
                        //Give the player claim for the tile
                        ResourcesManager.Instance.UpdateClaim(secondRingTile.TileData.ClaimCost, Transaction.Gain);
                        ExpansionManager.Instance.ClaimTile(secondRingTile);
                    }
                }
            }
        }
    }

    #region Actions
    private void LeftClickAction()
    {
        if (ExplorationManager.Instance.ChoosingScoutDirection)
        {
            ExplorationManager.Instance.ConfirmDirection();
            return;
        }

        _previousSelectedTile = _selectedTile;
        _selectedTile = null;

        if (_highlightObject)
        {
            Destroy(_highlightObject);
            event_tileUnselected.Invoke();
        }

        if (!_isPointerOverUI)
        {
            _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_mouseRay, out _mouseRayHit))
            {
                if (_mouseRayHit.collider.gameObject.GetComponent<Tile>())
                {
                    SelectTile(_mouseRayHit.collider.gameObject.GetComponent<Tile>());
                }
                if (_mouseRayHit.collider.gameObject.GetComponent<UI_InteractionButton>())
                {
                    InteractWithButton(_mouseRayHit.collider.gameObject.GetComponent<UI_InteractionButton>());
                }
            }
        }
    }

    private void SelectTile(Tile tile)
    {
        if (tile.Revealed)
        {
            _selectedTile = tile;
            if (_selectedTile == _previousSelectedTile)
            {
                _previousSelectedTile = null;
                _selectedTile = null;
                return;
            }

            _highlightObject = Instantiate(_highlighPrefab, _selectedTile.transform.position + new Vector3(0, 0.01f, 0), Quaternion.identity);
            event_newTileSelected.Invoke(_selectedTile);
        }
    }

    private void InteractWithButton(UI_InteractionButton button)
    {
        switch (button.Interaction)
        {
            case Interaction.Claim:
                ExpansionManager.Instance.ClaimTile(button.AssociatedTile);
                break;
            case Interaction.Town:
                ExpansionManager.Instance.BuildTown(button.AssociatedTile);
                break;
            case Interaction.Scout:
                ExplorationManager.Instance.SpawnScout(button.AssociatedTile, button.UnitData);
                break;
            case Interaction.Infrastructure:
                ExploitationManager.Instance.BuildInfrastructure(button.AssociatedTile, button.InfrastructureData);
                break;
            case Interaction.Destroy:
                if(_currentPhase == Phase.Exploit)
                    ExploitationManager.Instance.DestroyInfrastructure(button.AssociatedTile);
                else if (_currentPhase == Phase.Entertain)
                    EntertainementManager.Instance.DestroyEntertainer(button.AssociatedTile);
                    break;
            case Interaction.Entertainer:
                EntertainementManager.Instance.SpawnEntertainer(button.AssociatedTile, button.UnitData as EntertainerData);
                break;
            default: 
                Debug.LogError("This interaction is not handle : " +  button.Interaction);
                break;
        }
    }
    #endregion

    #region Phase confirmation
    public void ConfirmPhase()
    {
        //The phase is finalizing its logic
        if (_waitingPhaseFinalization)
            return;
        //Avoid confirming phase when scout aren't properly initialized
        if (ExplorationManager.Instance.ChoosingScoutDirection)
            return;

        switch (_currentPhase)
        {
            case Phase.Explore:
                EventEndExplorationPhase.Invoke();
                //Waiting scouts movement
                _waitingPhaseFinalization = true;
                return;
            case Phase.Expand:
                EventEndExpansionPhase.Invoke();
                PhaseFinalized();
                break;
            case Phase.Exploit:
                EventEndExploitationPhase.Invoke();
                PhaseFinalized();
                break;
            case Phase.Entertain:
                EventEndEntertainementPhase.Invoke();
                PhaseFinalized();
                break;
        } 
    }

    private void PhaseFinalized()
    {
        _waitingPhaseFinalization = false;

        switch (_currentPhase)
        {
            case Phase.Explore:
                _currentPhase++;
                EventStartExpansionPhase.Invoke();
                break;
            case Phase.Expand:
                _currentPhase++;
                EventStartExploitationPhase.Invoke();
                break;
            case Phase.Exploit:
                _currentPhase++;
                EventStartEntertainementPhase.Invoke();
                break;
            case Phase.Entertain:
                _currentPhase = Phase.Explore;
                EventStartExplorationPhase.Invoke();
                _turnCounter++;
                EventNewTurn.Invoke(_turnCounter);
                break;
        }
    }
    #endregion
}

