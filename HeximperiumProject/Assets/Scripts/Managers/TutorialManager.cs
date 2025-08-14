using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the in-game tutorial sequence. This component controls
/// which instructions are displayed and monitors player actions to
/// progress through each tutorial step.
/// </summary>
public class TutorialManager : Singleton<TutorialManager>
{
    private enum TutorialStep
    {
        None,
        Intro,
        Explo1_Init,
        Explo1_ObjSelectTown,
        Explo1_ObjScoutSpawn,
        Explo1_ObjDirectScout,
        Explo1_ObjEndPhase,
        Expand1_Init,
        Expand1_ObjSelectTile,
        Expand1_ObjClaimTile,
        Expand1_ObjEndPhase,
        Exploit1_Init,
        Exploit1_ObjSelectTile,
        Exploit1_ObjBuildFarm,
        Exploit1bis_Init,
        Exploit1bis_ObjSelectTile,
        Exploit1_ObjBuildWindmill,
        Exploit1ter_Init,
        Exploit1_ObjEndTurn,
        Explo2_Init,
        Explo2_ObjEndPhase,
        Expand2_Init,
        Expand2_ObjSelectTile,
        Expand2_ObjBuildTown,
        Expand2_ObjEndPhase,
        Exploit2_Init,
        Exploit2_ObjSelectTile,
        Exploit2_ObjEnchanceInfra,
        Exploit2bis_Init,
        Exploit2_ObjEndTurn,
        Entertain_Init,
        Entertain_ObjSelectTile,
        Entertain_ObjPlaceEntertainment,
        Entertain_ObjEndGame,
        Outro
    }

    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Intro")]
    [SerializeField] private GameObject _introduction;
    [Header("_________________________________________________________")]
    [Header("Exploration Turn 1")]
    [SerializeField] private GameObject _explo1;
    [SerializeField] private Animator _explo1_ObjSelectTown;
    [SerializeField] private Animator _explo1_ObjScoutSpawn;
    [SerializeField] private Animator _explo1_ObjDirectScout;
    [SerializeField] private Animator _explo1_ObjEndPhase;
    [Header("_________________________________________________________")]
    [Header("Expansion Turn 1")]
    [SerializeField] private GameObject _expand1;
    [SerializeField] private Animator _expand1_ObjSelectTile;
    [SerializeField] private Animator _expand1_ObjClaimTile;
    [SerializeField] private Animator _expand1_ObjEndPhase;
    [Header("_________________________________________________________")]
    [Header("Exploitation Turn 1")]
    [SerializeField] private GameObject _exploit1;
    [SerializeField] private Animator _exploit1_ObjSelectTile;
    [SerializeField] private Vector2 _exploit1_TargetTileCoor;
    [SerializeField] private InfrastructureData _farmData;
    [SerializeField] private Animator _exploit1_ObjBuildFarm;
    [SerializeField] private GameObject _exploit1bis;
    [SerializeField] private Animator _exploit1bis_ObjSelectTile;
    [SerializeField] private Vector2 _exploit1bis_TargetTileCoor;
    [SerializeField] private InfrastructureData _windmillData;
    [SerializeField] private Animator _exploit1_ObjBuildWindmill;
    [SerializeField] private GameObject _exploit1ter;
    [SerializeField] private Animator _exploit1_ObjEndTurn;
    [Header("_________________________________________________________")]
    [Header("Exploration Turn 2")]
    [SerializeField] private GameObject _explo2;
    [SerializeField] private Animator _explo2_ObjEndPhase;
    [Header("_________________________________________________________")]
    [Header("Expansion Turn 2")]
    [SerializeField] private GameObject _expand2;
    [SerializeField] private InfrastructureData _townData;
    [SerializeField] private Animator _expand2_ObjSelectTile;
    [SerializeField] private Animator _expand2_ObjBuildTown;
    [SerializeField] private Animator _expand2_ObjEndPhase;
    [Header("_________________________________________________________")]
    [Header("Exploitation Turn 2")]
    [SerializeField] private GameObject _exploit2;
    [SerializeField] private Animator _exploit2_ObjSelectTile;
    [SerializeField] private Vector2 _exploit2_TargetTileCoor;
    [SerializeField] private InfrastructureData _enchancementData;
    [SerializeField] private Animator _exploit2_ObjEnchanceInfra;
    [SerializeField] private GameObject _exploit2bis;
    [SerializeField] private Animator _exploit2_ObjEndTurn;
    [Header("_________________________________________________________")]
    [Header("Entertainment")]
    [SerializeField] private GameObject _entertain;
    [SerializeField] private List<ResourceToIntMap> _budget = new List<ResourceToIntMap>();
    [SerializeField] private Animator _entertain_ObjSelectTile;
    [SerializeField] private Animator _entertain_ObjPlaceEntertainment;
    [SerializeField] private Animator _entertain_ObjEndGame;
    [Header("_________________________________________________________")]
    [Header("Outro")]
    [SerializeField] private GameObject _outro;
    #endregion

