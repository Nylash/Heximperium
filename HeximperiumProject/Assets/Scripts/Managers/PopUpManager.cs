using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : Singleton<PopUpManager>
{
    #region CONSTANTS
    const float REF_WIDTH = 1920f;
    const string INFRA_LVL_TUTO_KEY = "InfraLvlTutoShown";
    const string REMOVING_INFRA_TUTO_KEY = "RemoveInfraTutoShown";
    const string LOCK_POPUP_TUTO_KEY = "LockPopupTutoShown";
    const string UPGRADE_TUTO_KEY = "UpgradeTutoShown";
    const string SAVINGS_TUTO_KEY = "SavingsTutoShown";
    #endregion

    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Spawning Configuration")]
    [SerializeField] private Transform _popUpParent;
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
    #endregion

    #region VARIABLES
    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _delayedHoverTimer;//For filling image purpose
    private List<GameObject> _popUps = new List<GameObject>();
    //private Dictionary<SpecialBehaviour, Tile> _highlightingBehaviours = new Dictionary<SpecialBehaviour, Tile>();
    //private Dictionary<SpecialEffect, Tile> _highlightingEffects = new Dictionary<SpecialEffect, Tile>();
    private float _maxAllowed;
    private float _minAllowed;
    private bool _popUpShown;
    private bool _showSourcesOnPopUp;
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
        if (obj == _objectUnderMouse)
        {
            if (obj.CompareTag("Untagged"))
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

                switch (obj.tag)
                {
                    case "ScoutLimitUI":
                        ScoutLimitPopUp();
                        break;
                    case "ClaimUI":
                        ClaimPopUp();
                        break;
                    case "TownLimitUI":
                        TownLimitPopUp();
                        break;
                    case "GoldUI":
                        GoldPopUp();
                        break;
                    case "SRUI":
                        SRPopUp();
                        break;
                    case "VisibilityUI":
                        VisibilityPopUp();
                        break;
                    case "UpgradeUI":
                        UpgradePopUp(obj.GetComponent<UpgradeHolder>().UpgradeEffect);
                        break;
                    case "ScoreUI":
                        ScorePopUp();
                        break;
                    case "CarnivalistUI":
                        CarnivalistPopUp();
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

    public void PopUpOnPopUp(RectTransform refObject, Family family = Family.None, InfrastructureData infra = null, EntertainmentData ent = null)
    {
        if (refObject.gameObject == _objectUnderMouse)
        {
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
                item.GetComponent<Animator>().SetTrigger("Close");
                if (item == JuiceManager.Instance.PopUpVisualizingCombo)
                {
                    JuiceManager.Instance.KillAllComboVFX();
                }
            }
            _popUps.Clear();
        }
        /*
        if (_highlightingBehaviours.Count > 0)
        {
            foreach (KeyValuePair<SpecialBehaviour, Tile> item in _highlightingBehaviours)
            {
                item.Key.HighlightImpactedTile(item.Value, false);
            }
            _highlightingBehaviours.Clear();
        }
        if (_highlightingEffects.Count > 0)
        {
            foreach (KeyValuePair<SpecialEffect, Tile> item in _highlightingEffects)
            {
                item.Key.HighlightImpactedEntertainment(item.Value, false);
            }
            _highlightingEffects.Clear();
        }
        */
        if (_lockingImage != null)
        {
            StopLockingPopup();
        }
    }
    #endregion

    #region UI POP UP
    private void ScoutLimitPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Scouts<sprite name=\"Scout_Emoji\"> limit";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Can be upgrades with specifics infrastructures";
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        #region AVAILABILITY
        TextMeshProUGUI availability = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        availability.text = $"{ExplorationManager.Instance.ScoutsLimit - ExplorationManager.Instance.CurrentScoutsCount}<sprite name=\"Scout_Emoji\"> available";
        ClampTextWidth(availability);
        textObjects.Add(availability.GetComponent<RectTransform>());
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
        textObjects.Add(baseSource.GetComponent<RectTransform>());
        if (scoutSources.Count > 0)
        {
            foreach (var kvp in scoutSources)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = $"{kvp.Key.ToCustomString(true)}: +{kvp.Value}<sprite name=\"Scout_Emoji\"> limit";
                ClampTextWidth(source);
                textObjects.Add(source.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void TownLimitPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Towns<sprite name=\"Town_Emoji\"> limit";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Can be upgrades with specifics infrastructures and upgrade";
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        #region AVAILABILITY
        TextMeshProUGUI availability = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        availability.text = $"{ExploitationManager.Instance.GetTownLimit()}<sprite name=\"Town_Emoji\"> available";
        ClampTextWidth(availability);
        textObjects.Add(availability.GetComponent<RectTransform>());
        #endregion

        #region SOURCE
        TextMeshProUGUI baseSource = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        baseSource.text = "Base value: +2<sprite name=\"Town_Emoji\"> limit";
        ClampTextWidth(baseSource);
        textObjects.Add(baseSource.GetComponent<RectTransform>());
        if (UpgradesManager.Instance.AppliedUpgrades.Any(b => b.GetType() == typeof(UpgradeTownLimit)))
        {
            TextMeshProUGUI upgradeSource = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            upgradeSource.text = "Imperial Mandate: +3<sprite name=\"Town_Emoji\"> limit";
            ClampTextWidth(upgradeSource);
            textObjects.Add(upgradeSource.GetComponent<RectTransform>());
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
                textObjects.Add(source.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void ClaimPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{ResourcesManager.Instance.Claim} Claims<sprite name=\"Claim_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region LOOSABLE CLAIMS
        if (ExpansionManager.Instance.UpgradeConserveClaims == false)
        {
            TextMeshProUGUI loosingClaims = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            loosingClaims.text = "Not used Claims<sprite name=\"Claim_Emoji\"> are lost at the end of the phase";
            ClampTextWidth(loosingClaims);
            loosingClaims.fontStyle = FontStyles.Italic;
            loosingClaims.alignment = TextAlignmentOptions.Center;
            textObjects.Add(loosingClaims.GetComponent<RectTransform>());
        }
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total <sprite name=\"Claim_Emoji\"> per turn +" + ExpansionManager.Instance.ClaimPerTurn + "<sprite name=\"Claim_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        #region SOURCES
        TextMeshProUGUI baseSource = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        baseSource.text = $"Base value: +{GameManager.Instance.BaseClaimPerTurn}<sprite name=\"Claim_Emoji\">";
        ClampTextWidth(baseSource);
        textObjects.Add(baseSource.GetComponent<RectTransform>());
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
        if (claimSources.Count > 0)
        {
            foreach (var kvp in claimSources)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = $"{kvp.Key.ToCustomString(true)}: +{kvp.Value}<sprite name=\"Claim_Emoji\">";
                ClampTextWidth(source);
                textObjects.Add(source.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void GoldPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{ResourcesManager.Instance.GetResourceStock(Resource.Gold)} Gold<sprite name=\"Gold_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total <sprite name=\"Gold_Emoji\"> per turn +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        #region INCOME NO INFRA
        int noInfraGoldIncome = ExploitationManager.Instance.GetResourceIncomeByNoInfraTiles(Resource.Gold);
        if (noInfraGoldIncome > 0)
        {
            TextMeshProUGUI incomeNoInfra = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            incomeNoInfra.text = "Basic tiles: +" + noInfraGoldIncome + "<sprite name=\"Gold_Emoji\">";
            ClampTextWidth(incomeNoInfra);
            textObjects.Add(incomeNoInfra.GetComponent<RectTransform>());
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
                textObjects.Add(source.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void SRPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{ResourcesManager.Instance.GetResourceStock(Resource.SpecialResources)} Stone<sprite name=\"SR_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total <sprite name=\"SR_Emoji\"> per turn +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.SpecialResources) + "<sprite name=\"SR_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
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
                textObjects.Add(source.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void ScorePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{EntertainmentManager.Instance.Score} Points<sprite name=\"Point_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region MINSTREL STAGE
        TextMeshProUGUI minstrel = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        minstrel.text = "<sprite name=\"Point_Emoji\"> from <u>Minstrel Stage</u>: +" + EntertainmentManager.Instance.GetPointsFromMinstrelStage() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(minstrel);
        textObjects.Add(minstrel.GetComponent<RectTransform>());
        #endregion

        #region TASTING PAVILION
        TextMeshProUGUI tasting = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        tasting.text = "<sprite name=\"Point_Emoji\"> from <u>Tasting Pavilion</u>: +" + EntertainmentManager.Instance.GetPointsFromTastingPavilion() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(tasting);
        textObjects.Add(tasting.GetComponent<RectTransform>());
        #endregion

        #region PARADE ROUTE
        TextMeshProUGUI parade = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        parade.text = "<sprite name=\"Point_Emoji\"> from <u>Parade Route</u>: +" + EntertainmentManager.Instance.GetPointsFromParadeRoute() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(parade);
        textObjects.Add(parade.GetComponent<RectTransform>());
        #endregion

        #region MYSTIC GARDEN
        TextMeshProUGUI garden = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        garden.text = "<sprite name=\"Point_Emoji\"> from <u>Mystic Garden</u>: +" + EntertainmentManager.Instance.GetPointsFromMysticGarden() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(garden);
        textObjects.Add(garden.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void CarnivalistPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{ResourcesManager.Instance.Carnivalist} Carnivalists<sprite name=\"Carnivalist_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Carnivalists<sprite name=\"Carnivalist_Emoji\"> are used during the Grand Jubilee to place " + Family.Entertainment.ToCustomString(true);
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        textObjects.Add(detail.GetComponent<RectTransform>());
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
                    textObjects.Add(source.GetComponent<RectTransform>());
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
                textObjects.Add(source.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void VisibilityPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

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
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowIncomePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreIncomesShown)
            detail.text = "Hide tiles' incomes and bonuses";
        else
            detail.text = "Show tiles' incomes and bonuses";
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowEntPlacementPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreEntPlacementShown)
            detail.text = "Hide which tiles can receive an " + Family.Entertainment.ToCustomString();
        else
            detail.text = "Show which tiles can receive an " + Family.Entertainment.ToCustomString();
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowDetailsPopupPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (_showSourcesOnPopUp)
            detail.text = "Hide advanced incomes/points sources details on pop-ups";
        else
            detail.text = "Show advanced incomes/points sources details on pop-ups";
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void UpgradePopUp(UpgradeEffect effect)
    {
        if (effect == null)
            return;

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = effect.EffectName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = effect.GetEffectDescription();
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }
    #endregion

    #region ON TILE POP UP
    private void TilePopUp(Tile tile)
    {
        bool isVisualizingCombo = false;
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
        {
            if (!tile.Entertainment && tile.EntImpactedByTile.Count > 0) // If tile an entertainment, its popup handle the combo visualization
                isVisualizingCombo = JuiceManager.Instance.VisualizeEntertainmentComboFromTileOnly(tile);
        }
        else
        {
            isVisualizingCombo = JuiceManager.Instance.VisualizeExploitationCombo(
                tile.InternalIncomesSources, tile.ExternalIncomesSources, tile.InternalCarnivalistsSources,
                tile.ImpactedTilesIncomes, tile.ImpactedTilesCarnivalists, tile);
        }

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = tile.TileData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region HAZARDOUS TILE
        if (tile.TileData is HazardousTileData)
        {
            TextMeshProUGUI slow = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            slow.text = "Slow down scouts, cannot be claimed";
            textObjects.Add(slow.GetComponent<RectTransform>());
            ClampTextWidth(slow);
            slow.fontStyle = FontStyles.Italic;
            slow.alignment = TextAlignmentOptions.Center;

            SetPopUpContentAnchors(textObjects);
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
            textObjects.Add(income.GetComponent<RectTransform>());
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
                textObjects.Add(sourceInc.GetComponent<RectTransform>());
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
            incomeGiven.text = $"({totalGiven.IncomeToString()} given over " +
                $"{tile.ImpactedTilesIncomes.Count} other {(tile.ImpactedTilesIncomes.Count > 1 ? "tiles" : "tile")}" +
                ", through effects or sheer presence)";
            incomeGiven.alignment = TextAlignmentOptions.Center;
            textObjects.Add(incomeGiven.GetComponent<RectTransform>());
        }
        #endregion

        #region CARNIVALISTS
        if (tile.RecruitedCarnivalists > 0)
        {
            TextMeshProUGUI carnivalists = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            carnivalists.text = tile.RecruitedCarnivalists + "<sprite name=\"Carnivalist_Emoji\">";
            carnivalists.fontStyle = FontStyles.Bold;
            carnivalists.alignment = TextAlignmentOptions.Center;
            textObjects.Add(carnivalists.GetComponent<RectTransform>());
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
            carnivalistsGiven.text = $"({totalCarnivalists}<sprite name=\"Carnivalist_Emoji\"> given over " +
                $"{tile.ImpactedTilesCarnivalists.Count} other {(tile.ImpactedTilesCarnivalists.Count > 1 ? "tiles" : "tile")}" +
                ", through effects or sheer presence)";
            carnivalistsGiven.alignment = TextAlignmentOptions.Center;
            textObjects.Add(carnivalistsGiven.GetComponent<RectTransform>());
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
            boostedEnt.text += "(+" + totalPoints + "<sprite name=\"Point_Emoji\"> given over "
                + tile.EntImpactedByTile.Count + " " + Family.Entertainment.ToCustomString(tile.EntImpactedByTile.Count > 1)
                + ", through effects or sheer presence)";
            boostedEnt.alignment = TextAlignmentOptions.Center;
            textObjects.Add(boostedEnt.GetComponent<RectTransform>());
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
            textObjects.Add(separationEffects.GetComponent<RectTransform>());
        }
        #endregion

        #region INCOME BONUS
        if (incomeBonus.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            income.text = "<sprite name=\"Puce_Emoji\"> Base income " + incomeBonus.IncomeToString() + " per turn";
            textObjects.Add(income.GetComponent<RectTransform>());
        }
        #endregion

        #region BEHAVIOURS
        if (tile.TileData.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in tile.TileData.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = "<sprite name=\"Puce_Emoji\"> " + behaviour.GetBehaviourDescription();
                textObjects.Add(behaviourText.GetComponent<RectTransform>());
                ClampTextWidth(behaviourText);
                /*
                behaviour.HighlightImpactedTile(tile, true);
                _highlightingBehaviours.Add(behaviour, tile);
                */
            }
        }
        #endregion

        #region SCOUT STARTING POINT
        if (tile.TileData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "<sprite name=\"Puce_Emoji\"> Scout<sprite name=\"Scout_Emoji\"> starting point";
            textObjects.Add(scoutText.GetComponent<RectTransform>());
            ClampTextWidth(scoutText);
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
            textObjects.Add(separationDetails.GetComponent<RectTransform>());
        }
        #endregion

        #region ENHANCEMENTS
        if (tile.TileData.AvailableInfrastructures.Count > 0 && GameManager.Instance.CurrentPhase != Phase.Entertain)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + tile.TileData.AvailableInfrastructures.ToCustomString(false, true);
            textObjects.Add(enhancement.GetComponent<RectTransform>());
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
            textObjects.Add(claimStatus.GetComponent<RectTransform>());
        }
        #endregion

        #region FAMILY
        if (tile.TileData is InfrastructureData infra)
        {
            TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            family.text = infra.Family.ToCustomString();
            family.alignment = TextAlignmentOptions.Right;
            textObjects.Add(family.GetComponent<RectTransform>());
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), tile.transform, !isVisualizingCombo);
        if (tile.Entertainment == null) // If tile has an entertainment, its popup will be the one lockable
            StartLockingPopup(popUp);
    }

    private void ScoutPopUp(Scout scout)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Scout";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region SPEED
        TextMeshProUGUI speedText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        speedText.text = "Speed: " + scout.Speed;
        textObjects.Add(speedText.GetComponent<RectTransform>());
        #endregion

        #region REVEAL RADIUS
        TextMeshProUGUI revealText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        revealText.text = "Reveal radius: " + scout.RevealRadius;
        textObjects.Add(revealText.GetComponent<RectTransform>());
        #endregion

        #region LIFESPAN
        TextMeshProUGUI lifespanText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        lifespanText.text = "Remaining turns: " + scout.Lifespan;
        textObjects.Add(lifespanText.GetComponent<RectTransform>());
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
                textObjects.Add(redirectText.GetComponent<RectTransform>());
            }
        }
        #endregion

        #region DIRECTION
        TextMeshProUGUI directionText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        directionText.text = scout.Direction.ToCustomString();
        directionText.fontStyle = FontStyles.Italic;
        directionText.alignment = TextAlignmentOptions.Right;
        textObjects.Add(directionText.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), scout.CurrentTile.transform, true);
    }

    private void MultipleScoutsPopUp(Tile tile)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = $"{tile.Scouts.Count} Scouts";
        textObjects.Add(title.GetComponent<RectTransform>());
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
            textObjects.Add(directionText.GetComponent<RectTransform>());
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
                textObjects.Add(redirectText.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), tile.transform, true);
    }

    private void EntertainmentPopUp(Entertainment ent)
    {
        bool isVisualizingCombo = JuiceManager.Instance.VisualizeEntertainmentCombo(
            ent.InternalPointsSources, ent.ExternalPointsSources, 
            ent.Tile.EntImpactedByEntertainment, ent.Tile.EntImpactedByTile,
            ent.Tile);

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = ent.Data.Type.ToCustomString(false);
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region POINTS
        TextMeshProUGUI pointsText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        pointsText.text = "+" + ent.Points + "<sprite name=\"Point_Emoji\">";
        pointsText.fontStyle = FontStyles.Bold;
        pointsText.alignment = TextAlignmentOptions.Center;
        textObjects.Add(pointsText.GetComponent<RectTransform>());
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
            textObjects.Add(sourceInc.GetComponent<RectTransform>());
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
            pointsGiven.text = "(+" + pointsGivenValue + "<sprite name=\"Point_Emoji\"> given over "
                + ent.Tile.EntImpactedByEntertainment.Count + " other " + Family.Entertainment.ToCustomString(ent.Tile.EntImpactedByEntertainment.Count > 1) 
                + ", through effects or sheer presence)";
            pointsGiven.alignment = TextAlignmentOptions.Center;
            textObjects.Add(pointsGiven.GetComponent<RectTransform>());
        }
        #endregion

        #region EFFECTS SEPARATION
        TextMeshProUGUI separationEffects = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationEffects.text = "--- Effects ---";
        separationEffects.fontStyle = FontStyles.Bold;
        separationEffects.alignment = TextAlignmentOptions.Center;
        textObjects.Add(separationEffects.GetComponent<RectTransform>());
        #endregion

        #region BASE POINTS
        TextMeshProUGUI basePointsText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        basePointsText.text = "<sprite name=\"Puce_Emoji\"> Base points +" + ent.Data.BasePoints + "<sprite name=\"Point_Emoji\">";
        textObjects.Add(basePointsText.GetComponent<RectTransform>());
        ClampTextWidth(basePointsText);
        #endregion

        #region EFFECT
        if (ent.Data.SpecialEffects.Count > 0)
        {
            foreach (SpecialEffect effect in ent.Data.SpecialEffects)
            {
                TextMeshProUGUI effectText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                effectText.text = "<sprite name=\"Puce_Emoji\"> " + effect.GetBehaviourDescription();
                textObjects.Add(effectText.GetComponent<RectTransform>());
                ClampTextWidth(effectText);
                /*
                effect.HighlightImpactedEntertainment(ent.Tile, true);
                _highlightingEffects.Add(effect, ent.Tile);
                */
            }
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = Family.Entertainment.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        textObjects.Add(family.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), ent.Tile.transform, !isVisualizingCombo);
        StartLockingPopup(popUp);
    }
    #endregion

    #region INTERACTION BUTTON POP UP
    private void ButtonScoutPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        BoostScoutOnSpawn boostingInfra = button.AssociatedTile.TileData.SpecialBehaviours
            .OfType<BoostScoutOnSpawn>()
            .FirstOrDefault();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Spawn a Scout";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region SPEED
        TextMeshProUGUI speedText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            speedText.text = "Speed: " + (button.ScoutData.Speed + boostingInfra.BoostSpeed + ExplorationManager.Instance.BoostScoutSpeed);
        else
            speedText.text = "Speed: " + (button.ScoutData.Speed + ExplorationManager.Instance.BoostScoutSpeed);
        textObjects.Add(speedText.GetComponent<RectTransform>());
        #endregion

        #region REVEAL RADIUS
        TextMeshProUGUI revealText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            revealText.text = "Reveal radius: " + (button.ScoutData.RevealRadius + boostingInfra.BoostRevealRadius + ExplorationManager.Instance.BoostScoutRevealRadius);
        else
            revealText.text = "Reveal radius: " + (button.ScoutData.RevealRadius + ExplorationManager.Instance.BoostScoutRevealRadius);
        textObjects.Add(revealText.GetComponent<RectTransform>());
        ClampTextWidth(revealText);
        #endregion

        #region LIFESPAN
        TextMeshProUGUI lifespanText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            lifespanText.text = "Lifespan: " + (button.ScoutData.Lifespan + boostingInfra.BoostLifespan + ExplorationManager.Instance.BoostScoutLifespan);
        else
            lifespanText.text = "Lifespan: " + (button.ScoutData.Lifespan + ExplorationManager.Instance.BoostScoutLifespan);
        textObjects.Add(lifespanText.GetComponent<RectTransform>());
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
        textObjects.Add(availability.GetComponent<RectTransform>());
        availability.fontStyle = FontStyles.Italic;
        availability.alignment = TextAlignmentOptions.Center;
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
        StartLockingPopup(popUp);
    }

    private void ButtonRedirectScoutPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        TextMeshProUGUI text = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        text.text = "Redirect a Scout";
        textObjects.Add(text.GetComponent<RectTransform>());

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
    }

    private void ButtonRevealAnywherePopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        TextMeshProUGUI text = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        text.text = $"Reveal this tile and all those in a {ExplorationManager.Instance.UpgradeRevealAnywhere.RevealRadius}-tile radius";
        textObjects.Add(text.GetComponent<RectTransform>());
        ClampTextWidth(text);

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
    }

    private void ButtonClaimPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Claim " + button.AssociatedTile.TileData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region CLAIM COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.AssociatedTile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">" + "(" + ResourcesManager.Instance.Claim + ")";
        if (!ResourcesManager.Instance.CanAffordClaim(button.AssociatedTile.TileData.ClaimCost))
            cost.color = UIManager.Instance.ColorCantAfford;
        textObjects.Add(cost.GetComponent<RectTransform>());
        ClampTextWidth(cost);
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
        StartLockingPopup(popUp);
    }

    private void ButtonDestroyPopUp(string text, InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = text;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
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

        bool isVisualizionCombo = JuiceManager.Instance.VisualizeEntertainmentCombo(
            predictedIntSources, predictedExtSources,
            predictedEntImpactedByEnt, predictedEntImpactedByTile,
            button.AssociatedTile);

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Place " + button.EntertainData.Type.ToCustomString(false);
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region PREDICTED POINTS
        TextMeshProUGUI predictedIncome = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        predictedIncome.text = "Predicted points: +" + predictedPoints + "<sprite name=\"Point_Emoji\">";
        predictedIncome.fontStyle = FontStyles.Bold;
        predictedIncome.alignment = TextAlignmentOptions.Center;
        textObjects.Add(predictedIncome.GetComponent<RectTransform>());
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
            textObjects.Add(sourceInc.GetComponent<RectTransform>());
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
            pointsGiven.text = "(Would give +" + totalPointsGiven + "<sprite name=\"Point_Emoji\"> over " 
                + predictedEntImpactedByEnt.Count + " other " + Family.Entertainment.ToCustomString(predictedEntImpactedByEnt.Count > 1) 
                + ", through effects or sheer presence)";
            pointsGiven.alignment = TextAlignmentOptions.Center;
            textObjects.Add(pointsGiven.GetComponent<RectTransform>());
        }
        #endregion

        #region EFFECTS SEPARATION
        TextMeshProUGUI separationEffects = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationEffects.text = "--- Effects ---";
        separationEffects.fontStyle = FontStyles.Bold;
        separationEffects.alignment = TextAlignmentOptions.Center;
        textObjects.Add(separationEffects.GetComponent<RectTransform>());
        #endregion

        #region POINTS
        TextMeshProUGUI pointsText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        pointsText.text = "<sprite name=\"Puce_Emoji\"> Base points +" + button.EntertainData.BasePoints + "<sprite name=\"Point_Emoji\">";
        textObjects.Add(pointsText.GetComponent<RectTransform>());
        #endregion

        #region EFFECT
        if (button.EntertainData.SpecialEffects.Count > 0)
        {
            foreach (SpecialEffect effect in button.EntertainData.SpecialEffects)
            {
                TextMeshProUGUI effectText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                effectText.text = "<sprite name=\"Puce_Emoji\"> " + effect.GetBehaviourDescription();
                textObjects.Add(effectText.GetComponent<RectTransform>());
                ClampTextWidth(effectText);
                /*
                effect.HighlightImpactedEntertainment(button.AssociatedTile, true);
                _highlightingEffects.Add(effect, button.AssociatedTile);
                */
            }
        }
        #endregion

        #region DETAILS SEPARATION
        TextMeshProUGUI separationDetails = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationDetails.text = "--- Details ---";
        separationDetails.fontStyle = FontStyles.Bold;
        separationDetails.alignment = TextAlignmentOptions.Center;
        textObjects.Add(separationDetails.GetComponent<RectTransform>());
        #endregion

        #region COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.EntertainData.GetActualCarnivalistCost(button.AssociatedTile) + "<sprite name=\"Carnivalist_Emoji\">";
        if (!ResourcesManager.Instance.CanAffordCarnivalist(button.EntertainData.GetActualCarnivalistCost(button.AssociatedTile)))
            cost.color = UIManager.Instance.ColorCantAfford;
        textObjects.Add(cost.GetComponent<RectTransform>());
        ClampTextWidth(cost);
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = Family.Entertainment.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        textObjects.Add(family.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, !isVisualizionCombo);
        StartLockingPopup(popUp);
    }

    private void ButtonInfraPopUp(InteractionButton button)
    {
        // Get predicted income
        List<ResourceToIntMap> predictedInc;
        List<ResourceToIntMap> predictedSelfInc;
        int predictectedCarnivalists;
        Dictionary<Tile, List<ResourceToIntMap>> predictedExtSources;
        Dictionary<Tile, List<ResourceToIntMap>> predictedIntSources;
        Dictionary<Tile, int> predictedInternalCarnivalistsSources;
        Dictionary<Tile, List<ResourceToIntMap>> predictedImpactedTilesIncomes;
        Dictionary<Tile, int> predictedImpactedTilesCarnivalists;
        ExploitationManager.Instance.GetPredictedIncomes(button.AssociatedTile, button.InfrastructureData,
            out predictedInc, out predictedExtSources, out predictedIntSources,
            out predictedSelfInc, out predictectedCarnivalists, out predictedInternalCarnivalistsSources,
            out predictedImpactedTilesIncomes, out predictedImpactedTilesCarnivalists);

        bool isVisualizingCombo = JuiceManager.Instance.VisualizeExploitationCombo(
            predictedIntSources, predictedExtSources, predictedInternalCarnivalistsSources,
            predictedImpactedTilesIncomes, predictedImpactedTilesCarnivalists,
            button.AssociatedTile);

        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Build " + button.InfrastructureData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        if (!Utilities.AreIncomesEqual(button.AssociatedTile.Incomes, predictedInc))
        {
            #region CURRENT INCOME
            if (button.AssociatedTile.Incomes.Count > 0)
            {
                TextMeshProUGUI currentIncome = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                currentIncome.text = "Current income: " + button.AssociatedTile.Incomes.IncomeToString() + " per turn";
                currentIncome.fontStyle = FontStyles.Bold;
                currentIncome.alignment = TextAlignmentOptions.Center;
                textObjects.Add(currentIncome.GetComponent<RectTransform>());
            }
            #endregion

            #region PREDICTED INCOME
            if (predictedInc.Count > 0)
            {
                TextMeshProUGUI predictedIncome = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                predictedIncome.text = "Predicted income: " + predictedInc.IncomeToString() + " per turn";
                predictedIncome.fontStyle = FontStyles.Bold;
                predictedIncome.alignment = TextAlignmentOptions.Center;
                textObjects.Add(predictedIncome.GetComponent<RectTransform>());
            }
            #endregion
        }
        else
        {
            #region INCOME
            if (button.AssociatedTile.Incomes.Count > 0)
            {
                TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                income.text = button.AssociatedTile.Incomes.IncomeToString() + " per turn";
                income.fontStyle = FontStyles.Bold;
                income.alignment = TextAlignmentOptions.Center;
                textObjects.Add(income.GetComponent<RectTransform>());
            }
            #endregion
        }

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
                textObjects.Add(sourceInc.GetComponent<RectTransform>());
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
            incomeGiven.text = $"(Would give {totalGiven.IncomeToString()} over " +
                $"{predictedImpactedTilesIncomes.Count} other {(predictedImpactedTilesIncomes.Count > 1 ? "tiles" : "tile")}" +
                ", through effects or sheer presence)";
            incomeGiven.alignment = TextAlignmentOptions.Center;
            textObjects.Add(incomeGiven.GetComponent<RectTransform>());
        }
        #endregion

        #region CURRENT CARNIVALISTS
        if (button.AssociatedTile.RecruitedCarnivalists > 0)
        {
            TextMeshProUGUI currentCarni = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            currentCarni.text = "Current recruited "+ (button.AssociatedTile.RecruitedCarnivalists > 1 ? "carnivalists" : "carnivalist") 
                + ": " + button.AssociatedTile.RecruitedCarnivalists + "<sprite name=\"Carnivalist_Emoji\">";
            currentCarni.fontStyle = FontStyles.Bold;
            currentCarni.alignment = TextAlignmentOptions.Center;
            textObjects.Add(currentCarni.GetComponent<RectTransform>());
        }
        #endregion

        #region PREDICTED CARNIVALISTS
        if (predictectedCarnivalists > 0)
        {
            TextMeshProUGUI predictedCarni = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            predictedCarni.text = "Predicted recruited " + (predictectedCarnivalists > 1 ? "carnivalists" : "carnivalist")
                + ": " + predictectedCarnivalists + "<sprite name=\"Carnivalist_Emoji\">";
            predictedCarni.fontStyle = FontStyles.Bold;
            predictedCarni.alignment = TextAlignmentOptions.Center;
            textObjects.Add(predictedCarni.GetComponent<RectTransform>());
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
            carnivalistsGiven.text = $"(Would give {totalCarnivalists}<sprite name=\"Carnivalist_Emoji\"> over " +
                $"{predictedImpactedTilesCarnivalists.Count} other {(predictedImpactedTilesCarnivalists.Count > 1 ? "tiles" : "tile")}" +
                ", through effects or sheer presence)";
            carnivalistsGiven.alignment = TextAlignmentOptions.Center;
            textObjects.Add(carnivalistsGiven.GetComponent<RectTransform>());
        }
        #endregion

        #region EFFECTS SEPARATION
        TextMeshProUGUI separationEffects = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationEffects.text = "--- Effects ---";
        separationEffects.fontStyle = FontStyles.Bold;
        separationEffects.alignment = TextAlignmentOptions.Center;
        textObjects.Add(separationEffects.GetComponent<RectTransform>());
        #endregion

        #region AUTO CLAIM TOWN UPGRADE
        if (GameManager.Instance.CurrentPhase == Phase.Expand && ExpansionManager.Instance.UpgradeTownAutoClaim)
        {
            TextMeshProUGUI autoClaimText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            autoClaimText.text = "<sprite name=\"Puce_Emoji\"> Automatically claims<sprite name=\"Claim_Emoji\"> the 6 surrounding tiles";
            textObjects.Add(autoClaimText.GetComponent<RectTransform>());
            ClampTextWidth(autoClaimText);
        }
        #endregion

        #region INCOME BONUS
        if (button.InfrastructureData.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            income.text = "<sprite name=\"Puce_Emoji\"> Improve base income by " + button.InfrastructureData.Incomes.IncomeToString() + " per turn";
            textObjects.Add(income.GetComponent<RectTransform>());
            ClampTextWidth(income);
        }
        #endregion

        #region BEHAVIOURS
        if (button.InfrastructureData.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in button.InfrastructureData.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = "<sprite name=\"Puce_Emoji\"> " + behaviour.GetBehaviourDescription();
                textObjects.Add(behaviourText.GetComponent<RectTransform>());
                ClampTextWidth(behaviourText);
                /*
                behaviour.HighlightImpactedTile(button.AssociatedTile, true);
                _highlightingBehaviours.Add(behaviour, button.AssociatedTile);
                */
            }
        }
        #endregion

        #region SCOUT STARTING POINT
        if (button.InfrastructureData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "<sprite name=\"Puce_Emoji\"> Scout<sprite name=\"Scout_Emoji\"> starting point";
            textObjects.Add(scoutText.GetComponent<RectTransform>());
            ClampTextWidth(scoutText);
        }
        #endregion

        #region DETAILS SEPARATION
        TextMeshProUGUI separationDetails = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        separationDetails.text = "--- Details ---";
        separationDetails.fontStyle = FontStyles.Bold;
        separationDetails.alignment = TextAlignmentOptions.Center;
        textObjects.Add(separationDetails.GetComponent<RectTransform>());
        #endregion

        #region ENHANCEMENTS
        if (button.InfrastructureData.AvailableInfrastructures.Count > 0)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + button.InfrastructureData.AvailableInfrastructures.ToCustomString(false, true);

            textObjects.Add(enhancement.GetComponent<RectTransform>());
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
        textObjects.Add(cost.GetComponent<RectTransform>());
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
            textObjects.Add(availability.GetComponent<RectTransform>());
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = button.InfrastructureData.Family.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        textObjects.Add(family.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, !isVisualizingCombo);
        StartLockingPopup(popUp);
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

    //Distribute the Y space of the popup content evenly
    private void SetPopUpContentAnchors(List<RectTransform> textObjects)
    {
        int count = textObjects.Count;
        for (int i = 0; i < count; i++)
        {
            int reversedIndex = count - 1 - i;
            textObjects[reversedIndex].anchorMin = new Vector2(0, i / (float)count);
            textObjects[reversedIndex].anchorMax = new Vector2(1, (i + 1f) / count);
        }
    }
    #endregion

    #region POPUP ON POPUP
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

    public void FamilyPopup(Family family, RectTransform refObject)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = family.ToCustomString();
        textObjects.Add(title.GetComponent<RectTransform>());
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
        textObjects.Add(familyList.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), refObject, true);
        StartLockingPopup(popUp);
    }

    public void InfrastructurePopup(InfrastructureData infra, RectTransform refObject)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = infra.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME BONUS
        if (infra.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            income.text = "<sprite name=\"Puce_Emoji\"> Improve income by " + infra.Incomes.IncomeToString() + " per turn";
            textObjects.Add(income.GetComponent<RectTransform>());
        }
        #endregion

        #region BEHAVIOURS
        if (infra.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in infra.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = "<sprite name=\"Puce_Emoji\"> " + behaviour.GetBehaviourDescription();
                textObjects.Add(behaviourText.GetComponent<RectTransform>());
                ClampTextWidth(behaviourText);
            }
        }
        #endregion

        #region SCOUT STARTING POINT
        if (infra.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "<sprite name=\"Puce_Emoji\"> Scout<sprite name=\"Scout_Emoji\"> starting point";
            textObjects.Add(scoutText.GetComponent<RectTransform>());
            ClampTextWidth(scoutText);
        }
        #endregion

        #region ENHANCEMENTS
        if (infra.AvailableInfrastructures.Count > 0)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + infra.AvailableInfrastructures.ToCustomString(false, true);

            textObjects.Add(enhancement.GetComponent<RectTransform>());
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
        textObjects.Add(cost.GetComponent<RectTransform>());
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
            textObjects.Add(availability.GetComponent<RectTransform>());
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = infra.Family.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        textObjects.Add(family.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), refObject, true);
        StartLockingPopup(popUp);
    }

    public void EntertainmentPopup(EntertainmentData ent, RectTransform refObject)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, _popUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = ent.Type.ToCustomString(false);
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region BASE POINTS
        TextMeshProUGUI basePointsText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        basePointsText.text = "<sprite name=\"Puce_Emoji\"> Base points +" + ent.BasePoints + "<sprite name=\"Point_Emoji\">";
        textObjects.Add(basePointsText.GetComponent<RectTransform>());
        ClampTextWidth(basePointsText);
        #endregion

        #region EFFECT
        if (ent.SpecialEffects.Count > 0)
        {
            foreach (SpecialEffect effect in ent.SpecialEffects)
            {
                TextMeshProUGUI effectText = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                effectText.text = "<sprite name=\"Puce_Emoji\"> " + effect.GetBehaviourDescription();
                textObjects.Add(effectText.GetComponent<RectTransform>());
                ClampTextWidth(effectText);
            }
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        family.text = Family.Entertainment.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        textObjects.Add(family.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), refObject, true);
        StartLockingPopup(popUp);
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
        _popUps.Remove(popUp);
        Button lockedButton = Instantiate(_lockedObject, _popUpParent).GetComponent<Button>();
        lockedButton.transform.SetAsLastSibling();
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

    #region TUTORIAL POPUP
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
    #endregion
}
