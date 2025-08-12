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
    [SerializeField] private Color _colorCantAfford;
    [Header("_________________________________________________________")]
    [Header("Phase UI")]
    [SerializeField] private TextMeshProUGUI _currentPhaseText;
    [SerializeField] private TextMeshProUGUI _confirmPhaseButtonText;
    [SerializeField] private TextMeshProUGUI _turnCounterText;
    [SerializeField] private Material _materialBack;
    [SerializeField] private Color _colorTopExplo;
    [SerializeField] private Color _colorBotExplo;
    [SerializeField] private Color _colorTopExpand;
    [SerializeField] private Color _colorBotExpand;
    [SerializeField] private Color _colorTopExploit;
    [SerializeField] private Color _colorBotExploit;
    [SerializeField] private Color _colorTopEntertain;
    [SerializeField] private Color _colorBotEntertain;
    [SerializeField] private Animator _popUpExploPhase;
    [SerializeField] private Animator _popUpExpandPhase;
    [SerializeField] private Animator _popUpExploitPhase;
    [SerializeField] private Animator _popUpEntertainPhase;
    [SerializeField] private Button _buttonEndPhase;
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
    [Header("UpgradesMenu")]
    [SerializeField] private GameObject _upgradesMenuButton;
    [SerializeField] private GameObject _upgradesMenu;
    [SerializeField] private List<UpgradeTree> _upgradeTrees = new List<UpgradeTree>();
    [SerializeField] private UpgradeTree _activatedTree;
    [SerializeField] private GameObject _lineRendererPrefab;
    [SerializeField] private Color _colorLocked;
    [SerializeField] private Color _colorUnlocked;
    [SerializeField] private Sprite _spriteButtonUnlocked;
    [SerializeField] private GameObject _markerExclusiveUpgrade;
    [SerializeField] private Sprite _markerExclusiveUpgradeLocked;
    [Header("_________________________________________________________")]
    [Header("VFX Anchors")]
    [SerializeField] private RectTransform _vfxAnchorEndConfetti1;
    [SerializeField] private RectTransform _vfxAnchorEndConfetti2;
    [SerializeField] private RectTransform _vfxAnchorEndFirework1;
    [SerializeField] private RectTransform _vfxAnchorEndFirework2;
    [SerializeField] private RectTransform _vfxAnchorClaim;
    [SerializeField] private RectTransform _vfxAnchorGold;
    [SerializeField] private RectTransform _vfxAnchorSR;
    #endregion

    #region VARIABLES
    private Transform _mainCanvas;
    private bool _areUnitsVisible;
    #endregion

    #region ACCESSORS
    public Color ColorCantAfford { get => _colorCantAfford;}
    public GameObject LineRendererPrefab { get => _lineRendererPrefab; }
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
    #endregion

    protected override void OnAwake()
    {
        _mainCanvas = GetComponent<Transform>();

        GameManager.Instance.OnNewTurn += UpdateTurnCounterText;

        GameManager.Instance.OnExplorationPhaseStarted += UpdatePhaseUI;
        GameManager.Instance.OnExpansionPhaseStarted += UpdatePhaseUI;
        GameManager.Instance.OnExploitationPhaseStarted += UpdatePhaseUI;
        GameManager.Instance.OnEntertainmentPhaseStarted += UpdatePhaseUI;

        GameManager.Instance.OnEntertainmentPhaseStarted += UpdateUIForEntertainment;

        GameManager.Instance.OnExplorationPhaseStarted += ForceScoutsToShow;

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
        UpdateResourceUI(Resource.Gold, ResourcesManager.Instance.GetResourceStock(Resource.Gold));
        UpdateResourceUI(Resource.SpecialResources, ResourcesManager.Instance.GetResourceStock(Resource.SpecialResources));
        UpdateTownLimit();
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

    private void ScoutsVisibility(bool visible)
    {
        _areUnitsVisible = visible;

        _visibilityImage.sprite = _areUnitsVisible ? _visible : _hidden;

        foreach (Scout item in ExplorationManager.Instance.Scouts)
        {
            item.ScoutVisibility(visible);
        }
    }

    private void ForceScoutsToShow()
    {
        ScoutsVisibility(true);
    }
    #endregion

    #region PHASE UI
    public void ConfirmPhase()
    {
        GameManager.Instance.ConfirmPhase();
    }
    
    private void UpdatePhaseUI()
    {
        switch (GameManager.Instance.CurrentPhase)
        {
            case Phase.Explore:
                _currentPhaseText.text = "Explore";
                _confirmPhaseButtonText.text = "End Phase";
                _materialBack.SetColor("_ColorTop", _colorTopExplo);
                _materialBack.SetColor("_ColorBottom", _colorBotExplo);
                EnableRenderers(_popUpEntertainPhase.gameObject, false);
                EnableRenderers(_popUpExploPhase.gameObject, true);
                _popUpExploPhase.SetTrigger("PopUp");
                break;
            case Phase.Expand:
                _currentPhaseText.text = "Expand";
                _confirmPhaseButtonText.text = "End Phase";
                _materialBack.SetColor("_ColorTop", _colorTopExpand);
                _materialBack.SetColor("_ColorBottom", _colorBotExpand);
                EnableRenderers(_popUpExploPhase.gameObject, false);
                EnableRenderers(_popUpExpandPhase.gameObject, true);
                _popUpExpandPhase.SetTrigger("PopUp");
                break;
            case Phase.Exploit:
                _currentPhaseText.text = "Exploit";
                _confirmPhaseButtonText.text = "End Turn";
                _materialBack.SetColor("_ColorTop", _colorTopExploit);
                _materialBack.SetColor("_ColorBottom", _colorBotExploit);
                EnableRenderers(_popUpExpandPhase.gameObject, false);
                EnableRenderers(_popUpExploitPhase.gameObject, true);
                _popUpExploitPhase.SetTrigger("PopUp");
                break;
            case Phase.Entertain:
                _currentPhaseText.text = "Entertain";
                _confirmPhaseButtonText.text = "End Game";
                _materialBack.SetColor("_ColorTop", _colorTopEntertain);
                _materialBack.SetColor("_ColorBottom", _colorBotEntertain);
                EnableRenderers(_popUpExploitPhase.gameObject, false);
                EnableRenderers(_popUpEntertainPhase.gameObject, true);
                _popUpEntertainPhase.SetTrigger("PopUp");
                break;
        }
    }

    private void UpdateTurnCounterText(int turnCounter)
    {
        _turnCounterText.text = "Turn : " + turnCounter;
    }

    private void EnableRenderers(GameObject item, bool enable)
    {
        item.GetComponent<Image>().enabled = enable;
        foreach (TextMeshProUGUI t in item.GetComponentsInChildren<TextMeshProUGUI>()) 
        {
            t.enabled = enable;
        }
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
}