    #region EVENTS
    public event Action OnTutorialStarted;
    //Event handlers
    private Action<Scout> _scoutSpawnedHandler;
    private Action<Tile> _tileClaimedHandler;
    private Action<Tile> _infraBuildHandler;
    private Action<Entertainment> _entertainmentSpawnedHandler;
    #endregion

    #region VARIABLES
    private TutorialStep _step = TutorialStep.None;
    private Tile _targetTile;

    public Tile TargetTile { get => _targetTile; }
    #endregion

    /// <summary>
    /// Registers callbacks when the scene is loaded and displays the
    /// introduction screen once loading has finished.
    /// </summary>
    protected override void OnAwake()
    {
        UIManager.Instance.ForceExploMat();

        if (LoadingManager.Instance != null)
            LoadingManager.Instance.OnLoadingDone += ShowIntro;
        else
            ShowIntro();
    }

    #region INTRODUCTION
    /// <summary>
    /// Displays the introduction canvas and sets the tutorial state
    /// to the first step.
    /// </summary>
    private void ShowIntro()
    {
        _introduction.SetActive(true);
        _step = TutorialStep.Intro;
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.OnLoadingDone -= ShowIntro;
    }

    /// <summary>
    /// Called from the UI to begin the tutorial sequence after the
    /// introduction is acknowledged by the player.
    /// </summary>
    public void StartTutorial()
    {
        _introduction.GetComponent<Animator>().SetTrigger("Shrink");
        OnTutorialStarted?.Invoke();
        InitializeExplo1();
    }
    #endregion

    #region EXPLORATION 1
    /// <summary>
    /// Prepare the first exploration tutorial step by pausing the game
    /// and showing the instruction panel.
    /// </summary>
    private void InitializeExplo1()
    {
        _explo1.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Explo1_Init;
    }

    /// <summary>
    /// Starts the first exploration turn of the tutorial. The player
    /// must select a town to continue.
    /// </summary>
    public void StartExplo1()
    {
        if (_step != TutorialStep.Explo1_Init) return;
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        _explo1.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Explo1_ObjSelectTown;
        _explo1_ObjSelectTown.SetTrigger("Unfold");
        ExplorationManager.Instance.OnTownSelected += OnTownSelected;
    }

    /// <summary>
    /// Callback when the player selects the starting town. Moves the
    /// tutorial forward to the scout spawning step.
    /// </summary>
    private void OnTownSelected()
    {
        if (_step != TutorialStep.Explo1_ObjSelectTown) return;
        ExplorationManager.Instance.OnTownSelected -= OnTownSelected;
        GameManager.Instance.OnTileUnselected += RollBackToObjSelectTown;

        _step = TutorialStep.Explo1_ObjScoutSpawn;
        _explo1_ObjSelectTown.SetTrigger("Fold");
        _explo1_ObjScoutSpawn.SetTrigger("Unfold");

        _scoutSpawnedHandler = scout => OnScoutSpawned();
        ExplorationManager.Instance.OnScoutSpawned += _scoutSpawnedHandler;
    }

    /// <summary>
    /// If the player deselects the town, return to the previous step.
    /// </summary>
    private void RollBackToObjSelectTown()
    {
        StartCoroutine(RollBackToObjSelectTown_Coroutine());
    }

    /// <summary>
    /// Coroutine used to reset the step when the player changes their
    /// selection before spawning a scout.
    /// </summary>
    private IEnumerator RollBackToObjSelectTown_Coroutine()
    {
        // Wait one frame to ensure the deselection event is processed
        yield return null;
        if (_step != TutorialStep.Explo1_ObjScoutSpawn)
            yield break;

        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectTown;
        ExplorationManager.Instance.OnScoutSpawned -= _scoutSpawnedHandler;
        ExplorationManager.Instance.OnTownSelected += OnTownSelected;

        _explo1_ObjScoutSpawn.SetTrigger("Fold");
        _explo1_ObjSelectTown.SetTrigger("Unfold");
        _step = TutorialStep.Explo1_ObjSelectTown;
    }

