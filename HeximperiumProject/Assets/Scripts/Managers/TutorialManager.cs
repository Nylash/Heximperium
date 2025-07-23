using System;
using System.Collections;
using UnityEngine;

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
        Expand1_ObjEndPhase
    }

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

    public event Action OnTutorialStarted;

    private TutorialStep _step = TutorialStep.None;

    private Action<Scout> _scoutSpawnedHandler;
    private Action<Tile> _tileClaimedHandler;

    protected override void OnAwake()
    {
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.OnLoadingDone += ShowIntro;
        else
            ShowIntro();
    }

    private void ShowIntro()
    {
        _introduction.SetActive(true);
        _step = TutorialStep.Intro;
    }

    public void StartTutorial()
    {
        _introduction.GetComponent<Animator>().SetTrigger("Shrink");
        OnTutorialStarted?.Invoke();
        InitializeExplo1();
    }

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
        ExplorationManager.Instance.OnTownSelected += OnTownSelected;
    }

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

    private void RollBackToObjSelectTown()
    {
        StartCoroutine(RollBackToObjSelectTown_Coroutine());
    }

    private IEnumerator RollBackToObjSelectTown_Coroutine()
    {
        yield return null;//Wait one frame to be sure the event isn't call by clicking on a interaction button
        if (_step != TutorialStep.Explo1_ObjScoutSpawn)
            yield break;

        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectTown;
        ExplorationManager.Instance.OnScoutSpawned -= _scoutSpawnedHandler;
        ExplorationManager.Instance.OnTownSelected += OnTownSelected;

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

        _step = TutorialStep.Expand1_Init;
        _explo1_ObjEndPhase.SetTrigger("Fold");
        GameManager.Instance.OnExpansionPhaseStarted += InitializeExpand1;
    }
    #endregion

    #region EXPANSION 1
    private void InitializeExpand1()
    {
        _expand1.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Expand1_Init;
    }

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

    private void RollBackToObjSelectTile()
    {
        StartCoroutine(RollBackToObjSelectTile_Coroutine());
    }

    private IEnumerator RollBackToObjSelectTile_Coroutine()
    {
        yield return null;//Wait one frame to be sure the event isn't call by clicking on a interaction button
        if (_step != TutorialStep.Expand1_ObjClaimTile)
            yield break;

        if (GameManager.Instance.SelectedTile != null)
        {
            //Check if the new tile is claimable
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

    private void OnExpansionPhaseEnded()
    {
        if (_step != TutorialStep.Expand1_ObjEndPhase) return;
        GameManager.Instance.OnExpansionPhaseEnded -= OnExpansionPhaseEnded;

        _step = TutorialStep.None;
        _expand1_ObjEndPhase.SetTrigger("Fold");
        // subscribe next tutorial phase here
    }
    #endregion
}
