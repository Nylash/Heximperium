using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UIManager : Singleton<UIManager>
{
    public const string BEST_SCORE_KEY = "BestScore";

    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Resources Bar")]
    [SerializeField] private TextMeshProUGUI _scoutsLimitText;
    [SerializeField] private TextMeshProUGUI _claimText;
    [SerializeField] private TextMeshProUGUI _townsLimitText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _srText;
    [SerializeField] private TextMeshProUGUI _carnivalistText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Color _colorCantAfford;
    [SerializeField] private Color _colorCantAffordVFX;
    [SerializeField] private Color _colorIvory;
    [SerializeField] private Color _colorEnhancementNewEffect;
    [Header("_________________________________________________________")]
    [Header("Phase UI")]
    [SerializeField] private TextMeshProUGUI _confirmPhaseButtonText;
    [SerializeField] private TextMeshProUGUI _turnCounterText;
    [SerializeField] private Color _colorExplo;
    [SerializeField] private Color _colorBotExplo;
    [SerializeField] private Color _colorExpand;
    [SerializeField] private Color _colorBotExpand;
    [SerializeField] private Color _colorExploit;
    [SerializeField] private Color _colorBotExploit;
    [SerializeField] private Color _colorEntertain;
    [SerializeField] private Color _colorBotEntertain;
    [SerializeField] private Animator _popUpExploPhase;
    [SerializeField] private Animator _popUpExpandPhase;
    [SerializeField] private Animator _popUpExploitPhase;
    [SerializeField] private Animator _popUpEntertainPhase;
    [SerializeField] private Animator _annunciatorNewTurn;
    [SerializeField] private Animator _annunciatorExploration;
    [SerializeField] private Animator _annunciatorExpansion;
    [SerializeField] private Animator _annunciatorExploitation;
    [SerializeField] private Animator _annunciatorEntertainment;
    [SerializeField] private Button _buttonEndPhase;
    [SerializeField] private Material _phaseMaterial;
    [Header("_________________________________________________________")]
    [Header("Menu")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _confirmQuit;
    [SerializeField] private GameObject _confirmMainMenu;
    [SerializeField] private GameObject _endMenu;
    [SerializeField] private GameObject _settingsMenu;
    [Header("_________________________________________________________")]
    [Header("End Menu")]
    [SerializeField] private UI_ScoreCounter _minstrelCounter;
    [SerializeField] private UI_ScoreCounter _pavilionCounter;
    [SerializeField] private UI_ScoreCounter _paradeCounter;
    [SerializeField] private UI_ScoreCounter _totalScoreCounter;
    [SerializeField] private UI_ScoreCounter _mysticCounter;
    [SerializeField] private TextMeshProUGUI _bestScore;
    [SerializeField] private float _targetDuration = 3f;
    [SerializeField] private float _minPointsPerSecond = 50f;
    [Header("_________________________________________________________")]
    [Header("Trade Menu")]
    [SerializeField] private GameObject _tradeMenuButton;
    [SerializeField] private GameObject _tradeMenu;
    [SerializeField] private GameObject _buyButton;
    [SerializeField] private GameObject _sellButton;
    [SerializeField] private GameObject _buyClaimButton;
    [Header("_________________________________________________________")]
    [Header("Upgrades Menu")]
    [SerializeField] private GameObject _upgradesMenuButton;
    [SerializeField] private GameObject _upgradesMenu;
    [SerializeField] private TextMeshProUGUI _counterForNextUpgrade;
    [SerializeField] private TextMeshProUGUI _upgrade1;
    [SerializeField] private TextMeshProUGUI _upgrade2;
    [SerializeField] private TextMeshProUGUI _upgrade3;
    [SerializeField] private TextMeshProUGUI _upgrade4;
    [SerializeField] private TextMeshProUGUI _upgrade5;
    [SerializeField] private TextMeshProUGUI _upgrade6;
    [SerializeField] private TextMeshProUGUI _upgrade7;
    [SerializeField] private TextMeshProUGUI _upgrade8;
    [SerializeField] private TextMeshProUGUI _upgrade9;
    [Header("_________________________________________________________")]
    [Header("Upgrades Choice Menu")]
    [SerializeField] private GameObject _upgradesChoiceMenuObject;
    [SerializeField] private TextMeshProUGUI _exploChoiceTitle;
    [SerializeField] private TextMeshProUGUI _exploChoiceDetail;
    [SerializeField] private TextMeshProUGUI _expandChoiceTitle;
    [SerializeField] private TextMeshProUGUI _expandChoiceDetail;
    [SerializeField] private TextMeshProUGUI _exploitChoiceTitle;
    [SerializeField] private TextMeshProUGUI _exploitChoiceDetail;
    [SerializeField] private TextMeshProUGUI _entertainChoiceTitle;
    [SerializeField] private TextMeshProUGUI _entertainChoiceDetail;
    [SerializeField] private Button _rerollExplo;
    [SerializeField] private Button _rerollExpand;
    [SerializeField] private Button _rerollExploit;
    [SerializeField] private Button _rerollEntertain;
    [SerializeField] private Button _confirmExplo;
    [SerializeField] private Button _confirmExpand;
    [SerializeField] private Button _confirmExploit;
    [SerializeField] private Button _confirmEntertain;
    [SerializeField] private Animator _animatorExplo;
    [SerializeField] private Animator _animatorExpand;
    [SerializeField] private Animator _animatorExploit;
    [SerializeField] private Animator _animatorEntertain;
    [Header("_________________________________________________________")]
    [Header("Scouts visibility button")]
    [SerializeField] private Image _scoutsVisibilityButton;
    [SerializeField] private Sprite _scoutsVisibilityOff;
    [SerializeField] private Sprite _scoutsVisibilityOn;
    [Header("_________________________________________________________")]
    [Header("Ent visibility button")]
    [SerializeField] private Image _entVisibilityButton;
    [SerializeField] private Sprite _entVisibilityOff;
    [SerializeField] private Sprite _entVisibilityOn;
    [Header("_________________________________________________________")]
    [Header("Show Income button")]
    [SerializeField] private Image _showIncomeButton;
    [SerializeField] private Sprite _showIncomeOff;
    [SerializeField] private Sprite _showIncomeOn;
    [Header("_________________________________________________________")]
    [Header("Show Entertainment Placement button")]
    [SerializeField] private Image _showEntPlacementButton;
    [SerializeField] private Sprite _showEntPlacementOff;
    [SerializeField] private Sprite _showEntPlacementOn;
    [Header("_________________________________________________________")]
    [Header("Show Details Popup button")]
    [SerializeField] private Image _showDetailsPopupButton;
    [SerializeField] private Sprite _showDetailsPopupOff;
    [SerializeField] private Sprite _showDetailsPopupOn;
    [Header("_________________________________________________________")]
    [Header("Show Enhanceable Status button")]
    [SerializeField] private Image _showEnhanceableButton;
    [SerializeField] private Sprite _showEnhanceableOff;
    [SerializeField] private Sprite _showEnhanceableOn;
    [Header("_________________________________________________________")]
    [Header("VFX Anchors")]
    [SerializeField] private RectTransform _vfxAnchorEndConfetti1;
    [SerializeField] private RectTransform _vfxAnchorEndConfetti2;
    [SerializeField] private RectTransform _vfxAnchorEndFirework1;
    [SerializeField] private RectTransform _vfxAnchorEndFirework2;
    [SerializeField] private RectTransform _vfxAnchorEndCurtain;
    [SerializeField] private RectTransform _vfxAnchorClaim;
    [SerializeField] private RectTransform _vfxAnchorGold;
    [SerializeField] private RectTransform _vfxAnchorSR;
    [SerializeField] private RectTransform _vfxAnchorCarnivalist;
    [Header("_________________________________________________________")]
    [Header("Various Objects")]
    [SerializeField] private Animator _scoutHint;
    [SerializeField] private Animator _revealAnywhereHint;
    [SerializeField] private Animator _buildTownHint;
    #endregion

    #region VARIABLES
    private bool _areScoutsVisible;
    private bool _areEntVisible;
    private bool _uiPhaseInAnimation;
    private bool _areIncomesShown;
    private bool _areEntPlacementShown;
    private bool _areEnhanceableStatusShown;
    #endregion

    #region ACCESSORS
    public Color ColorCantAfford { get => _colorCantAfford;}
    public RectTransform VfxAnchorEndConfetti1 { get => _vfxAnchorEndConfetti1; }
    public RectTransform VfxAnchorEndConfetti2 { get => _vfxAnchorEndConfetti2; }
    public RectTransform VfxAnchorEndFirework1 { get => _vfxAnchorEndFirework1; }
    public RectTransform VfxAnchorEndFirework2 { get => _vfxAnchorEndFirework2; }
    public RectTransform VfxAnchorClaim { get => _vfxAnchorClaim; }
    public RectTransform VfxAnchorGold { get => _vfxAnchorGold; }
    public RectTransform VfxAnchorSR { get => _vfxAnchorSR; }
    public Button ButtonEndPhase { get => _buttonEndPhase; }
    public GameObject UpgradesChoiceMenuObject { get => _upgradesChoiceMenuObject; }
    public Color ColorEntertain { get => _colorEntertain; }
    public bool UiPhaseInAnimation { get => _uiPhaseInAnimation; set => _uiPhaseInAnimation = value; }
    public Animator ScoutHint { get => _scoutHint; }
    public Color ColorExpand { get => _colorExpand; }
    public Color ColorExploit { get => _colorExploit; }
    public Color ColorExplo { get => _colorExplo; }
    public RectTransform VfxAnchorCarnivalist { get => _vfxAnchorCarnivalist; }
    public bool AreIncomesShown { get => _areIncomesShown; }
    public bool AreEntPlacementShown { get => _areEntPlacementShown; }
    public TextMeshProUGUI ExploChoiceTitle { get => _exploChoiceTitle; }
    public TextMeshProUGUI ExploChoiceDetail { get => _exploChoiceDetail; }
    public TextMeshProUGUI ExpandChoiceTitle { get => _expandChoiceTitle; }
    public TextMeshProUGUI ExpandChoiceDetail { get => _expandChoiceDetail; }
    public TextMeshProUGUI ExploitChoiceTitle { get => _exploitChoiceTitle; }
    public TextMeshProUGUI ExploitChoiceDetail { get => _exploitChoiceDetail; }
    public TextMeshProUGUI EntertainChoiceTitle { get => _entertainChoiceTitle; }
    public TextMeshProUGUI EntertainChoiceDetail { get => _entertainChoiceDetail; }
    public Animator RevealAnywhereHint { get => _revealAnywhereHint; }
    public Color ColorIvory { get => _colorIvory; }
    public Animator BuildTownHint { get => _buildTownHint; }
    public Color ColorEnhancementNewEffect { get => _colorEnhancementNewEffect; }
    public Button ConfirmExplo { get => _confirmExplo; }
    public Button ConfirmExpand { get => _confirmExpand; }
    public Button ConfirmExploit { get => _confirmExploit; }
    public Button ConfirmEntertain { get => _confirmEntertain; }
    public bool AreEnhanceableStatusShown { get => _areEnhanceableStatusShown; }
    public RectTransform VfxAnchorEndCurtain { get => _vfxAnchorEndCurtain; }
    public bool AreScoutsVisible { get => _areScoutsVisible; }
    public bool AreEntVisible { get => _areEntVisible; }
    public Color ColorCantAffordVFX { get => _colorCantAffordVFX; }
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnNewTurn += UpdateTurnCounterText;
        GameManager.Instance.OnLastTurnStarted += () => _annunciatorNewTurn.GetComponentInChildren<TextMeshProUGUI>().text = "Last Turn!";

        GameManager.Instance.OnExplorationPhaseStarted += NewPhaseStarted;
        ExplorationManager.Instance.OnPhaseFinalized += () => PhaseEnded(Phase.Explore);
        GameManager.Instance.OnExpansionPhaseStarted += NewPhaseStarted;
        ExpansionManager.Instance.OnPhaseFinalized += () => PhaseEnded(Phase.Expand);
        GameManager.Instance.OnExploitationPhaseStarted += NewPhaseStarted;
        ExploitationManager.Instance.OnPhaseFinalized += () => PhaseEnded(Phase.Exploit);
        GameManager.Instance.OnEntertainmentPhaseStarted += NewPhaseStarted;
        EntertainmentManager.Instance.OnPhaseFinalized += () => PhaseEnded(Phase.Entertain);

        GameManager.Instance.OnExplorationPhaseStarted += () => ScoutsVisibility(true);
        GameManager.Instance.OnExpansionPhaseStarted += () => ScoutsVisibility(false);
        GameManager.Instance.OnEntertainmentPhaseStarted += () => EntertainmentsVisibility(true);
        GameManager.Instance.OnExplorationPhaseStarted -= () => EntertainmentsVisibility(false);

        GameManager.Instance.OnGameFinished += GameFinished;

        ExplorationManager.Instance.OnScoutsLimitModified += UpdateScoutLimit;

        EntertainmentManager.Instance.OnScoreUpdated += () => _scoreText.text = EntertainmentManager.Instance.Score.ToString();
    }

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        UpdateScoutLimit();
        UpdateClaimUI(ResourcesManager.Instance.Claim);
        UpdateCarnivalistUI(ResourcesManager.Instance.Carnivalist);
        UpdateResourceUI(Resource.Gold, ResourcesManager.Instance.GetResourceStock(Resource.Gold));
        UpdateResourceUI(Resource.SpecialResources, ResourcesManager.Instance.GetResourceStock(Resource.SpecialResources));
        UpdateTownLimit();
        UpdateTurnCounterText(1);
    }

    #region RESOURCES BAR UI
    private void UpdateScoutLimit()
    {
        _scoutsLimitText.text = ExplorationManager.Instance.CurrentScoutsCount + "/" + ExplorationManager.Instance.ScoutsLimit;
    }

    public void UpdateClaimUI(int value)
    {
        _claimText.text = value.ToString();
    }

    public void UpdateCarnivalistUI(int value)
    {
        _carnivalistText.text = value.ToString();
    }

    public void UpdateResourceUI(Resource resource, int value)
    {
        switch (resource)
        {
            case Resource.Gold:
                _goldText.text = value.ToString();
                break;
            case Resource.SpecialResources:
                _srText.text = value.ToString();
                break;
        }
        if(_tradeMenu.activeSelf)
            UpdateTradeTextsColors();
    }

    public void UpdateTownLimit()
    {
        _townsLimitText.text = ExploitationManager.Instance.GetTownCount() + "/" + 
            (ExploitationManager.Instance.GetTownLimit() + ExploitationManager.Instance.GetTownCount());
    }

    public void TradeMenu()
    {
        if (_tradeMenu.activeSelf)
            _tradeMenu.GetComponent<Animator>().SetTrigger("Fold");
        else
        {
            if (_upgradesMenu.activeSelf)
                UpgradesMenu();
            _tradeMenu.SetActive(true);
            UpdateTradeTextsColors();

            PopUpManager.Instance.ShowTradeTutoPopUp();
        }
    }

    public void BuySR()
    {
        if (ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.BuySRCost))
        {
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.BuySRCost, Transaction.Spent);
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.BuySRGain, Transaction.Gain);
        }
    }

    public void BuyGold()
    {
        if (ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.BuyGoldCost))
        {
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.BuyGoldCost, Transaction.Spent);
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.BuyGoldGain, Transaction.Gain);
        }
    }

    public void BuyClaim()
    {
        if (ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.BuyClaimCost))
        {
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.BuyClaimCost, Transaction.Spent);
            ResourcesManager.Instance.UpdateClaim(ResourcesManager.Instance.BuyClaimGain, Transaction.Gain);
            ExpansionManager.Instance.UpdateInteractableTiles();
        }
    }

    private void UpdateTradeTextsColors()
    {
        if (!ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.BuySRCost))
        {
            foreach (TextMeshProUGUI text in _buyButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = _colorCantAfford;
        }
        else
        {
            foreach (TextMeshProUGUI text in _buyButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = _colorIvory;
        }
        if (!ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.BuyGoldCost))
        {
            foreach (TextMeshProUGUI text in _sellButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = _colorCantAfford;
        }
        else
        {
            foreach (TextMeshProUGUI text in _sellButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = _colorIvory;
        }
        if (!ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.BuyClaimCost))
        {
            foreach (TextMeshProUGUI text in _buyClaimButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = _colorCantAfford;
        }
        else
        {
            foreach (TextMeshProUGUI text in _buyClaimButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = _colorIvory;
        }
    }
    #endregion

    #region FILTER UI
    //OnClick for UI button
    public void ClickScoutVisibility() => ScoutsVisibility(!_areScoutsVisible);

    public void ClickEntVisiblity() => EntertainmentsVisibility(!_areEntVisible);

    public void UnitsVisibility(string unitType)
    {
        if (unitType == "Scouts")
            ScoutsVisibility(!_areScoutsVisible);
        else if (unitType == "Entertainments")
            EntertainmentsVisibility(!_areEntVisible);
        else
            Debug.LogError("Unknown unit type for visibility toggle : " + unitType);
    }

    public void EntertainmentsVisibility(bool visible)
    {
        if (visible == _areEntVisible)
            return;

        _areEntVisible = visible;

        _entVisibilityButton.sprite = _areEntVisible ? _entVisibilityOn : _entVisibilityOff;

        foreach (Entertainment item in EntertainmentManager.Instance.Entertainments)
        {
            item.EntertainmentVisibility(visible);
        }
    }

    public void ScoutsVisibility(bool visible)
    {
        if (visible == _areScoutsVisible)
            return;

        _areScoutsVisible = visible;

        _scoutsVisibilityButton.sprite = _areScoutsVisible ? _scoutsVisibilityOn : _scoutsVisibilityOff;

        foreach (Scout item in ExplorationManager.Instance.Scouts)
        {
            item.ScoutVisibility(visible);
        }
    }

    public void SwitchIncomesVisibility()
    {
        _areIncomesShown = !_areIncomesShown;
        _showIncomeButton.sprite = _areIncomesShown ? _showIncomeOn : _showIncomeOff;
        foreach (Tile tile in ExplorationManager.Instance.RevealedTiles)
        {
            tile.ShowIncomeUI(_areIncomesShown);
        }
    }

    public void SwitchEntPlacementVisibility()
    {
        _areEntPlacementShown = !_areEntPlacementShown;
        _showEntPlacementButton.sprite = _areEntPlacementShown ? _showEntPlacementOn : _showEntPlacementOff;
        foreach (Tile tile in ExplorationManager.Instance.RevealedTiles)
        {
            tile.ShowEntPlacementUI(_areEntPlacementShown);
        }
    }

    public void SwitchEnhanceableStatusVisibility()
    {
        _areEnhanceableStatusShown = !_areEnhanceableStatusShown;
        _showEnhanceableButton.sprite = _areEnhanceableStatusShown ? _showEnhanceableOn : _showEnhanceableOff;
        foreach (Tile tile in ExpansionManager.Instance.ClaimedTiles)
        {
            tile.ShowEnhanceableTileStatus(_areEnhanceableStatusShown);
        }
    }

    public void SwitchPopupDetailsVisibility()
    {
        PopUpManager.Instance.ShowSourcesOnPopUp = !PopUpManager.Instance.ShowSourcesOnPopUp;
        _showDetailsPopupButton.sprite = PopUpManager.Instance.ShowSourcesOnPopUp ? _showDetailsPopupOn : _showDetailsPopupOff;
    }
    #endregion

    #region PHASE UI
    public void ConfirmPhase()
    {
        GameManager.Instance.ConfirmPhase();
    }
    
    public void ForceExploColor()
    {
        _phaseMaterial.color = _colorExplo;
    }

    private void PhaseEnded(Phase endedPhase)
    {
        switch (endedPhase)
        {
            case Phase.Explore:
                _popUpExploPhase.SetTrigger("Hide");
                break;
            case Phase.Expand:
                _popUpExpandPhase.SetTrigger("Hide");
                break;
            case Phase.Exploit:
                _popUpExploitPhase.SetTrigger("Hide");
                break;
            case Phase.Entertain:
                _popUpEntertainPhase.SetTrigger("Hide");
                break;
            default:
                break;
        }
    }

    private void NewPhaseStarted()
    {
        switch (GameManager.Instance.CurrentPhase)
        {
            case Phase.Explore:
                _confirmPhaseButtonText.text = "End Phase";
                _phaseMaterial.color = _colorExplo;
                _annunciatorNewTurn.SetTrigger("Show");
                _annunciatorExploration.SetTrigger("Show");
                _popUpExploPhase.SetTrigger("Show");
                break;
            case Phase.Expand:
                _confirmPhaseButtonText.text = "End Phase";
                _phaseMaterial.color = _colorExpand;
                _annunciatorExpansion.SetTrigger("Show");
                _popUpExpandPhase.SetTrigger("Show");
                break;
            case Phase.Exploit:
                if (!GameManager.Instance.EntertainPhaseTurns.Contains(GameManager.Instance.TurnCounter))
                    _confirmPhaseButtonText.text = "End Turn";
                else
                    _confirmPhaseButtonText.text = "End Phase";
                _phaseMaterial.color = _colorExploit;
                _annunciatorExploitation.SetTrigger("Show");
                _popUpExploitPhase.SetTrigger("Show");
                break;
            case Phase.Entertain:
                if (GameManager.Instance.IsLastTurn)
                    _confirmPhaseButtonText.text = "End Game";
                else
                    _confirmPhaseButtonText.text = "End Turn";
                _phaseMaterial.color = _colorEntertain;
                _annunciatorEntertainment.SetTrigger("Show");
                _popUpEntertainPhase.SetTrigger("Show");
                if (!_areEntVisible)
                    UnitsVisibility("Entertainments");
                break;
        }
    }

    public void UpdateTurnCounterText(int turnCounter)
    {
        _turnCounterText.text = "Turn : " + turnCounter + "/" + GameManager.Instance.TurnLimit;
        UpdateTurnCounterBeforeNextUpgrade(turnCounter);
    }
    #endregion

    #region END SCORE UI
    private void GameFinished()
    {
        if (TutorialManager.Instance != null)
            return;

        _endMenu.SetActive(true);

        StartCoroutine(PlayEndSequence());
    }

    private IEnumerator PlayEndSequence()
    {
        var em = EntertainmentManager.Instance;

        int minstrel = em.GetPointsFromMinstrelStage();
        int pavilion = em.GetPointsFromTastingPavilion();
        int parade = em.GetPointsFromParadeRoute();
        int mystic = em.GetPointsFromMysticGarden();
        int total = em.Score;

        int maxDetail = Mathf.Max(minstrel, pavilion, parade, mystic);

        float requiredSpeed = maxDetail / _targetDuration;
        float pointsPerSecond = Mathf.Max(requiredSpeed, _minPointsPerSecond);
        float longestDetailDuration = maxDetail / pointsPerSecond;

        _totalScoreCounter.CountTo(total, longestDetailDuration);
        _minstrelCounter.CountTo(minstrel, minstrel / pointsPerSecond);
        _pavilionCounter.CountTo(pavilion, pavilion / pointsPerSecond);
        _paradeCounter.CountTo(parade, parade / pointsPerSecond);
        _mysticCounter.CountTo(mystic, mystic / pointsPerSecond);

        yield return new WaitForSeconds(longestDetailDuration);

        JuiceManager.Instance.EndGameEndCountingVFX();

        _totalScoreCounter.GetComponent<Animator>().SetTrigger("Pulse");

        int bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, -1);
        _bestScore.gameObject.SetActive(true);
        if (EntertainmentManager.Instance.Score > bestScore)
        {
            PlayerPrefs.SetInt(BEST_SCORE_KEY, EntertainmentManager.Instance.Score);
            _bestScore.text = "New Best Score !";
            _bestScore.GetComponent<Animator>().SetTrigger("Pulse");
        }
        else
        {
            _bestScore.text = "Current Best Score : " + PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        }
    }
    #endregion

    #region MENU
    public void OpenCloseMenu()
    {
        if (PopUpManager.Instance.LockedPopUps.Count > 0)
        {
            PopUpManager.Instance.CloseAllLockedPopup();
            return;
        }

        if (_confirmQuit.activeSelf)
        {
            ConfirmQuit();
            return;
        }
        if (_confirmMainMenu.activeSelf)
        {
            ConfirmMainMenu();
            return;
        }

        if (_menu.activeSelf)
        {
            _menu.GetComponent<Animator>().SetTrigger("Hide");
            GameManager.Instance.GamePaused = false;
            PopUpManager.Instance.ResetPopUp(null);
        }
        else
        {
            _menu.SetActive(true);
            GameManager.Instance.GamePaused = true;
            PopUpManager.Instance.ResetPopUp(null);
        }
    }

    public void SettingsMenu()
    {
        if (_settingsMenu.activeSelf)
        {
            _settingsMenu.GetComponent<Animator>().SetTrigger("Hide");
        }
        else
        {
            _settingsMenu.SetActive(true);
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ConfirmQuit()
    {
        if (_confirmQuit.activeSelf)
            _confirmQuit.GetComponent<Animator>().SetTrigger("Hide");
        else
            _confirmQuit.SetActive(true);
    }

    public void ConfirmMainMenu()
    {
        if (_confirmMainMenu.activeSelf)
            _confirmMainMenu.GetComponent<Animator>().SetTrigger("Hide");
        else
            _confirmMainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region UPGRADES MENU
    public void UpgradesMenu()
    {
        if (_upgradesMenu.activeSelf)
        {
            _upgradesMenu.GetComponent<Animator>().SetTrigger("Fold");
        }
        else
        {
            if (_tradeMenu.activeSelf)
                TradeMenu();
            _upgradesMenu.SetActive(true);
        }
    }

    public void UpgradesChoiceMenu(bool activateRerollExplo, bool activateRerollExpand, bool activateRerollExploit, bool activateRerollEnt)
    {
        if (_upgradesChoiceMenuObject.activeSelf)
        {
            _upgradesChoiceMenuObject.GetComponent<Animator>().SetTrigger("Fold");
            GameManager.Instance.GamePaused = false;
        }
        else
        {
            if (_tradeMenu.activeSelf)
                TradeMenu();
            if (_upgradesMenu.activeSelf)
                UpgradesMenu();
            if (activateRerollExplo)
                _rerollExplo.interactable = true;
            else
                _rerollExplo.gameObject.SetActive(false);
            if (activateRerollExpand)
                _rerollExpand.interactable = true;
            else
                _rerollExpand.gameObject.SetActive(false);
            if (activateRerollExploit)
                _rerollExploit.interactable = true;
            else
                _rerollExploit.gameObject.SetActive(false);
            if (activateRerollEnt)
                _rerollEntertain.interactable = true;
            else
                _rerollEntertain.gameObject.SetActive(false);
            _upgradesChoiceMenuObject.SetActive(true);
            GameManager.Instance.GamePaused = true;
        }
    }

    public void ConfirmUpgrade(int phase)
    {
        UpgradesManager.Instance.ConfirmUpgrade((Phase)phase);
    }

    public void RerollUpgrade(int phase)
    {
        switch ((Phase)phase)
        {
            case Phase.Explore:
                _animatorExplo.SetTrigger("Reroll");
                _rerollExplo.interactable = false;
                break;
            case Phase.Expand:
                _animatorExpand.SetTrigger("Reroll");
                _rerollExpand.interactable = false;
                break;
            case Phase.Exploit:
                _animatorExploit.SetTrigger("Reroll");
                _rerollExploit.interactable = false;
                break;
            case Phase.Entertain:
                _animatorEntertain.SetTrigger("Reroll");
                _rerollEntertain.interactable = false;
                break;
            default:
                Debug.LogError("Invalid phase for rerolling upgrade.");
                break;
        }
    }

    public void FillUpgradesList(int upgradeNumber, UpgradeEffect upgrade)
    {
        switch (upgradeNumber)
        {
            case 1:
                _upgrade1.text = upgrade.EffectName;
                _upgrade1.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade1.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade1.gameObject.SetActive(true);
                break;
            case 2:
                _upgrade2.text = upgrade.EffectName;
                _upgrade2.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade2.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade2.gameObject.SetActive(true);
                break;
            case 3:
                _upgrade3.text = upgrade.EffectName;
                _upgrade3.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade3.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade3.gameObject.SetActive(true);
                break;
            case 4:
                _upgrade4.text = upgrade.EffectName;
                _upgrade4.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade4.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade4.gameObject.SetActive(true);
                break;
            case 5:
                _upgrade5.text = upgrade.EffectName;
                _upgrade5.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade5.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade5.gameObject.SetActive(true);
                break;
            case 6:
                _upgrade6.text = upgrade.EffectName;
                _upgrade6.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade6.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade6.gameObject.SetActive(true);
                break;
            case 7:
                _upgrade7.text = upgrade.EffectName;
                _upgrade7.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade7.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade7.gameObject.SetActive(true);
                break;
            case 8:
                _upgrade8.text = upgrade.EffectName;
                _upgrade8.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade8.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade8.gameObject.SetActive(true);
                break;
            case 9:
                _upgrade9.text = upgrade.EffectName;
                _upgrade9.color = GetColorOfPhase(upgrade.AssociatedSystem);
                _upgrade9.GetComponent<UpgradeHolder>().UpgradeEffect = upgrade;
                _upgrade9.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("Shouldn't reach 10 upgrades.");
                break;
        }
        UpdateTurnCounterBeforeNextUpgrade(GameManager.Instance.TurnCounter);
    }

    private void UpdateTurnCounterBeforeNextUpgrade(int currentTurn)
    {
        int nextTurn = UpgradesManager.Instance.GetNextUpgradeTurn();
        if (nextTurn == -1)
            _counterForNextUpgrade.text = "No more Upgrades";
        else
        {
            int counter = nextTurn - currentTurn;
            _counterForNextUpgrade.text = $"Next upgrade in {counter} {(counter > 1 ? "turns" : "turn")}";
        } 
    }
    #endregion

    public Color GetColorOfPhase(Phase phase)
    {
        return phase switch
        {
            Phase.Explore => _colorExplo,
            Phase.Expand => _colorExpand,
            Phase.Exploit => _colorExploit,
            Phase.Entertain => _colorEntertain,
            _ => _colorIvory,
        };
    }
}