    /// <summary>
    /// Triggered once the scout is spawned. Guides the player to direct
    /// the unit toward unexplored tiles.
    /// </summary>
    private void OnScoutSpawned()
    {
        if (_step != TutorialStep.Explo1_ObjScoutSpawn) return;
        ExplorationManager.Instance.OnScoutSpawned -= _scoutSpawnedHandler;
        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectTown;

        _step = TutorialStep.Explo1_ObjDirectScout;
        _explo1_ObjScoutSpawn.SetTrigger("Fold");
        _explo1_ObjDirectScout.SetTrigger("Unfold");

        ExplorationManager.Instance.OnScoutDirected += OnScoutDirected;
    }

    /// <summary>
    /// Called when the player has given a movement order to the scout.
    /// Enables ending the exploration phase.
    /// </summary>
    private void OnScoutDirected()
    {
        if (_step != TutorialStep.Explo1_ObjDirectScout) return;
        ExplorationManager.Instance.OnScoutDirected -= OnScoutDirected;

        _step = TutorialStep.Explo1_ObjEndPhase;
        _explo1_ObjDirectScout.SetTrigger("Fold");
        _explo1_ObjEndPhase.SetTrigger("Unfold");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        GameManager.Instance.OnExplorationPhaseEnded += OnExplorationPhaseEnded;
    }

    /// <summary>
    /// Transition from exploration to the first expansion tutorial step
    /// once the player ends the phase.
    /// </summary>
    private void OnExplorationPhaseEnded()
    {
        if (_step != TutorialStep.Explo1_ObjEndPhase) return;
        GameManager.Instance.OnExplorationPhaseEnded -= OnExplorationPhaseEnded;

        _step = TutorialStep.Expand1_Init;
        _explo1_ObjEndPhase.SetTrigger("Fold");
        GameManager.Instance.OnExpansionPhaseStarted += InitializeExpand1;
    }
    #endregion

    #region EXPANSION 1
    /// <summary>
    /// Prepare the expansion tutorial by pausing the game and showing
    /// the instructions for claiming a new tile.
    /// </summary>
    private void InitializeExpand1()
    {
        GameManager.Instance.OnExpansionPhaseStarted -= InitializeExpand1;
        _expand1.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Expand1_Init;
    }

    /// <summary>
    /// Begins the first expansion turn where the player chooses a
    /// neighboring tile to claim.
    /// </summary>
    public void StartExpand1()
    {
        if (_step != TutorialStep.Expand1_Init) return;
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        _expand1.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Expand1_ObjSelectTile;
        _expand1_ObjSelectTile.SetTrigger("Unfold");
        ExpansionManager.Instance.OnClaimableTileSelected += OnClaimableTileSelected;
    }

    /// <summary>
    /// Player selected a valid tile to claim. Switches UI to the claim
    /// confirmation step.
    /// </summary>
    private void OnClaimableTileSelected()
    {
        if (_step != TutorialStep.Expand1_ObjSelectTile) return;
        ExpansionManager.Instance.OnClaimableTileSelected -= OnClaimableTileSelected;
        GameManager.Instance.OnTileUnselected += RollBackToObjSelectTile;

        _step = TutorialStep.Expand1_ObjClaimTile;
        _expand1_ObjSelectTile.SetTrigger("Fold");
        _expand1_ObjClaimTile.SetTrigger("Unfold");

        _tileClaimedHandler = tile => OnTileClaimed();
        ExpansionManager.Instance.OnTileClaimed += _tileClaimedHandler;
    }

    /// <summary>
    /// Return to tile selection if the player cancels before claiming.
    /// </summary>
    private void RollBackToObjSelectTile()
    {
        StartCoroutine(RollBackToObjSelectTile_Coroutine());
    }

    private IEnumerator RollBackToObjSelectTile_Coroutine()
    {
        // Wait one frame to avoid reacting to UI button clicks
        yield return null;
        if (_step != TutorialStep.Expand1_ObjClaimTile)
            yield break;

        if (GameManager.Instance.SelectedTile != null)
        {
            // Check if the new tile is claimable
            if (!GameManager.Instance.SelectedTile.Claimed && GameManager.Instance.SelectedTile.IsOneNeighborClaimed())
                yield break;
        }

        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectTile;
        ExpansionManager.Instance.OnTileClaimed -= _tileClaimedHandler;
        ExpansionManager.Instance.OnClaimableTileSelected += OnClaimableTileSelected;

        _expand1_ObjClaimTile.SetTrigger("Fold");
        _expand1_ObjSelectTile.SetTrigger("Unfold");
        _step = TutorialStep.Expand1_ObjSelectTile;
    }

