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
    [SerializeField] private float _offsetBetweenSeveralPopUps = 1f;
    [SerializeField] private float _marginAtMinZoom = 45f;
    [SerializeField] private float _marginAtMaxZoom = 150f;
    [SerializeField] private float _maxScreenFraction = 0.15f;
    [Header("_________________________________________________________")]
    [Header("Prefabs")]
    [SerializeField] private GameObject _basePopUp;
    [SerializeField] private GameObject _title;
    [SerializeField] private GameObject _text;
    #endregion

    #region VARIABLES
    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _delayedHoverTimer;//For filling image purpose
    private float _screenWidth;
    private float _screenHeight;
    private List<GameObject> _popUps = new List<GameObject>();
    private Dictionary<SpecialBehaviour, Tile> _highlightingBehaviours = new Dictionary<SpecialBehaviour, Tile>();
    private Dictionary<SpecialEffect, Tile> _highlightingEffects = new Dictionary<SpecialEffect, Tile>();
    private float _maxAllowed;
    #endregion

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        float dynamicFraction = _maxScreenFraction * (REF_WIDTH / _screenWidth);

        _maxAllowed = _screenWidth * dynamicFraction;
    }

    #region BASE LOGIC
    public void UIPopUp(GameObject obj)
    {
        if (obj == _objectUnderMouse)
        {
            if (obj.CompareTag("Untagged"))
                return;

            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            // delay start
            _delayedHoverTimer = _durationHoverForUI * _percentageOfTimerForVisualHint;
            // Fill is 0 until t >= t0, then rises linearly to 1 at t == d.
            _timerOverImage.fillAmount = Mathf.InverseLerp(_delayedHoverTimer, _durationHoverForUI, _hoverTimer);

            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                _timerOverImage.enabled = false;
                switch (obj.tag)
                {
                    case "ScoutLimitUI":
                        LimitPopUp("Scouts");
                        break;
                    case "ClaimUI":
                        ClaimPopUp();
                        break;
                    case "TownLimitUI":
                        LimitPopUp("Towns");
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

            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            // delay start
            _delayedHoverTimer = _durationHoverForUI * _percentageOfTimerForVisualHint;
            // Fill is 0 until t >= t0, then rises linearly to 1 at t == d.
            _timerOverImage.fillAmount = Mathf.InverseLerp(_delayedHoverTimer, _durationHoverForUI, _hoverTimer);
            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                _timerOverImage.enabled = false;
                if (obj.GetComponent<Tile>() is Tile tile)
                {
                    TilePopUp(tile);
                    if (tile.Scouts.Count > 0)
                    {
                        foreach (Scout item in tile.Scouts)
                            ScoutPopUp(item);
                    }
                    if (tile.Entertainment != null)
                        EntertainmentPopUp(tile.Entertainment);
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
                                ButtonDestroyPopUp("Destroy " + button.AssociatedTile.TileData.TileName);
                            else
                            {
                                if (button.AssociatedTile.Entertainment == null)
                                    ButtonDestroyPopUp("Remove the entertainment");
                                else
                                    ButtonDestroyPopUp("Remove " + button.AssociatedTile.Entertainment.Data.Type.ToCustomString());
                            }
                            break;
                        case Interaction.Entertainment:
                            ButtonEntertainmentPopUp(button);
                            break;
                        case Interaction.RedirectScout:
                            ButtonRedirectScoutPopUp();
                            break;
                        case Interaction.RevealAnywhere:
                            ButtonRevealAnywherePopUp();
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

    public void ResetPopUp(GameObject obj)
    {
        _objectUnderMouse = obj;
        _hoverTimer = 0.0f;
        _timerOverImage.fillAmount = 0.0f;
        _delayedHoverTimer = 0.0f;
        _timerOverImage.enabled = true;
        GameManager.Instance.InteractionButtonsFade(false, null);

        if (_popUps.Count > 0)
        {
            foreach (GameObject item in _popUps)
            {
                    Destroy(item);
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
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = text + " limit";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Can be upgrades with specifics enhancements and upgrades";
        ClampTextWidth(detail);
        detail.alignment = TextAlignmentOptions.Center;
        detail.fontStyle = FontStyles.Italic;
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void VisibilityPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
            title.text = "Entertainments visibility";
        else
            title.text = "Scouts visibility";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
            detail.text = "Hide or show Entertainments' icon";
        else
            detail.text = "Hide or show Scouts' icon";
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void ClaimPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Claims";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        income.text = "Claim per turn: +" + ExpansionManager.Instance.ClaimPerTurn + "<sprite name=\"Claim_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void GoldPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Gold";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total gold per turn: +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        #region INCOME NO INFRA
        TextMeshProUGUI incomeNoInfra = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        incomeNoInfra.text = "Gold from non enhanced tiles: +" + ExploitationManager.Instance.GetResourceIncomeByNoInfraTiles(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        ClampTextWidth(incomeNoInfra);
        textObjects.Add(incomeNoInfra.GetComponent<RectTransform>());
        #endregion

        #region INCOME INFRA
        TextMeshProUGUI incomeInfra = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        incomeInfra.text = "Gold from enhanced tiles: +" + ExploitationManager.Instance.GetResourceIncomeByInfra(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        ClampTextWidth(incomeInfra);
        textObjects.Add(incomeInfra.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void SRPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Special Resources";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        income.text = "Special Resources per turn: +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.SpecialResources) + "<sprite name=\"SR_Emoji\">";
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void ScorePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Points";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region MINSTREL STAGE
        TextMeshProUGUI minstrel = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        minstrel.text = "Points from Minstrel Stage: +" + EntertainmentManager.Instance.GetPointsFromMinstrelStage() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(minstrel);
        textObjects.Add(minstrel.GetComponent<RectTransform>());
        #endregion

        #region TASTING PAVILION
        TextMeshProUGUI tasting = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        tasting.text = "Points from Tasting Pavilion: +" + EntertainmentManager.Instance.GetPointsFromTastingPavilion() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(tasting);
        textObjects.Add(tasting.GetComponent<RectTransform>());
        #endregion

        #region PARADE ROUTE
        TextMeshProUGUI parade = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        parade.text = "Points from Parade Route: +" + EntertainmentManager.Instance.GetPointsFromParadeRoute() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(parade);
        textObjects.Add(parade.GetComponent<RectTransform>());
        #endregion

        #region MYSTIC GARDEN
        TextMeshProUGUI garden = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        garden.text = "Points from Mystic Garden: +" + EntertainmentManager.Instance.GetPointsFromMysticGarden() + "<sprite name=\"Point_Emoji\">";
        ClampTextWidth(garden);
        textObjects.Add(garden.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void CarnivalistPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Carnivalists";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Carnivalists are used during the Grand Jubilee";
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
                TextMeshProUGUI source = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
                source.text = pair.Key.TileName + ": " + pair.Value + "<sprite name=\"Carnivalist_Emoji\">";
                ClampTextWidth(source);
                textObjects.Add(source.GetComponent<RectTransform>());
            }
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void ShowIncomePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreIncomesShown)
            detail.text = "Hide tiles' incomes and bonuses";
        else
            detail.text = "Show tiles' incomes and bonuses";
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void ShowEntPlacementPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (UIManager.Instance.AreEntPlacementShown)
            detail.text = "Hide which tiles can receive an entertainment";
        else
            detail.text = "Show which tiles can receive an entertainment";
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void UpgradePopUp(UpgradeEffect effect)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = effect.EffectName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        detail.text = effect.GetEffectDescription();
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }
    #endregion

    #region ON TILE POP UP
    private void TilePopUp(Tile tile)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = tile.TileData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region HAZARDOUS TILE
        if (tile.TileData is HazardousTileData && !tile.Claimed)
        {
            TextMeshProUGUI slow = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            slow.text = "Slow down scouts, cannot be claimed";
            textObjects.Add(slow.GetComponent<RectTransform>());
            ClampTextWidth(slow);
            slow.fontStyle = FontStyles.Italic;
            slow.alignment = TextAlignmentOptions.Center;

            SetPopUpContentAnchors(textObjects);
            PositionPopup(popUp.GetComponent<RectTransform>());

            // No need to go further, hazardous tile have no other info
            return;
        }
        #endregion

        #region BEHAVIOURS
        if (tile.TileData.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in tile.TileData.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = "<sprite name=\"Puce_Emoji\"> " + behaviour.GetBehaviourDescription();
                textObjects.Add(behaviourText.GetComponent<RectTransform>());
                ClampTextWidth(behaviourText);
                behaviour.HighlightImpactedTile(tile, true);
                _highlightingBehaviours.Add(behaviour, tile);
            }
        }
        #endregion

        #region INCOME
        if (tile.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            income.text = tile.Incomes.IncomeToString() + " per turn";
            income.fontStyle = FontStyles.Bold;
            income.alignment = TextAlignmentOptions.Center;
            textObjects.Add(income.GetComponent<RectTransform>());
        }
        #endregion

        #region ENHANCEMENTS
        if (tile.TileData.AvailableInfrastructures.Count > 0 && GameManager.Instance.CurrentPhase != Phase.Entertain)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + tile.TileData.AvailableInfrastructures.ToCustomString(true);
            textObjects.Add(enhancement.GetComponent<RectTransform>());
            ClampTextWidth(enhancement);
            enhancement.fontStyle = FontStyles.Italic;
            enhancement.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region SCOUT STARTING POINT
        if (tile.TileData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "Scout starting point";
            scoutText.fontStyle = FontStyles.Italic;
            scoutText.alignment = TextAlignmentOptions.Center;
            textObjects.Add(scoutText.GetComponent<RectTransform>());
        }
        #endregion

        #region CLAIM COST
        if (!tile.Claimed && tile.TileData is not HazardousTileData)
        {
            TextMeshProUGUI claimStatus = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            claimStatus.text = "Claim cost: " + tile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">";
            textObjects.Add(claimStatus.GetComponent<RectTransform>());
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void ScoutPopUp(Scout scout)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Scout";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region SPEED
        TextMeshProUGUI speedText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        speedText.text = "Speed: " + scout.Speed;
        textObjects.Add(speedText.GetComponent<RectTransform>());
        #endregion

        #region REVEAL RADIUS
        TextMeshProUGUI revealText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        revealText.text = "Reveal radius: " + scout.RevealRadius;
        textObjects.Add(revealText.GetComponent<RectTransform>());
        #endregion

        #region LIFESPAN
        TextMeshProUGUI lifespanText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        lifespanText.text = "Remaining turns: " + scout.Lifespan;
        textObjects.Add(lifespanText.GetComponent<RectTransform>());
        #endregion

        #region REDIRECTABLE
        if (ExplorationManager.Instance.UpgradeScoutRedirectable)
        {
            TextMeshProUGUI redirectText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
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
        TextMeshProUGUI directionText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        directionText.text = scout.Direction.ToCustomString();
        directionText.fontStyle = FontStyles.Italic;
        directionText.alignment = TextAlignmentOptions.Right;
        textObjects.Add(directionText.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void EntertainmentPopUp(Entertainment ent)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = ent.Data.Type.ToCustomString();
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region EFFECT
        if (ent.Data.SpecialEffects.Count > 0)
        {
            foreach (SpecialEffect effect in ent.Data.SpecialEffects)
            {
                TextMeshProUGUI effectText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
                effectText.text = effect.GetBehaviourDescription();
                textObjects.Add(effectText.GetComponent<RectTransform>());
                ClampTextWidth(effectText);
                effect.HighlightImpactedEntertainment(ent.Tile, true);
                _highlightingEffects.Add(effect, ent.Tile);
            }
        }
        #endregion

        #region POINTS
        TextMeshProUGUI pointsText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        pointsText.text = "Points: +" + ent.Points + "<sprite name=\"Point_Emoji\">";
        textObjects.Add(pointsText.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
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
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Spawn a Scout";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region SPEED
        TextMeshProUGUI speedText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            speedText.text = "Speed: " + (button.ScoutData.Speed + boostingInfra.BoostSpeed + ExplorationManager.Instance.BoostScoutSpeed);
        else
            speedText.text = "Speed: " + (button.ScoutData.Speed + ExplorationManager.Instance.BoostScoutSpeed);
        textObjects.Add(speedText.GetComponent<RectTransform>());
        #endregion

        #region REVEAL RADIUS
        TextMeshProUGUI revealText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            revealText.text = "Reveal radius: " + (button.ScoutData.RevealRadius + boostingInfra.BoostRevealRadius + ExplorationManager.Instance.BoostScoutRevealRadius);
        else
            revealText.text = "Reveal radius: " + (button.ScoutData.RevealRadius + ExplorationManager.Instance.BoostScoutRevealRadius);
        textObjects.Add(revealText.GetComponent<RectTransform>());
        #endregion

        #region LIFESPAN
        TextMeshProUGUI lifespanText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            lifespanText.text = "Lifespan: " + (button.ScoutData.Lifespan + boostingInfra.BoostLifespan + ExplorationManager.Instance.BoostScoutLifespan);
        else
            lifespanText.text = "Lifespan: " + (button.ScoutData.Lifespan + ExplorationManager.Instance.BoostScoutLifespan);
        textObjects.Add(lifespanText.GetComponent<RectTransform>());
        #endregion

        #region AVAILABILITY
        TextMeshProUGUI availability = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
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
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void ButtonRedirectScoutPopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        TextMeshProUGUI text = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        text.text = "Redirect a Scout";
        textObjects.Add(text.GetComponent<RectTransform>());

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void ButtonRevealAnywherePopUp()
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        TextMeshProUGUI text = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        text.text = $"Reveal this tile and all those in a {ExplorationManager.Instance.UpgradeRevealAnywhere.RevealRadius}-tile radius";
        textObjects.Add(text.GetComponent<RectTransform>());
        ClampTextWidth(text);

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void ButtonClaimPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Claim " + button.AssociatedTile.TileData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region CLAIM COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.AssociatedTile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">" + "(" + ResourcesManager.Instance.Claim + ")";
        if (!ResourcesManager.Instance.CanAffordClaim(button.AssociatedTile.TileData.ClaimCost))
            cost.color = UIManager.Instance.ColorCantAfford;
        textObjects.Add(cost.GetComponent<RectTransform>());
        ClampTextWidth(cost);
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void ButtonDestroyPopUp(string text)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = text;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void ButtonEntertainmentPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Place " + button.EntertainData.Type.ToCustomString();
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region EFFECT
        if (button.EntertainData.SpecialEffects.Count > 0)
        {
            foreach (SpecialEffect effect in button.EntertainData.SpecialEffects)
            {
                TextMeshProUGUI effectText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
                effectText.text = effect.GetBehaviourDescription();
                textObjects.Add(effectText.GetComponent<RectTransform>());
                ClampTextWidth(effectText);
                effect.HighlightImpactedEntertainment(button.AssociatedTile, true);
                _highlightingEffects.Add(effect, button.AssociatedTile);
            }
        }
        #endregion

        #region POINTS
        TextMeshProUGUI pointsText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        pointsText.text = "Base points: +" + button.EntertainData.BasePoints + "<sprite name=\"Point_Emoji\">";
        textObjects.Add(pointsText.GetComponent<RectTransform>());
        #endregion

        #region COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.EntertainData.GetActualCarnivalistCost(button.AssociatedTile) + "<sprite name=\"Carnivalist_Emoji\">";
        if (!ResourcesManager.Instance.CanAffordCarnivalist(button.EntertainData.GetActualCarnivalistCost(button.AssociatedTile)))
            cost.color = UIManager.Instance.ColorCantAfford;
        textObjects.Add(cost.GetComponent<RectTransform>());
        ClampTextWidth(cost);
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void ButtonInfraPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region TITLE
        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Build " + button.InfrastructureData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME BONUS
        if (button.InfrastructureData.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            income.text = "Improve income by " + button.InfrastructureData.Incomes.IncomeToString() + " per turn";
            textObjects.Add(income.GetComponent<RectTransform>());
        }
        #endregion

        #region BEHAVIOURS
        if (button.InfrastructureData.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in button.InfrastructureData.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = "<sprite name=\"Puce_Emoji\"> " + behaviour.GetBehaviourDescription();
                textObjects.Add(behaviourText.GetComponent<RectTransform>());
                ClampTextWidth(behaviourText);
                behaviour.HighlightImpactedTile(button.AssociatedTile, true);
                _highlightingBehaviours.Add(behaviour, button.AssociatedTile);
            }
        }
        #endregion

        #region CURRENT INCOME
        if (button.AssociatedTile.Incomes.Count > 0)
        {
            TextMeshProUGUI currentIncome = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            currentIncome.text = "Current income: " + button.AssociatedTile.Incomes.IncomeToString() + " per turn";
            currentIncome.fontStyle = FontStyles.Bold;
            currentIncome.alignment = TextAlignmentOptions.Center;
            textObjects.Add(currentIncome.GetComponent<RectTransform>());
        }
        #endregion

        #region PREDICTED INCOME
        TextMeshProUGUI projectedIncome = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        projectedIncome.text = "Predicted income: " + button.AssociatedTile.GetPredictedIncomes(button.InfrastructureData).IncomeToString() + " per turn";
        projectedIncome.fontStyle = FontStyles.Bold;
        projectedIncome.alignment = TextAlignmentOptions.Center;
        textObjects.Add(projectedIncome.GetComponent<RectTransform>());
        #endregion

        #region ENHANCEMENTS
        if (button.InfrastructureData.AvailableInfrastructures.Count > 0)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + button.InfrastructureData.AvailableInfrastructures.ToCustomString(true);

            textObjects.Add(enhancement.GetComponent<RectTransform>());
            ClampTextWidth(enhancement);
            enhancement.fontStyle = FontStyles.Italic;
            enhancement.alignment = TextAlignmentOptions.Center;
        }
        #endregion

        #region SCOUT STARTING POINT
        if (button.InfrastructureData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "Scout starting point";
            scoutText.fontStyle = FontStyles.Italic;
            scoutText.alignment = TextAlignmentOptions.Center;
            textObjects.Add(scoutText.GetComponent<RectTransform>());
        }
        #endregion

        #region COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
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
            TextMeshProUGUI availability = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
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
            availability.fontStyle = FontStyles.Italic;
            availability.alignment = TextAlignmentOptions.MidlineRight;
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }
    #endregion

    #region POSITIONING & LAYOUT
    private void PositionPopupRelativeToUI(RectTransform popupRect, RectTransform targetRect)
    {
        // Decide side/over-under from target center in SCREEN space
        Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(null, targetRect.TransformPoint(targetRect.rect.center));
        bool placeLeft = targetScreen.x >= Screen.width * 0.5f; // right half -> place to left
        bool placeUnder = targetScreen.y >= Screen.height * 0.5f; // top half  -> place under

        LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        RectTransform parent = (RectTransform)popupRect.parent;

        // Get target edges in PARENT-LOCAL space
        Vector3[] wc = new Vector3[4]; // 0:BL, 1:TL, 2:TR, 3:BR
        targetRect.GetWorldCorners(wc);

        Vector2 bl, tr;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, RectTransformUtility.WorldToScreenPoint(null, wc[0]), null, out bl);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, RectTransformUtility.WorldToScreenPoint(null, wc[2]), null, out tr);

        // Compute target center in parent-local space
        Vector2 centerLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, targetRect.TransformPoint(targetRect.rect.center)),
            null,
            out centerLocal);

        // Pivot decides which popup edge is used; X aligns to target center
        popupRect.anchorMin = popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(placeLeft ? 1f : 0f, placeUnder ? 1f : 0f);

        // X from center, Y from top/bottom edge
        Vector2 pos = new Vector2(
            centerLocal.x,
            placeUnder ? bl.y : tr.y
        );

        popupRect.anchoredPosition = pos;
    }

    private void PositionPopup(RectTransform popupRect)
    {
        Vector2 mouse = Input.mousePosition;

        // Quadrant → choose which popup corner sits under the cursor
        bool isLeft = mouse.x <= (_screenWidth * 0.5f);
        bool isBottom = mouse.y <= (_screenHeight * 0.5f);

        LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        // Stack offset from existing popups
        float stackOffset = 0f;
        for (int i = 0; i < _popUps.Count - 1; i++)
            stackOffset += _popUps[i].GetComponent<RectTransform>().rect.height + _offsetBetweenSeveralPopUps;

        // Anchor & pivot at the same corner so that corner == cursor
        Vector2 corner = new Vector2(isLeft ? 0f : 1f, isBottom ? 0f : 1f);
        popupRect.anchorMin = corner;
        popupRect.anchorMax = corner;
        popupRect.pivot = corner;

        RectTransform parent = (RectTransform)popupRect.parent;

        // Convert screen → parent local (relative to parent *pivot*)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, mouse, null, out var localFromParentPivot);

        // Convert parent-pivot space → anchor space
        Vector2 anchorOffset = new Vector2(
            parent.rect.width * (parent.pivot.x - popupRect.anchorMin.x),
            parent.rect.height * (parent.pivot.y - popupRect.anchorMin.y)
        );
        Vector2 anchoredPos = localFromParentPivot + anchorOffset;

        // Apply stacking offset along the outward direction
        anchoredPos.y += isBottom ? +stackOffset : -stackOffset;

        /*
        // --- zoom-based cursor margin ---
        float yZoom = CameraManager.Instance.transform.position.y;       // smaller => closer
        float t = Mathf.InverseLerp(CameraManager.Instance.MaxZoomLevel,
                                          CameraManager.Instance.MinZoomLevel, yZoom);
        // bigger y -> smaller margin
        float margin = Mathf.Lerp(_marginAtMaxZoom, _marginAtMinZoom, t);

        // push away from the cursor based on corner
        float signX = isLeft ? +1f : -1f; // BL/TL -> +x ; BR/TR -> -x
        float signY = isBottom ? +1f : -1f; // BL/BR -> +y ; TL/TR -> -y
        anchoredPos += new Vector2(signX * margin, signY * margin);
        // --- end zoom-based cursor margin ---
        */

        popupRect.anchoredPosition = anchoredPos;
    }

    private void ClampTextWidth(TextMeshProUGUI tmp)
    {
        tmp.ForceMeshUpdate();
        tmp.alignment = TextAlignmentOptions.Justified;
        float contentWidth = tmp.preferredWidth;
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
}
