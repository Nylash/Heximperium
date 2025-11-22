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
    #endregion

    #region EVENTS
    public event Action OnTutorialStarted;
    //Event handlers
    private Action<Scout> _scoutSpawnedHandler;
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
        UIManager.Instance.ForceExploColor();

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
        ExplorationManager.Instance.OnScoutStartingPointSelected += OnTownSelected;
    }

    /// <summary>
    /// Callback when the player selects the starting town. Moves the
    /// tutorial forward to the scout spawning step.
    /// </summary>
    private void OnTownSelected()
    {
        if (_step != TutorialStep.Explo1_ObjSelectTown) return;
        ExplorationManager.Instance.OnScoutStartingPointSelected -= OnTownSelected;
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
        ExplorationManager.Instance.OnScoutStartingPointSelected += OnTownSelected;

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

        //_step = TutorialStep.Expand1_Init;
        _explo1_ObjEndPhase.SetTrigger("Fold");
        //GameManager.Instance.OnExpansionPhaseStarted += InitializeExpand1;
    }
    #endregion
}
