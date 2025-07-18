using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExplorationManager : PhaseManager<ExplorationManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Scouts Configuration")]
    [SerializeField] private ScoutData _scoutData;
    [SerializeField] private int _baseScoutsLimit = 1;
    [SerializeField] private float _awaitTimeScoutMovement = 0.25f;
    [Header("_________________________________________________________")]
    [Header("Scouts Related Objects")]
    [SerializeField] private Transform _scoutsParent;
    [SerializeField] private GameObject _scoutPrefab;
    [SerializeField] private GameObject _scoutCounterPrefab;
    #endregion

    #region VARIABLES
    private bool _finalizingPhase;
    //Scouts variables
    private List<Scout> _scouts = new List<Scout>();
    private int _scoutsLimit;
    private int _currentScoutsCount;
    private Scout _currentScout;
    //Scout direction variables
    private bool _choosingScoutDirection;
    private Tile _tileRefForScoutDirection;
    //Upgrades variables
    private int _boostScoutLifespan;
    private int _boostScoutSpeed;
    private int _boostScoutRevealRadius;
    private int _upgradeScoutRevealOnDeathRadius;
    private bool _upgradeScoutIgnoreHazard;
    private bool _upgradeScoutRedirectable;
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent OnScoutsLimitModified = new UnityEvent();
    public event Action<Scout> OnScoutSpawned;
    #endregion

    #region ACCESSORS
    public bool ChoosingScoutDirection { get => _choosingScoutDirection;}
    public GameObject ScoutCounterPrefab { get => _scoutCounterPrefab;}
    public float AwaitTimeScoutMovement { get => _awaitTimeScoutMovement;}
    public List<Scout> Scouts { get => _scouts;}
    public int ScoutsLimit
    {
        get => _scoutsLimit;
        set
        {
            _scoutsLimit = value;
            OnScoutsLimitModified.Invoke();
        }
    }
    public int CurrentScoutsCount
    {
        get => _currentScoutsCount;
        set
        {
            _currentScoutsCount = value;
            OnScoutsLimitModified.Invoke();
        }
    }

    public ScoutData ScoutData { get => _scoutData;}
    public int BoostScoutLifespan 
    { 
        get => _boostScoutLifespan; 
        set
        {
            foreach (Scout scout in _scouts)
                scout.Lifespan += (value - _boostScoutLifespan);
            _boostScoutLifespan = value;
        } 
    }
    public int BoostScoutSpeed { 
        get => _boostScoutSpeed; 
        set
        {
            foreach (Scout scout in _scouts)
                scout.Speed += (value - _boostScoutSpeed);
            _boostScoutSpeed = value;

        }
    }
    public int BoostScoutRevealRadius { 
        get => _boostScoutRevealRadius; 
        set
        {
            foreach (Scout scout in _scouts)
                scout.RevealRadius += (value - _boostScoutRevealRadius);
            _boostScoutRevealRadius = value;
        }
    }
    public int UpgradeScoutRevealOnDeathRadius { get => _upgradeScoutRevealOnDeathRadius; set => _upgradeScoutRevealOnDeathRadius = value; }
    public bool UpgradeScoutIgnoreHazard { get => _upgradeScoutIgnoreHazard; set => _upgradeScoutIgnoreHazard = value; }
    public bool UpgradeScoutRedirectable { get => _upgradeScoutRedirectable; set => _upgradeScoutRedirectable = value; }
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnExplorationPhaseStarted.AddListener(StartPhase);
        GameManager.Instance.OnExplorationPhaseEnded.AddListener(ConfirmPhase);
        GameManager.Instance.OnNewTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.OnTileUnselected.AddListener(TileUnselected);

        ScoutsLimit = _baseScoutsLimit;
    }

    private void Update()
    {
        //Update scout's orientation during scout spawning
        if (ChoosingScoutDirection)
            _currentScout.Direction = GetAngleForScout();

        if (GameManager.Instance.CurrentPhase != Phase.Explore)
            return;

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
    protected override void StartPhase()
    {
        //Highlight all tiles that are starting points for scouts
        foreach (Tile tile in ExpansionManager.Instance.ClaimedTiles)
        {
            if(tile.TileData is InfrastructureData data)
            {
                if (data.ScoutStartingPoint)
                    tile.Highlight(true);
            }
        }
    }

    protected override void ConfirmPhase()
    {
        _finalizingPhase = true;

        //Unhighlight all tiles that were starting points for scouts
        foreach (Tile tile in ExpansionManager.Instance.ClaimedTiles)
        {
            if (tile.TileData is InfrastructureData data)
            {
                if (data.ScoutStartingPoint)
                    tile.Highlight(false);
            }
        }

        foreach (Scout scout in _scouts)
        {
            StartCoroutine(scout.Move());
        }

        GameManager.Instance.UnselectTile();
    }
    #endregion

    protected override void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Explore)
            return;

        _interactionPositions.Clear();

        if (_upgradeScoutRedirectable)
        {
            if(tile.Scouts.Count > 0)
            {
                foreach (Scout scout in tile.Scouts)
                {
                    if (!scout.HasRedirected)
                    {
                        if(tile.TileData is InfrastructureData data && data.ScoutStartingPoint)
                        {
                            _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 2);
                            ScoutInteraction(tile, 0);
                            RedirectScoutInteraction(tile, 1, scout);
                        }
                        else
                        {
                            _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                            RedirectScoutInteraction(tile, 0, scout);
                        }
                        return;
                    }
                }
            }
        }

        if (tile.TileData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
            ScoutInteraction(tile, 0);
        }
    }

    #region INTERACTION
    public void SpawnScout(Tile tile, bool freeScout = false)
    {
        if(_currentScoutsCount < _scoutsLimit)
        {
            _currentScout = Instantiate(_scoutPrefab, 
                tile.transform.position + _scoutPrefab.transform.localPosition,
                _scoutPrefab.transform.rotation, _scoutsParent).GetComponent<Scout>();
            _currentScout.CurrentTile = tile;
            _scouts.Add(_currentScout);

            if(!freeScout)
                CurrentScoutsCount++;

            tile.Scouts.Add(_currentScout);
            _tileRefForScoutDirection = tile;
            tile.UpdateScoutCounter();

            _currentScout.InitializeScout();

            _choosingScoutDirection = true;

            OnScoutSpawned?.Invoke(_currentScout);
        }
    }

    public void RedirectScout(Tile tile, Scout scout)
    {
        _currentScout = scout;
        _currentScout.HasRedirected = true;
        _tileRefForScoutDirection = tile;
        _choosingScoutDirection = true;
        scout.Animator.SetTrigger("Redirecting");
    }

    private void ScoutInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Scout));
    }

    private void RedirectScoutInteraction(Tile tile, int positionIndex, Scout scout)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.RedirectScout, null, null, scout));
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