    /// <summary>
    /// Finalizes the claim of the selected tile and prepares to end the
    /// expansion phase.
    /// </summary>
    private void OnTileClaimed()
    {
        if (_step != TutorialStep.Expand1_ObjClaimTile) return;
        ExpansionManager.Instance.OnTileClaimed -= _tileClaimedHandler;
        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectTile;

        _step = TutorialStep.Expand1_ObjEndPhase;
        _expand1_ObjClaimTile.SetTrigger("Fold");
        _expand1_ObjEndPhase.SetTrigger("Unfold");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        GameManager.Instance.OnExpansionPhaseEnded += OnExpansionPhaseEnded;
    }

    /// <summary>
    /// Called when the first expansion phase ends and moves the
    /// tutorial to the exploitation turn.
    /// </summary>
    private void OnExpansionPhaseEnded()
    {
        if (_step != TutorialStep.Expand1_ObjEndPhase) return;
        GameManager.Instance.OnExpansionPhaseEnded -= OnExpansionPhaseEnded;

        _step = TutorialStep.Exploit1_Init;
        _expand1_ObjEndPhase.SetTrigger("Fold");

        GameManager.Instance.OnExploitationPhaseStarted += InitializeExploit1;
    }
    #endregion

    #region EXPLOITATION 1
    /// <summary>
    /// Prepare the exploitation tutorial by pausing the game and
    /// activating the relevant UI instructions.
    /// </summary>
    private void InitializeExploit1()
    {
        GameManager.Instance.OnExploitationPhaseStarted -= InitializeExploit1;
        _exploit1.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Exploit1_Init;
    }

    /// <summary>
    /// Begins the exploitation tutorial where the player must select
    /// a specific tile to build a farm.
    /// </summary>
    public void StartExploit1()
    {
        if (_step != TutorialStep.Exploit1_Init) return;
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        _exploit1.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Exploit1_ObjSelectTile;
        _exploit1_ObjSelectTile.SetTrigger("Unfold");

        _targetTile = MapManager.Instance.Tiles[_exploit1_TargetTileCoor];
        _targetTile.Highlight(true);

        ExploitationManager.Instance.OnRightTileSelected += OnFarmTileSelected;
    }

    /// <summary>
    /// The correct tile was selected. Show the build farm prompt and
    /// wait for the player to confirm.
    /// </summary>
    private void OnFarmTileSelected()
    {
        if (_step != TutorialStep.Exploit1_ObjSelectTile) return;
        ExploitationManager.Instance.OnRightTileSelected -= OnFarmTileSelected;
        GameManager.Instance.OnTileUnselected += Exploit1_RollBackToObjSelectTile;

        _step = TutorialStep.Exploit1_ObjBuildFarm;
        _exploit1_ObjSelectTile.SetTrigger("Fold");
        _exploit1_ObjBuildFarm.SetTrigger("Unfold");

        _infraBuildHandler = tile => OnFarmBuilded();
        ExploitationManager.Instance.OnInfraBuilded += _infraBuildHandler;

        if (!ResourcesManager.Instance.CanAfford(_farmData.Costs))
        {
            ResourcesManager.Instance.UpdateResource(_farmData.Costs, Transaction.Gain);
        }
    }

    /// <summary>
    /// Called when the player deselects the tile before building the
    /// farm, returning to the selection step.
    /// </summary>
    private void Exploit1_RollBackToObjSelectTile()
    {
        StartCoroutine(Exploit1_RollBackToObjSelectTile_Coroutine());
    }

    private IEnumerator Exploit1_RollBackToObjSelectTile_Coroutine()
    {
        // Wait one frame to ensure button events are processed
        yield return null;
        if (_step != TutorialStep.Exploit1_ObjBuildFarm)
            yield break;

        GameManager.Instance.OnTileUnselected -= Exploit1_RollBackToObjSelectTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        ExploitationManager.Instance.OnRightTileSelected += OnFarmTileSelected;

        _exploit1_ObjBuildFarm.SetTrigger("Fold");
        _exploit1_ObjSelectTile.SetTrigger("Unfold");
        _step = TutorialStep.Exploit1_ObjSelectTile;
    }

    /// <summary>
    /// Farm construction completed. Transition to the next exploitation
    /// tutorial which introduces the windmill.
    /// </summary>
    private void OnFarmBuilded()
    {
        if (_step != TutorialStep.Exploit1_ObjBuildFarm) return;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        GameManager.Instance.OnTileUnselected -= Exploit1_RollBackToObjSelectTile;
        _exploit1_ObjBuildFarm.SetTrigger("Fold");
        _targetTile.Highlight(false);
        _targetTile = null;

        _exploit1bis.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Exploit1bis_Init;
    }

