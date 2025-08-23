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
    [SerializeField] private Image _timerOverImage;
    [SerializeField] private float _offsetBetweenSeveralPopUps = 1f;
    [SerializeField] private float _marginAtMinZoom = 45f;
    [SerializeField] private float _marginAtMaxZoom = 150f;
    [SerializeField] private float _maxScreenFraction = 0.15f;
    [SerializeField] private Vector4 _horizontalMargin = new Vector4(10, 0, 10, 0); // left, top, right, bottom
    [SerializeField] private Vector4 _fullMargin = new Vector4(10, 5, 10, 5);
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
    private InteractionButton _clonedButton;

    public InteractionButton ClonedButton { get => _clonedButton; set => _clonedButton = value; }
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
            _delayedHoverTimer = _durationHoverForUI * .75f;
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
                    case "UpgradeNodeUI":
                        UpgradeNodePopUp(obj.GetComponent<UI_UpgradeNode>());
                        break;
                    case "ScoreUI":
                        ScorePopUp();
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
            _delayedHoverTimer = _durationHoverForUI * 0.75f;
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
                    _clonedButton = button;
                    button.CreateHighlightedClone();
                    GameManager.Instance.InteractionButtonsFade(true);
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
                                ButtonDestroyPopUp("Remove " + button.AssociatedTile.Entertainment.Data.Type.ToCustomString());
                            break;
                        case Interaction.Entertainment:
                            ButtonEntertainmentPopUp(button);
                            break;
                        case Interaction.RedirectScout:
                            ButtonRedirectScoutPopUp(button);
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
        GameManager.Instance.InteractionButtonsFade(false);

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
        if (_clonedButton)
        {
            _clonedButton.DestroyHighlightedClone();
            _clonedButton = null;
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        detail.text = "Can be upgrades with specifics enhancements and upgrades";
        detail.margin = _horizontalMargin;
        ClampTextWidth(detail);
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
            detail.text = "Hide or show Entertainments' icon";
        else
            detail.text = "Hide or show Scouts' icon";
        detail.margin = _horizontalMargin;
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
        title.text = "Claim";
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        income.text = "Claim per turn: +" + ExpansionManager.Instance.ClaimPerTurn + "<sprite name=\"Claim_Emoji\">";
        income.margin = _horizontalMargin;
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        #region SAVE
        TextMeshProUGUI save = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        save.text = "Max stockable claim: " + ExpansionManager.Instance.SavedClaimPerTurn;
        save.margin = _horizontalMargin;
        ClampTextWidth(save);
        textObjects.Add(save.GetComponent<RectTransform>());
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        income.text = "Total gold per turn: +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        income.margin = _horizontalMargin;
        ClampTextWidth(income);
        textObjects.Add(income.GetComponent<RectTransform>());
        #endregion

        #region INCOME NO INFRA
        TextMeshProUGUI incomeNoInfra = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        incomeNoInfra.text = "Gold from non enhanced tiles: +" + ExploitationManager.Instance.GetResourceIncomeByNoInfraTiles(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        incomeNoInfra.margin = _horizontalMargin;
        ClampTextWidth(incomeNoInfra);
        textObjects.Add(incomeNoInfra.GetComponent<RectTransform>());
        #endregion

        #region INCOME INFRA
        TextMeshProUGUI incomeInfra = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        incomeInfra.text = "Gold from enhanced tiles: +" + ExploitationManager.Instance.GetResourceIncomeByInfra(Resource.Gold) + "<sprite name=\"Gold_Emoji\">";
        incomeInfra.margin = _horizontalMargin;
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region INCOME
        TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        income.text = "Special Resources per turn: +" + ExploitationManager.Instance.GetResourceIncomeByAllTiles(Resource.SpecialResources) + "<sprite name=\"SR_Emoji\">";
        income.margin = _horizontalMargin;
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region MINSTREL STAGE
        TextMeshProUGUI minstrel = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        minstrel.text = "Points from Minstrel Stage: +" + EntertainmentManager.Instance.GetPointsFromMinstrelStage() + "<sprite name=\"Point_Emoji\">";
        minstrel.margin = _horizontalMargin;
        ClampTextWidth(minstrel);
        textObjects.Add(minstrel.GetComponent<RectTransform>());
        #endregion

        #region TASTING PAVILION
        TextMeshProUGUI tasting = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        tasting.text = "Points from Tasting Pavilion: +" + EntertainmentManager.Instance.GetPointsFromTastingPavilion() + "<sprite name=\"Point_Emoji\">";
        tasting.margin = _horizontalMargin;
        ClampTextWidth(tasting);
        textObjects.Add(tasting.GetComponent<RectTransform>());
        #endregion

        #region PARADE ROUTE
        TextMeshProUGUI parade = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        parade.text = "Points from Parade Route: +" + EntertainmentManager.Instance.GetPointsFromParadeRoute() + "<sprite name=\"Point_Emoji\">";
        parade.margin = _horizontalMargin;
        ClampTextWidth(parade);
        textObjects.Add(parade.GetComponent<RectTransform>());
        #endregion

        #region MYSTIC GARDEN
        TextMeshProUGUI garden = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        garden.text = "Points from Mystic Garden: +" + EntertainmentManager.Instance.GetPointsFromMysticGarden() + "<sprite name=\"Point_Emoji\">";
        garden.margin = _horizontalMargin;
        ClampTextWidth(garden);
        textObjects.Add(garden.GetComponent<RectTransform>());
        #endregion

        #region CONVERTED POINTS
        TextMeshProUGUI converted = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        converted.text = "Points from savings: +" + EntertainmentManager.Instance.ConvertedPoints + "<sprite name=\"Point_Emoji\">";
        converted.margin = _horizontalMargin;
        ClampTextWidth(converted);
        textObjects.Add(converted.GetComponent<RectTransform>());
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopupRelativeToUI(popUp.GetComponent<RectTransform>(), _objectUnderMouse.GetComponent<RectTransform>());
    }

    private void UpgradeNodePopUp(UI_UpgradeNode node)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        #region DETAIL
        TextMeshProUGUI detail = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        detail.text = node.NodeData.Effect.GetEffectDescription();
        detail.margin = _horizontalMargin;
        ClampTextWidth(detail);
        textObjects.Add(detail.GetComponent<RectTransform>());
        #endregion

        #region EXCLUSIVE
        if (node.NodeData.ExclusiveNode != null)
        {
            TextMeshProUGUI exclusive = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            if (UpgradesManager.Instance.IsNodeUnlocked(node.NodeData.ExclusiveNode))
            {
                exclusive.text = "Locked by opposite node";
                exclusive.color = UIManager.Instance.ColorCantAfford;
            }
            else
            {
                exclusive.text = "Choice node";
            }
            exclusive.margin = _fullMargin;
            exclusive.fontStyle = FontStyles.Italic;
            exclusive.alignment = TextAlignmentOptions.Center;
            textObjects.Add(exclusive.GetComponent<RectTransform>());
        }
        #endregion

        #region COST
        if (!UpgradesManager.Instance.IsNodeUnlocked(node.NodeData) && !UpgradesManager.Instance.IsNodeUnlocked(node.NodeData.ExclusiveNode))
        {
            TextMeshProUGUI cost = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            cost.text = "Cost: " + node.NodeData.Costs.CostToString();
            if (!ResourcesManager.Instance.CanAfford(node.NodeData.Costs))
                cost.color = UIManager.Instance.ColorCantAfford;
            cost.margin = _fullMargin;
            textObjects.Add(cost.GetComponent<RectTransform>());
            ClampTextWidth(cost);
        }
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region HAZARDOUS TILE
        if (tile.TileData is HazardousTileData && !tile.Claimed)
        {
            TextMeshProUGUI slow = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            slow.text = "Slow down scouts, slow removed when tile is claimed";
            slow.margin = _horizontalMargin;
            textObjects.Add(slow.GetComponent<RectTransform>());
            ClampTextWidth(slow);
        }
        #endregion

        #region BEHAVIOURS
        if (tile.TileData.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in tile.TileData.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = behaviour.GetBehaviourDescription();
                behaviourText.margin = _fullMargin;
                textObjects.Add(behaviourText.GetComponent<RectTransform>());
                ClampTextWidth(behaviourText);
                behaviour.HighlightImpactedTile(tile, true);
                _highlightingBehaviours.Add(behaviour, tile);
            }
        }
        #endregion

        #region ENHANCEMENTS
        if (tile.TileData.AvailableInfrastructures.Count > 0 && GameManager.Instance.CurrentPhase != Phase.Entertain)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            if (tile.TileData is ResourceTileData resourceTileData)
            {
                enhancement.text = "Can be upgraded to produce Special Resources and bringing a reduction to ";
                //Resource tile only have one potential enhancement with a unique special behaviour
                enhancement.text += (tile.TileData.AvailableInfrastructures[0].SpecialBehaviours[0] as SpecialResourcesCostReduction).AssociatedSystem;
                enhancement.text += " oriented Infrastructures and Upgrades";
            }
            else
            {
                enhancement.text = "Can be upgraded with " + tile.TileData.AvailableInfrastructures.Count;
                if (tile.TileData.AvailableInfrastructures.Count > 1)
                    enhancement.text += " different enhancements";
                else
                    enhancement.text += " unique enhancement";
            }
            enhancement.margin = _horizontalMargin;
            textObjects.Add(enhancement.GetComponent<RectTransform>());
            ClampTextWidth(enhancement);
            enhancement.fontStyle = FontStyles.Italic;
        }
        #endregion

        #region INCOME
        if (tile.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            income.text = "Income: " + tile.Incomes.IncomeToString();
            income.margin = _horizontalMargin;
            textObjects.Add(income.GetComponent<RectTransform>());
        }
        #endregion

        #region SCOUT STARTING POINT
        if (tile.TileData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "Scout starting point";
            scoutText.fontStyle = FontStyles.Italic;
            scoutText.alignment = TextAlignmentOptions.Center;
            scoutText.margin = _horizontalMargin;
            textObjects.Add(scoutText.GetComponent<RectTransform>());
        }
        #endregion

        #region CLAIM COST
        if (!tile.Claimed)
        {
            TextMeshProUGUI claimStatus = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            claimStatus.text = "Claim cost: " + tile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">";
            claimStatus.margin = _horizontalMargin;
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region SPEED
        TextMeshProUGUI speedText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        speedText.text = "Speed: " + scout.Speed;
        speedText.margin = _horizontalMargin;
        textObjects.Add(speedText.GetComponent<RectTransform>());
        #endregion

        #region REVEAL RADIUS
        TextMeshProUGUI revealText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        revealText.text = "Reveal radius: " + scout.RevealRadius;
        revealText.margin = _horizontalMargin;
        textObjects.Add(revealText.GetComponent<RectTransform>());
        #endregion

        #region LIFESPAN
        TextMeshProUGUI lifespanText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        lifespanText.text = "Remaining turns: " + scout.Lifespan;
        lifespanText.margin = _horizontalMargin;
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
            redirectText.margin = _fullMargin;
            redirectText.fontStyle = FontStyles.Italic;
            redirectText.alignment = TextAlignmentOptions.Center;
            textObjects.Add(redirectText.GetComponent<RectTransform>());
        }
        #endregion

        #region DIRECTION
        TextMeshProUGUI directionText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        directionText.text = scout.Direction.ToCustomString();
        directionText.margin = _horizontalMargin;
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region EFFECT
        if (ent.Data.SpecialEffect != null)
        {
            TextMeshProUGUI effectText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            effectText.text = ent.Data.SpecialEffect.GetBehaviourDescription();
            effectText.margin = _horizontalMargin;
            textObjects.Add(effectText.GetComponent<RectTransform>());
            ClampTextWidth(effectText);
            ent.Data.SpecialEffect.HighlightImpactedEntertainment(ent.Tile, true);
            _highlightingEffects.Add(ent.Data.SpecialEffect, ent.Tile);
        }
        #endregion

        #region POINTS
        TextMeshProUGUI pointsText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        pointsText.text = "Points: +" + ent.Points + "<sprite name=\"Point_Emoji\">";
        pointsText.margin = _horizontalMargin;
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region SPEED
        TextMeshProUGUI speedText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            speedText.text = "Speed: " + (button.ScoutData.Speed + boostingInfra.BoostSpeed + ExplorationManager.Instance.BoostScoutSpeed);
        else
            speedText.text = "Speed: " + (button.ScoutData.Speed + ExplorationManager.Instance.BoostScoutSpeed);
        speedText.margin = _horizontalMargin;
        textObjects.Add(speedText.GetComponent<RectTransform>());
        #endregion

        #region REVEAL RADIUS
        TextMeshProUGUI revealText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            revealText.text = "Reveal radius: " + (button.ScoutData.RevealRadius + boostingInfra.BoostRevealRadius + ExplorationManager.Instance.BoostScoutRevealRadius);
        else
            revealText.text = "Reveal radius: " + (button.ScoutData.RevealRadius + ExplorationManager.Instance.BoostScoutRevealRadius);
        revealText.margin = _horizontalMargin;
        textObjects.Add(revealText.GetComponent<RectTransform>());
        #endregion

        #region LIFESPAN
        TextMeshProUGUI lifespanText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (boostingInfra != null)
            lifespanText.text = "Lifespan: " + (button.ScoutData.Lifespan + boostingInfra.BoostLifespan + ExplorationManager.Instance.BoostScoutLifespan);
        else
            lifespanText.text = "Lifespan: " + (button.ScoutData.Lifespan + ExplorationManager.Instance.BoostScoutLifespan);
        lifespanText.margin = _horizontalMargin;
        textObjects.Add(lifespanText.GetComponent<RectTransform>());
        #endregion

        #region AVAILABILITY
        TextMeshProUGUI availability = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        availability.text = (ExplorationManager.Instance.ScoutsLimit - ExplorationManager.Instance.CurrentScoutsCount) + " Scout(s) available";
        if (ExplorationManager.Instance.CurrentScoutsCount >= ExplorationManager.Instance.ScoutsLimit)
            availability.color = UIManager.Instance.ColorCantAfford;
        availability.margin = _fullMargin;
        textObjects.Add(availability.GetComponent<RectTransform>());
        availability.fontStyle = FontStyles.Italic;
        availability.alignment = TextAlignmentOptions.Center;
        #endregion

        SetPopUpContentAnchors(textObjects);
        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    private void ButtonRedirectScoutPopUp(InteractionButton button)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = "Redirect a Scout";
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());

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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region CLAIM COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.AssociatedTile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">" + "(" + ResourcesManager.Instance.Claim + ")";
        if (!ResourcesManager.Instance.CanAffordClaim(button.AssociatedTile.TileData.ClaimCost))
            cost.color = UIManager.Instance.ColorCantAfford;
        cost.margin = _fullMargin;
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
        title.margin = _fullMargin;
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region EFFECT
        if (button.EntertainData.SpecialEffect != null)
        {
            TextMeshProUGUI effectText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            effectText.text = button.EntertainData.SpecialEffect.GetBehaviourDescription();
            effectText.margin = _horizontalMargin;
            textObjects.Add(effectText.GetComponent<RectTransform>());
            ClampTextWidth(effectText);
            button.EntertainData.SpecialEffect.HighlightImpactedEntertainment(button.AssociatedTile, true);
            _highlightingEffects.Add(button.EntertainData.SpecialEffect, button.AssociatedTile);
        }
        #endregion

        #region POINTS
        TextMeshProUGUI pointsText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        pointsText.text = "Base points: +" + button.EntertainData.BasePoints + "<sprite name=\"Point_Emoji\">";
        pointsText.margin = _horizontalMargin;
        textObjects.Add(pointsText.GetComponent<RectTransform>());
        #endregion

        #region COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.EntertainData.Costs.CostToString();
        if (!ResourcesManager.Instance.CanAfford(button.EntertainData.Costs))
            cost.color = UIManager.Instance.ColorCantAfford;
        cost.margin = _fullMargin;
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
        title.margin = _fullMargin;
        textObjects.Add(title.GetComponent<RectTransform>());
        #endregion

        #region BEHAVIOURS
        if (button.InfrastructureData.SpecialBehaviours.Count > 0)
        {
            foreach (SpecialBehaviour behaviour in button.InfrastructureData.SpecialBehaviours)
            {
                TextMeshProUGUI behaviourText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
                behaviourText.text = behaviour.GetBehaviourDescription();
                behaviourText.margin = _fullMargin;
                textObjects.Add(behaviourText.GetComponent<RectTransform>());
                ClampTextWidth(behaviourText);
                behaviour.HighlightImpactedTile(button.AssociatedTile, true);
                _highlightingBehaviours.Add(behaviour, button.AssociatedTile);
            }
        }
        #endregion

        #region ENHANCEMENTS
        if (button.InfrastructureData.AvailableInfrastructures.Count > 0)
        {
            TextMeshProUGUI enhancement = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            enhancement.text = "Can be further upgraded with " + button.InfrastructureData.AvailableInfrastructures.Count;
            if (button.InfrastructureData.AvailableInfrastructures.Count > 1)
                enhancement.text += " different enhancements";
            else
                enhancement.text += " unique enhancement";

            enhancement.margin = _horizontalMargin;
            textObjects.Add(enhancement.GetComponent<RectTransform>());
            ClampTextWidth(enhancement);
            enhancement.fontStyle = FontStyles.Italic;
        }
        #endregion

        #region INCOME
        if (button.InfrastructureData.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            income.text = "Income increase: " + button.InfrastructureData.Incomes.IncomeToString();
            income.margin = _horizontalMargin;
            textObjects.Add(income.GetComponent<RectTransform>());
        }
        #endregion

        #region SCOUT STARTING POINT
        if (button.InfrastructureData is InfrastructureData infrastructureData && infrastructureData.ScoutStartingPoint)
        {
            TextMeshProUGUI scoutText = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            scoutText.text = "Scout starting point";
            scoutText.fontStyle = FontStyles.Italic;
            scoutText.alignment = TextAlignmentOptions.Center;
            scoutText.margin = _horizontalMargin;
            textObjects.Add(scoutText.GetComponent<RectTransform>());
        }
        #endregion

        #region COST
        TextMeshProUGUI cost = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        cost.text = "Cost: " + button.InfrastructureData.Costs.CostToString();
        if (!ResourcesManager.Instance.CanAfford(button.InfrastructureData.Costs))
            cost.color = UIManager.Instance.ColorCantAfford;
        cost.margin = _fullMargin;
        textObjects.Add(cost.GetComponent<RectTransform>());
        ClampTextWidth(cost);
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
