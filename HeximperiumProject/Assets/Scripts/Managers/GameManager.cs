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
    [SerializeField] private GameObject _highlighPrefab;
    [SerializeField] private GameObject _interactionPrefab;

    #region VARIABLES
    private Phase _currentPhase;
    private int _turnCounter;
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent<int> event_newTurn;
    [HideInInspector] public UnityEvent<Phase> event_newPhase;
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

        if (event_newTurn == null)
            event_newTurn = new UnityEvent<int>();
        if (event_newPhase == null)
            event_newPhase = new UnityEvent<Phase>();
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

    private void InitializeGame()
    {
        if (_turnCounter == 1)
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
            foreach (ResourceCost cost in townData.Costs)
                ResourcesManager.Instance.UpdateResource(cost.resource, cost.cost, false);
            ExpansionManager.Instance.BuildTown(centralTile);
            
            //Claim 1 hex radius around central tile
            foreach (Tile tile in centralTile.Neighbors)
            {
                //Give the player claim for the tile
                ResourcesManager.Instance.UpdateResource(Resource.Claim, tile.TileData.ClaimCost, false);
                ExpansionManager.Instance.ClaimTile(tile);
            }
        }
    }

    private void LeftClickAction()
    {
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
            default: 
                Debug.LogError("This interaction is not handle : " +  button.Interaction);
                break;
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
