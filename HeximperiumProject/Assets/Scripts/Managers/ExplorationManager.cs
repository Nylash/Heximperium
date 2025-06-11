using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExplorationManager : Singleton<ExplorationManager>
{
    #region CONFIGURATION
    [SerializeField] private Transform _scoutsParent;
    [SerializeField] private GameObject _scoutPrefab;
    [SerializeField] private GameObject _scoutCounterPrefab;
    [SerializeField] private float _awaitTimeScoutMovement = 0.25f;
    #endregion

    #region VARIABLES
    private List<Scout> _scouts = new List<Scout>();
    private List<GameObject> _buttons = new List<GameObject>();
    private List<Vector3> _interactionPositions = new List<Vector3>();
    private int _freeScouts;
    private bool _finalizingPhase;

    private Scout _currentScout;
    private bool _choosingScoutDirection;
    private Tile _tileRefForScoutDirection;
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent OnPhaseFinalized = new UnityEvent();
    #endregion

    #region ACCESSORS
    public int FreeScouts { get => _freeScouts; set => _freeScouts = value; }
    public bool ChoosingScoutDirection { get => _choosingScoutDirection;}
    public GameObject ScoutCounterPrefab { get => _scoutCounterPrefab;}
    public float AwaitTimeScoutMovement { get => _awaitTimeScoutMovement;}
    public List<Scout> Scouts { get => _scouts;}
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnExplorationPhaseStarted.AddListener(StartPhase);
        GameManager.Instance.OnExplorationPhaseEnded.AddListener(ConfirmPhase);
        GameManager.Instance.OnNewTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.OnTileUnselected.AddListener(TileUnselected);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnExplorationPhaseStarted.RemoveListener(StartPhase);
        GameManager.Instance.OnExplorationPhaseEnded.RemoveListener(ConfirmPhase);
        GameManager.Instance.OnNewTileSelected.RemoveListener(NewTileSelected);
        GameManager.Instance.OnTileUnselected.RemoveListener(TileUnselected);
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentPhase != Phase.Explore)
            return;

        //Update scout's orientation during scout spawning
        if(ChoosingScoutDirection)
            _currentScout.Direction = GetAngleForScout();

        //Check if scouts have finished their movement
        if (_finalizingPhase)
        {
            foreach (Scout scout in _scouts)
            {
                if (!scout.HasDoneMoving)
                    return;
            }
            _finalizingPhase = false;
            StartCoroutine(PhaseFinalized());
        }
        
    }

    #region PHASE LOGIC
    private void StartPhase()
    {

    }

    private void ConfirmPhase()
    {
        _finalizingPhase = true;
        foreach (Scout scout in _scouts)
        {
            StartCoroutine(scout.Move());
        }
    }

    private IEnumerator PhaseFinalized()
    {
        // Wait for one frame
        yield return null;

        OnPhaseFinalized.Invoke();
    }
    #endregion

    #region TILE SELECTION
    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Explore)
            return;

        _interactionPositions.Clear();

        if (tile.TileData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
            ScoutInteraction(tile, 0);
        }
    }

    private void TileUnselected()
    {
        foreach (GameObject button in _buttons)
        {
            Destroy(button);
        }
        _buttons.Clear();
    }
    #endregion

    #region INTERACTION
    public void SpawnScout(Tile tile, UnitData data)
    {
        if(ResourcesManager.Instance.CanAfford(data.Costs) || _freeScouts != 0)
        {
            if (_freeScouts != 0)
                _freeScouts--;
            else
                ResourcesManager.Instance.UpdateResource(data.Costs, Transaction.Spent);

            _currentScout = Instantiate(_scoutPrefab, 
                tile.transform.position + _scoutPrefab.transform.localPosition,
                _scoutPrefab.transform.rotation, _scoutsParent).GetComponent<Scout>();
            _currentScout.CurrentTile = tile;
            _scouts.Add(_currentScout);
            tile.Scouts.Add(_currentScout);
            _tileRefForScoutDirection = tile;
            tile.UpdateScoutCounter();

            _choosingScoutDirection = true;
        }
    }

    private void ScoutInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Scout));
    }

    public void ButtonsFade(bool fade)
    {
        foreach (GameObject item in _buttons)
        {
            item.GetComponent<InteractionButton>().FadeAnimation(fade);
        }
    }
    #endregion

    #region SCOUT SPAWN
    public void ConfirmDirection()
    {
        _choosingScoutDirection = false;
        _currentScout.Direction = GetAngleForScout();
        _tileRefForScoutDirection = null;
        _currentScout = null;
    }

    private Direction GetAngleForScout()
    {
        Vector3 mousePosition = Input.mousePosition;
        // Convert the target object's position to screen space
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_tileRefForScoutDirection.transform.position);
        // Calculate the direction vector from the target object to the cursor in screen space
        Vector2 direction = mousePosition - screenPosition;
        // Calculate the angle between the direction vector and the upwards direction (positive Y-axis)
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        // Invert the angle to correct the direction
        angle = -angle;
        // Normalize the angle to be within 0 to 360 degrees
        angle = (angle + 360) % 360;

        // Determine the enum direction based on the angle
        return GetDirectionFromAngle(angle);
    }

    private Direction GetDirectionFromAngle(float angle)
    {
        if (angle >= 0 && angle < 60)
        {
            return Direction.TopRight;
        }
        else if (angle >= 60 && angle < 120)
        {
            return Direction.Right;
        }
        else if (angle >= 120 && angle < 180)
        {
            return Direction.BottomRight;
        }
        else if (angle >= 180 && angle < 240)
        {
            return Direction.BottomLeft;
        }
        else if (angle >= 240 && angle < 300)
        {
            return Direction.Left;
        }
        else
        {
            return Direction.TopLeft;
        }
    }
    #endregion
}
