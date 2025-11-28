using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        S15_IntroInfrastructure,
        S16_SelectTile,
        S17_BuildInfrastructure,
        S18_EndTurn,
        S19_IntroNoMoreScout,
        S20_EndPhase,
        S21_IntroTown,
        S22_SelectTile,
        S23_BuildTown,
        S24_EndPhase,
        S25_WindmillIntro,
        S26_SelectTile,
        S27_BuildWindmill,
        S28_SelectWindmill,
        S29_UpgradeWindmill,
        S30_EndTurn,
        S31_IntroEntertainment,
        S32_SelectTile,
        S33_PlaceEntertainment,
        S34_EndGame,
        S35_Outro
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
    [Header("_________________________________________________________")]
    [Header("Farm")]
    [SerializeField] private Animator _step15;
    [SerializeField] private Animator _commandsReminder_4;
    [SerializeField] private Animator _step16;
    [SerializeField] private Animator _step17;
    [SerializeField] private Animator _step18;
    [Header("_________________________________________________________")]
    [Header("No more scout")]
    [SerializeField] private Animator _step19;
    [SerializeField] private Animator _commandsReminder_5;
    [SerializeField] private Animator _step20;
    [Header("_________________________________________________________")]
    [Header("Town")]
    [SerializeField] private Animator _step21;
    [SerializeField] private Animator _commandsReminder_6;
    [SerializeField] private Animator _step22;
    [SerializeField] private Animator _step23;
    [SerializeField] private Animator _step24;
    [Header("_________________________________________________________")]
    [Header("Windmill")]
    [SerializeField] private Animator _step25;
    [SerializeField] private Animator _commandsReminder_7;
    [SerializeField] private Animator _step26;
    [SerializeField] private Animator _step27;
    [SerializeField] private Animator _step28;
    [SerializeField] private Animator _step29;
    [SerializeField] private Animator _step30;
    [Header("_________________________________________________________")]
    [Header("Entertainment")]
    [SerializeField] private Animator _step31;
    [SerializeField] private Animator _commandsReminder_8;
    [SerializeField] private Animator _step32;
    [SerializeField] private Animator _step33;
    [SerializeField] private Animator _step34;
    [Header("_________________________________________________________")]
    [Header("Outro")]
    [SerializeField] private Animator _step35;
    #endregion

    #region EVENTS
    public event Action OnTutorialStarted;
    #endregion

    #region VARIABLES
    private TutorialStep _step = TutorialStep.None;
    private bool _isSpawningScout;
    private bool _isClaimingTile;
    private bool _isBuildingFarm;
    private bool _isBuildingTown;
    private bool _isBuildingWindmill;
    private bool _isUpgradingWindmill;

    public bool IsClaimingTile { get => _isClaimingTile; }
    public bool IsSpawningScout { get => _isSpawningScout; }
    public bool IsBuildingFarm { get => _isBuildingFarm; }
    public bool IsBuildingTown { get => _isBuildingTown; }
    public bool IsBuildingWindmill { get => _isBuildingWindmill; }
    public bool IsUpgradingWindmill { get => _isUpgradingWindmill; }
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
        GameManager.Instance.OnExpansionPhaseEnded -= HideStep14;
        _step14.SetTrigger("Shrink");
        _commandsReminder_3.SetTrigger("Shrink");
    }
    #endregion

    #region INFRASTRUCTURE TUTORIAL
    private void ShowStep15()
    {
        if (_step != TutorialStep.S14_EndPhase)
            return;

        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        GameManager.Instance.GamePaused = true;

        GameManager.Instance.OnExploitationPhaseStarted -= ShowStep15;

        _step = TutorialStep.S15_IntroInfrastructure;
        _step15.SetTrigger("Show");
    }

    public void Button_ValidateStep15()
    {
        _step15.SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;
        ShowStep16();
    }

    private void ShowStep16()
    {
        if (_step != TutorialStep.S15_IntroInfrastructure)
            return;

        _step = TutorialStep.S16_SelectTile;
        _commandsReminder_4.SetTrigger("Show");
        _step16.SetTrigger("Show");
        _isBuildingFarm = true;

        ExploitationManager.Instance.OnNotUpgradedTileSelected += ShowStep17;
    }

    private void ShowStep17()
    {
        if (_step != TutorialStep.S16_SelectTile)
            return;

        ExploitationManager.Instance.OnNotUpgradedTileSelected -= ShowStep17;

        _step = TutorialStep.S17_BuildInfrastructure;
        _step16.SetTrigger("Shrink");
        _step17.SetTrigger("Show");

        ExploitationManager.Instance.OnInfraBuilded += ShowStep18;
        GameManager.Instance.OnTileUnselected += RollBackToStep16;
    }

    private void RollBackToStep16()
    {
        if (_step != TutorialStep.S17_BuildInfrastructure)
            return;
        StartCoroutine(Coroutine_RollBackToStep16());
    }

    private IEnumerator Coroutine_RollBackToStep16()
    {
        // Wait one frame to ensure that is a deselection and not the unselect called when doing an action
        yield return null;
        if (_step != TutorialStep.S17_BuildInfrastructure)
            yield break;

        // Check if the new tile is upgradable
        if (GameManager.Instance.SelectedTile != null)
        {
            if (GameManager.Instance.SelectedTile.TileData is not InfrastructureData && GameManager.Instance.SelectedTile.Claimed)
                yield break;
        }
        GameManager.Instance.OnTileUnselected -= RollBackToStep16;
        ExploitationManager.Instance.OnInfraBuilded -= ShowStep18;

        _step = TutorialStep.S16_SelectTile;
        _step17.SetTrigger("Shrink");
        _step16.SetTrigger("Show");

        ExploitationManager.Instance.OnNotUpgradedTileSelected += ShowStep17;
    }

    private void ShowStep18(Tile tile)
    {
        if (_step != TutorialStep.S17_BuildInfrastructure)
            return;

        ExploitationManager.Instance.OnInfraBuilded -= ShowStep18;

        _step = TutorialStep.S18_EndTurn;
        _step17.SetTrigger("Shrink");
        _step18.SetTrigger("Show");
        _isBuildingFarm = false;

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;

        GameManager.Instance.OnExploitationPhaseEnded += HideStep18;
        GameManager.Instance.OnExplorationPhaseStarted += ShowStep19;
    }

    private void HideStep18()
    {
        GameManager.Instance.OnExploitationPhaseEnded -= HideStep18;

        _step18.SetTrigger("Shrink");
        _commandsReminder_4.SetTrigger("Shrink");
    }
    #endregion

    #region NO MORE SCOUT TUTORIAL
    private void ShowStep19()
    {
        if (_step != TutorialStep.S18_EndTurn)
            return;

        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        GameManager.Instance.GamePaused = true;

        GameManager.Instance.OnExplorationPhaseStarted -= ShowStep19;

        _step = TutorialStep.S19_IntroNoMoreScout;
        _step19.SetTrigger("Show");
    }

    public void Button_ValidateStep19()
    {
        _step19.SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;
        ShowStep20();
    }

    private void ShowStep20()
    {
        if (_step != TutorialStep.S19_IntroNoMoreScout)
            return;

        _step = TutorialStep.S20_EndPhase;
        _commandsReminder_5.SetTrigger("Show");
        _step20.SetTrigger("Show");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;

        GameManager.Instance.OnExplorationPhaseEnded += HideStep20;
        GameManager.Instance.OnExpansionPhaseStarted += ShowStep21;
    }

    private void HideStep20()
    {
        GameManager.Instance.OnExplorationPhaseEnded -= HideStep20;
        _commandsReminder_5.SetTrigger("Shrink");
        _step20.SetTrigger("Shrink");
    }
    #endregion

    #region TOWN TUTORIAL
    private void ShowStep21()
    {
        if (_step != TutorialStep.S20_EndPhase)
            return;

        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        GameManager.Instance.GamePaused = true;

        GameManager.Instance.OnExpansionPhaseStarted -= ShowStep21;

        _step = TutorialStep.S21_IntroTown;
        _step21.SetTrigger("Show");
    }

    public void Button_ValidateStep21()
    {
        _step21.SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;
        ResourcesManager.Instance.UpdateClaim(ExpansionManager.Instance.NewTownData.ClaimCost, Transaction.Gain); // Give extra claims to be able to build a town
        ShowStep22();
    }

    private void ShowStep22()
    {
        if (_step != TutorialStep.S21_IntroTown)
            return;

        _step = TutorialStep.S22_SelectTile;
        _commandsReminder_6.SetTrigger("Show");
        _step22.SetTrigger("Show");
        _isBuildingTown = true;

        ExpansionManager.Instance.OnTownableTileSelected += ShowStep23;
    }

    private void ShowStep23()
    {
        if (_step != TutorialStep.S22_SelectTile)
            return;

        ExpansionManager.Instance.OnTownableTileSelected -= ShowStep23;

        _step = TutorialStep.S23_BuildTown;
        _step22.SetTrigger("Shrink");
        _step23.SetTrigger("Show");

        ExploitationManager.Instance.OnInfraBuilded += ShowStep24;
        GameManager.Instance.OnTileUnselected += RollBackToStep22;
    }

    private void RollBackToStep22()
    {
        if (_step != TutorialStep.S23_BuildTown)
            return;
        StartCoroutine(Coroutine_RollBackToStep22());
    }

    private IEnumerator Coroutine_RollBackToStep22()
    {
        // Wait one frame to ensure that is a deselection and not the unselect called when doing an action
        yield return null;
        if (_step != TutorialStep.S23_BuildTown)
            yield break;

        // Check if the new tile allows building a town
        if (GameManager.Instance.SelectedTile != null)
        {
            if(GameManager.Instance.SelectedTile.TileData is BasicTileData || GameManager.Instance.SelectedTile.TileData is ResourceTileData)
                yield break;
        }

        GameManager.Instance.OnTileUnselected -= RollBackToStep22;
        ExploitationManager.Instance.OnInfraBuilded -= ShowStep24;

        _step = TutorialStep.S22_SelectTile;
        _step23.SetTrigger("Shrink");
        _step22.SetTrigger("Show");

        ExpansionManager.Instance.OnTownableTileSelected += ShowStep23;
    }

    private void ShowStep24(Tile tile)
    {
        if (_step != TutorialStep.S23_BuildTown)
            return;

        ExploitationManager.Instance.OnInfraBuilded -= ShowStep24;
        GameManager.Instance.OnTileUnselected -= RollBackToStep22;

        _step = TutorialStep.S24_EndPhase;
        _step23.SetTrigger("Shrink");
        _step24.SetTrigger("Show");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        _isBuildingTown = false;

        GameManager.Instance.OnExpansionPhaseEnded += HideStep24;
        GameManager.Instance.OnExploitationPhaseStarted += ShowStep25;
    }

    private void HideStep24()
    {
        GameManager.Instance.OnExpansionPhaseEnded -= HideStep24;
        _step24.SetTrigger("Shrink");
        _commandsReminder_6.SetTrigger("Shrink");
    }
    #endregion

    #region WINDMILL TUTORIAL
    private void ShowStep25()
    {
        if (_step != TutorialStep.S24_EndPhase)
            return;

        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        GameManager.Instance.GamePaused = true;
        _isBuildingWindmill = true;

        GameManager.Instance.OnExploitationPhaseStarted -= ShowStep25;

        _step = TutorialStep.S25_WindmillIntro;
        _step25.SetTrigger("Show");
    }

    public void Button_ValidateStep25()
    {
        _step25.SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        ResourcesManager.Instance.UpdateResource(
            new System.Collections.Generic.List<ResourceToIntMap> {
                new ResourceToIntMap(Resource.Gold, 20),
                new ResourceToIntMap(Resource.SpecialResources, 5)
            }, Transaction.Gain);// Give extra resources to be able to build and upgrade a windmill

        ShowStep26();
    }

    private void ShowStep26()
    {
        if (_step != TutorialStep.S25_WindmillIntro)
            return;

        _step = TutorialStep.S26_SelectTile;
        _commandsReminder_7.SetTrigger("Show");
        _step26.SetTrigger("Show");
        _isBuildingWindmill = true;

        ExploitationManager.Instance.OnFarmNeighborSelected += ShowStep27;
    }

    private void ShowStep27()
    {
        if (_step != TutorialStep.S26_SelectTile)
            return;

        ExploitationManager.Instance.OnFarmNeighborSelected -= ShowStep27;

        _step = TutorialStep.S27_BuildWindmill;
        _step26.SetTrigger("Shrink");
        _step27.SetTrigger("Show");

        ExploitationManager.Instance.OnInfraBuilded += ShowStep28;
        GameManager.Instance.OnTileUnselected += RollBackToStep26;
    }

    private void RollBackToStep26()
    {
        if (_step != TutorialStep.S27_BuildWindmill)
            return;
        StartCoroutine(Coroutine_RollBackToStep26());
    }

    private IEnumerator Coroutine_RollBackToStep26()
    {
        // Wait one frame to ensure that is a deselection and not the unselect called when doing an action
        yield return null;
        if (_step != TutorialStep.S27_BuildWindmill)
            yield break;

        // Check if the new tile is next to a farm
        if (GameManager.Instance.SelectedTile != null)
        {
            if (GameManager.Instance.SelectedTile.Claimed && GameManager.Instance.SelectedTile.TileData is not InfrastructureData)
            {
                foreach (Tile neighbor in GameManager.Instance.SelectedTile.Neighbors)
                {
                    if (!neighbor)
                        continue;
                    if (neighbor.TileData.Family == Family.Farm)
                        yield break;
                }
            }
        }

        GameManager.Instance.OnTileUnselected -= RollBackToStep26;
        ExploitationManager.Instance.OnInfraBuilded -= ShowStep28;

        _step = TutorialStep.S26_SelectTile;
        _step27.SetTrigger("Shrink");
        _step26.SetTrigger("Show");

        ExploitationManager.Instance.OnFarmNeighborSelected += ShowStep27;
    }

    private void ShowStep28(Tile tile)
    {
        if (_step != TutorialStep.S27_BuildWindmill)
            return;

        ExploitationManager.Instance.OnInfraBuilded -= ShowStep28;
        GameManager.Instance.OnTileUnselected -= RollBackToStep26;

        _step = TutorialStep.S28_SelectWindmill;
        _step27.SetTrigger("Shrink");
        _step28.SetTrigger("Show");
        _isBuildingWindmill = false;
        _isUpgradingWindmill = true;

        ExploitationManager.Instance.OnWindmillSelected += ShowStep29;
    }

    private void ShowStep29()
    {
        if (_step != TutorialStep.S28_SelectWindmill)
            return;

        ExploitationManager.Instance.OnWindmillSelected -= ShowStep29;

        _step = TutorialStep.S29_UpgradeWindmill;
        _step28.SetTrigger("Shrink");
        _step29.SetTrigger("Show");

        ExploitationManager.Instance.OnInfraBuilded += ShowStep30;
        GameManager.Instance.OnTileUnselected += RollBackToStep28;
    }

    private void RollBackToStep28()
    {
        if (_step != TutorialStep.S29_UpgradeWindmill)
            return;
        StartCoroutine(Coroutine_RollBackToStep28());
    }

    private IEnumerator Coroutine_RollBackToStep28()
    {
        // Wait one frame to ensure that is a deselection and not the unselect called when doing an action
        yield return null;
        if (_step != TutorialStep.S29_UpgradeWindmill)
            yield break;

        // Check if the new tile is a windmill
        if (GameManager.Instance.SelectedTile != null)
        {
            if (GameManager.Instance.SelectedTile.TileData.Family == Family.Mill)
                yield break;
        }

        GameManager.Instance.OnTileUnselected -= RollBackToStep28;
        ExploitationManager.Instance.OnInfraBuilded -= ShowStep30;

        _step = TutorialStep.S28_SelectWindmill;
        _step29.SetTrigger("Shrink");
        _step28.SetTrigger("Show");

        ExploitationManager.Instance.OnWindmillSelected += ShowStep29;
    }

    private void ShowStep30(Tile tile)
    {
        if (_step != TutorialStep.S29_UpgradeWindmill)
            return;

        ExploitationManager.Instance.OnInfraBuilded -= ShowStep30;
        GameManager.Instance.OnTileUnselected -= RollBackToStep28;

        _step = TutorialStep.S30_EndTurn;
        _step29.SetTrigger("Shrink");
        _step30.SetTrigger("Show");
        _isUpgradingWindmill = false;

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
        
        GameManager.Instance.OnExploitationPhaseEnded += HideStep30;
        GameManager.Instance.OnEntertainmentPhaseStarted += ShowStep31;
    }

    private void HideStep30()
    {
        GameManager.Instance.OnExploitationPhaseEnded -= HideStep30;
        _step30.SetTrigger("Shrink");
        _commandsReminder_7.SetTrigger("Shrink");
    }
    #endregion

    #region ENTERTAINMENT TUTORIAL
    private void ShowStep31()
    {
        if (_step != TutorialStep.S30_EndTurn)
            return;

        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;
        GameManager.Instance.GamePaused = true;

        GameManager.Instance.OnEntertainmentPhaseStarted -= ShowStep31;

        _step = TutorialStep.S31_IntroEntertainment;
        _step31.SetTrigger("Show");
    }

    public void Button_ValidateStep31()
    {
        _step31.SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;
        ShowStep32();
    }

    private void ShowStep32()
    {
        if (_step != TutorialStep.S31_IntroEntertainment)
            return;

        _step = TutorialStep.S32_SelectTile;
        _commandsReminder_8.SetTrigger("Show");
        _step32.SetTrigger("Show");

        EntertainmentManager.Instance.OnTileAllowingEntSelected += ShowStep33;
    }

    private void ShowStep33()
    {
        if (_step != TutorialStep.S32_SelectTile)
            return;

        EntertainmentManager.Instance.OnTileAllowingEntSelected -= ShowStep33;

        _step = TutorialStep.S33_PlaceEntertainment;
        _step32.SetTrigger("Shrink");
        _step33.SetTrigger("Show");

        EntertainmentManager.Instance.OnEntertainmentSpawned += ShowStep34;
        GameManager.Instance.OnTileUnselected += RollBackToStep32;
    }

    private void RollBackToStep32()
    {
        if (_step != TutorialStep.S33_PlaceEntertainment)
            return;
        StartCoroutine(Coroutine_RollBackToStep32());
    }

    private IEnumerator Coroutine_RollBackToStep32()
    {
        // Wait one frame to ensure that is a deselection and not the unselect called when doing an action
        yield return null;
        if (_step != TutorialStep.S33_PlaceEntertainment)
            yield break;

        // Check if the new tile allows placing entertainment
        if (GameManager.Instance.SelectedTile != null)
        {
            if (GameManager.Instance.SelectedTile.CanReceiveEntertainment())
                yield break;
        }

        GameManager.Instance.OnTileUnselected -= RollBackToStep32;
        EntertainmentManager.Instance.OnEntertainmentSpawned -= ShowStep34;

        _step = TutorialStep.S32_SelectTile;
        _step33.SetTrigger("Shrink");
        _step32.SetTrigger("Show");

        EntertainmentManager.Instance.OnTileAllowingEntSelected += ShowStep33;
    }

    private void ShowStep34(Entertainment ent)
    {
        if (_step != TutorialStep.S33_PlaceEntertainment)
            return;

        EntertainmentManager.Instance.OnEntertainmentSpawned -= ShowStep34;

        _step = TutorialStep.S34_EndGame;
        _step33.SetTrigger("Shrink");
        _step34.SetTrigger("Show");

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;

        GameManager.Instance.OnGameFinished += ShowStep35;
    }
    #endregion

    private void ShowStep35()
    {
        if (_step != TutorialStep.S34_EndGame)
            return;

        GameManager.Instance.OnGameFinished -= ShowStep35;

        _step = TutorialStep.S35_Outro;
        _step34.SetTrigger("Shrink");
        _step35.SetTrigger("Show");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
