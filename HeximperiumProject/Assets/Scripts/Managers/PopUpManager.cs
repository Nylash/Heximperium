using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : Singleton<PopUpManager>
{
    const float REF_WIDTH = 1920f;

    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Spawning Configuration")]
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
    #endregion

    #region VARIABLES
    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _delayedHoverTimer;//For filling image purpose
    private List<GameObject> _popUps = new List<GameObject>();
    private Dictionary<SpecialBehaviour, Tile> _highlightingBehaviours = new Dictionary<SpecialBehaviour, Tile>();
    private Dictionary<SpecialEffect, Tile> _highlightingEffects = new Dictionary<SpecialEffect, Tile>();
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
                        LimitPopUp("Scouts<sprite name=\"Scout_Emoji\">");
                        break;
                    case "ClaimUI":
                        ClaimPopUp();
                        break;
                    case "TownLimitUI":
                        LimitPopUp("Towns<sprite name=\"Town_Emoji\">");
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
                    if (tile.Scouts.Count > 0)
                    {
                        foreach (Scout item in tile.Scouts)
                            ScoutPopUp(item);
                    }
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
        if (_lockingImage != null)
        {
            StopLockingPopup();
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
    #endregion

    #region UI POP UP
    private void LimitPopUp(string text)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = text + " limit";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Can be upgrades with specifics enhancements and upgrades";
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void VisibilityPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
            title.text = Family.Entertainment.ToCustomString(true) + " visibility";
        else
            title.text = "Scouts visibility";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
            detail.text = "Hide or show " + Family.Entertainment.ToCustomString(true);
        else
            detail.text = "Hide or show Scouts";
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ClaimPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Claims<sprite name=\"Claim_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "Claim per turn: +" + ExpansionManager.Instance.ClaimPerTurn + "<sprite name=\"Claim_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void GoldPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Gold<sprite name=\"Gold_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total <sprite name=\"Gold_Emoji\"> per turn: +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        #region INCOME NO INFRA
        TextMeshProUGUI incomeNoInfra = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        incomeNoInfra.text = "<sprite name=\"Gold_Emoji\"> from non enhanced tiles: +" + ExploitationManager.Instance.GetResourceIncomeByNoInfraTiles(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        ClampTextWidth(incomeNoInfra);
        textObjects.Add(incomeNoInfra.GetComponent<RectTransform>());
        #endregion

        #region INCOME INFRA
        TextMeshProUGUI incomeInfra = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        incomeInfra.text = "<sprite name=\"Gold_Emoji\"> from enhanced tiles: +" + ExploitationManager.Instance.GetResourceIncomeByInfra(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        ClampTextWidth(incomeInfra);
        textObjects.Add(incomeInfra.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void SRPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Stone<sprite name=\"SR_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        income.text = "<sprite name=\"SR_Emoji\"> per turn: +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.SpecialResources) + "<sprite name=\"SR_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void ScorePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Points<sprite name=\"Point_Emoji\">";
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Carnivalists<sprite name=\"Carnivalist_Emoji\">";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        detail.text = "<sprite name=\"Carnivalist_Emoji\"> are used during the Grand Jubilee to place " + Family.Entertainment.ToCustomString(true);
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        #region SOURCE
        if (ResourcesManager.Instance.CarnivalistSources.Count > 0)
        {
            foreach (KeyValuePair<TileData, int> pair in ResourcesManager.Instance.CarnivalistSources)
            {
                TextMeshProUGUI source = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                source.text = pair.Key.TileName + ": " + pair.Value + "<sprite name=\"Carnivalist_Emoji\">";
                ClampTextWidth(source);
                textObjects.Add(source.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
        StartLockingPopup(popUp);
    }

    private void ShowIncomePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreIncomesShown)
            detail.text = "Hide tiles' incomes and bonuses";
        else
            detail.text = "Show tiles' incomes and bonuses";
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowEntPlacementPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreEntPlacementShown)
            detail.text = "Hide which tiles can receive an " + Family.Entertainment.ToCustomString();
        else
            detail.text = "Show which tiles can receive an " + Family.Entertainment.ToCustomString();
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>(), true);
    }

    private void ShowDetailsPopupPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        if (_showSourcesOnPopUp)
            detail.text = "Hide advanced incomes/points sources details on pop-ups";
        else
            detail.text = "Show advanced incomes/points sources details on pop-ups";
        ClampTextWidth(detail);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
            // COMBO HERE
        }

        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = tile.TileData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region HAZARDOUS TILE
        if (tile.TileData is HazardousTileData && !tile.Claimed)
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
                    foreach (var kvp in tile.ExternalIncomesSources)
                    {
                        sourceInc.text += "(" + kvp.Value.IncomeToString() + " from " + kvp.Key.TileName + ")" + "\n";
                    }
                }
                sourceInc.alignment = TextAlignmentOptions.Center;
                sourceInc.fontStyle = FontStyles.Italic;
                textObjects.Add(sourceInc.GetComponent<RectTransform>());
            }
        }
        #endregion

        #region INCOME BONUS
        List<ResourceToIntMap> incomeBonus = new List<ResourceToIntMap>(tile.TileData.Incomes);
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
                behaviour.HighlightImpactedTile(tile, true);
                _highlightingBehaviours.Add(behaviour, tile);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
        #endregion

        #region REDIRECTABLE
        if (ExplorationManager.Instance.UpgradeScoutRedirectable)
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

    private void EntertainmentPopUp(Entertainment ent)
    {
        bool isVisualizingCombo = JuiceManager.Instance.VisualizeEntertainmentCombo(
            ent.InternalPointsSources, ent.ExternalPointsSources, 
            ent.Tile.EntImpactedByEntertainment, ent.Tile.EntImpactedByTile,
            ent.Tile);

        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
                effect.HighlightImpactedEntertainment(ent.Tile, true);
                _highlightingEffects.Add(effect, ent.Tile);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = text;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
                effect.HighlightImpactedEntertainment(button.AssociatedTile, true);
                _highlightingEffects.Add(effect, button.AssociatedTile);
            }
        }
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
        Dictionary<TileData, List<ResourceToIntMap>> predictedSources;
        ExploitationManager.Instance.GetPredictedIncomes(button.AssociatedTile, button.InfrastructureData,
            out predictedInc, out predictedSources, out predictedSelfInc);

        bool isVisualizingCombo = false; // Call here JuiceManager when implemented

        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
        title.text = "Build " + button.InfrastructureData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

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

        #region INCOME SOURCES
        if (_showSourcesOnPopUp)
        {
            if (predictedInc.Count > 0)
            {
                TextMeshProUGUI sourceInc = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
                if (predictedSelfInc.Count > 0)
                    sourceInc.text = "(" + predictedSelfInc.IncomeToString() + " based on the tile effects)" + "\n";
                if (predictedSources.Count > 0)
                {

                    foreach (var kvp in predictedSources)
                    {
                        sourceInc.text += "(" + kvp.Value.IncomeToString() + " from " + kvp.Key.TileName + ")" + "\n";
                    }
                }
                sourceInc.alignment = TextAlignmentOptions.Center;
                sourceInc.fontStyle = FontStyles.Italic;
                textObjects.Add(sourceInc.GetComponent<RectTransform>());
            }
        }
        #endregion

        #region INCOME BONUS
        if (button.InfrastructureData.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform.GetChild(1).transform).GetComponent<TextMeshProUGUI>();
            income.text = "<sprite name=\"Puce_Emoji\"> Improve base income by " + button.InfrastructureData.Incomes.IncomeToString() + " per turn";
            textObjects.Add(income.GetComponent<RectTransform>());
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
                behaviour.HighlightImpactedTile(button.AssociatedTile, true);
                _highlightingBehaviours.Add(behaviour, button.AssociatedTile);
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
    public void FamilyPopup(Family family, RectTransform refObject)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
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
        _lockingPopup = popUp;
        _isLockingPopup = true;
        _lockingTimer = 0f;

        _lockingImage = Instantiate(_lockingObject, UIManager.Instance.PopUpParent).GetComponent<Image>();
        Utilities.PlacePrefabAroundTargetTopRight(_lockingImage.GetComponent<RectTransform>(), popUp.GetComponent<RectTransform>());
    }

    private void StopLockingPopup()
    {
        Destroy(_lockingImage.gameObject);
        _lockingImage = null;
        _isLockingPopup = false;
        _lockingPopup = null;
        _lockingTimer = 0f;
    }

    private void LockPopup(GameObject popUp)
    {
        _popUps.Remove(popUp);
        Button lockedButton = Instantiate(_lockedObject, UIManager.Instance.PopUpParent).GetComponent<Button>();
        _lockedPopUps.Add(popUp, lockedButton);
        Utilities.PlacePrefabAroundTargetTopRight(lockedButton.GetComponent<RectTransform>(), popUp.GetComponent<RectTransform>());
        StopLockingPopup();
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
}
