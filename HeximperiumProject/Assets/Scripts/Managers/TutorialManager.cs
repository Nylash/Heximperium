using System;
using System.Collections;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    private enum TutorialStep
    {
        None,
        S1_Intro,
        S2_IntroCommands,
        S3_CamMovement,
        S4_CamZoom,
        S5_CamCenter,
        S6_IntroScout,
        S7_SelectTile,
        S8_SpawnScout,
        S9_DirectScout,
        S10_EndPhase,
        S11_IntroClaim,
        S12_SelectTile,
        S13_ClaimTile,
        S14_EndPhase,
    }

    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Intro")]
    [SerializeField] private Animator _step1;
    [Header("_________________________________________________________")]
    [Header("Commands")]
    [SerializeField] private Animator _step2;
    [SerializeField] private Animator _commandsReminder_1;
    [SerializeField] private Animator _step3;
    [SerializeField] private Animator _step4;
    [SerializeField] private Animator _step5;
    [Header("_________________________________________________________")]
    [Header("Scout")]
    [SerializeField] private Animator _step6;
    [SerializeField] private Animator _commandsReminder_2;
    [SerializeField] private Animator _step7;
    [SerializeField] private Animator _step8;
    [SerializeField] private Animator _step9;
    [SerializeField] private Animator _step10;
    [Header("_________________________________________________________")]
    [Header("Claim")]
    [SerializeField] private Animator _step11;
    [SerializeField] private Animator _commandsReminder_3;
    [SerializeField] private Animator _step12;
    [SerializeField] private Animator _step13;
    [SerializeField] private Animator _step14;
    #endregion

    #region EVENTS
    public event Action OnTutorialStarted;
    #endregion

    #region VARIABLES
    private TutorialStep _step = TutorialStep.None;
    private bool _isSpawningScout;
    private bool _isClaimingTile;
    private Tile _targetTile;

    public Tile TargetTile { get => _targetTile; }
    public bool IsClaimingTile { get => _isClaimingTile; }
    public bool IsSpawningScout { get => _isSpawningScout; }
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
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        CameraManager.Instance.TutorialNoPopup = true;

        _step1.SetTrigger("Show");
        _step = TutorialStep.S1_Intro;
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.OnLoadingDone -= ShowStep1;
    }

    public void Button_ValidateStep1()
    {
        _step1.SetTrigger("Shrink");
        OnTutorialStarted?.Invoke();
        ShowStep2();
    }
    #endregion

    #region COMMANDS
    private void ShowStep2()
    {
        _step2.SetTrigger("Show");
        _step = TutorialStep.S2_IntroCommands;
        GameManager.Instance.GamePaused = true;
    }

    public void Button_ValidateStep2()
    {
        _step2.SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;
        ShowStep3();
    }

    private void ShowStep3()
    {
        if (_step != TutorialStep.S2_IntroCommands)
            return;

        _step = TutorialStep.S3_CamMovement;
        _commandsReminder_1.SetTrigger("Show");
        _step3.SetTrigger("Show");

        CameraManager.Instance.OnCameraMoved += ShowStep4;
    }

    private void ShowStep4()
    {
        if (_step != TutorialStep.S3_CamMovement)
            return;

        CameraManager.Instance.OnCameraMoved -= ShowStep4;

        _step = TutorialStep.S4_CamZoom;
        _step3.SetTrigger("Shrink");
        _step4.SetTrigger("Show");

        CameraManager.Instance.OnCameraZoomed += ShowStep5;
    }

    private void ShowStep5()
    {
        if (_step != TutorialStep.S4_CamZoom)
            return;

        CameraManager.Instance.OnCameraZoomed -= ShowStep5;

        _step = TutorialStep.S5_CamCenter;
        _step4.SetTrigger("Shrink");
        _step5.SetTrigger("Show");

        CameraManager.Instance.OnCameraCentered += ShowStep6;
    }
    #endregion

    #region SCOUT TUTORIAL
    private void ShowStep6()
    {
        if (_step != TutorialStep.S5_CamCenter)
            return;

        GameManager.Instance.GamePaused = true;
        CameraManager.Instance.OnCameraCentered -= ShowStep6;

        _step = TutorialStep.S6_IntroScout;
        _commandsReminder_1.SetTrigger("Shrink");
        _step5.SetTrigger("Shrink");
        _step6.SetTrigger("Show");
    }

    public void Button_ValidateStep6()
    {
        if (_step != TutorialStep.S6_IntroScout)
            return;

        _step6.SetTrigger("Shrink");
        ShowStep7();
    }

    private void ShowStep7()
    {
        if (_step != TutorialStep.S6_IntroScout)
            return;

        GameManager.Instance.GamePaused = false;

        _step = TutorialStep.S7_SelectTile;
        _commandsReminder_2.SetTrigger("Show");
        _step7.SetTrigger("Show");

        _isSpawningScout = true;
        ExplorationManager.Instance.OnScoutStartingPointSelected += ShowStep8;

        foreach (Tile item in ExploitationManager.Instance.Infrastructures)
        {
            if (item.TileData is InfrastructureData data && data.ScoutStartingPoint)
                item.Highlight(true);
        }
    }

    private void ShowStep8()
    {
        if (_step != TutorialStep.S7_SelectTile)
            return;

        ExplorationManager.Instance.OnScoutStartingPointSelected -= ShowStep8;

        _step = TutorialStep.S8_SpawnScout;
        _step7.SetTrigger("Shrink");
        _step8.SetTrigger("Show");

        ExplorationManager.Instance.OnScoutSpawned += ShowStep9;
        GameManager.Instance.OnTileUnselected += RollBackToStep7;

        foreach (Tile item in ExploitationManager.Instance.Infrastructures)
        {
            if (item.TileData is InfrastructureData data && data.ScoutStartingPoint)
                item.Highlight(false);
        }
    }

    private void RollBackToStep7()
    {
        if (_step != TutorialStep.S8_SpawnScout)
            return;
        StartCoroutine(Coroutine_RollBackToStep7());
    }

    private IEnumerator Coroutine_RollBackToStep7()
    {
        // Wait one frame to ensure that is a deselection and not the unselect called when doing an action
        yield return null;
        if (_step != TutorialStep.S8_SpawnScout)
            yield break;

        if (GameManager.Instance.SelectedTile != null)
        {
            if (GameManager.Instance.SelectedTile.TileData is InfrastructureData data && data.ScoutStartingPoint)
                yield break;
        }

        GameManager.Instance.OnTileUnselected -= RollBackToStep7;
        ExplorationManager.Instance.OnScoutSpawned -= ShowStep9;

        _step = TutorialStep.S7_SelectTile;
        _step8.SetTrigger("Shrink");
        _step7.SetTrigger("Show");

        ExplorationManager.Instance.OnScoutStartingPointSelected += ShowStep8;

        foreach (Tile item in ExploitationManager.Instance.Infrastructures)
        {
            if (item.TileData is InfrastructureData data && data.ScoutStartingPoint)
                item.Highlight(true);
        }
    }

    private void ShowStep9(Scout scout)
    {
        if (_step != TutorialStep.S8_SpawnScout)
            return;

        ExplorationManager.Instance.OnScoutSpawned -= ShowStep9;
        GameManager.Instance.OnTileUnselected -= RollBackToStep7;

        _step = TutorialStep.S9_DirectScout;
        _step8.SetTrigger("Shrink");
        _step9.SetTrigger("Show");

        _isSpawningScout = false;
        ExplorationManager.Instance.OnScoutDirected += ShowStep10;
    }

    private void ShowStep10()
    {
        if (_step != TutorialStep.S9_DirectScout)
            return;

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        ExplorationManager.Instance.OnScoutDirected -= ShowStep10;

        _step = TutorialStep.S10_EndPhase;
        _step9.SetTrigger("Shrink");
        _step10.SetTrigger("Show");

        GameManager.Instance.OnExplorationPhaseEnded += HideStep10;
        GameManager.Instance.OnExpansionPhaseStarted += ShowStep11;
    }

    private void HideStep10()
    {
        GameManager.Instance.OnExplorationPhaseEnded -= HideStep10;
        _step10.SetTrigger("Shrink");
        _commandsReminder_2.SetTrigger("Shrink");
    }
    #endregion

    #region CLAIM TUTORIAL
    private void ShowStep11()
    {
        if (_step != TutorialStep.S10_EndPhase)
            return;

        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        GameManager.Instance.GamePaused = true;

        GameManager.Instance.OnExpansionPhaseStarted -= ShowStep11;

        _step = TutorialStep.S11_IntroClaim;
        _step10.SetTrigger("Shrink");
        _step11.SetTrigger("Show");
    }

    public void Button_ValidateStep11()
    {
        _step11.SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;
        ShowStep12();
    }

    private void ShowStep12()
    {
        if (_step != TutorialStep.S11_IntroClaim)
            return;

        _step = TutorialStep.S12_SelectTile;
        _commandsReminder_3.SetTrigger("Show");
        _step12.SetTrigger("Show");

        _isClaimingTile = true;
        ExpansionManager.Instance.OnClaimableTileSelected += ShowStep13;
    }

    private void ShowStep13()
    {
        if (_step != TutorialStep.S12_SelectTile)
            return;

        ExpansionManager.Instance.OnClaimableTileSelected -= ShowStep13;

        _step = TutorialStep.S13_ClaimTile;
        _step12.SetTrigger("Shrink");
        _step13.SetTrigger("Show");

        GameManager.Instance.OnTileUnselected += RollBackToStep12;
        ExpansionManager.Instance.OnTileClaimed += ShowStep14;
    }

    private void RollBackToStep12()
    {
        if (_step != TutorialStep.S13_ClaimTile)
            return;
        StartCoroutine(Coroutine_RollBackToStep12());
    }

    private IEnumerator Coroutine_RollBackToStep12()
    {
        // Wait one frame to ensure that is a deselection and not the unselect called when doing an action
        yield return null;
        if (_step != TutorialStep.S13_ClaimTile)
            yield break;

        // Check if the new tile is claimable
        if (GameManager.Instance.SelectedTile != null)
        {
            if (!GameManager.Instance.SelectedTile.Claimed && GameManager.Instance.SelectedTile.IsOneNeighborClaimed())
                yield break;
        }

        GameManager.Instance.OnTileUnselected -= RollBackToStep12;
        ExpansionManager.Instance.OnTileClaimed -= ShowStep14;

        _step = TutorialStep.S12_SelectTile;
        _step13.SetTrigger("Shrink");
        _step12.SetTrigger("Show");

        ExpansionManager.Instance.OnClaimableTileSelected += ShowStep13;
    }

    private void ShowStep14(Tile tile)
    {
        if (_step != TutorialStep.S13_ClaimTile)
            return;

        ExpansionManager.Instance.OnTileClaimed -= ShowStep14;
        GameManager.Instance.OnTileUnselected -= RollBackToStep12;

        _step = TutorialStep.S14_EndPhase;
        _step13.SetTrigger("Shrink");
        _step14.SetTrigger("Show");

        _isClaimingTile = false;
        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;

        GameManager.Instance.OnExpansionPhaseEnded += HideStep14;
        GameManager.Instance.OnExploitationPhaseStarted += ShowStep15;
    }

    private void HideStep14()
    {
        _step14.SetTrigger("Shrink");
        _commandsReminder_3.SetTrigger("Shrink");
    }
    #endregion

    #region INFRASTRUCTURE TUTORIAL
    private void ShowStep15()
    {
        GameManager.Instance.OnExpansionPhaseEnded -= ShowStep15;
    }
    #endregion
}