    /// <summary>
    /// Begins the second exploitation objective where the player places a
    /// windmill on a highlighted tile.
    /// </summary>
    public void StartExploit1Bis()
    {
        if (_step != TutorialStep.Exploit1bis_Init) return;
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        _exploit1bis.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Exploit1bis_ObjSelectTile;
        _exploit1bis_ObjSelectTile.SetTrigger("Unfold");

        _targetTile = MapManager.Instance.Tiles[_exploit1bis_TargetTileCoor];
        _targetTile.Highlight(true);

        ExploitationManager.Instance.OnRightTileSelected += OnWindmillTileSelected;
    }

    /// <summary>
    /// Called when the correct windmill tile is chosen. Displays the
    /// build prompt to construct the windmill.
    /// </summary>
    private void OnWindmillTileSelected()
    {
        if (_step != TutorialStep.Exploit1bis_ObjSelectTile) return;
        ExploitationManager.Instance.OnRightTileSelected -= OnWindmillTileSelected;
        GameManager.Instance.OnTileUnselected += Exploit1bis_RollBackToObjSelectTile;

        _step = TutorialStep.Exploit1_ObjBuildWindmill;
        _exploit1bis_ObjSelectTile.SetTrigger("Fold");
        _exploit1_ObjBuildWindmill.SetTrigger("Unfold");

        _infraBuildHandler = tile => OnWindmillBuilded();
        ExploitationManager.Instance.OnInfraBuilded += _infraBuildHandler;

        if (!ResourcesManager.Instance.CanAfford(_windmillData.Costs))
        {
            ResourcesManager.Instance.UpdateResource(_windmillData.Costs, Transaction.Gain);
        }
    }

    /// <summary>
    /// Cancels windmill placement and goes back to tile selection.
    /// </summary>
    private void Exploit1bis_RollBackToObjSelectTile()
    {
        StartCoroutine(Exploit1bis_RollBackToObjSelectTile_Coroutine());
    }

    private IEnumerator Exploit1bis_RollBackToObjSelectTile_Coroutine()
    {
        // Wait one frame to avoid reacting to UI button clicks
        yield return null;
        if (_step != TutorialStep.Exploit1_ObjBuildWindmill)
            yield break;

        GameManager.Instance.OnTileUnselected -= Exploit1bis_RollBackToObjSelectTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        ExploitationManager.Instance.OnRightTileSelected += OnWindmillTileSelected;

        _exploit1_ObjBuildWindmill.SetTrigger("Fold");
        _exploit1bis_ObjSelectTile.SetTrigger("Unfold");
        _step = TutorialStep.Exploit1bis_ObjSelectTile;
    }

    /// <summary>
    /// Windmill successfully built. Enables the tutorial message about
    /// ending the turn.
    /// </summary>
    private void OnWindmillBuilded()
    {
        if (_step != TutorialStep.Exploit1_ObjBuildWindmill) return;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        GameManager.Instance.OnTileUnselected -= Exploit1bis_RollBackToObjSelectTile;

        _targetTile.Highlight(false);
        _targetTile = null;

        _step = TutorialStep.Exploit1_ObjEndTurn;
        _exploit1_ObjBuildWindmill.SetTrigger("Fold");

        _exploit1ter.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Exploit1ter_Init;
    }

    /// <summary>
    /// Final step of the first exploitation phase where the player ends
    /// their turn.
    /// </summary>
    public void StartExploit1Ter()
    {
        if (_step != TutorialStep.Exploit1ter_Init) return;
        _exploit1ter.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Exploit1_ObjEndTurn;
        _exploit1_ObjEndTurn.SetTrigger("Unfold");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        GameManager.Instance.OnExploitationPhaseEnded += OnExploitationPhaseEnded;
    }

    /// <summary>
    /// Transition to the next exploration tutorial when the player ends
    /// the exploitation turn.
    /// </summary>
    private void OnExploitationPhaseEnded()
    {
        if (_step != TutorialStep.Exploit1_ObjEndTurn) return;
        GameManager.Instance.OnExploitationPhaseEnded -= OnExploitationPhaseEnded;

        _step = TutorialStep.Explo2_Init;
        _exploit1_ObjEndTurn.SetTrigger("Fold");

        GameManager.Instance.OnExplorationPhaseStarted += InitializeExplo2;
    }
    #endregion

