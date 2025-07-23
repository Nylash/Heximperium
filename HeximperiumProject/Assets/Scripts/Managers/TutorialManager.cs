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
        Expand1_ObjEndPhase,
        Exploit1_Init,
        Exploit1_ObjSelectTile,
        Exploit1_ObjBuildFarm,
        Exploit1bis_Init,
        Exploit1bis_ObjSelectTile,
        Exploit1_ObjBuildWindmill,
        Exploit1_ObjEndTurn,
        Explo2_Init,
        Explo2_ObjEndPhase,
        Expand2_Init,
        Expand2_ObjSelectTile,
        Expand2_ObjBuildTown,
        Expand2_ObjEndPhase,
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

    public event Action OnTutorialStarted;

    private TutorialStep _step = TutorialStep.None;

    private Tile _targetTile;

    private Action<Scout> _scoutSpawnedHandler;
    private Action<Tile> _tileClaimedHandler;
    private Action<Tile> _infraBuildHandler;

    public Tile TargetTile { get => _targetTile; }

    protected override void OnAwake()
    {
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.OnLoadingDone += ShowIntro;
        else
            ShowIntro();
    }

    #region INTRODUCTION
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
        GameManager.Instance.OnExpansionPhaseStarted -= InitializeExpand1;
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

        _step = TutorialStep.Exploit1_Init;
        _expand1_ObjEndPhase.SetTrigger("Fold");

        GameManager.Instance.OnExploitationPhaseStarted += InitializeExploit1;
    }
    #endregion

    #region EXPLOITATION 1
    private void InitializeExploit1()
    {
        GameManager.Instance.OnExploitationPhaseStarted -= InitializeExploit1;
        _exploit1.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Exploit1_Init;
    }

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

    private void Exploit1_RollBackToObjSelectTile()
    {
        StartCoroutine(Exploit1_RollBackToObjSelectTile_Coroutine());
    }

    private IEnumerator Exploit1_RollBackToObjSelectTile_Coroutine()
    {
        yield return null;//Wait one frame to be sure the event isn't call by clicking on a interaction button
        if (_step != TutorialStep.Exploit1_ObjBuildFarm)
            yield break;

        GameManager.Instance.OnTileUnselected -= Exploit1_RollBackToObjSelectTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        ExploitationManager.Instance.OnRightTileSelected += OnFarmTileSelected;

        _exploit1_ObjBuildFarm.SetTrigger("Fold");
        _exploit1_ObjSelectTile.SetTrigger("Unfold");
        _step = TutorialStep.Exploit1_ObjSelectTile;
    }

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

    private void Exploit1bis_RollBackToObjSelectTile()
    {
        StartCoroutine(Exploit1bis_RollBackToObjSelectTile_Coroutine());
    }

    private IEnumerator Exploit1bis_RollBackToObjSelectTile_Coroutine()
    {
        yield return null;//Wait one frame to be sure the event isn't call by clicking on a interaction button
        if (_step != TutorialStep.Exploit1_ObjBuildWindmill)
            yield break;

        GameManager.Instance.OnTileUnselected -= Exploit1bis_RollBackToObjSelectTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        ExploitationManager.Instance.OnRightTileSelected += OnWindmillTileSelected;

        _exploit1_ObjBuildWindmill.SetTrigger("Fold");
        _exploit1bis_ObjSelectTile.SetTrigger("Unfold");
        _step = TutorialStep.Exploit1bis_ObjSelectTile;
    }

    private void OnWindmillBuilded()
    {
        if (_step != TutorialStep.Exploit1_ObjBuildWindmill) return;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        GameManager.Instance.OnTileUnselected -= Exploit1bis_RollBackToObjSelectTile;

        _targetTile.Highlight(false);
        _targetTile = null;

        _step = TutorialStep.Exploit1_ObjEndTurn;
        _exploit1_ObjBuildWindmill.SetTrigger("Fold");
        _exploit1_ObjEndTurn.SetTrigger("Unfold");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        GameManager.Instance.OnExploitationPhaseEnded += OnExploitationPhaseEnded;
    }

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
    private void InitializeExplo2()
    {
        GameManager.Instance.OnExplorationPhaseStarted -= InitializeExplo2;
        _explo2.SetActive(true);
        GameManager.Instance.GamePaused = true;
        _step = TutorialStep.Explo2_Init;
    }

    public void StartExplo2()
    {
        if (_step != TutorialStep.Explo2_Init) return;
        _explo2.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.Explo2_ObjEndPhase;
        _explo2_ObjEndPhase.SetTrigger("Unfold");
        GameManager.Instance.OnExplorationPhaseEnded += OnExplorationPhaseEnded2;
    }

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

    private void RollBackToObjSelectBasicTile()
    {
        StartCoroutine(RollBackToObjSelectBasicTile_Coroutine());
    }

    private IEnumerator RollBackToObjSelectBasicTile_Coroutine()
    {
        yield return null;//Wait one frame to be sure the event isn't call by clicking on a interaction button
        if (_step != TutorialStep.Expand2_ObjBuildTown)
            yield break;

        GameManager.Instance.OnTileUnselected -= RollBackToObjSelectBasicTile;
        ExploitationManager.Instance.OnInfraBuilded -= _infraBuildHandler;
        ExpansionManager.Instance.OnBasicTileSelected += OnBasicTileSelected;

        _expand2_ObjSelectTile.SetTrigger("Unfold");
        _expand2_ObjBuildTown.SetTrigger("Fold");
        _step = TutorialStep.Expand2_ObjSelectTile;
    }


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

    private void OnExpansionPhaseEnded2()
    {
        if (_step != TutorialStep.Expand2_ObjEndPhase) return;
        GameManager.Instance.OnExpansionPhaseEnded -= OnExpansionPhaseEnded2;

        //_step = TutorialStep.Exploit2_init;
        _expand2_ObjEndPhase.SetTrigger("Fold");
        //GameManager.Instance.OnExploitationPhaseStarted += ;
    }
    #endregion
}
