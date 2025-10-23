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
    [SerializeField][Range(0, 1f)] private float _maxScreenFraction = 0.15f;
    [SerializeField][Range(0, 1f)] private float _offsetNormX = 0.1f;
    [SerializeField][Range(0, 1f)] private float _maxDistanceBetweenObjectAndPopup = 0.3f;
    [SerializeField][Range(0, 1f)] private float _offsetPopupOnCursor = 0.02f;
    [SerializeField] private RectTransform _topLimit;
    [SerializeField] private RectTransform _bottomLimit;
    [SerializeField] private RectTransform _rightLimit;
    [SerializeField] private RectTransform _leftLimit;
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
    private List<GameObject> _popUps = new List<GameObject>();
    private Dictionary<SpecialBehaviour, Tile> _highlightingBehaviours = new Dictionary<SpecialBehaviour, Tile>();
    private Dictionary<SpecialEffect, Tile> _highlightingEffects = new Dictionary<SpecialEffect, Tile>();
    private float _maxAllowed;
    #endregion

    private void Start()
    {
        float dynamicFraction = _maxScreenFraction * (REF_WIDTH / Screen.width);

        _maxAllowed = Screen.width * dynamicFraction;
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
                                ButtonDestroyPopUp("Destroy " + button.AssociatedTile.TileData.TileName, button);
                            else
                            {
                                if (button.AssociatedTile.Entertainment == null)
                                    ButtonDestroyPopUp("Remove the entertainment", button);
                                else
                                    ButtonDestroyPopUp("Remove " + button.AssociatedTile.Entertainment.Data.Type.ToCustomString(), button);
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
        title.text = "Stone";
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        income.text = "Stone per turn: +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.SpecialResources) + "<sprite name=\"SR_Emoji\">";
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
            PositionPopup(popUp.GetComponent<RectTransform>(), tile.transform, true);

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

        #region SCOUT STARTING POINT
        if (tile.TileData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "<sprite name=\"Puce_Emoji\"> Scout starting point";
            textObjects.Add(scoutText.GetComponent<RectTransform>());
            ClampTextWidth(scoutText);
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

        #region INCOME SOURCES
        if (tile.IncomesSources.Count > 0)
        {
            TextMeshProUGUI sourceInc = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            List<ResourceToIntMap> ownInc = tile.GetIncomeFromTileOnly();
            if (ownInc.Count > 0)
                sourceInc.text = "(" + ownInc.IncomeToString() + " from the tile itself)" + "\n";
            foreach (var kvp in tile.IncomesSources)
            {
                sourceInc.text += "(" + kvp.Value.IncomeToString() + " from " + kvp.Key.TileName + ")" + "\n";
            }
            sourceInc.alignment = TextAlignmentOptions.Center;
            textObjects.Add(sourceInc.GetComponent<RectTransform>());
        }
        #endregion

        #region ENHANCEMENTS
        if (tile.TileData.AvailableInfrastructures.Count > 0 && GameManager.Instance.CurrentPhase != Phase.Entertain)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
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
            TextMeshProUGUI claimStatus = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            claimStatus.text = "Claim cost: " + tile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">";
            textObjects.Add(claimStatus.GetComponent<RectTransform>());
        }
        #endregion

        #region FAMILY
        if (tile.TileData is InfrastructureData infra)
        {
            TextMeshProUGUI family = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            family.text = infra.Family.ToCustomString();
            family.alignment = TextAlignmentOptions.Right;
            textObjects.Add(family.GetComponent<RectTransform>());
        }
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), tile.transform, tile.TileData.SpecialBehaviours.Count == 0);
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
        PositionPopup(popUp.GetComponent<RectTransform>(), scout.CurrentTile.transform, true);
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
        PositionPopup(popUp.GetComponent<RectTransform>(), ent.Tile.transform, ent.Data.SpecialEffects.Count == 0);
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
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
    }

    private void ButtonRedirectScoutPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        TextMeshProUGUI text = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
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

        TextMeshProUGUI text = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
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
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
    }

    private void ButtonDestroyPopUp(string text, InteractionButton button)
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
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, true);
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
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, button.EntertainData.SpecialEffects.Count == 0);
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
            income.text = "<sprite name=\"Puce_Emoji\"> Improve income by " + button.InfrastructureData.Incomes.IncomeToString() + " per turn";
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

        #region SCOUT STARTING POINT
        if (button.InfrastructureData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "<sprite name=\"Puce_Emoji\"> Scout starting point";
            textObjects.Add(scoutText.GetComponent<RectTransform>());
            ClampTextWidth(scoutText);
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
        TextMeshProUGUI predictedIncome = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        List<ResourceToIntMap> predictedInc;
        List<ResourceToIntMap> predictedSelfInc;
        Dictionary<TileData, List<ResourceToIntMap>> predictedSources;
        ExploitationManager.Instance.GetPredictedIncomes(button.AssociatedTile, button.InfrastructureData, out predictedInc, out predictedSources, out predictedSelfInc);
        predictedIncome.text = "Predicted income: " + predictedInc.IncomeToString() + " per turn";
        predictedIncome.fontStyle = FontStyles.Bold;
        predictedIncome.alignment = TextAlignmentOptions.Center;
        textObjects.Add(predictedIncome.GetComponent<RectTransform>());
        #endregion

        #region INCOME SOURCES
        if (predictedSources.Count > 0)
        {
            TextMeshProUGUI sourceInc = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            if (predictedSelfInc.Count > 0)
                sourceInc.text = "(" + predictedSelfInc.IncomeToString() + " from the tile itself)" + "\n";
            foreach (var kvp in predictedSources)
            {
                sourceInc.text += "(" + kvp.Value.IncomeToString() + " from " + kvp.Key.TileName + ")" + "\n";
            }
            sourceInc.alignment = TextAlignmentOptions.Center;
            textObjects.Add(sourceInc.GetComponent<RectTransform>());
        }
        #endregion

        #region ENHANCEMENTS
        if (button.InfrastructureData.AvailableInfrastructures.Count > 0)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be enhanced into " + button.InfrastructureData.AvailableInfrastructures.ToCustomString(false, true);

            textObjects.Add(enhancement.GetComponent<RectTransform>());
            ClampTextWidth(enhancement);
            enhancement.fontStyle = FontStyles.Italic;
            enhancement.alignment = TextAlignmentOptions.Center;
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
        }
        #endregion

        #region FAMILY
        TextMeshProUGUI family = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        family.text = button.InfrastructureData.Family.ToCustomString();
        family.alignment = TextAlignmentOptions.Right;
        textObjects.Add(family.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>(), button.transform, button.InfrastructureData.SpecialBehaviours.Count == 0);
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

    private void PositionPopup(RectTransform popupRect, Transform refTransform, bool nextToCursor)
    {
        // Convert pixel position to normalized anchors
        RectTransform parent = popupRect.parent as RectTransform;
        float screenW = parent.rect.width;
        float screenH = parent.rect.height;
        Vector2 pixelPos = popupRect.anchoredPosition;
        float normX = pixelPos.x / screenW;
        float normY = pixelPos.y / screenH;
        float normWidth = popupRect.rect.width / screenW;
        float normHeight = popupRect.rect.height / screenH; popupRect.anchorMin = new Vector2(normX, normY);
        popupRect.anchorMax = new Vector2(normX + normWidth, normY + normHeight);
        popupRect.anchoredPosition = Vector2.zero;
        popupRect.sizeDelta = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

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
                SnapVerticalEdge(refTransform, popupRect, isBottom ? _topLimit : _bottomLimit, snapTopEdge: isBottom);
                PlaceHorizontal(popupRect, refTransform, isLeft, _offsetNormX, _leftLimit, _rightLimit);
            }
        }
        // Subsequent popups → stack relative to previous
        else
        {
            RectTransform prev = _popUps[_popUps.Count - 2].GetComponent<RectTransform>();

            SnapVerticalAfterPrev(popupRect, prev, isBottom, _offsetBetweenSeveralPopUps);
            AlignHorizontalToPrevEdge(popupRect, prev, isLeft, _leftLimit, _rightLimit);
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


    private void SnapVerticalEdge(Transform refObject, RectTransform popup, RectTransform lineLimit, bool snapTopEdge)
    {
        // assume popup.parent == lineLimit.parent
        RectTransform parent = popup.parent as RectTransform;

        float parentH = parent.rect.height;
        float normHeight = popup.rect.height / parentH;

        // if your "line" rect has identical min/max Y anchors, either is fine:
        float lineY = lineLimit.anchorMin.y; // == lineLimit.anchorMax.y

        // avoid being too far from the reference Y (clamp the popup edge)
        float refY = Mathf.Clamp01(Camera.main.WorldToViewportPoint(refObject.position).y);

        // current edge based on which edge we snap
        float edgeY = snapTopEdge ? (lineY - normHeight)   // bottom edge
                                  : (lineY + normHeight);  // top edge

        float dist = Mathf.Abs(edgeY - refY);
        if (dist > _maxDistanceBetweenObjectAndPopup)
        {
            // keep edge on the same side of refY it currently is
            float desiredEdge =
                (edgeY > refY)
                ? (refY + _maxDistanceBetweenObjectAndPopup)
                : (refY - _maxDistanceBetweenObjectAndPopup);

            // rebuild lineY from the desired edge + known height
            lineY = snapTopEdge
                ? (desiredEdge + normHeight)  // edge = line - H  -> line = edge + H
                : (desiredEdge - normHeight); // edge = line + H  -> line = edge - H
        }

        Vector2 aMin = popup.anchorMin;
        Vector2 aMax = popup.anchorMax;

        if (snapTopEdge)
        {
            // snap popup's TOP to the line
            aMax.y = lineY;
            aMin.y = lineY - normHeight;
        }
        else
        {
            // snap popup's BOTTOM to the line
            aMin.y = lineY;
            aMax.y = lineY + normHeight;
        }

        // clamp to [0,1] to avoid drift
        aMin.y = Mathf.Clamp01(aMin.y);
        aMax.y = Mathf.Clamp01(aMax.y);

        popup.anchorMin = aMin;
        popup.anchorMax = aMax;

        // zero offsets so it's purely anchor-driven
        popup.anchoredPosition = new Vector2(popup.anchoredPosition.x, 0f);
        popup.sizeDelta = new Vector2(popup.sizeDelta.x, 0f);
    }

    private void PlaceHorizontal(RectTransform popup, Transform refTransform, bool isLeft, float offsetNorm, RectTransform _leftLimit, RectTransform _rightLimit)
    {
        RectTransform parent = popup.parent as RectTransform;
        float parentW = parent.rect.width;
        float normWidth = popup.rect.width / parentW;

        // 1) tile x in normalized [0..1] using viewport space (works for overlay/camera canvases covering the screen)
        float centerNorm = Mathf.Clamp01(Camera.main.WorldToViewportPoint(refTransform.position).x);

        // 2) available horizontal band from limits (assumes limits share parent and are vertical "lines")
        float bandMin = _leftLimit.anchorMin.x;   // == _leftLimit.anchorMax.x
        float bandMax = _rightLimit.anchorMin.x;  // == _rightLimit.anchorMax.x

        // If popup wider than band, clamp to band
        if (normWidth >= (bandMax - bandMin))
        {
            popup.anchorMin = new Vector2(bandMin, popup.anchorMin.y);
            popup.anchorMax = new Vector2(bandMax, popup.anchorMax.y);
            popup.anchoredPosition = new Vector2(0f, popup.anchoredPosition.y);
            popup.sizeDelta = new Vector2(0f, popup.sizeDelta.y);
            return;
        }

        // 3) initial placement: edge relative to tile.x ± offset
        float aMinX, aMaxX;
        if (isLeft)
        {
            // popup to the RIGHT of the tile: left edge starts at tile + offset
            aMinX = centerNorm + offsetNorm;
            aMaxX = aMinX + normWidth;
        }
        else
        {
            // popup to the LEFT of the tile: right edge ends at tile - offset
            aMaxX = centerNorm - offsetNorm;
            aMinX = aMaxX - normWidth;
        }

        // 4) clamp inside [bandMin, bandMax] by shifting the rect if needed
        float shift = 0f;
        if (aMinX < bandMin) shift = bandMin - aMinX;
        else if (aMaxX > bandMax) shift = bandMax - aMaxX;

        aMinX += shift;
        aMaxX += shift;

        // 5) assign anchors and zero offsets (pure anchor-driven on X)
        Vector2 aMin = popup.anchorMin;
        Vector2 aMax = popup.anchorMax;
        aMin.x = Mathf.Clamp01(aMinX);
        aMax.x = Mathf.Clamp01(aMaxX);

        popup.anchorMin = aMin;
        popup.anchorMax = aMax;

        popup.anchoredPosition = new Vector2(0f, popup.anchoredPosition.y);
        popup.sizeDelta = new Vector2(0f, popup.sizeDelta.y);
    }

    private void SnapVerticalAfterPrev(RectTransform popup, RectTransform prev, bool isBottom, float offsetNormY)
    {
        RectTransform parent = (RectTransform)popup.parent;
        float parentH = parent.rect.height;
        float normH = popup.rect.height / parentH;

        // offsetNormY is already normalized (0–1)
        float oy = offsetNormY;

        float prevMinY = prev.anchorMin.y; // prev bottom
        float prevMaxY = prev.anchorMax.y; // prev top

        Vector2 aMin = popup.anchorMin;
        Vector2 aMax = popup.anchorMax;

        if (isBottom)
        {
            // TOP to prev BOTTOM (gap goes downward)
            aMax.y = prevMinY - oy;
            aMin.y = aMax.y - normH;
        }
        else
        {
            // BOTTOM to prev TOP (gap goes upward)
            aMin.y = prevMaxY + oy;
            aMax.y = aMin.y + normH;
        }

        aMin.y = Mathf.Clamp01(aMin.y);
        aMax.y = Mathf.Clamp01(aMax.y);

        popup.anchorMin = aMin;
        popup.anchorMax = aMax;
        popup.anchoredPosition = new Vector2(popup.anchoredPosition.x, 0f);
        popup.sizeDelta = new Vector2(popup.sizeDelta.x, 0f);
    }

    private void AlignHorizontalToPrevEdge(RectTransform popup, RectTransform prev, bool isLeft, RectTransform _leftLimit, RectTransform _rightLimit)
    {
        RectTransform parent = (RectTransform)popup.parent;
        float parentW = parent.rect.width;
        float normW = popup.rect.width / parentW;

        float bandMin = _leftLimit.anchorMin.x;
        float bandMax = _rightLimit.anchorMin.x;

        Vector2 aMin = popup.anchorMin;
        Vector2 aMax = popup.anchorMax;

        if (isLeft)
        {
            // Right edge to prev right
            aMax.x = prev.anchorMax.x;
            aMin.x = aMax.x - normW;
        }
        else
        {
            // Left edge to prev left
            aMin.x = prev.anchorMin.x;
            aMax.x = aMin.x + normW;
        }

        // Clamp inside limits by shifting if necessary
        float shift = 0f;
        if (aMin.x < bandMin) shift = bandMin - aMin.x;
        else if (aMax.x > bandMax) shift = bandMax - aMax.x;

        aMin.x += shift;
        aMax.x += shift;

        aMin.x = Mathf.Clamp01(aMin.x);
        aMax.x = Mathf.Clamp01(aMax.x);

        popup.anchorMin = aMin;
        popup.anchorMax = aMax;

        // zero X offsets (anchor-driven)
        popup.anchoredPosition = new Vector2(0f, popup.anchoredPosition.y);
        popup.sizeDelta = new Vector2(0f, popup.sizeDelta.y);
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
