using System;
using System.Collections;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    private enum TutorialStep
    {
        None,
        S1_Intro,
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

    protected override void OnAwake()
    {
        UIManager.Instance.ForceExploColor();

        if (LoadingManager.Instance != null)
            LoadingManager.Instance.OnLoadingDone += ShowStep1;
        else
            ShowStep1();
    }

    #region INTRODUCTION
    private void ShowStep1()
    {
        _introduction.SetActive(true);
        _step = TutorialStep.S1_Intro;
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.OnLoadingDone -= ShowStep1;
    }

    public void Button_ValidateStep1()
    {
        _introduction.GetComponent<Animator>().SetTrigger("Shrink");
        OnTutorialStarted?.Invoke();
        //InitializeExplo1();
    }
    #endregion

    #region EXPLORATION 1
    private void InitializeExplo1()
    {
        _explo1.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Explo1_Init;
    }
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
    private void RollBackToObjSelectTown()
    {
        StartCoroutine(RollBackToObjSelectTown_Coroutine());
    }
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
