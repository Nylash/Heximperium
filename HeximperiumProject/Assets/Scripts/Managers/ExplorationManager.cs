using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    [SerializeField] private GameObject _scoutPrefab;
    #endregion

    #region VARIABLES
    private bool _finalizingPhase;
    private List<Tile> _revealedTiles = new List<Tile>();
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
    private UnlockRevealAnywhere _upgradeRevealAnywhere;
    private bool _hasUsedRevealAnywhereThisPhase;
    #endregion

    #region EVENTS
    public event Action OnScoutsLimitModified;
    public event Action<Scout> OnScoutSpawned;
    //Tutorial events
    public event Action OnScoutStartingPointSelected;
    public event Action OnScoutDirected;
    #endregion

    #region ACCESSORS
    public bool ChoosingScoutDirection { get => _choosingScoutDirection;}
    public float AwaitTimeScoutMovement { get => _awaitTimeScoutMovement;}
    public List<Scout> Scouts { get => _scouts;}
    public int ScoutsLimit
    {
        get => _scoutsLimit;
        set
        {
            _scoutsLimit = value;
            OnScoutsLimitModified?.Invoke();
            if (GameManager.Instance.CurrentPhase == Phase.Explore)
                UpdateInteractableTiles();
        }
    }
    public int CurrentScoutsCount
    {
        get => _currentScoutsCount;
        set
        {
            _currentScoutsCount = value;
            OnScoutsLimitModified?.Invoke();
        }
    }

    public ScoutData ScoutData { get => _scoutData;}
    public int BoostScoutLifespan 
    { 
        get => _boostScoutLifespan; 
        set
        {
            foreach (Scout scout in _scouts)
            {
                scout.Lifespan += (value - _boostScoutLifespan);
                scout.UpdateLifeHints();
            }
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
    public bool UpgradeScoutRedirectable { 
        get => _upgradeScoutRedirectable; 
        set 
        {
            _upgradeScoutRedirectable = value;
            if (GameManager.Instance.CurrentPhase == Phase.Explore)
                UpdateInteractableTiles();
        } 
    }
    public List<Tile> RevealedTiles { get => _revealedTiles; }
    public UnlockRevealAnywhere UpgradeRevealAnywhere { 
        get => _upgradeRevealAnywhere; 
        set
        {
            _upgradeRevealAnywhere = value;
            if (GameManager.Instance.CurrentPhase == Phase.Explore)
                UIManager.Instance.RevealAnywhereHint.gameObject.SetActive(true);
        }
    }

    public int BaseScoutsLimit { get => _baseScoutsLimit; }
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnExplorationPhaseStarted += StartPhase;
        GameManager.Instance.OnExplorationPhaseEnded += ConfirmPhase;
        GameManager.Instance.OnNewTileSelected += NewTileSelected;
        GameManager.Instance.OnTileUnselected += TileUnselected;

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
        _hasUsedRevealAnywhereThisPhase = false;
        if (_upgradeRevealAnywhere)
            UIManager.Instance.RevealAnywhereHint.gameObject.SetActive(true);

        GameManager.Instance.UnselectTile();

        UpdateInteractableTiles();

        ResourcesManager.Instance.CHEAT_RESOURCES();
    }

    protected override void ConfirmPhase()
    {
        _finalizingPhase = true;

        ClearInteractableTiles();

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

        List<Interaction> interactions = new List<Interaction>();

        if (_upgradeScoutRedirectable)
        {
            if(tile.Scouts.Count > 0)
            {
                if (tile.Scouts?.FirstOrDefault(s => !s.HasRedirected))
                    interactions.Add(Interaction.RedirectScout);
            }
        }

        if (_upgradeRevealAnywhere)
        {
            if (!_hasUsedRevealAnywhereThisPhase)
                interactions.Add(Interaction.RevealAnywhere);
        }

        if (tile.TileData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            interactions.Add(Interaction.Scout);
            OnScoutStartingPointSelected?.Invoke();
        }

        if (interactions.Count == 0)
            return;

        _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, interactions.Count);
        for (int i = 0; i < interactions.Count; i++)
        {
            switch (interactions[i])
            {
                case Interaction.Scout:
                    ScoutInteraction(tile, i);
                    break;
                case Interaction.RedirectScout:
                    Scout scout = tile.Scouts.FirstOrDefault(s => !s.HasRedirected);
                    if (scout != null)
                        RedirectScoutInteraction(tile, i, scout);
                    break;
                case Interaction.RevealAnywhere:
                    RevealAnywhereInteraction(tile, i);
                    break;
                default:
                    break;
            }
        }
    }

    #region INTERACTION
    public void SpawnScout(Tile tile, bool freeScout = false, bool fromInteraction = false)
    {
        if(_currentScoutsCount < _scoutsLimit)
        {
            _currentScout = Instantiate(_scoutPrefab, 
                tile.transform.position + _scoutPrefab.transform.localPosition,
                _scoutPrefab.transform.rotation, tile.Visual).GetComponent<Scout>();
            _currentScout.CurrentTile = tile;
            _scouts.Add(_currentScout);

            if(!freeScout)
                CurrentScoutsCount++;

            tile.Scouts.Add(_currentScout);
            _tileRefForScoutDirection = tile;

            _currentScout.InitializeScout(freeScout);

            _choosingScoutDirection = true;

            OnScoutSpawned?.Invoke(_currentScout);
            UIManager.Instance.ScoutHint.SetTrigger("Show");

            if (fromInteraction)
                UpdateInteractableTiles();
        }
    }

    public void RedirectScout(Tile tile, Scout scout)
    {
        _currentScout = scout;
        _currentScout.HasRedirected = true;
        _tileRefForScoutDirection = tile;
        _choosingScoutDirection = true;
        scout.Animator.SetTrigger("Redirecting");
        UIManager.Instance.ScoutHint.SetTrigger("RedirectShow");

        UpdateInteractableTiles();
    }

    public void RevealAnywhere(Tile tile)
    {
        if (_upgradeRevealAnywhere && !_hasUsedRevealAnywhereThisPhase)
        {
            tile.RevealTile(false);
            RevealTilesRecursively(tile, _upgradeRevealAnywhere.RevealRadius);
            _hasUsedRevealAnywhereThisPhase = true;
            UIManager.Instance.RevealAnywhereHint.SetTrigger("Hide");
        }
    }

    // Clone of scout reveal method but without the event
    private void RevealTilesRecursively(Tile currentTile, int depth)
    {
        if (depth <= 0)
        {
            return;
        }

        foreach (Tile neighbor in currentTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Revealed)
            {
                neighbor.RevealTile(false);
            }
            // Recursively reveal the neighbors of the current neighbor
            RevealTilesRecursively(neighbor, depth - 1);
        }
    }

    private void ScoutInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Scout));
    }

    private void RedirectScoutInteraction(Tile tile, int positionIndex, Scout scout)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.RedirectScout, null, null, scout));
    }

    private void RevealAnywhereInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.RevealAnywhere));
    }
    #endregion

    #region SCOUT SPAWN
    public void ConfirmDirection()
    {
        _choosingScoutDirection = false;
        _currentScout.Direction = GetAngleForScout();
        _tileRefForScoutDirection = null;
        if (!_currentScout.HasRedirected)// We were spawning a new scout
            UIManager.Instance.ScoutHint.SetTrigger("Hide");
        else
            UIManager.Instance.ScoutHint.SetTrigger("RedirectHide");
        _currentScout = null;
        OnScoutDirected?.Invoke();
    }

    public void CancelScout()
    {
        if (_currentScout.HasRedirected)// We were redirecting a scout, no cancel
        {
            return;
        }
        else// We were spawning a new scout, we need to remove it
        {
            _choosingScoutDirection = false;
            _currentScout.CurrentTile.Scouts.Remove(_currentScout);
            _scouts.Remove(_currentScout);
            Destroy(_currentScout.gameObject);
            CurrentScoutsCount--;
            _tileRefForScoutDirection = null;
            _currentScout = null;

            UIManager.Instance.ScoutHint.SetTrigger("Hide");
        }
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

    public override void UpdateInteractableTiles()
    {
        HashSet<Tile> validTiles = new HashSet<Tile>();

        if (_currentScoutsCount < _scoutsLimit)
        {
            foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
            {
                if (tile.TileData is InfrastructureData data)
                {
                    if (data.ScoutStartingPoint)
                    {
                        validTiles.Add(tile);
                    }
                }
            }
        }
        if (_upgradeScoutRedirectable)
        {
            foreach (Scout scout in _scouts)
            {
                if (!scout.HasRedirected)
                {
                    validTiles.Add(scout.CurrentTile);
                }
            }
        }

        LaunchInteractableTiles(validTiles);
    }

    protected override void LaunchInteractableTiles(HashSet<Tile> validTiles)
    {
        bool alreadyAffected = false;
        if (_interactibleTiles.Count == 0)
        {
            alreadyAffected = true;
            _interactibleTiles = new HashSet<Tile>(validTiles);
        }
        foreach (var tile in _interactibleTiles) 
            tile.Highlight(validTiles.Contains(tile));
        if (!alreadyAffected)
            _interactibleTiles = new HashSet<Tile>(validTiles);
    }

    protected override void ClearInteractableTiles()
    {
        foreach (Tile tile in _interactibleTiles)
        {
            tile.Highlight(false);
        }
        _interactibleTiles.Clear();
    }
}
