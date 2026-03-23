using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : Singleton<PopUpManager>
{
    #region CONSTANTS
    const float REF_WIDTH = 1920f;
    const string VERSION_KEY = "V0.1";
    const string INFRA_LVL_TUTO_KEY = "InfraLvlTutoShown" + VERSION_KEY;
    const string REMOVING_INFRA_TUTO_KEY = "RemoveInfraTutoShown" + VERSION_KEY;
    const string LOCK_POPUP_TUTO_KEY = "LockPopupTutoShown" + VERSION_KEY;
    const string UPGRADE_TUTO_KEY = "UpgradeTutoShown" + VERSION_KEY;
    const string SAVINGS_TUTO_KEY = "SavingsTutoShown" + VERSION_KEY;
    const string FILTERS_TUTO_KEY = "FiltersTutoShown" + VERSION_KEY;
    const string TRADE_TUTO_KEY = "TradeTutoShown" + VERSION_KEY;
    #endregion

    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Spawning Configuration")]
    [SerializeField] private Transform _popUpParent;
    [SerializeField] private float _survivablePopupDuration;
    [SerializeField] private float _durationHoverForUI = 1f;
    [SerializeField][Range(0f,1f)] private float _percentageOfTimerForVisualHint = 0.75f;
    [SerializeField] private Image _timerOverImage;
    [SerializeField][Range(0, 1f)] private float _offsetBetweenSeveralPopUps;
    [SerializeField][Range(0, 1f)] private float _maxScreenFraction = 0.3f;
    [SerializeField][Range(0, 1f)] private float _minScreenFraction = 0.1f;
    [SerializeField][Range(0, 1f)] private float _offsetPopupOnCursor = 0.02f;
    [SerializeField] private RectTransform _popUpLeftBottomAnchor;
    [Header("_________________________________________________________")]
    [Header("Prefabs")]
    [SerializeField] private GameObject _basePopUp;
    [SerializeField] private GameObject _title;
    [SerializeField] private GameObject _text;
    [SerializeField] private GameObject _survivingTimer;
    [Header("_________________________________________________________")]
    [Header("Locking Popup")]
    [SerializeField] private GameObject _lockingObject;
    [SerializeField] private GameObject _lockedObject;
    [SerializeField] private float _durationForLockingPopup = 5f;
    [SerializeField] private Vector2 _lockImagePopupOffset;
    [Header("_________________________________________________________")]
    [Header("Tutorial Popup")]
    [SerializeField] private Animator _infraLevelTutoPopup;
    [SerializeField] private Animator _removeInfraTutoPopup;
    [SerializeField] private Animator _lockPopupTutoPopup;
    [SerializeField] private Animator _upgradeTutoPopup;
    [SerializeField] private Animator _savingsTutoPopup;
    [SerializeField] private Animator _filtersTutoPopup;
    [SerializeField] private Animator _tradeTutoPopup;
    #endregion

    #region VARIABLES
    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _delayedHoverTimer;//For filling image purpose
    private List<GameObject> _popUps = new List<GameObject>();
    private float _maxAllowed;
    private float _minAllowed;
    private bool _popUpShown;
    private bool _showSourcesOnPopUp;
    private List<UI_SurvivingPopup> _survivingPopups = new List<UI_SurvivingPopup>();
    //POPUP ON POPUP Variables
    private GameObject _lockingPopup;
    private bool _isLockingPopup;
    private Image _lockingImage;
    private float _lockingTimer;
    private Dictionary<GameObject, Button> _lockedPopUps = new();

    public Dictionary<GameObject, Button> LockedPopUps { get => _lockedPopUps; }
    public bool ShowSourcesOnPopUp { get => _showSourcesOnPopUp; set => _showSourcesOnPopUp = value; }
    #endregion

    private void Start()
    {
        float dynamicFraction = _maxScreenFraction * (REF_WIDTH / Screen.width);
        _maxAllowed = Screen.width * dynamicFraction;
        dynamicFraction = _minScreenFraction * (REF_WIDTH / Screen.width);
        _minAllowed = Screen.width * dynamicFraction;

        // delay start for pop up UI
        _delayedHoverTimer = _durationHoverForUI * _percentageOfTimerForVisualHint;

        GameManager.Instance.OnLastTurnStarted += ShowSavingsTutoPopUp;
    }

    #region BASE LOGIC
    public void UIPopUp(GameObject obj)
    {
        if (obj == null)
            return;

        if (obj == _objectUnderMouse)
        {
            if (obj.CompareTag("Untagged"))
                return;
            if (_popUpShown)
                return;

            // Some popup (those on resource UI) are shown instantly
            bool instantPopUp = true;

            switch (obj.tag)
            {
                case "Popup": // Meaning we are hovering over an existing popup so we start locking it
                    UI_SurvivingPopup survivingPopup = obj.GetComponent<UI_SurvivingPopup>();
                    if (survivingPopup != null)
                    {
                        if (survivingPopup.enabled == true && !_isLockingPopup)
                        {
                            survivingPopup.IsSurviving = false;
                            StartLockingPopup(survivingPopup.gameObject);
                        }
                    }
                    break;
                case "ScoutLimitUI":
                    // Close any surviving popup if needed
                    CloseSurvivingPopups();
                    ScoutLimitPopUp();
                    break;
                case "ClaimUI":
                    CloseSurvivingPopups();
                    ClaimPopUp();
                    break;
                case "TownLimitUI":
                    CloseSurvivingPopups();
                    TownLimitPopUp();
                    break;
                case "GoldUI":
                    CloseSurvivingPopups();
                    GoldPopUp();
                    break;
                case "SRUI":
                    CloseSurvivingPopups();
                    SRPopUp();
                    break;
                case "ScoreUI":
                    CloseSurvivingPopups();
                    ScorePopUp();
                    break;
                case "CarnivalistUI":
                    CloseSurvivingPopups();
                    CarnivalistPopUp();
                    break;
                case "UpgradeUI":
                    CloseSurvivingPopups();
                    UpgradePopUp(obj.GetComponent<UpgradeHolder>().UpgradeEffect);
                    break;
                default:
                    instantPopUp = false;
                    break;
            }

            if (instantPopUp)
            {
                _popUpShown = true;
                _hoverTimer = 0.0f;
                _timerOverImage.fillAmount = 0.0f;
                _timerOverImage.enabled = false;
                if (!obj.CompareTag("Popup"))
                    JuiceManager.Instance.KillAllComboVFX();
                return;
            }

            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            // Fill is 0 until t >= t0, then rises linearly to 1 at t == d.
            _timerOverImage.fillAmount = Mathf.InverseLerp(_delayedHoverTimer, _durationHoverForUI, _hoverTimer);

            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                _popUpShown = true;
                _hoverTimer = 0.0f;
                _timerOverImage.fillAmount = 0.0f;
                _timerOverImage.enabled = false;

                JuiceManager.Instance.KillAllComboVFX();

                // Close any surviving popup if needed
                CloseSurvivingPopups();

                switch (obj.tag)
                {
                    case "VisibilityUI":
                        VisibilityPopUp();
                        break;
                    case "ShowIncomeUI":
                        ShowIncomePopUp();
                        break;
                    case "ShowEntPlacementUI":
                        ShowEntPlacementPopUp();
                        break;
                    case "ShowDetailsPopupUI":
                        ShowDetailsPopupPopUp();
                        break;
                    case "ShowEnhanceableUI":
                        ShowEnhanceablePopUp();
                        break;
                    case "Untagged":
                        break;
                    default:
                        Debug.LogWarning("PopUpManager: Object with wrong tag for pop up found " + obj.name);
                        break;
                }
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            ResetPopUp(obj);
        }
    }

    public void NonUIPopUp(GameObject obj)
    {
        if (obj == _objectUnderMouse)
        {
            if (obj.GetComponent<Tile>() is Tile t && !t.Revealed)
                return;
            if (_popUpShown)
                return;

            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            // Fill is 0 until t >= t0, then rises linearly to 1 at t == d.
            _timerOverImage.fillAmount = Mathf.InverseLerp(_delayedHoverTimer, _durationHoverForUI, _hoverTimer);
            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                _popUpShown = true;
                _hoverTimer = 0.0f;
                _timerOverImage.fillAmount = 0.0f;
                _timerOverImage.enabled = false;

                JuiceManager.Instance.KillAllComboVFX();

                // Close any surviving popup if needed
                CloseSurvivingPopups();

                if (obj.GetComponent<Tile>() is Tile tile)
                {
                    if (tile.Entertainment != null)
                        EntertainmentPopUp(tile.Entertainment);
                    TilePopUp(tile);
                    if (tile.Scouts.Count > 1)
                        MultipleScoutsPopUp(tile);
                    else if (tile.Scouts.Count == 1)
                        ScoutPopUp(tile.Scouts.First());
                }
                else if (obj.GetComponent<InteractionButton>() is InteractionButton button)
                {
                    GameManager.Instance.InteractionButtonsFade(true, button.gameObject);
                    switch (button.Interaction)
                    {
                        case Interaction.Claim:
                            ButtonClaimPopUp(button);
                            break;
                        case Interaction.Scout:
                            ButtonScoutPopUp(button);
                            break;
                        case Interaction.Infrastructure:
                            ButtonInfraPopUp(button);
                            break;
                        case Interaction.Destroy:
                            if (GameManager.Instance.CurrentPhase == Phase.Exploit)
                                ButtonDestroyPopUp("Destroy " + button.AssociatedTile.TileData.TileName, button);
                            else
                            {
                                if (button.AssociatedTile.Entertainment == null)
                                    ButtonDestroyPopUp("Remove the entertainment", button);
                                else
                                    ButtonDestroyPopUp("Remove " + button.AssociatedTile.Entertainment.Data.Type.ToCustomString(false), button);
                            }
                            break;
                        case Interaction.Entertainment:
                            ButtonEntertainmentPopUp(button);
                            break;
                        case Interaction.RedirectScout:
                            ButtonRedirectScoutPopUp(button);
                            break;
                        case Interaction.RevealAnywhere:
                            ButtonRevealAnywherePopUp(button);
                            break;
                        default:
                            Debug.LogWarning("PopUpManager: InteractionButton with no interaction type found " + button.Interaction);
                            break;
                    }
                }
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            ResetPopUp(obj);
        }
    }

    public void PopUpOnPopUp(GameObject popupRoot, RectTransform refObject, Family family = Family.None, InfrastructureData infra = null, EntertainmentData ent = null)
    {
        if (refObject.gameObject == _objectUnderMouse)
        {
            if (_popUpShown)
                return;

            if (_survivingPopups.Count != 0)
            {
                if (popupRoot != null)
                {
                    UI_SurvivingPopup currentSurvivingPopup = popupRoot.GetComponent<UI_SurvivingPopup>();
                    if (currentSurvivingPopup.enabled == true && !_isLockingPopup)
                    {
                        currentSurvivingPopup.IsSurviving = false;
                        StartLockingPopup(currentSurvivingPopup.gameObject);
                    }
                }
                CloseSurvivingPopups();
            }

            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            // Fill is 0 until t >= t0, then rises linearly to 1 at t == d.
            _timerOverImage.fillAmount = Mathf.InverseLerp(_delayedHoverTimer, _durationHoverForUI, _hoverTimer);
            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                _popUpShown = true;
                _hoverTimer = 0.0f;
                _timerOverImage.fillAmount = 0.0f;
                _timerOverImage.enabled = false;

                JuiceManager.Instance.KillAllComboVFX();

                if (family != Family.None)
                {
                    FamilyPopup(family, refObject);
                }
                else if (infra != null)
                {
                    InfrastructurePopup(infra, refObject);
                }
                else if (ent != null)
                {
                    EntertainmentPopup(ent, refObject);
                }
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            ResetPopUp(refObject.gameObject);
        }
    }

    public void ResetPopUp(GameObject obj)
    {
        _popUpShown = false;
        _objectUnderMouse = obj;
        _hoverTimer = 0.0f;
        _timerOverImage.fillAmount = 0.0f;
        _timerOverImage.enabled = true;
        GameManager.Instance.InteractionButtonsFade(false, null);

        if (_popUps.Count > 0)
        {
            foreach (GameObject item in _popUps)
            {
                if (obj != null) //Meaning we are not forcing the closing of pop ups but just changing the object under mouse
                {
                    UI_SurvivingPopup survivingPopup = item.GetComponent<UI_SurvivingPopup>();
                    if (survivingPopup != null)
                    {
                        survivingPopup.IsSurviving = true;
                        continue;
                    }
                }
                else
                {
                    UI_SurvivingPopup survivingPopup = item.GetComponent<UI_SurvivingPopup>();
                    if (survivingPopup != null)
                    {
                        survivingPopup.ClosePopup();
                        continue;
                    }
                }

                item.GetComponent<Animator>().SetTrigger("Close");
                if (item == JuiceManager.Instance.PopUpVisualizingCombo)
                {
                    JuiceManager.Instance.KillAllComboVFX();
                }
            }
            _popUps.Clear();
        }
        if (_lockingImage != null)
        {
            UI_SurvivingPopup survivingPopup = _lockingPopup.GetComponent<UI_SurvivingPopup>();

            if (obj == null) // Meaning we are forcing the closing of pop ups
            {
                survivingPopup?.ClosePopup();
                StopLockingPopup();
                return;
            }

            if (!obj.CompareTag("Popup")) // Meaning we are changing the object under mouse to a non-popup object
            {
                if (survivingPopup != null)
                {
                    survivingPopup.IsSurviving = true;
                }
                StopLockingPopup();
            }
            else // We are changing the object under mouse to a popup object we need to check if it's the same popup or not
            {
                if (obj.GetComponentInParent<UI_SurvivingPopup>() == survivingPopup)
                {
                    // Same popup so we do nothing
                    return;
                }
                else
                {
                    if (survivingPopup != null)
                    {
                        survivingPopup.IsSurviving = true;
                    }
                    StopLockingPopup();
                }
            }
        }
    }
    #endregion

    #region UI POP UP
    private void ScoutLimitPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Scouts<sprite name=\"Scout_Emoji\"> limit";
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Can be upgrades with\nspecifics infrastructures";
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        #endregion

        #region AVAILABILITY
        TextMeshProUGUI availability = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        availability.text = $"{ExplorationManager.Instance.ScoutsLimit - ExplorationManager.Instance.CurrentScoutsCount}<sprite name=\"Scout_Emoji\"> available";
        ClampTextWidth(availability);
        #endregion

        #region SOURCE
        Dictionary<Family, int> scoutSources = new Dictionary<Family, int>();
        foreach (var item in ExploitationManager.Instance.Infrastructures)
        {
            if (item.TileData.SpecialBehaviours.Any(b => b.GetType() == typeof(BoostScoutsLimit)))
            {
                foreach (SpecialBehaviour behaviour in item.TileData.SpecialBehaviours)
                {
                    if (behaviour.GetType() == typeof(BoostScoutsLimit))
                    {
                        BoostScoutsLimit boost = (BoostScoutsLimit)behaviour;
                        if (scoutSources.ContainsKey(item.TileData.Family))
                            scoutSources[item.TileData.Family] += boost.ScoutsIncrease;
                        else
                            scoutSources.Add(item.TileData.Family, boost.ScoutsIncrease);
                    }
                }
            }
        }
        TextMeshProUGUI baseSource = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        baseSource.text = $"Base value: +{ExplorationManager.Instance.BaseScoutsLimit}<sprite name=\"Scout_Emoji\"> limit";
        ClampTextWidth(baseSource);
        if (scoutSources.Count > 0)
        {
            foreach (var kvp in scoutSources)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = $"{kvp.Key.ToCustomString(true)}: +{kvp.Value}<sprite name=\"Scout_Emoji\"> limit";
                ClampTextWidth(source);
            }
        }
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void TownLimitPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Towns<sprite name=\"Town_Emoji\"> limit";
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Can be upgrades with specifics\ninfrastructures and upgrade";
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        #endregion

        #region AVAILABILITY
        TextMeshProUGUI availability = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        availability.text = $"{ExploitationManager.Instance.GetTownLimit()}<sprite name=\"Town_Emoji\"> available";
        ClampTextWidth(availability);
        #endregion

        #region SOURCE
        TextMeshProUGUI baseSource = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        baseSource.text = "Base value: +2<sprite name=\"Town_Emoji\"> limit";
        ClampTextWidth(baseSource);
        if (UpgradesManager.Instance.AppliedUpgrades.Any(b => b.GetType() == typeof(UpgradeTownLimit)))
        {
            TextMeshProUGUI upgradeSource = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            upgradeSource.text = "Imperial Mandate: +3<sprite name=\"Town_Emoji\"> limit";
            ClampTextWidth(upgradeSource);
        }
        Dictionary<Family, int> townSources = new Dictionary<Family, int>();
        foreach (var item in ExploitationManager.Instance.Infrastructures)
        {
            if (item.TileData.SpecialBehaviours.Any(b => b.GetType() == typeof(BoostTownsLimit)))
            {
                foreach (SpecialBehaviour behaviour in item.TileData.SpecialBehaviours)
                {
                    if (behaviour.GetType() == typeof(BoostTownsLimit))
                    {
                        if (townSources.ContainsKey(item.TileData.Family))
                            townSources[item.TileData.Family] += 1;
                        else
                            townSources.Add(item.TileData.Family, 1);
                    }
                }
            }
        }
        if (townSources.Count > 0)
        {
            foreach (var kvp in townSources)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = $"{kvp.Key.ToCustomString(true)}: +{kvp.Value}<sprite name=\"Town_Emoji\"> limit";
                ClampTextWidth(source);
            }
        }
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ClaimPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{ResourcesManager.Instance.Claim} Claims<sprite name=\"Claim_Emoji\">";
        #endregion

        #region LOOSABLE CLAIMS
        if (ExpansionManager.Instance.UpgradeConserveClaims == false)
        {
            TextMeshProUGUI loosingClaims = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            loosingClaims.text = "Not used Claims<sprite name=\"Claim_Emoji\"> are lost\nat the end of the Expansion phase";
            ClampTextWidth(loosingClaims);
            loosingClaims.fontStyle = FontStyles.Italic;
            loosingClaims.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        Dictionary<Family, int> claimSources = new Dictionary<Family, int>();
        foreach (var item in ExploitationManager.Instance.Infrastructures)
        {
            if (item.TileData.SpecialBehaviours.Any(b => b.GetType() == typeof(BoostClaimIncome)))
            {
                foreach (SpecialBehaviour behaviour in item.TileData.SpecialBehaviours)
                {
                    if (behaviour.GetType() == typeof(BoostClaimIncome))
                    {
                        BoostClaimIncome boost = (BoostClaimIncome)behaviour;
                        if (claimSources.ContainsKey(item.TileData.Family))
                            claimSources[item.TileData.Family] += boost.ClaimQuantity;
                        else
                            claimSources.Add(item.TileData.Family, boost.ClaimQuantity);
                    }
                }
            }
        }

        #region INCOME
        int totalClaimsPerTurn = ExpansionManager.Instance.ClaimPerTurn;
        if (claimSources.Count > 0)
        {
            foreach (var kvp in claimSources)
            {
                totalClaimsPerTurn += kvp.Value;
            }
        }
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total <sprite name=\"Claim_Emoji\"> per turn +" + totalClaimsPerTurn + "<sprite name=\"Claim_Emoji\">";
        ClampTextWidth(income);
        #endregion

        #region SOURCES
        TextMeshProUGUI baseSource = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        baseSource.text = $"Base value: +{GameManager.Instance.BaseClaimPerTurn}<sprite name=\"Claim_Emoji\">";
        ClampTextWidth(baseSource);
        if (claimSources.Count > 0)
        {
            foreach (var kvp in claimSources)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = $"{kvp.Key.ToCustomString(true)}: +{kvp.Value}<sprite name=\"Claim_Emoji\">";
                ClampTextWidth(source);
            }
        }
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void GoldPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{ResourcesManager.Instance.GetResourceStock(Resource.Gold)} Gold<sprite name=\"Gold_Emoji\">";
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total <sprite name=\"Gold_Emoji\"> per turn +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        ClampTextWidth(income);
        #endregion

        #region INCOME NO INFRA
        int noInfraGoldIncome = ExploitationManager.Instance.GetResourceIncomeByNoInfraTiles(Resource.Gold);
        if (noInfraGoldIncome > 0)
        {
            TextMeshProUGUI incomeNoInfra = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            incomeNoInfra.text = "Basic tiles: +" + noInfraGoldIncome + "<sprite name=\"Gold_Emoji\">";
            ClampTextWidth(incomeNoInfra);
        }
        #endregion

        #region INCOME INFRA
        Dictionary<Family, int> familyProducingGold = ExploitationManager.Instance.GetResourceIncomeByFamilly(Resource.Gold);
        if (familyProducingGold.Count > 0)
        {
            foreach (var kvp in familyProducingGold)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = kvp.Key.ToCustomString(true) + ": +" + kvp.Value + "<sprite name=\"Gold_Emoji\">";
                ClampTextWidth(source);
            }
        }
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void SRPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{ResourcesManager.Instance.GetResourceStock(Resource.SpecialResources)} Stone<sprite name=\"SR_Emoji\">";
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total <sprite name=\"SR_Emoji\"> per turn +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.SpecialResources) + "<sprite name=\"SR_Emoji\">";
        ClampTextWidth(income);
        #endregion

        #region SOURCE
        Dictionary<Family, int> familyProducingSR = ExploitationManager.Instance.GetResourceIncomeByFamilly(Resource.SpecialResources);
        if (familyProducingSR.Count > 0)
        {
            foreach (var kvp in familyProducingSR)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = kvp.Key.ToCustomString(true) + ": +" + kvp.Value + "<sprite name=\"SR_Emoji\">";
                ClampTextWidth(source);
            }
        }
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ScorePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{EntertainmentManager.Instance.Score} Points<sprite name=\"Point_Emoji\">";
        #endregion

        #region MINSTREL STAGE
        TextMeshProUGUI minstrel = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        minstrel.text = "<sprite name=\"Point_Emoji\"> from <u>Minstrel Stage</u>: +" + EntertainmentManager.Instance.GetPointsFromMinstrelStage() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(minstrel);
        #endregion

        #region TASTING PAVILION
        TextMeshProUGUI tasting = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        tasting.text = "<sprite name=\"Point_Emoji\"> from <u>Tasting Pavilion</u>: +" + EntertainmentManager.Instance.GetPointsFromTastingPavilion() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(tasting);
        #endregion

        #region PARADE ROUTE
        TextMeshProUGUI parade = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        parade.text = "<sprite name=\"Point_Emoji\"> from <u>Parade Route</u>: +" + EntertainmentManager.Instance.GetPointsFromParadeRoute() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(parade);
        #endregion

        #region MYSTIC GARDEN
        TextMeshProUGUI garden = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        garden.text = "<sprite name=\"Point_Emoji\"> from <u>Mystic Garden</u>: +" + EntertainmentManager.Instance.GetPointsFromMysticGarden() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(garden);
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void CarnivalistPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{ResourcesManager.Instance.Carnivalist} Carnivalists<sprite name=\"Carnivalist_Emoji\">";
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Carnivalists<sprite name=\"Carnivalist_Emoji\"> are used during the Celebration\nphases to place " + Family.Entertainment.ToCustomString(true);
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        #endregion

        #region SOURCE
        if (ResourcesManager.Instance.CarnivalistSources.Count > 0)
        {
            Dictionary<Family, int> familyProducingCarnivalists = new Dictionary<Family, int>();
            foreach (KeyValuePair<TileData, int> pair in ResourcesManager.Instance.CarnivalistSources)
            {
                if ( pair.Key.Family == Family.None)
                {
                    TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                    source.text = pair.Key.TileName + ": " + pair.Value + "<sprite name=\"Carnivalist_Emoji\">";
                    ClampTextWidth(source);
                }
                else
                {
                    if (familyProducingCarnivalists.ContainsKey(pair.Key.Family))
                        familyProducingCarnivalists[pair.Key.Family] += pair.Value;
                    else
                        familyProducingCarnivalists.Add(pair.Key.Family, pair.Value);
                }
            }
            foreach (var kvp in familyProducingCarnivalists)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = kvp.Key.ToCustomString(true) + ": " + kvp.Value + "<sprite name=\"Carnivalist_Emoji\">";
                ClampTextWidth(source);
            }
        }
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void VisibilityPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreUnitsVisible)
            detail.text = "Hide " +
                $"{(GameManager.Instance.CurrentPhase == Phase.Entertain ? Family.Entertainment.ToCustomString(true) : "Scouts<sprite name=\"Scout_Emoji\">")} " +
                "on tiles";
        else
            detail.text = "Show " +
                $"{(GameManager.Instance.CurrentPhase == Phase.Entertain ? Family.Entertainment.ToCustomString(true) : "Scouts<sprite name=\"Scout_Emoji\">")} " +
                "on tiles";
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowIncomePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreIncomesShown)
            detail.text = "Hide tiles' incomes and bonuses";
        else
            detail.text = "Show tiles' incomes and bonuses";
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowEntPlacementPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreEntPlacementShown)
            detail.text = "Hide which tiles can receive an " + Family.Entertainment.ToCustomString();
        else
            detail.text = "Show which tiles can receive an " + Family.Entertainment.ToCustomString();
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowDetailsPopupPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (_showSourcesOnPopUp)
            detail.text = "Hide advanced incomes/points sources details on pop-ups";
        else
            detail.text = "Show advanced incomes/points sources details on pop-ups";
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowEnhanceablePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreEnhanceableStatusShown)
            detail.text = "Hide which tiles can be enhanced";
        else
            detail.text = "Show which tiles can be enhanced";
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }
    private void UpgradePopUp(UpgradeEffect effect)
    {
        if (effect == null)
            return;

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = effect.EffectName;
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = effect.GetEffectDescription();
        ClampTextWidth(detail);
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }
    #endregion

    #region ON TILE POP UP
    private void TilePopUp(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
        {
            if (!tile.Entertainment && tile.EntImpactedByTile.Count > 0) // If tile an entertainment, its popup handle the combo visualization
                JuiceManager.Instance.VisualizeEntertainmentComboFromTileOnly(tile);
        }
        else
        {
            JuiceManager.Instance.VisualizeExploitationCombo(
                tile.InternalIncomesSources, tile.ExternalIncomesSources, tile.InternalCarnivalistsSources,
                tile.ImpactedTilesIncomes, tile.ImpactedTilesCarnivalists, tile);
        }

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = tile.TileData.TileName;
        #endregion

        #region HAZARDOUS TILE
        if (tile.TileData is HazardousTileData)
        {
            TextMeshProUGUI slow = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            slow.text = "Slow down scouts, cannot be claimed";
            ClampTextWidth(slow);
            slow.fontStyle = FontStyles.Italic;
            slow.alignment = TextAlignmentOptions.Center;

            PositionPopup(popUp.GetComponent<RectTransform>(), tile.transform, true);
        }
        #endregion

        #region INCOME
        if (tile.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            income.text = tile.Incomes.IncomeToString() + " per turn";
            income.fontStyle = FontStyles.Bold;
            income.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region INCOME SOURCES
        if (_showSourcesOnPopUp)
        {
            if (tile.Incomes.Count > 0)
            {
                TextMeshProUGUI sourceInc = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                List<ResourceToIntMap> ownInc = tile.GetIncomeFromTileOnly();
                if (ownInc.Count > 0)
                    sourceInc.text = "(" + ownInc.IncomeToString() + " based on the tile effects)" + "\n";
                if (tile.ExternalIncomesSources.Count > 0)
                {
                    Dictionary<TileData, List<ResourceToIntMap>> datas = new Dictionary<TileData, List<ResourceToIntMap>>();
                    foreach (var kvp in tile.ExternalIncomesSources)
                    {
                        if (datas.ContainsKey(kvp.Key.TileData))
                            datas[kvp.Key.TileData] = Utilities.MergeResourceToIntMaps(datas[kvp.Key.TileData], kvp.Value);
                        else
                            datas.Add(kvp.Key.TileData, Utilities.CloneResourceToIntMaps(kvp.Value));
                    }
                    foreach (var kvpBis in datas)
                    {
                        sourceInc.text += "(" + kvpBis.Value.IncomeToString() + " from " + kvpBis.Key.TileName + ")" + "\n";
                    }
                }
                sourceInc.alignment = TextAlignmentOptions.Center;
                sourceInc.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region INCOMES GIVEN
        if (tile.ImpactedTilesIncomes.Count > 0)
        {
            TextMeshProUGUI incomeGiven = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            List<ResourceToIntMap> totalGiven = new List<ResourceToIntMap>();
            foreach (var kvp in tile.ImpactedTilesIncomes)
            {
                totalGiven = Utilities.MergeResourceToIntMaps(totalGiven, kvp.Value);
            }
            incomeGiven.text = $"Provides {totalGiven.IncomeToString()} over " +
                $"{tile.ImpactedTilesIncomes.Count} other {(tile.ImpactedTilesIncomes.Count > 1 ? "tiles" : "tile")}";
            incomeGiven.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region INCOMES GIVEN DESTINATION
        if (_showSourcesOnPopUp)
        {
            if (tile.ImpactedTilesIncomes.Count > 0)
            {
                TextMeshProUGUI incomeGivenDest = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                Dictionary<TileData, List<ResourceToIntMap>> datas = new Dictionary<TileData, List<ResourceToIntMap>>();
                foreach (var kvp in tile.ImpactedTilesIncomes)
                {
                    if (datas.ContainsKey(kvp.Key.TileData))
                        datas[kvp.Key.TileData] = Utilities.MergeResourceToIntMaps(datas[kvp.Key.TileData], kvp.Value);
                    else
                        datas.Add(kvp.Key.TileData, Utilities.CloneResourceToIntMaps(kvp.Value));
                }
                foreach (var kvpBis in datas)
                {
                    incomeGivenDest.text += "(" + kvpBis.Value.IncomeToString() + " to " + kvpBis.Key.TileName + ")" + "\n";
                }
                incomeGivenDest.alignment = TextAlignmentOptions.Center;
                incomeGivenDest.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region CARNIVALISTS
        if (tile.RecruitedCarnivalists > 0)
        {
            TextMeshProUGUI carnivalists = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            carnivalists.text = tile.RecruitedCarnivalists + "<sprite name=\"Carnivalist_Emoji\">";
            carnivalists.fontStyle = FontStyles.Bold;
            carnivalists.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region CARNIVALIST SOURCES
        if (_showSourcesOnPopUp)
        {
            if (tile.RecruitedCarnivalists > 0)
            {
                TextMeshProUGUI sourceCarn = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                sourceCarn.text = "(" + tile.RecruitedCarnivalists + "<sprite name=\"Carnivalist_Emoji\"> based on the tile effects)";
                sourceCarn.alignment = TextAlignmentOptions.Center;
                sourceCarn.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region CARNIVALISTS GIVEN
        if (tile.ImpactedTilesCarnivalists.Count > 0)
        {
            TextMeshProUGUI carnivalistsGiven = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            int totalCarnivalists = 0;
            foreach (var kvp in tile.ImpactedTilesCarnivalists)
            {
                totalCarnivalists += kvp.Value;
            }
            carnivalistsGiven.text = $"Provides {totalCarnivalists}<sprite name=\"Carnivalist_Emoji\"> over " +
                $"{tile.ImpactedTilesCarnivalists.Count} other {(tile.ImpactedTilesCarnivalists.Count > 1 ? "tiles" : "tile")}";
            carnivalistsGiven.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region CARNIVALISTS GIVEN DESTINATION
        if (_showSourcesOnPopUp)
        {
            if (tile.ImpactedTilesCarnivalists.Count > 0)
            {
                TextMeshProUGUI carnivalistsGivenDest = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                Dictionary<TileData, int> datas = new Dictionary<TileData, int>();
                foreach (var kvp in tile.ImpactedTilesCarnivalists)
                {
                    if (datas.ContainsKey(kvp.Key.TileData))
                        datas[kvp.Key.TileData] += kvp.Value;
                    else
                        datas.Add(kvp.Key.TileData, kvp.Value);
                }
                foreach (var kvpBis in datas)
                {
                    carnivalistsGivenDest.text += "(" + kvpBis.Value + "<sprite name=\"Carnivalist_Emoji\"> to " + kvpBis.Key.TileName + ")" + "\n";
                }
                carnivalistsGivenDest.alignment = TextAlignmentOptions.Center;
                carnivalistsGivenDest.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region BOOSTED ENT
        if (tile.EntImpactedByTile.Count > 0)
        {
            int totalPoints = 0;
            foreach (var kvp in tile.EntImpactedByTile)
            {
                totalPoints += kvp.Value;
            }
            TextMeshProUGUI boostedEnt = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            boostedEnt.text += "Provides +" + totalPoints + "<sprite name=\"Point_Emoji\"> over "
                + tile.EntImpactedByTile.Count + " " + Family.Entertainment.ToCustomString(tile.EntImpactedByTile.Count > 1);
            boostedEnt.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region BOOSTED ENT DESTINATION
        if (_showSourcesOnPopUp)
        {
            if (tile.EntImpactedByTile.Count > 0)
            {
                TextMeshProUGUI boostedEntDest = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                Dictionary<EntertainmentData, int> datas = new Dictionary<EntertainmentData, int>();
                foreach (var kvp in tile.EntImpactedByTile)
                {
                    if (datas.ContainsKey(kvp.Key.Entertainment.Data))
                        datas[kvp.Key.Entertainment.Data] += kvp.Value;
                    else
                        datas.Add(kvp.Key.Entertainment.Data, kvp.Value);
                }
                foreach (var kvpBis in datas)
                {
                    boostedEntDest.text += "(" + kvpBis.Value + "<sprite name=\"Point_Emoji\"> to " + kvpBis.Key.Type.ToCustomString(false) + ")" + "\n";
                }
                boostedEntDest.alignment = TextAlignmentOptions.Center;
                boostedEntDest.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region BONUS INCOME CALCULATION
        List<ResourceToIntMap> incomeBonus = Utilities.CloneResourceToIntMaps(tile.TileData.Incomes);
        if (tile.InitialData.Incomes.Count > 0 && tile.TileData != tile.InitialData)
        {
            incomeBonus = Utilities.MergeResourceToIntMaps(incomeBonus, tile.InitialData.Incomes);
        }
        if (tile.PreviousData != null)
        {
            if (tile.PreviousData != tile.InitialData)
            {
                if (tile.PreviousData.Incomes.Count > 0)
                {
                    incomeBonus = Utilities.MergeResourceToIntMaps(incomeBonus, tile.PreviousData.Incomes);
                }
            }
        }
        #endregion

        #region EFFECTS SEPARATION
        if ((tile.TileData is not HazardousTileData) && 
            (incomeBonus.Count > 0 || tile.TileData.SpecialBehaviours.Count > 0 
            || (tile.TileData is InfrastructureData infraData && infraData.ScoutStartingPoint)))
        {
            TextMeshProUGUI separationEffects = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            separationEffects.text = "--- Effects ---";
            separationEffects.fontStyle = FontStyles.Bold;
            separationEffects.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region INCOME BONUS
        if (incomeBonus.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            income.text = "<sprite name=\"Puce_Emoji\"> Base income " + incomeBonus.IncomeToString() + " per turn";
        }
        #endregion

        #region BEHAVIOURS
        if (tile.TileData.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in tile.TileData.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = "<sprite name=\"Puce_Emoji\"> " + behaviour.GetBehaviourDescription();
                ClampTextWidth(behaviourText);
            }
        }
        #endregion

        #region SCOUT STARTING POINT
        if (tile.TileData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "<sprite name=\"Puce_Emoji\"> Scout<sprite name=\"Scout_Emoji\"> starting point";
            ClampTextWidth(scoutText);
        }
        #endregion

        #region ENTERTAINMENT PLACEMENT
        if (tile.CanReceiveEntertainment() && 
            !tile.TileData.SpecialBehaviours
            .Any(b => b is AllowEntertainmentOnTileAndNeighbors))
        {
            TextMeshProUGUI entPlacement = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            entPlacement.text = "<sprite name=\"Puce_Emoji\"> Can receive an " + Family.Entertainment.ToCustomString();
            ClampTextWidth(entPlacement);
        }
        #endregion

        #region DETAILS SEPARATION
        if ((tile.TileData.AvailableInfrastructures.Count > 0 && GameManager.Instance.CurrentPhase != Phase.Entertain) 
            || (!tile.Claimed && tile.TileData is not HazardousTileData))
        {
            TextMeshProUGUI separationDetails = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            separationDetails.text = "--- Details ---";
            separationDetails.fontStyle = FontStyles.Bold;
            separationDetails.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region ENHANCEMENTS
        if (tile.TileData.AvailableInfrastructures.Count > 0 && GameManager.Instance.CurrentPhase != Phase.Entertain)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + tile.TileData.AvailableInfrastructures.ToCustomString(false, true);
            ClampTextWidth(enhancement);
            enhancement.fontStyle = FontStyles.Italic;
            enhancement.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region CLAIM COST
        if (!tile.Claimed && tile.TileData is not HazardousTileData)
        {
            TextMeshProUGUI claimStatus = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            claimStatus.text = "Claim cost: " + tile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">";
        }
        #endregion

        #region FAMILY
        if (tile.TileData is InfrastructureData infra)
        {
            TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            family.text = infra.Family.ToCustomString();
            family.alignment = TextAlignmentOptions.Right;
        }
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), tile.transform, false);
    }

    private void ScoutPopUp(Scout scout)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Scout<sprite name=\"Scout_Emoji\">";
        #endregion

        #region SPEED
        TextMeshProUGUI speedText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        speedText.text = "Speed: " + scout.Speed;
        #endregion

        #region REVEAL RADIUS
        TextMeshProUGUI revealText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        revealText.text = "Reveal radius: " + scout.RevealRadius;
        #endregion

        #region LIFESPAN
        TextMeshProUGUI lifespanText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        lifespanText.text = "Remaining turns: " + scout.Lifespan;
        ClampTextWidth(lifespanText);
        #endregion

        #region REDIRECTABLE
        if (ExplorationManager.Instance.UpgradeScoutRedirectable)
        {
            if (GameManager.Instance.CurrentPhase == Phase.Explore)
            {
                TextMeshProUGUI redirectText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                if (scout.HasRedirected)
                    redirectText.text = "Scout has already been redirected";
                else
                    redirectText.text = "Scout can be redirected";
                redirectText.fontStyle = FontStyles.Italic;
                redirectText.alignment = TextAlignmentOptions.Center;
            }
        }
        #endregion

        #region DIRECTION
        TextMeshProUGUI directionText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        directionText.text = scout.Direction.ToCustomString();
        directionText.fontStyle = FontStyles.Italic;
        directionText.alignment = TextAlignmentOptions.Right;
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), scout.CurrentTile.transform, true);
    }

    private void MultipleScoutsPopUp(Tile tile)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{tile.Scouts.Count} Scouts<sprite name=\"Scout_Emoji\">";
        #endregion

        #region DIRECTION
        Dictionary<Direction, int> directionCounts = new Dictionary<Direction, int>();
        foreach (Scout scout in tile.Scouts)
        {
            if (directionCounts.ContainsKey(scout.Direction))
                directionCounts[scout.Direction]++;
            else
                directionCounts.Add(scout.Direction, 1);
        }
        foreach (var kvp in directionCounts)
        {
            TextMeshProUGUI directionText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            directionText.text = $"{kvp.Value} {(kvp.Value > 1 ? "scouts" : "scout")} heading {kvp.Key.ToCustomString()}";
            ClampTextWidth(directionText);
        }
        #endregion

        #region REDIRECTABLE
        if (ExplorationManager.Instance.UpgradeScoutRedirectable)
        {
            if (GameManager.Instance.CurrentPhase == Phase.Explore)
            {
                int redirectableCount = 0;
                foreach (Scout scout in tile.Scouts)
                {
                    if (!scout.HasRedirected)
                        redirectableCount++;
                }
                TextMeshProUGUI redirectText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                if (redirectableCount == 0)
                    redirectText.text = "All scouts have already been redirected";
                else if (redirectableCount == tile.Scouts.Count)
                    redirectText.text = "All scouts can be redirected";
                else
                    redirectText.text = $"{redirectableCount} {(redirectableCount > 1 ? "scouts" : "scout")} can be redirected";
                redirectText.fontStyle = FontStyles.Italic;
                redirectText.alignment = TextAlignmentOptions.Center;
            }
        }
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), tile.transform, true);
    }

    private void EntertainmentPopUp(Entertainment ent)
    {
        JuiceManager.Instance.VisualizeEntertainmentCombo(
            ent.InternalPointsSources, ent.ExternalPointsSources,
            ent.Tile.EntImpactedByEntertainment, ent.Tile.EntImpactedByTile,
            ent.Tile);

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = ent.Data.Type.ToCustomString(false);
        #endregion

        #region POINTS
        TextMeshProUGUI pointsText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        pointsText.text = "+" + ent.Points + "<sprite name=\"Point_Emoji\">";
        pointsText.fontStyle = FontStyles.Bold;
        pointsText.alignment = TextAlignmentOptions.Center;
        #endregion

        #region POINTS SOURCES
        if (_showSourcesOnPopUp)
        {
            TextMeshProUGUI sourceInc = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            int predictedSelfPoints = ent.GetPointsFromEntertainmentOnly();
            if (predictedSelfPoints > 0)
                sourceInc.text = "(+" + predictedSelfPoints + "<sprite name=\"Point_Emoji\"> based on the entertainment effects)" + "\n";
            if (ent.ExternalPointsSources.Count > 0)
            {
                Dictionary<TileData, int> datas = new Dictionary<TileData, int>();
                foreach (var kvp in ent.ExternalPointsSources)
                {
                    if (datas.ContainsKey(kvp.Key.TileData))
                        datas[kvp.Key.TileData] += kvp.Value;
                    else
                        datas.Add(kvp.Key.TileData, kvp.Value);
                }
                foreach (var kvpBis in datas)
                    sourceInc.text += "(+" + kvpBis.Value + "<sprite name=\"Point_Emoji\"> from " + kvpBis.Key.TileName + ")" + "\n";
            }
            sourceInc.alignment = TextAlignmentOptions.Center;
            sourceInc.fontStyle = FontStyles.Italic;
        }
        #endregion

        #region POINTS GIVEN
        if (ent.Tile.EntImpactedByEntertainment.Count > 0)
        {
            TextMeshProUGUI pointsGiven = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            int pointsGivenValue = 0;
            foreach (var kvp in ent.Tile.EntImpactedByEntertainment)
            {
                pointsGivenValue += kvp.Value;
            }
            pointsGiven.text = "Provides +" + pointsGivenValue + "<sprite name=\"Point_Emoji\"> over "
                + ent.Tile.EntImpactedByEntertainment.Count + " other " + Family.Entertainment.ToCustomString(ent.Tile.EntImpactedByEntertainment.Count > 1); 
            pointsGiven.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region POINTS GIVEN DESTINATION
        if (_showSourcesOnPopUp)
        {
            if (ent.Tile.EntImpactedByEntertainment.Count > 0)
            {
                TextMeshProUGUI pointsGivenDest = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                Dictionary<EntertainmentData, int> datas = new Dictionary<EntertainmentData, int>();
                foreach (var kvp in ent.Tile.EntImpactedByEntertainment)
                {
                    if (datas.ContainsKey(kvp.Key.Entertainment.Data))
                        datas[kvp.Key.Entertainment.Data] += kvp.Value;
                    else
                        datas.Add(kvp.Key.Entertainment.Data, kvp.Value);
                }
                foreach (var kvpBis in datas)
                {
                    pointsGivenDest.text += "(+" + kvpBis.Value + "<sprite name=\"Point_Emoji\"> to " + kvpBis.Key.Type.ToCustomString(false) + ")" + "\n";
                }
                pointsGivenDest.alignment = TextAlignmentOptions.Center;
                pointsGivenDest.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region EFFECTS SEPARATION
        TextMeshProUGUI separationEffects = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationEffects.text = "--- Effects ---";
        separationEffects.fontStyle = FontStyles.Bold;
        separationEffects.alignment = TextAlignmentOptions.Center;
        #endregion

        #region BASE POINTS
        TextMeshProUGUI basePointsText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        basePointsText.text = "<sprite name=\"Puce_Emoji\"> Base points +" + ent.Data.BasePoints + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(basePointsText);
        #endregion

        #region EFFECT
        if (ent.Data.SpecialEffects.Count > 0)
        {
            foreach (SpecialEffect effect in ent.Data.SpecialEffects)
            {
                TextMeshProUGUI effectText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                effectText.text = "<sprite name=\"Puce_Emoji\"> " + effect.GetBehaviourDescription();
                ClampTextWidth(effectText);
            }
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = Family.Entertainment.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), ent.Tile.transform, false);
    }
    #endregion

    #region INTERACTION BUTTON POP UP
    private void ButtonScoutPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        BoostScoutOnSpawn boostingInfra = button.AssociatedTile.TileData.SpecialBehaviours
            .OfType<BoostScoutOnSpawn>()
            .FirstOrDefault();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Spawn a Scout<sprite name=\"Scout_Emoji\">";
        #endregion

        #region SPEED
        TextMeshProUGUI speedText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            speedText.text = "Speed: " + (button.ScoutData.Speed + boostingInfra.BoostSpeed + ExplorationManager.Instance.BoostScoutSpeed);
        else
            speedText.text = "Speed: " + (button.ScoutData.Speed + ExplorationManager.Instance.BoostScoutSpeed);
        #endregion

        #region REVEAL RADIUS
        TextMeshProUGUI revealText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            revealText.text = "Reveal radius: " + (button.ScoutData.RevealRadius + boostingInfra.BoostRevealRadius + ExplorationManager.Instance.BoostScoutRevealRadius);
        else
            revealText.text = "Reveal radius: " + (button.ScoutData.RevealRadius + ExplorationManager.Instance.BoostScoutRevealRadius);
        ClampTextWidth(revealText);
        #endregion

        #region LIFESPAN
        TextMeshProUGUI lifespanText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            lifespanText.text = "Lifespan: " + (button.ScoutData.Lifespan + boostingInfra.BoostLifespan + ExplorationManager.Instance.BoostScoutLifespan);
        else
            lifespanText.text = "Lifespan: " + (button.ScoutData.Lifespan + ExplorationManager.Instance.BoostScoutLifespan);
        #endregion

        #region AVAILABILITY
        TextMeshProUGUI availability = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        int availableCount = ExplorationManager.Instance.ScoutsLimit - ExplorationManager.Instance.CurrentScoutsCount;
        if (availableCount > 0)
        {
            if (availableCount == 1)
                availability.text = "1 Scout available";
            else
                availability.text = availableCount + " Scouts available";
        }
        else
        {
            availability.text = "No available Scout";
            availability.color = UIManager.Instance.ColorCantAfford;
        }
        availability.fontStyle = FontStyles.Italic;
        availability.alignment = TextAlignmentOptions.Center;
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
    }

    private void ButtonRedirectScoutPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        TextMeshProUGUI text = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        text.text = "Redirect a Scout<sprite name=\"Scout_Emoji\">";

        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
    }

    private void ButtonRevealAnywherePopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        TextMeshProUGUI text = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        text.text = $"Reveal this tile and all those in a {ExplorationManager.Instance.UpgradeRevealAnywhere.RevealRadius}-tile radius";
        ClampTextWidth(text);

        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
    }

    private void ButtonClaimPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Claim<sprite name=\"Claim_Emoji\"> " + button.AssociatedTile.TileData.TileName;
        #endregion

        #region CLAIM COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.AssociatedTile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">" + "(" + ResourcesManager.Instance.Claim + ")";
        if (!ResourcesManager.Instance.CanAffordClaim(button.AssociatedTile.TileData.ClaimCost))
            cost.color = UIManager.Instance.ColorCantAfford;
        ClampTextWidth(cost);
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
    }

    private void ButtonDestroyPopUp(string text, InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = text;
        #endregion

        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);

        ShowRemoveInfraTutoPopUp();
    }

    private void ButtonEntertainmentPopUp(InteractionButton button)
    {
        // Prediction
        int predictedPoints;
        int predictedSelfPoints;
        Dictionary<Tile, int> predictedExtSources;
        Dictionary<Tile, int> predictedIntSources;
        Dictionary<Tile, int> predictedEntImpactedByEnt;
        Dictionary<Tile, int> predictedEntImpactedByTile;
        EntertainmentManager.Instance.PredictEntertainmentSpawn(button.AssociatedTile, button.EntertainData,
            out predictedPoints, out predictedExtSources, out predictedIntSources, out predictedSelfPoints,
            out predictedEntImpactedByEnt, out predictedEntImpactedByTile);

        JuiceManager.Instance.VisualizeEntertainmentCombo(
            predictedIntSources, predictedExtSources,
            predictedEntImpactedByEnt, predictedEntImpactedByTile,
            button.AssociatedTile);

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Place " + button.EntertainData.Type.ToCustomString(false);
        #endregion

        #region PREDICTED POINTS
        TextMeshProUGUI predictedIncome = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        predictedIncome.text = "+" + predictedPoints + "<sprite name=\"Point_Emoji\">";
        predictedIncome.fontStyle = FontStyles.Bold;
        predictedIncome.alignment = TextAlignmentOptions.Center;
        #endregion

        #region POINTS SOURCES
        if (_showSourcesOnPopUp)
        {
            TextMeshProUGUI sourceInc = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            if (predictedSelfPoints > 0)
                sourceInc.text = "(+" + predictedSelfPoints + "<sprite name=\"Point_Emoji\"> based on the entertainment effects)" + "\n";
            if (predictedExtSources.Count > 0)
            {
                Dictionary<TileData, int> datas = new Dictionary<TileData, int>();
                foreach (var kvp in predictedExtSources)
                {
                    if (datas.ContainsKey(kvp.Key.TileData))
                        datas[kvp.Key.TileData] += kvp.Value;
                    else
                        datas.Add(kvp.Key.TileData, kvp.Value);
                }
                foreach (var kvpBis in datas)
                    sourceInc.text += "(+" + kvpBis.Value + "<sprite name=\"Point_Emoji\"> from " + kvpBis.Key.TileName + ")" + "\n";
            }
            sourceInc.alignment = TextAlignmentOptions.Center;
            sourceInc.fontStyle = FontStyles.Italic;
        }
        #endregion

        #region POINTS GIVEN
        if (predictedEntImpactedByEnt.Count > 0)
        {
            int totalPointsGiven = 0;
            foreach (var kvp in predictedEntImpactedByEnt)
            {
                totalPointsGiven += kvp.Value;
            }
            TextMeshProUGUI pointsGiven = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            pointsGiven.text = "Provides +" + totalPointsGiven + "<sprite name=\"Point_Emoji\"> over "
                + predictedEntImpactedByEnt.Count + " other " + Family.Entertainment.ToCustomString(predictedEntImpactedByEnt.Count > 1);
            pointsGiven.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region POINTS GIVEN DESTINATION
        if (_showSourcesOnPopUp)
        {
            if (predictedEntImpactedByEnt.Count > 0)
            {
                TextMeshProUGUI pointsGivenDest = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                Dictionary<EntertainmentData, int> datas = new Dictionary<EntertainmentData, int>();
                foreach (var kvp in predictedEntImpactedByEnt)
                {
                    if (datas.ContainsKey(kvp.Key.Entertainment.Data))
                        datas[kvp.Key.Entertainment.Data] += kvp.Value;
                    else
                        datas.Add(kvp.Key.Entertainment.Data, kvp.Value);
                }
                foreach (var kvpBis in datas)
                {
                    pointsGivenDest.text += "(+" + kvpBis.Value + "<sprite name=\"Point_Emoji\"> to " + kvpBis.Key.Type.ToCustomString(false) + ")" + "\n";
                }
                pointsGivenDest.alignment = TextAlignmentOptions.Center;
                pointsGivenDest.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region EFFECTS SEPARATION
        TextMeshProUGUI separationEffects = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationEffects.text = "--- Effects ---";
        separationEffects.fontStyle = FontStyles.Bold;
        separationEffects.alignment = TextAlignmentOptions.Center;
        #endregion

        #region POINTS
        TextMeshProUGUI pointsText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        pointsText.text = "<sprite name=\"Puce_Emoji\"> Base points +" + button.EntertainData.BasePoints + "<sprite name=\"Point_Emoji\">";
        #endregion

        #region EFFECT
        if (button.EntertainData.SpecialEffects.Count > 0)
        {
            foreach (SpecialEffect effect in button.EntertainData.SpecialEffects)
            {
                TextMeshProUGUI effectText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                effectText.text = "<sprite name=\"Puce_Emoji\"> " + effect.GetBehaviourDescription();
                ClampTextWidth(effectText);
            }
        }
        #endregion

        #region DETAILS SEPARATION
        TextMeshProUGUI separationDetails = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationDetails.text = "--- Details ---";
        separationDetails.fontStyle = FontStyles.Bold;
        separationDetails.alignment = TextAlignmentOptions.Center;
        #endregion

        #region COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.EntertainData.GetActualCarnivalistCost(button.AssociatedTile) + "<sprite name=\"Carnivalist_Emoji\">";
        if (!ResourcesManager.Instance.CanAffordCarnivalist(button.EntertainData.GetActualCarnivalistCost(button.AssociatedTile)))
            cost.color = UIManager.Instance.ColorCantAfford;
        ClampTextWidth(cost);
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = Family.Entertainment.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, false);
    }

    private void ButtonInfraPopUp(InteractionButton button)
    {
        // Get predicted income
        List<ResourceToIntMap> predictedInc;
        List<ResourceToIntMap> predictedSelfInc;
        int predictedCarnivalists;
        Dictionary<Tile, List<ResourceToIntMap>> predictedExtSources;
        Dictionary<Tile, List<ResourceToIntMap>> predictedIntSources;
        Dictionary<Tile, int> predictedInternalCarnivalistsSources;
        Dictionary<Tile, List<ResourceToIntMap>> predictedImpactedTilesIncomes;
        Dictionary<Tile, int> predictedImpactedTilesCarnivalists;
        ExploitationManager.Instance.GetPredictedIncomes(button.AssociatedTile, button.InfrastructureData,
            out predictedInc, out predictedExtSources, out predictedIntSources,
            out predictedSelfInc, out predictedCarnivalists, out predictedInternalCarnivalistsSources,
            out predictedImpactedTilesIncomes, out predictedImpactedTilesCarnivalists);

        JuiceManager.Instance.VisualizeExploitationCombo(
            predictedIntSources, predictedExtSources, predictedInternalCarnivalistsSources,
            predictedImpactedTilesIncomes, predictedImpactedTilesCarnivalists,
            button.AssociatedTile);

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Build " + button.InfrastructureData.TileName;
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();

        if (!Utilities.AreIncomesEqual(button.AssociatedTile.Incomes, predictedInc))
        {
            income.text = predictedInc.IncomeToString() + " per turn";
            string hex = ColorUtility.ToHtmlStringRGBA(UIManager.Instance.ColorEnhancementNewEffect);
            income.text += $" <color=#{hex}>(gain " + Utilities.SubtractResourceToIntMaps(predictedInc, button.AssociatedTile.Incomes).IncomeToString() + ")</color>";
        }
        else
        {
            income.text = button.AssociatedTile.Incomes.IncomeToString() + " per turn";
        }

        income.fontStyle = FontStyles.Bold;
        income.alignment = TextAlignmentOptions.Center;
        #endregion

        #region INCOME SOURCES
        if (_showSourcesOnPopUp)
        {
            if (predictedInc.Count > 0)
            {
                TextMeshProUGUI sourceInc = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                if (predictedSelfInc.Count > 0)
                    sourceInc.text = "(" + predictedSelfInc.IncomeToString() + " based on the tile effects)" + "\n";
                if (predictedExtSources.Count > 0)
                {
                    Dictionary<TileData, List<ResourceToIntMap>> datas = new Dictionary<TileData, List<ResourceToIntMap>>();
                    foreach (var kvp in predictedExtSources)
                    {
                        if (datas.ContainsKey(kvp.Key.TileData))
                            datas[kvp.Key.TileData] = Utilities.MergeResourceToIntMaps(datas[kvp.Key.TileData], kvp.Value);
                        else
                            datas.Add(kvp.Key.TileData, Utilities.CloneResourceToIntMaps(kvp.Value));
                    }
                    foreach (var kvpBis in datas)
                    {
                        sourceInc.text += "(" + kvpBis.Value.IncomeToString() + " from " + kvpBis.Key.TileName + ")" + "\n";
                    }
                }
                sourceInc.alignment = TextAlignmentOptions.Center;
                sourceInc.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region INCOMES GIVEN
        if (predictedImpactedTilesIncomes.Count > 0)
        {
            TextMeshProUGUI incomeGiven = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            List<ResourceToIntMap> totalGiven = new List<ResourceToIntMap>();
            foreach (var kvp in predictedImpactedTilesIncomes)
            {
                totalGiven = Utilities.MergeResourceToIntMaps(totalGiven, kvp.Value);
            }
            List<ResourceToIntMap> previousTotalGiven = new List<ResourceToIntMap>();
            foreach (var kvp in button.AssociatedTile.ImpactedTilesIncomes)
            {
                previousTotalGiven = Utilities.MergeResourceToIntMaps(previousTotalGiven, kvp.Value);
            }
            
            if (!Utilities.AreIncomesEqual(totalGiven, previousTotalGiven))
            {
                incomeGiven.text = $"Provides {totalGiven.IncomeToString()} over " + 
                    $"{predictedImpactedTilesIncomes.Count} other {(predictedImpactedTilesIncomes.Count > 1 ? "tiles" : "tile")}";
                string hex = ColorUtility.ToHtmlStringRGBA(UIManager.Instance.ColorEnhancementNewEffect);
                incomeGiven.text += $" <color=#{hex}>(gain " + Utilities.SubtractResourceToIntMaps(totalGiven, previousTotalGiven).IncomeToString() + ")</color>";
            }
            else
            {
                incomeGiven.text = $"Provides {previousTotalGiven.IncomeToString()} over " +
                $"{button.AssociatedTile.ImpactedTilesIncomes.Count} other {(button.AssociatedTile.ImpactedTilesIncomes.Count > 1 ? "tiles" : "tile")}";
            }
            incomeGiven.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region INCOME GIVEN DESTINATION
        if (_showSourcesOnPopUp)
        {
            if (predictedImpactedTilesIncomes.Count > 0)
            {
                TextMeshProUGUI incomeGivenDest = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                Dictionary<TileData, List<ResourceToIntMap>> datas = new Dictionary<TileData, List<ResourceToIntMap>>();
                foreach (var kvp in predictedImpactedTilesIncomes)
                {
                    if (datas.ContainsKey(kvp.Key.TileData))
                        datas[kvp.Key.TileData] = Utilities.MergeResourceToIntMaps(datas[kvp.Key.TileData], kvp.Value);
                    else
                        datas.Add(kvp.Key.TileData, Utilities.CloneResourceToIntMaps(kvp.Value));
                }
                foreach (var kvpBis in datas)
                {
                    incomeGivenDest.text += "(" + kvpBis.Value.IncomeToString() + " to " + kvpBis.Key.TileName + ")" + "\n";
                }
                incomeGivenDest.alignment = TextAlignmentOptions.Center;
                incomeGivenDest.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region CARNIVALISTS
        if (button.AssociatedTile.RecruitedCarnivalists > 0 || predictedCarnivalists > 0)
        {
            TextMeshProUGUI carnivalists = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            int countCarnivalists = Mathf.Max(button.AssociatedTile.RecruitedCarnivalists, predictedCarnivalists);
            carnivalists.text = $"{countCarnivalists}<sprite name=\"Carnivalist_Emoji\"></color>";

            if (button.AssociatedTile.RecruitedCarnivalists != predictedCarnivalists)
            {
                string hex = ColorUtility.ToHtmlStringRGBA(UIManager.Instance.ColorEnhancementNewEffect);
                carnivalists.text += $" <color=#{hex}>(gain {predictedCarnivalists - button.AssociatedTile.RecruitedCarnivalists}<sprite name=\"Carnivalist_Emoji\">)</color>";
            }

            carnivalists.fontStyle = FontStyles.Bold;
            carnivalists.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region CARNIVALISTS SOURCES
        if (_showSourcesOnPopUp)
        {
            if (predictedCarnivalists > 0)
            {
                TextMeshProUGUI sourceCarn = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                sourceCarn.text = "(" + predictedCarnivalists + "<sprite name=\"Carnivalist_Emoji\"> based on the tile effects)";
                sourceCarn.alignment = TextAlignmentOptions.Center;
                sourceCarn.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region CARNIVALISTS GIVEN
        if (predictedImpactedTilesCarnivalists.Count > 0)
        {
            TextMeshProUGUI carnivalistsGiven = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            int totalCarnivalists = 0;
            foreach (var kvp in predictedImpactedTilesCarnivalists)
            {
                totalCarnivalists += kvp.Value;
            }
            int previousTotalCarnivalists = 0;
            foreach (var item in button.AssociatedTile.ImpactedTilesCarnivalists)
            {
                previousTotalCarnivalists += item.Value;
            }

            if (totalCarnivalists != previousTotalCarnivalists)
            {
                string hex = ColorUtility.ToHtmlStringRGBA(UIManager.Instance.ColorEnhancementNewEffect);
                carnivalistsGiven.text = $"Provides {totalCarnivalists}<sprite name=\"Carnivalist_Emoji\"> over " +
                $"{predictedImpactedTilesCarnivalists.Count} other {(predictedImpactedTilesCarnivalists.Count > 1 ? "tiles" : "tile")}" +
                $" <color=#{hex}>(gain {totalCarnivalists - previousTotalCarnivalists}<sprite name=\"Carnivalist_Emoji\">)</color>";
            }
            else
                carnivalistsGiven.text = $"Provides {totalCarnivalists}<sprite name=\"Carnivalist_Emoji\"> over " +
                $"{button.AssociatedTile.ImpactedTilesCarnivalists.Count} other {(button.AssociatedTile.ImpactedTilesCarnivalists.Count > 1 ? "tiles" : "tile")}";
            carnivalistsGiven.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region CARNIVALISTS GIVEN DESTINATION
        if (_showSourcesOnPopUp)
        {
            if (predictedImpactedTilesCarnivalists.Count > 0)
            {
                TextMeshProUGUI carnivalistsGivenDest = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                Dictionary<TileData, int> datas = new Dictionary<TileData, int>();
                foreach (var kvp in predictedImpactedTilesCarnivalists)
                {
                    if (datas.ContainsKey(kvp.Key.TileData))
                        datas[kvp.Key.TileData] += kvp.Value;
                    else
                        datas.Add(kvp.Key.TileData, kvp.Value);
                }
                foreach (var kvpBis in datas)
                {
                    carnivalistsGivenDest.text += "(" + kvpBis.Value + "<sprite name=\"Carnivalist_Emoji\"> to " + kvpBis.Key.TileName + ")" + "\n";
                }
                carnivalistsGivenDest.alignment = TextAlignmentOptions.Center;
                carnivalistsGivenDest.fontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region EFFECTS SEPARATION
        TextMeshProUGUI separationEffects = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationEffects.text = "--- Effects ---";
        separationEffects.fontStyle = FontStyles.Bold;
        separationEffects.alignment = TextAlignmentOptions.Center;

        List<RectTransform> newEffects = new List<RectTransform>();
        List<RectTransform> oldEffects = new List<RectTransform>();
        #endregion

        #region AUTO CLAIM TOWN UPGRADE
        if (GameManager.Instance.CurrentPhase == Phase.Expand && ExpansionManager.Instance.UpgradeTownAutoClaim)
        {
            TextMeshProUGUI autoClaimText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            autoClaimText.text = "<sprite name=\"Plus_Emoji\"> Automatically claims<sprite name=\"Claim_Emoji\"> the 6 surrounding tiles";
            autoClaimText.color = UIManager.Instance.ColorEnhancementNewEffect;
            newEffects.Add(autoClaimText.GetComponent<RectTransform>());
            ClampTextWidth(autoClaimText);
        }
        #endregion

        #region INCOME BONUS
        if (button.InfrastructureData.Incomes.Count > 0)
        {
            TextMeshProUGUI incomeBonus = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            incomeBonus.text = "<sprite name=\"Plus_Emoji\"> Improve base income by " + button.InfrastructureData.Incomes.IncomeToString() + " per turn";
            incomeBonus.color = UIManager.Instance.ColorEnhancementNewEffect;
            newEffects.Add(incomeBonus.GetComponent<RectTransform>());
            ClampTextWidth(incomeBonus);
        }
        #endregion

        #region BEHAVIOURS
        if (button.InfrastructureData.SpecialBehaviours.Count > 0)
        {
            List<SpecialBehaviour> previousBehaviours = new List<SpecialBehaviour>(button.AssociatedTile.TileData.SpecialBehaviours);
            foreach (SpecialBehaviour behaviour in button.InfrastructureData.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                if (!previousBehaviours.ContainsOriginal(behaviour))
                {
                    behaviourText.text = "<sprite name=\"Plus_Emoji\"> " + behaviour.GetBehaviourDescription();
                    behaviourText.color = UIManager.Instance.ColorEnhancementNewEffect;
                    newEffects.Add(behaviourText.GetComponent<RectTransform>());
                }
                else
                {
                    behaviourText.text = "<sprite name=\"Puce_Emoji\"> " + behaviour.GetBehaviourDescription();
                    oldEffects.Add(behaviourText.GetComponent<RectTransform>());
                }
                ClampTextWidth(behaviourText);
            }
        }
        #endregion

        #region SCOUT STARTING POINT
        if (button.InfrastructureData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            if (button.AssociatedTile.TileData is InfrastructureData infraData)
            {
                if (!infraData.ScoutStartingPoint)
                {
                    scoutText.text = "<sprite name=\"Plus_Emoji\"> Scout<sprite name=\"Scout_Emoji\"> starting point";
                    scoutText.color = UIManager.Instance.ColorEnhancementNewEffect;
                    newEffects.Add(scoutText.GetComponent<RectTransform>());
                }

                else
                {
                    scoutText.text = "<sprite name=\"Puce_Emoji\"> Scout<sprite name=\"Scout_Emoji\"> starting point";
                    oldEffects.Add(scoutText.GetComponent<RectTransform>());
                }
            }
            else
            {
                scoutText.text = "<sprite name=\"Plus_Emoji\"> Scout<sprite name=\"Scout_Emoji\"> starting point";
                scoutText.color = UIManager.Instance.ColorEnhancementNewEffect;
                newEffects.Add(scoutText.GetComponent<RectTransform>());
            }
            ClampTextWidth(scoutText);
        }
        #endregion

        #region EFFECTS ORDER
        foreach (RectTransform rect in newEffects)
        {
            rect.SetAsLastSibling();
        }
        foreach (RectTransform rect in oldEffects)
        {
            rect.SetAsLastSibling();
        }
        #endregion

        #region DETAILS SEPARATION
        TextMeshProUGUI separationDetails = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationDetails.text = "--- Details ---";
        separationDetails.fontStyle = FontStyles.Bold;
        separationDetails.alignment = TextAlignmentOptions.Center;
        #endregion

        #region ENHANCEMENTS
        if (button.InfrastructureData.AvailableInfrastructures.Count > 0)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + button.InfrastructureData.AvailableInfrastructures.ToCustomString(false, true);

            ClampTextWidth(enhancement);
            enhancement.fontStyle = FontStyles.Italic;
            enhancement.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (GameManager.Instance.CurrentPhase == Phase.Expand)
        {
            cost.text = "Cost: " + button.InfrastructureData.ClaimCost + "<sprite name=\"Claim_Emoji\">" + "(" + ResourcesManager.Instance.Claim + ")";
            if (!ResourcesManager.Instance.CanAffordClaim(button.InfrastructureData.ClaimCost))
                cost.color = UIManager.Instance.ColorCantAfford;
        }
        else
        {
            cost.text = "Cost: " + button.InfrastructureData.Costs.CostToString();
            if (!ResourcesManager.Instance.CanAfford(button.InfrastructureData.Costs))
                cost.color = UIManager.Instance.ColorCantAfford;
        }
        ClampTextWidth(cost);
        #endregion

        #region AVAILABILITY
        if (ExploitationManager.Instance.DoesInfraLimitExist(button.InfrastructureData))
        {
            TextMeshProUGUI availability = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            int existingCount = ExploitationManager.Instance.GetAvailableInfraCopies(button.InfrastructureData);
            if (existingCount > 0)
            {
                if (existingCount == 1)
                    availability.text = "1 copy available";
                else
                    availability.text = existingCount + " copies available";
            }
            else
            {
                availability.text = "No available copy";
                availability.color = UIManager.Instance.ColorCantAfford;
            }
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = button.InfrastructureData.Family.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, false);
    }
    #endregion

    #region POSITIONING & LAYOUT
    private void PositionPopup(RectTransform popupRect, Transform refTransform, bool nextToCursor)
    {
        popupRect.SetAsLastSibling();

        if (!nextToCursor) // If not next to cursor it indicates that we are visualizing combo with this popup;
            JuiceManager.Instance.PopUpVisualizingCombo = popupRect.gameObject;

        Utilities.AnchorWrapperToContent(popupRect, popupRect.GetChild(1).GetComponent<RectTransform>());

        Vector3 refPos = Camera.main.WorldToScreenPoint(refTransform.position);
        // Quadrant → in which quadrant of the screen is the tile
        bool isLeft = refPos.x <= (Screen.width * 0.5f);
        bool isBottom = refPos.y <= (Screen.height * 0.5f);

        // First popup → use limits
        if (_popUps.Count == 1)
        { 
            if (nextToCursor)
            {
                SnapToCursor(popupRect);
            }
            else
            {
                SnapToBottomLeftAnchor(popupRect);
            }
        }
        // Subsequent popups → stack relative to previous
        else
        {
            SnapToRelatives(popupRect);
        }
    }

    private void SnapToCursor(RectTransform popup)
    {
        Vector2 screenPos = Input.mousePosition;
        Vector2 normalized = new Vector2(
            screenPos.x / Screen.width,
            screenPos.y / Screen.height
        );

        bool isLeft = normalized.x <= 0.5f;
        bool isBottom = normalized.y <= 0.5f;

        RectTransform parent = popup.parent as RectTransform;
        float widthNorm = popup.rect.width / parent.rect.width;
        float heightNorm = popup.rect.height / parent.rect.height;

        Vector2 aMin, aMax;

        // X
        if (isLeft)
        {
            aMin.x = normalized.x + _offsetPopupOnCursor;
            aMax.x = aMin.x + widthNorm;
        }
        else
        {
            aMax.x = normalized.x - _offsetPopupOnCursor;
            aMin.x = aMax.x - widthNorm;
        }

        // Y
        if (isBottom)
        {
            aMin.y = normalized.y + _offsetPopupOnCursor;
            aMax.y = aMin.y + heightNorm;
        }
        else
        {
            aMax.y = normalized.y - _offsetPopupOnCursor;
            aMin.y = aMax.y - heightNorm;
        }

        // Clamp dans [0,1]
        aMin.x = Mathf.Clamp01(aMin.x);
        aMax.x = Mathf.Clamp01(aMax.x);
        aMin.y = Mathf.Clamp01(aMin.y);
        aMax.y = Mathf.Clamp01(aMax.y);

        // Appliquer
        popup.anchorMin = aMin;
        popup.anchorMax = aMax;
        popup.offsetMin = Vector2.zero;
        popup.offsetMax = Vector2.zero;
    }

    private void SnapToRelatives(RectTransform popup)
    {
        // Récupère l'index du popup dans la liste
        int i = _popUps.IndexOf(popup.gameObject);
        if (i < 1) return; // rien à faire si pas de précédent

        RectTransform parent = (RectTransform)popup.parent;
        if (parent == null) return;

        // Normes de taille du popup
        float normW = popup.rect.width / parent.rect.width;
        float normH = popup.rect.height / parent.rect.height;

        // Récupère prev et (optionnel) beforePrev
        RectTransform prev = _popUps[i - 1]?.GetComponent<RectTransform>();
        if (prev == null) return;

        RectTransform beforePrev = (i >= 2) ? _popUps[i - 2]?.GetComponent<RectTransform>() : null;

        // Décision verticale + horizontale
        bool placeAbove;
        bool alignLeft;

        if (beforePrev == null)
        {
            // Cas: un seul autre popup déjà placé -> quadrant du prev
            float centerX = (prev.anchorMin.x + prev.anchorMax.x) * 0.5f;
            float centerY = (prev.anchorMin.y + prev.anchorMax.y) * 0.5f;

            bool isLeft = centerX <= 0.5f;
            bool isBottom = centerY <= 0.5f;

            placeAbove = isBottom;     // bas -> on place au-dessus, sinon en dessous
            alignLeft = isLeft;       // gauche -> aligner sur X min, sinon X max
        }
        else
        {
            // Cas: plusieurs déjà placés -> reproduire la logique des deux derniers
            float prevCenterY = (prev.anchorMin.y + prev.anchorMax.y) * 0.5f;
            float beforePrevCenterY = (beforePrev.anchorMin.y + beforePrev.anchorMax.y) * 0.5f;

            placeAbove = prevCenterY >= beforePrevCenterY;

            const float eps = 1e-4f;
            bool sameMin = Mathf.Abs(prev.anchorMin.x - beforePrev.anchorMin.x) <= eps;
            bool sameMax = Mathf.Abs(prev.anchorMax.x - beforePrev.anchorMax.x) <= eps;

            // Si les mins sont alignés, on continue à aligner à gauche, sinon à droite.
            // (Si rien ne matche exactement, on privilégie le côté le plus proche.)
            if (sameMin) alignLeft = true;
            else if (sameMax) alignLeft = false;
            else
            {
                // Heuristique : choisir le côté dont l'écart est le plus faible
                float dMin = Mathf.Abs(prev.anchorMin.x - beforePrev.anchorMin.x);
                float dMax = Mathf.Abs(prev.anchorMax.x - beforePrev.anchorMax.x);
                alignLeft = (dMin <= dMax);
            }
        }

        // Calcul des anchors
        Vector2 aMin = popup.anchorMin;
        Vector2 aMax = popup.anchorMax;

        // Horizontal : aligner sur le bord choisi du prev
        if (alignLeft)
        {
            aMin.x = prev.anchorMin.x;
            aMax.x = aMin.x + normW;
        }
        else
        {
            aMax.x = prev.anchorMax.x;
            aMin.x = aMax.x - normW;
        }

        // Vertical : empilement avec l'offset normalisé
        float oy = _offsetBetweenSeveralPopUps;
        if (placeAbove)
        {
            // au-dessus du prev
            aMin.y = prev.anchorMax.y + oy;
            aMax.y = aMin.y + normH;
        }
        else
        {
            // en dessous du prev
            aMax.y = prev.anchorMin.y - oy;
            aMin.y = aMax.y - normH;
        }

        // Clamp [0,1] (même philosophie que tes méthodes existantes)
        aMin.x = Mathf.Clamp01(aMin.x);
        aMax.x = Mathf.Clamp01(aMax.x);
        aMin.y = Mathf.Clamp01(aMin.y);
        aMax.y = Mathf.Clamp01(aMax.y);

        // Appliquer
        popup.anchorMin = aMin;
        popup.anchorMax = aMax;

        // Zéro offsets pour un layout 100% drivé par les anchors
        popup.offsetMin = Vector2.zero;
        popup.offsetMax = Vector2.zero;
        popup.anchoredPosition = Vector2.zero;
        popup.sizeDelta = Vector2.zero;
    }

    private void SnapToBottomLeftAnchor(RectTransform popup)
    {
        RectTransform parent = popup.parent as RectTransform;
        if (parent == null) return;

        // Taille du popup en normalisé du parent
        float widthNorm = popup.rect.width / parent.rect.width;
        float heightNorm = popup.rect.height / parent.rect.height;

        // Position MONDE du point (_bottomLeftSnap est un point : anchorMin == anchorMax)
        Vector3 worldPos = _popUpLeftBottomAnchor.transform.position;

        // Conversion en local parent du popup
        Vector2 localPos = parent.InverseTransformPoint(worldPos);

        // Local -> normalisé [0..1] du parent
        Rect pr = parent.rect;
        Vector2 pNorm = new Vector2(
            (localPos.x - pr.xMin) / pr.width,
            (localPos.y - pr.yMin) / pr.height
        );

        Vector2 aMin = new Vector2(pNorm.x, pNorm.y);
        Vector2 aMax = new Vector2(aMin.x + widthNorm, aMin.y + heightNorm);

        // Clamp [0,1] (même comportement que SnapToCursor)
        aMin.x = Mathf.Clamp01(aMin.x);
        aMin.y = Mathf.Clamp01(aMin.y);
        aMax.x = Mathf.Clamp01(aMax.x);
        aMax.y = Mathf.Clamp01(aMax.y);

        // Appliquer (anchors only)
        popup.anchorMin = aMin;
        popup.anchorMax = aMax;
        popup.offsetMin = Vector2.zero;
        popup.offsetMax = Vector2.zero;
    }

    private void ClampTextWidth(TextMeshProUGUI tmp)
    {
        tmp.ForceMeshUpdate();
        tmp.alignment = TextAlignmentOptions.Justified;
        float contentWidth = tmp.preferredWidth;
        if (contentWidth < _minAllowed)
            contentWidth = _minAllowed;
        tmp.GetComponent<LayoutElement>().preferredWidth = Mathf.Min(contentWidth, _maxAllowed);
    }

    #endregion

    #region POPUP ON POPUP
    #region POPUP
    public void FamilyPopup(Family family, RectTransform refObject)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = family.ToCustomString();
        #endregion

        #region FAMILY LIST
        TextMeshProUGUI familyList = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (family == Family.Entertainment)
        {
            List<EntertainmentData> entOfFamily = new List<EntertainmentData>();
            foreach (EntertainmentData ent in EntertainmentManager.Instance.EntertainmentsData)
            {
                entOfFamily.Add(ent);
            }
            familyList.text = entOfFamily.ToCustomString();
        }
        else
        {
            List<InfrastructureData> infraOfFamily = new List<InfrastructureData>();
            foreach (InfrastructureData infra in ExploitationManager.Instance.AllInfraDatas)
            {
                if (infra.Family == family)
                {
                    infraOfFamily.Add(infra);
                }
            }
            familyList.text = infraOfFamily.ToCustomString(false);
        }
        ClampTextWidth(familyList);
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), refObject, true);
    }

    public void InfrastructurePopup(InfrastructureData infra, RectTransform refObject)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = infra.TileName;
        #endregion

        #region INCOME BONUS
        if (infra.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            income.text = "<sprite name=\"Puce_Emoji\"> Improve income by " + infra.Incomes.IncomeToString() + " per turn";
        }
        #endregion

        #region BEHAVIOURS
        if (infra.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in infra.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = "<sprite name=\"Puce_Emoji\"> " + behaviour.GetBehaviourDescription();
                ClampTextWidth(behaviourText);
            }
        }
        #endregion

        #region SCOUT STARTING POINT
        if (infra.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "<sprite name=\"Puce_Emoji\"> Scout<sprite name=\"Scout_Emoji\"> starting point";
            ClampTextWidth(scoutText);
        }
        #endregion

        #region ENHANCEMENTS
        if (infra.AvailableInfrastructures.Count > 0)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + infra.AvailableInfrastructures.ToCustomString(false, true);

            ClampTextWidth(enhancement);
            enhancement.fontStyle = FontStyles.Italic;
            enhancement.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (GameManager.Instance.CurrentPhase == Phase.Expand)
        {
            cost.text = "Cost: " + infra.ClaimCost + "<sprite name=\"Claim_Emoji\">" + "(" + ResourcesManager.Instance.Claim + ")";
            if (!ResourcesManager.Instance.CanAffordClaim(infra.ClaimCost))
                cost.color = UIManager.Instance.ColorCantAfford;
        }
        else
        {
            cost.text = "Cost: " + infra.Costs.CostToString();
            if (!ResourcesManager.Instance.CanAfford(infra.Costs))
                cost.color = UIManager.Instance.ColorCantAfford;
        }
        ClampTextWidth(cost);
        #endregion

        #region AVAILABILITY
        if (ExploitationManager.Instance.DoesInfraLimitExist(infra))
        {
            TextMeshProUGUI availability = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            int existingCount = ExploitationManager.Instance.GetAvailableInfraCopies(infra);
            if (existingCount > 0)
            {
                if (existingCount == 1)
                    availability.text = "1 copy available";
                else
                    availability.text = existingCount + " copies available";
            }
            else
            {
                availability.text = "No available copy";
                availability.color = UIManager.Instance.ColorCantAfford;
            }
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = infra.Family.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), refObject, true);
    }

    public void EntertainmentPopup(EntertainmentData ent, RectTransform refObject)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = ent.Type.ToCustomString(false);
        #endregion

        #region BASE POINTS
        TextMeshProUGUI basePointsText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        basePointsText.text = "<sprite name=\"Puce_Emoji\"> Base points +" + ent.BasePoints + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(basePointsText);
        #endregion

        #region EFFECT
        if (ent.SpecialEffects.Count > 0)
        {
            foreach (SpecialEffect effect in ent.SpecialEffects)
            {
                TextMeshProUGUI effectText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                effectText.text = "<sprite name=\"Puce_Emoji\"> " + effect.GetBehaviourDescription();
                ClampTextWidth(effectText);
            }
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = Family.Entertainment.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        #endregion

        SurvivingPopup(popUp);
        PositionPopup(popUp.GetComponent<RectTransform>(), refObject, true);
    }
    #endregion

    #region LOGIC
    private void SurvivingPopup(GameObject popUp)
    {
        GameObject survivingTimer = Instantiate(_survivingTimer, popUp.transform.GetChild(1).transform);
        survivingTimer.transform.SetAsLastSibling();
        UI_SurvivingPopup survivingPopup = popUp.AddComponent<UI_SurvivingPopup>();
        survivingPopup.TimerRoot = survivingTimer.GetComponent<RectTransform>();
        _survivingPopups.Add(survivingPopup);
        survivingPopup.SurvivingDuration = _survivablePopupDuration;
    }

    private void CloseSurvivingPopups()
    {
        if (_survivingPopups.Count != 0)
        {
            foreach (UI_SurvivingPopup item in _survivingPopups)
            {
                if (_isLockingPopup)
                {
                    if (item.gameObject == _lockingPopup)
                        continue;
                }
                item.ClosePopup();
            }
            _survivingPopups.Clear();
        }
    }

    //Used to lock a popup on screen until user closes it
    private void Update()
    {
        if (_isLockingPopup && _lockingImage != null)
        {
            _lockingTimer += Time.deltaTime;
            float t = (_durationForLockingPopup <= 0f) ? 1f : Mathf.Clamp01(_lockingTimer / _durationForLockingPopup);
            _lockingImage.fillAmount = t;
            if (_lockingTimer >= _durationForLockingPopup)
            {
                _isLockingPopup = false;
                _lockingTimer = 0f;
                LockPopup(_lockingPopup);
            }
        }
    }

    private void StartLockingPopup(GameObject popUp)
    {
        if (TutorialManager.Instance != null)
            return;

        _lockingPopup = popUp;
        _isLockingPopup = true;
        _lockingTimer = 0f;

        GameObject lockObject = Instantiate(_lockingObject, _popUpParent);
        lockObject.transform.SetAsLastSibling();
        foreach (var item in lockObject.GetComponentsInChildren<Image>())
        {
            if (item.type == Image.Type.Filled)
            {
                _lockingImage = item;
                break;
            }
        }
        Utilities.PlacePrefabAroundTargetTopRight(lockObject.GetComponent<RectTransform>(), popUp.GetComponent<RectTransform>(), _lockImagePopupOffset);
    }

    private void StopLockingPopup()
    {
        Destroy(_lockingImage.transform.parent.gameObject);
        _lockingImage = null;
        _isLockingPopup = false;
        _lockingPopup = null;
        _lockingTimer = 0f;
    }

    private void LockPopup(GameObject popUp)
    {
        UI_SurvivingPopup survivingPopup = popUp.GetComponent<UI_SurvivingPopup>();
        survivingPopup.enabled = false;
        _survivingPopups.Remove(survivingPopup);

        _popUps.Remove(popUp);
        Button lockedButton = Instantiate(_lockedObject, _popUpParent).GetComponent<Button>();
        lockedButton.transform.SetSiblingIndex(popUp.transform.GetSiblingIndex() + 1);
        _lockedPopUps.Add(popUp, lockedButton);
        Utilities.PlacePrefabAroundTargetTopRight(lockedButton.GetComponent<RectTransform>(), popUp.GetComponent<RectTransform>(), _lockImagePopupOffset);
        StopLockingPopup();

        ShowLockPopupTutoPopUp();
    }

    public void CloseLockedPopup(Button button)
    {
        GameObject popUpToRemove = null;
        foreach (var item in _lockedPopUps)
        {
            if (item.Value == button)
            {
                popUpToRemove = item.Key;
                if (popUpToRemove == JuiceManager.Instance.PopUpVisualizingCombo)
                {
                    JuiceManager.Instance.KillAllComboVFX();
                }
                popUpToRemove.GetComponent<Animator>().SetTrigger("Close");
                Destroy(item.Value.gameObject);
                ResetPopUp(null);
                break;
            }
        }
        if (popUpToRemove != null)
        {
            _lockedPopUps.Remove(popUpToRemove);
        }
    }

    public void CloseAllLockedPopup()
    {
        foreach (var item in _lockedPopUps)
        {
            if (item.Key == JuiceManager.Instance.PopUpVisualizingCombo)
            {
                JuiceManager.Instance.KillAllComboVFX();
            }
            item.Key.GetComponent<Animator>().SetTrigger("Close");
            Destroy(item.Value.gameObject);
        }
        _lockedPopUps.Clear();
        ResetPopUp(null);
    }
    #endregion
    #endregion

    #region TUTORIAL POPUP
    public void ShowFiltersTutoPopUp()
    {
        if (TutorialManager.Instance != null) // Prevent pop-up if tutorial is running
            return;

        if (PlayerPrefs.GetInt(FILTERS_TUTO_KEY, 0) == 1)
            return;

        GameManager.Instance.GamePaused = true;
        _filtersTutoPopup.SetTrigger("Show");

        PlayerPrefs.SetInt(FILTERS_TUTO_KEY, 1);
        PlayerPrefs.Save();
    }

    public void HideFiltersTutoPopUp()
    {
        GameManager.Instance.GamePaused = false;
        _filtersTutoPopup.SetTrigger("Shrink");
    }

    public void ShowInfraLevelTutoPopUp()
    {
        if (TutorialManager.Instance != null) // Prevent pop-up if tutorial is running
            return;

        if (PlayerPrefs.GetInt(INFRA_LVL_TUTO_KEY, 0) == 1)
            return;

        GameManager.Instance.GamePaused = true;
        _infraLevelTutoPopup.SetTrigger("Show");

        PlayerPrefs.SetInt(INFRA_LVL_TUTO_KEY, 1);
        PlayerPrefs.Save();
    }

    public void HideInfraLevelTutoPopUp()
    {
        GameManager.Instance.GamePaused = false;
        _infraLevelTutoPopup.SetTrigger("Shrink");
    }

    public void ShowRemoveInfraTutoPopUp()
    {
        if (TutorialManager.Instance != null) // Prevent pop-up if tutorial is running
            return;

        if (PlayerPrefs.GetInt(REMOVING_INFRA_TUTO_KEY, 0) == 1)
            return;

        GameManager.Instance.GamePaused = true;
        _removeInfraTutoPopup.SetTrigger("Show");

        PlayerPrefs.SetInt(REMOVING_INFRA_TUTO_KEY, 1);
        PlayerPrefs.Save();
    }

    public void HideRemoveInfraTutoPopUp()
    {
        GameManager.Instance.GamePaused = false;
        _removeInfraTutoPopup.SetTrigger("Shrink");
    }

    public void ShowLockPopupTutoPopUp()
    {
        if (TutorialManager.Instance != null) // Prevent pop-up if tutorial is running
            return;

        if (PlayerPrefs.GetInt(LOCK_POPUP_TUTO_KEY, 0) == 1)
            return;

        GameManager.Instance.GamePaused = true;
        _lockPopupTutoPopup.SetTrigger("Show");

        PlayerPrefs.SetInt(LOCK_POPUP_TUTO_KEY, 1);
        PlayerPrefs.Save();
    }

    public void HideLockPopupTutoPopUp()
    {
        GameManager.Instance.GamePaused = false;
        _lockPopupTutoPopup.SetTrigger("Shrink");
    }

    public void ShowUpgradeTutoPopUp()
    {
        if (TutorialManager.Instance != null) // Prevent pop-up if tutorial is running
            return;

        if (PlayerPrefs.GetInt(UPGRADE_TUTO_KEY, 0) == 1)
            return;

        GameManager.Instance.GamePaused = true;
        _upgradeTutoPopup.SetTrigger("Show");

        PlayerPrefs.SetInt(UPGRADE_TUTO_KEY, 1);
        PlayerPrefs.Save();
    }

    public void HideUpgradeTutoPopUp()
    {
        GameManager.Instance.GamePaused = false;
        _upgradeTutoPopup.SetTrigger("Shrink");
    }

    public void ShowSavingsTutoPopUp()
    {
        if (TutorialManager.Instance != null) // Prevent pop-up if tutorial is running
            return;

        if (PlayerPrefs.GetInt(SAVINGS_TUTO_KEY, 0) == 1)
            return;

        GameManager.Instance.GamePaused = true;
        _savingsTutoPopup.SetTrigger("Show");

        PlayerPrefs.SetInt(SAVINGS_TUTO_KEY, 1);
        PlayerPrefs.Save();
    }

    public void HideSavingsTutoPopUp()
    {
        GameManager.Instance.GamePaused = false;
        _savingsTutoPopup.SetTrigger("Shrink");
    }

    public void ShowTradeTutoPopUp()
    {
        if (TutorialManager.Instance != null) // Prevent pop-up if tutorial is running
            return;

        if (PlayerPrefs.GetInt(TRADE_TUTO_KEY, 0) == 1)
            return;

        GameManager.Instance.GamePaused = true;
        _tradeTutoPopup.SetTrigger("Show");

        PlayerPrefs.SetInt(TRADE_TUTO_KEY, 1);
        PlayerPrefs.Save();
    }

    public void HideTradeTutoPopUp()
    {
        GameManager.Instance.GamePaused = false;
        _tradeTutoPopup.SetTrigger("Shrink");
    }
    #endregion
}
