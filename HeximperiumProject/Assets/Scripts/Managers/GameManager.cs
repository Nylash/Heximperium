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

    private void InteractWithButton(UI_InteractionButton button)
    {
        switch (button.Interaction)
        {
            case Interaction.Claim:
                ExpansionManager.Instance.ClaimTile(button.AssociatedTile);
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
