using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    #region CONFIGURATION
    [SerializeField] private Transform _popUpParent;
    [Header("_________________________________________________________")]
    [Header("Resources Bar")]
    [SerializeField] private TextMeshProUGUI _scoutsLimitText;
    [SerializeField] private TextMeshProUGUI _claimText;
    [SerializeField] private TextMeshProUGUI _townsLimitText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _srText;
    [SerializeField] private TextMeshProUGUI _carnivalistText;
    [SerializeField] private Color _colorCantAfford;
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
    [SerializeField] private Button _buttonEndPhase;
    [SerializeField] private List<Image> _phaseBorders;
    [Header("_________________________________________________________")]
    [Header("Units visibility UI")]
    [SerializeField] private Image _visibilityImage;
    [SerializeField] private Image _scoutImageVisibility;
    [SerializeField] private Image _entertainmentImageVisibility;
    [SerializeField] private Sprite _visible;
    [SerializeField] private Sprite _hidden;
    [Header("_________________________________________________________")]
    [Header("Menu")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _confirmQuit;
    [SerializeField] private GameObject _endMenu;
    [SerializeField] private TextMeshProUGUI _endScore;
    [Header("_________________________________________________________")]
    [Header("Trade Menu")]
    [SerializeField] private GameObject _tradeMenuButton;
    [SerializeField] private GameObject _tradeMenu;
    [SerializeField] private GameObject _buyButton;
    [SerializeField] private GameObject _sellButton;
    [Header("_________________________________________________________")]
    [Header("Score")]
    [SerializeField] private GameObject _scoreUI;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [Header("_________________________________________________________")]
    [Header("Upgrades Menu")]
    [SerializeField] private GameObject _upgradesMenuButton;
    [SerializeField] private GameObject _upgradesMenu;
    [SerializeField] private List<UpgradeTree> _upgradeTrees = new List<UpgradeTree>();
    [SerializeField] private UpgradeTree _activatedTree;
    [SerializeField] private Color _colorLocked;
    [SerializeField] private Color _colorUnlocked;
    [SerializeField] private Sprite _spriteButtonUnlocked;
    [SerializeField] private GameObject _markerExclusiveUpgrade;
    [SerializeField] private Sprite _markerExclusiveUpgradeLocked;
    [Header("_________________________________________________________")]
    [Header("Show Income button")]
    [SerializeField] private Image _showIncomeButton;
    [SerializeField] private Sprite _showIncomeOff;
    [SerializeField] private Sprite _showIncomeOn;
    [Header("_________________________________________________________")]
    [Header("VFX Anchors")]
    [SerializeField] private RectTransform _vfxAnchorEndConfetti1;
    [SerializeField] private RectTransform _vfxAnchorEndConfetti2;
    [SerializeField] private RectTransform _vfxAnchorEndFirework1;
    [SerializeField] private RectTransform _vfxAnchorEndFirework2;
    [SerializeField] private RectTransform _vfxAnchorClaim;
    [SerializeField] private RectTransform _vfxAnchorGold;
    [SerializeField] private RectTransform _vfxAnchorSR;
    [SerializeField] private RectTransform _vfxAnchorCarnivalist;
    [Header("_________________________________________________________")]
    [Header("Various Objects")]
    [SerializeField] private Animator _scoutHint;
    #endregion

    #region VARIABLES
    private bool _areUnitsVisible;
    private bool _uiPhaseInAnimation;
    private bool _areIncomesShown;
    #endregion

    #region ACCESSORS
    public Color ColorCantAfford { get => _colorCantAfford;}
    public Color ColorLocked { get => _colorLocked; }
    public Color ColorUnlocked { get => _colorUnlocked; }
    public UpgradeTree ActivatedTree { get => _activatedTree; }
    public Sprite SpriteUnlocked { get => _spriteButtonUnlocked; }
    public GameObject MarkerExclusiveUpgrade { get => _markerExclusiveUpgrade; }
    public Sprite MarkerExclusiveUpgradeLocked { get => _markerExclusiveUpgradeLocked; }
    public RectTransform VfxAnchorEndConfetti1 { get => _vfxAnchorEndConfetti1; }
    public RectTransform VfxAnchorEndConfetti2 { get => _vfxAnchorEndConfetti2; }
    public RectTransform VfxAnchorEndFirework1 { get => _vfxAnchorEndFirework1; }
    public RectTransform VfxAnchorEndFirework2 { get => _vfxAnchorEndFirework2; }
    public RectTransform VfxAnchorClaim { get => _vfxAnchorClaim; }
    public RectTransform VfxAnchorGold { get => _vfxAnchorGold; }
    public RectTransform VfxAnchorSR { get => _vfxAnchorSR; }
    public Button ButtonEndPhase { get => _buttonEndPhase; }
    public Transform PopUpParent { get => _popUpParent; }
    public GameObject UpgradesMenuObject { get => _upgradesMenu; }
    public Color ColorEntertain { get => _colorEntertain; }
    public bool UiPhaseInAnimation { get => _uiPhaseInAnimation; set => _uiPhaseInAnimation = value; }
    public Animator ScoutHint { get => _scoutHint; }
    public Color ColorExpand { get => _colorExpand; }
    public Color ColorExploit { get => _colorExploit; }
    public Color ColorExplo { get => _colorExplo; }
    public RectTransform VfxAnchorCarnivalist { get => _vfxAnchorCarnivalist; }
    public bool AreIncomesShown { get => _areIncomesShown; }
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnNewTurn += UpdateTurnCounterText;

        GameManager.Instance.OnExplorationPhaseStarted += NewPhaseStarted;
        ExplorationManager.Instance.OnPhaseFinalized += () => PhaseEnded(Phase.Explore);
        GameManager.Instance.OnExpansionPhaseStarted += NewPhaseStarted;
        ExpansionManager.Instance.OnPhaseFinalized += () => PhaseEnded(Phase.Expand);
        GameManager.Instance.OnExploitationPhaseStarted += NewPhaseStarted;
        ExploitationManager.Instance.OnPhaseFinalized += () => PhaseEnded(Phase.Exploit);
        GameManager.Instance.OnEntertainmentPhaseStarted += NewPhaseStarted;

        GameManager.Instance.OnEntertainmentPhaseStarted += UpdateUIForEntertainment;

        GameManager.Instance.OnExplorationPhaseStarted += () => ScoutsVisibility(true);
        GameManager.Instance.OnExpansionPhaseStarted += () => ScoutsVisibility(false);

        GameManager.Instance.OnGameFinished += GameFinished;

        ExplorationManager.Instance.OnScoutsLimitModified += UpdateScoutLimit;

        EntertainmentManager.Instance.OnScoreUpdated += () => _scoreText.text = EntertainmentManager.Instance.Score.ToString();

        if (TutorialManager.Instance != null)
        {
            _tradeMenuButton.SetActive(false);
            _upgradesMenuButton.SetActive(false);
        }
    }

    private void Start()
    {
        InitializeUI();
    }

    private void UpdateUIForEntertainment()
    {
        _scoutImageVisibility.enabled = false;
        _entertainmentImageVisibility.enabled = true;
        _visibilityImage.sprite = _visible;

        _scoreUI.SetActive(true);
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
        }
    }

    public void TradeBuy()
    {
        if (ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.TradeBuyCost))
        {
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.TradeBuyCost, Transaction.Spent);
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.TradeBuyGain, Transaction.Gain);
        }
    }

    public void TradeSell()
    {
        if (ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.TradeSellCost))
        {
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.TradeSellCost, Transaction.Spent);
            ResourcesManager.Instance.UpdateResource(ResourcesManager.Instance.TradeSellGain, Transaction.Gain);
        }
    }

    private void UpdateTradeTextsColors()
    {
        if (!ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.TradeBuyCost))
        {
            foreach (TextMeshProUGUI text in _buyButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = _colorCantAfford;
        }
        else
        {
            foreach (TextMeshProUGUI text in _buyButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = Color.white;
        }
        if (!ResourcesManager.Instance.CanAfford(ResourcesManager.Instance.TradeSellCost))
        {
            foreach (TextMeshProUGUI text in _sellButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = _colorCantAfford;
        }
        else
        {
            foreach (TextMeshProUGUI text in _sellButton.GetComponentsInChildren<TextMeshProUGUI>())
                text.color = Color.white;
        }
    }
    #endregion

    #region UNITS VISIBILITY UI
    //OnClick for UI button
    public void UnitsVisibility()
    {
        _areUnitsVisible = !_areUnitsVisible;

        _visibilityImage.sprite = _areUnitsVisible ? _visible : _hidden;

        if (GameManager.Instance.CurrentPhase != Phase.Entertain)
        {
            foreach (Scout item in ExplorationManager.Instance.Scouts)
            {
                item.ScoutVisibility(_areUnitsVisible);
            }
        }
        else
        {
            foreach (Entertainment item in EntertainmentManager.Instance.Entertainments)
            {
                item.EntertainmentVisibility(_areUnitsVisible);
            }
        }
    }

    public void ScoutsVisibility(bool visible)
    {
        _areUnitsVisible = visible;

        _visibilityImage.sprite = _areUnitsVisible ? _visible : _hidden;

        foreach (Scout item in ExplorationManager.Instance.Scouts)
        {
            item.ScoutVisibility(visible);
        }
    }
    #endregion

    #region PHASE UI
    public void ConfirmPhase()
    {
        GameManager.Instance.ConfirmPhase();
    }
    
    public void ForceExploColor()
    {
        foreach (Image item in _phaseBorders)
        {
            item.color = _colorExplo;
        }
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
                foreach (Image item in _phaseBorders)
                {
                    item.color = _colorExplo;
                }
                _popUpExploPhase.SetTrigger("Show");
                break;
            case Phase.Expand:
                _confirmPhaseButtonText.text = "End Phase";
                foreach (Image item in _phaseBorders)
                {
                    item.color = _colorExpand;
                }
                _popUpExpandPhase.SetTrigger("Show");
                break;
            case Phase.Exploit:
                _confirmPhaseButtonText.text = "End Turn";
                foreach (Image item in _phaseBorders)
                {
                    item.color = _colorExploit;
                }
                _popUpExploitPhase.SetTrigger("Show");
                break;
            case Phase.Entertain:
                _confirmPhaseButtonText.text = "End Game";
                foreach (Image item in _phaseBorders)
                {
                    item.color = _colorEntertain;
                }
                _popUpEntertainPhase.SetTrigger("Show");
                break;
        }
    }

    public void UpdateTurnCounterText(int turnCounter)
    {
        _turnCounterText.text = "Turn : " + turnCounter + "/" + GameManager.Instance.TurnLimit;
    }
    #endregion

    #region MENU
    private void GameFinished()
    {
        if (TutorialManager.Instance != null)
            return;

        _endMenu.SetActive(true);
        _endScore.text = EntertainmentManager.Instance.Score.ToString();
    }

    public void OpenCloseMenu()
    {
        //Close the upgrades menu if it's open instead of opening the main menu
        if (_upgradesMenu.activeSelf)
        {
            _upgradesMenu.GetComponent<Animator>().SetTrigger("Fold");
            GameManager.Instance.GamePaused = false;
            return;
        }

        _menu.SetActive(!_menu.activeSelf);
        GameManager.Instance.GamePaused = _menu.activeSelf;
        PopUpManager.Instance.ResetPopUp(null);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ConfirmQuit()
    {
        _confirmQuit.SetActive(!_confirmQuit.activeSelf);
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
            GameManager.Instance.GamePaused = false;
        }
        else
        {
            if (_tradeMenu.activeSelf)
                TradeMenu();
            _upgradesMenu.SetActive(true);
            GameManager.Instance.GamePaused = true;
            foreach (UpgradeTree tree in _upgradeTrees)
            {
                if (tree.treeObject.activeSelf)
                {
                    _activatedTree = tree;
                    tree.nodes.ForEach(node => node.UpdateVisual());
                    break;
                }
            }
        }
    }

    public void ShowUpgradeTree(GameObject associatedTree)
    {
        associatedTree.SetActive(true);

        foreach (UpgradeTree tree in _upgradeTrees)
        {
            if (tree.treeObject == associatedTree)
            {
                _activatedTree = tree;
                tree.nodes.ForEach(node => node.UpdateVisual());
            }
            if (tree.treeObject != associatedTree)
                tree.treeObject.SetActive(false);
        }
    }

    [ContextMenu("Fill Trees List")]
    private void FillTreesList()
    {
        foreach (UpgradeTree tree in _upgradeTrees)
            tree.nodes = tree.treeObject.GetComponentsInChildren<UI_UpgradeNode>().ToList();
    }
    #endregion

    public void SwitchIncomesVisibility()
    {
        _areIncomesShown = !_areIncomesShown;
        _showIncomeButton.sprite = _areIncomesShown ? _showIncomeOn : _showIncomeOff;
        foreach (Tile tile in ExpansionManager.Instance.ClaimedTiles)
        {
            tile.ShowIncomeUI(_areIncomesShown);
        }
    }
}