    #region EXPLORATION 2
    /// <summary>
    /// Setup for the second exploration tutorial turn.
    /// </summary>
    private void InitializeExplo2()
    {
        GameManager.Instance.OnExplorationPhaseStarted -= InitializeExplo2;
        _explo2.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Explo2_Init;
    }

    /// <summary>
    /// Starts the second exploration phase. The objective is simply to
    /// end the phase.
    /// </summary>
    public void StartExplo2()
    {
        if (_step != TutorialStep.Explo2_Init) return;
        _explo2.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Explo2_ObjEndPhase;
        _explo2_ObjEndPhase.SetTrigger("Unfold");
        GameManager.Instance.OnExplorationPhaseEnded += OnExplorationPhaseEnded2;
    }

    /// <summary>
    /// When the exploration phase ends, move to the second expansion
    /// tutorial.
    /// </summary>
    public void OnExplorationPhaseEnded2()
    {
        if (_step != TutorialStep.Explo2_ObjEndPhase) return;
        GameManager.Instance.OnExplorationPhaseEnded -= OnExplorationPhaseEnded2;

        _step = TutorialStep.Expand2_Init;
        _explo2_ObjEndPhase.SetTrigger("Fold");
        GameManager.Instance.OnExpansionPhaseStarted += InitializeExpand2;
    }
    #endregion

    #region EXPANSION 2
    /// <summary>
    /// Setup for the second expansion phase and grant resources if needed
    /// to build the new town.
    /// </summary>
    private void InitializeExpand2()
    {
        GameManager.Instance.OnExpansionPhaseStarted -= InitializeExpand2;
        _expand2.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Expand2_Init;

        if (!ResourcesManager.Instance.CanAfford(_townData.Costs))
        {
            ResourcesManager.Instance.UpdateResource(_townData.Costs, Transaction.Gain);
        }
    }

    /// <summary>
    /// Begins the second expansion phase where the player selects a
    /// location for a new town.
    /// </summary>
    public void StartExpand2()
    {
        if (_step != TutorialStep.Expand2_Init) return;
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        _expand2.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Expand2_ObjSelectTile;
        _expand2_ObjSelectTile.SetTrigger("Unfold");
        ExpansionManager.Instance.OnBasicTileSelected += OnBasicTileSelected;
    }

    /// <summary>
    /// Called when the player selects a valid basic tile to place the
    /// new town.
    /// </summary>
    private void OnBasicTileSelected()
    {
        if (_step != TutorialStep.Expand2_ObjSelectTile) return;
        ExpansionManager.Instance.OnBasicTileSelected -= OnBasicTileSelected;
        GameManager.Instance.OnTileUnselected += RollBackToObjSelectBasicTile;

        _step = TutorialStep.Expand2_ObjBuildTown;
        _expand2_ObjSelectTile.SetTrigger("Fold");
        _expand2_ObjBuildTown.SetTrigger("Unfold");

        _infraBuildHandler = tile => OnTownBuilded();
        ExploitationManager.Instance.OnInfraBuilded += _infraBuildHandler;
    }

    /// <summary>
    /// Returns to tile selection if the player cancels before building
    /// the town.
    /// </summary>
    private void RollBackToObjSelectBasicTile()
    {
        StartCoroutine(RollBackToObjSelectBasicTile_Coroutine());
    }

    private IEnumerator RollBackToObjSelectBasicTile_Coroutine()
    {
        // Wait one frame to avoid reacting to UI button clicks
        yield return null;
        if (_step != TutorialStep.Expand2_ObjBuildTown)
            yield break;

        if (GameManager.Instance.SelectedTile != null)
        {
            // Check if the new tile is still a basic tile
            if (GameManager.Instance.SelectedTile.TileData is BasicTileData)
                yield break;
        }

        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectBasicTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        ExpansionManager.Instance.OnBasicTileSelected += OnBasicTileSelected;

        _expand2_ObjSelectTile.SetTrigger("Unfold");
        _expand2_ObjBuildTown.SetTrigger("Fold");
        _step = TutorialStep.Expand2_ObjSelectTile;
    }


    /// <summary>
    /// Town construction finished. Allow the player to end the expansion
    /// phase.
    /// </summary>
    private void OnTownBuilded()
    {
        if (_step != TutorialStep.Expand2_ObjBuildTown) return;
        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectBasicTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;

        _step = TutorialStep.Expand2_ObjEndPhase;
        _expand2_ObjBuildTown.SetTrigger("Fold");
        _expand2_ObjEndPhase.SetTrigger("Unfold");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        GameManager.Instance.OnExpansionPhaseEnded += OnExpansionPhaseEnded2;
    }

    /// <summary>
    /// Begin the second exploitation phase after ending expansion.
    /// </summary>
    private void OnExpansionPhaseEnded2()
    {
        if (_step != TutorialStep.Expand2_ObjEndPhase) return;
        GameManager.Instance.OnExpansionPhaseEnded -= OnExpansionPhaseEnded2;

        _step = TutorialStep.Exploit2_Init;
        _expand2_ObjEndPhase.SetTrigger("Fold");
        GameManager.Instance.OnExploitationPhaseStarted += InitializeExploit2;
    }
    #endregion

    #region EXPLOITATION 2
    /// <summary>
    /// Prepare the second exploitation phase.
    /// </summary>
    private void InitializeExploit2()
    {
        GameManager.Instance.OnExploitationPhaseStarted -= InitializeExploit2;
        _exploit2.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Exploit2_Init;
    }

    /// <summary>
    /// Starts the second exploitation turn where an infrastructure
    /// enhancement is built.
    /// </summary>
    public void StartExploit2()
    {
        if (_step != TutorialStep.Exploit2_Init) return;
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        _exploit2.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Exploit2_ObjSelectTile;
        _exploit2_ObjSelectTile.SetTrigger("Unfold");

        _targetTile = MapManager.Instance.Tiles[_exploit2_TargetTileCoor];
        _targetTile.Highlight(true);

        ExploitationManager.Instance.OnRightTileSelected += OnEnhancementTileSelected;
    }

    /// <summary>
    /// Correct enhancement tile selected; display build option.
    /// </summary>
    private void OnEnhancementTileSelected()
    {
        if (_step != TutorialStep.Exploit2_ObjSelectTile) return;
        ExploitationManager.Instance.OnRightTileSelected -= OnEnhancementTileSelected;
        GameManager.Instance.OnTileUnselected += RollBackToObjSelectEnhancementTile;

        _step = TutorialStep.Exploit2_ObjEnchanceInfra;
        _exploit2_ObjSelectTile.SetTrigger("Fold");
        _exploit2_ObjEnchanceInfra.SetTrigger("Unfold");

        _infraBuildHandler = tile => OnInfraEnhanced();
        ExploitationManager.Instance.OnInfraBuilded += _infraBuildHandler;

        if (!ResourcesManager.Instance.CanAfford(_enchancementData.Costs))
        {
            ResourcesManager.Instance.UpdateResource(_enchancementData.Costs, Transaction.Gain);
        }
    }

    /// <summary>
    /// Cancel enhancement building and return to tile selection.
    /// </summary>
    private void RollBackToObjSelectEnhancementTile()
    {
        StartCoroutine(RollBackToObjSelectEnhancementTile_Coroutine());
    }

    private IEnumerator RollBackToObjSelectEnhancementTile_Coroutine()
    {
        // Wait one frame to avoid reacting to UI button clicks
        yield return null;
        if (_step != TutorialStep.Exploit2_ObjEnchanceInfra)
            yield break;

        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectEnhancementTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        ExploitationManager.Instance.OnRightTileSelected += OnEnhancementTileSelected;

        _exploit2_ObjSelectTile.SetTrigger("Unfold");
        _exploit2_ObjEnchanceInfra.SetTrigger("Fold");
        _step = TutorialStep.Exploit2_ObjSelectTile;
    }


    /// <summary>
    /// Enhancement built successfully, move on to the end-turn step.
    /// </summary>
    private void OnInfraEnhanced()
    {
        if (_step != TutorialStep.Exploit2_ObjEnchanceInfra) return;
        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectEnhancementTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;

        _step = TutorialStep.Exploit2bis_Init;
        _exploit2_ObjEnchanceInfra.SetTrigger("Fold");
        _targetTile.Highlight(false);
        _targetTile = null;

        _exploit2bis.SetActive(true);
        GameManager.Instance.GamePaused = true;
    }

    /// <summary>
    /// Final instruction of the second exploitation phase: end the turn.
    /// </summary>
    public void StartExploit2Bis()
    {
        if (_step != TutorialStep.Exploit2bis_Init) return;
        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        _exploit2bis.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Exploit2_ObjEndTurn;
        _exploit2_ObjEndTurn.SetTrigger("Unfold");

        GameManager.Instance.OnExploitationPhaseEnded += OnExploitationPhaseEnded2;
    }

    /// <summary>
    /// After the second exploitation turn ends, begin the entertainment
    /// phase tutorial.
    /// </summary>
    private void OnExploitationPhaseEnded2()
    {
        if (_step != TutorialStep.Exploit2_ObjEndTurn) return;
        GameManager.Instance.OnExploitationPhaseEnded -= OnExploitationPhaseEnded2;

        _step = TutorialStep.Entertain_Init;
        _exploit2_ObjEndTurn.SetTrigger("Fold");
        GameManager.Instance.OnEntertainmentPhaseStarted += InitializeEntertainment;
    }
    #endregion

    #region ENTERTAINMENT
    /// <summary>
    /// Prepare the entertainment phase tutorial.
    /// </summary>
    private void InitializeEntertainment()
    {
        GameManager.Instance.OnEntertainmentPhaseStarted -= InitializeEntertainment;
        _entertain.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Entertain_Init;
    }

    /// <summary>
    /// Begins the entertainment tutorial where an attraction is placed
    /// on a claimed tile.
    /// </summary>
    public void StartEntertain()
    {
        if (_step != TutorialStep.Entertain_Init) return;
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        _entertain.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Entertain_ObjSelectTile;
        _entertain_ObjSelectTile.SetTrigger("Unfold");
        EntertainmentManager.Instance.OnClaimedTileSelected += OnClaimedTileSelected;

        ResourcesManager.Instance.UpdateResource(_budget, Transaction.Gain);
    }

    /// <summary>
    /// The player selected a valid claimed tile to place entertainment.
    /// </summary>
    private void OnClaimedTileSelected()
    {
        if (_step != TutorialStep.Entertain_ObjSelectTile) return;
        EntertainmentManager.Instance.OnClaimedTileSelected -= OnClaimedTileSelected;
        GameManager.Instance.OnTileUnselected += RollBackToObjSelectClaimedTile;

        _step = TutorialStep.Entertain_ObjPlaceEntertainment;
        _entertain_ObjSelectTile.SetTrigger("Fold");
        _entertain_ObjPlaceEntertainment.SetTrigger("Unfold");

        _entertainmentSpawnedHandler = ent => OnEntertainmentPlaced();
        EntertainmentManager.Instance.OnEntertainmentSpawned += _entertainmentSpawnedHandler;
    }

    /// <summary>
    /// Returns to tile selection if entertainment placement is canceled.
    /// </summary>
    private void RollBackToObjSelectClaimedTile()
    {
        StartCoroutine(RollBackToObjSelectClaimedTile_Coroutine());
    }

    private IEnumerator RollBackToObjSelectClaimedTile_Coroutine()
    {
        // Wait one frame to avoid reacting to UI button clicks
        yield return null;
        if (_step != TutorialStep.Entertain_ObjPlaceEntertainment)
            yield break;

        if (GameManager.Instance.SelectedTile != null)
        {
            // Check if the new tile is still claimed
            if (GameManager.Instance.SelectedTile.Claimed)
                yield break;
        }

        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectClaimedTile;
        EntertainmentManager.Instance.OnEntertainmentSpawned -= _entertainmentSpawnedHandler;
        EntertainmentManager.Instance.OnClaimedTileSelected += OnClaimedTileSelected;

        _entertain_ObjPlaceEntertainment.SetTrigger("Fold");
        _entertain_ObjSelectTile.SetTrigger("Unfold");
        _step = TutorialStep.Entertain_ObjSelectTile;
    }

    /// <summary>
    /// Entertainment successfully placed; allow the player to end the
    /// game and show the outro.
    /// </summary>
    private void OnEntertainmentPlaced()
    {
        if (_step != TutorialStep.Entertain_ObjPlaceEntertainment) return;
        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectClaimedTile;
        EntertainmentManager.Instance.OnEntertainmentSpawned -= _entertainmentSpawnedHandler;

        _step = TutorialStep.Entertain_ObjEndGame;
        _entertain_ObjPlaceEntertainment.SetTrigger("Fold");
        _entertain_ObjEndGame.SetTrigger("Unfold");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        GameManager.Instance.OnEntertainmentPhaseEnded += OnEntertainmentPhaseEnded;
    }

    /// <summary>
    /// Ends the tutorial and shows the closing screen.
    /// </summary>
    private void OnEntertainmentPhaseEnded()
    {
        if (_step != TutorialStep.Entertain_ObjEndGame) return;
        GameManager.Instance.OnEntertainmentPhaseEnded -= OnEntertainmentPhaseEnded;

        _step = TutorialStep.Outro;
        _entertain_ObjEndGame.SetTrigger("Fold");

        _outro.SetActive(true);
    }
    #endregion
}
