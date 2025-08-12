using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : Singleton<PopUpManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Spawning Configuration")]
    [SerializeField] private float _durationHoverForUI = 1f;
    [SerializeField] private float _offsetBetweenSeveralPopUps = 1f;
    [SerializeField] private float _minOffset = 80f;
    [SerializeField] private float _maxOffset = 275f;
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
    private float _screenWidth;
    private float _screenHeight;
    private List<GameObject> _popUps = new List<GameObject>();
    private Dictionary<SpecialBehaviour, Tile> _highlightingBehaviours = new Dictionary<SpecialBehaviour, Tile>();
    private Dictionary<SpecialEffect, Tile> _highlightingEffects = new Dictionary<SpecialEffect, Tile>();
    float _maxAllowed;
    #endregion

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        _maxAllowed = _screenWidth * _maxScreenFraction;
    }

    #region BASE LOGIC
    public void UIPopUp(GameObject obj)
    {
        if (obj == _objectUnderMouse)
        {
            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                //spawn UI pop up
                //_popUps.Add(popUp);
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
            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                if (obj.GetComponent<Tile>() is Tile tile)
                {
                    if (tile.Revealed)
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
                }
                else if (obj.GetComponent<InteractionButton>() is InteractionButton button)
                {
                    switch (button.Interaction)
                    {
                        case Interaction.Claim:
                            ButtonClaimPopUp(button);
                            break;
                        case Interaction.Scout:
                            ButtonScoutPopUp(button);
                            break;
                        case Interaction.Infrastructure:
                            /*
                            if (_currentPhase == Phase.Expand)
                                Town popup
                            else
                                Infrastructure popup
                            */
                            break;
                        case Interaction.Destroy:
                            if (GameManager.Instance.CurrentPhase == Phase.Exploit)
                                ButtonDestroyPopUp("Destroy " + button.AssociatedTile.TileData.TileName);
                            else
                                ButtonDestroyPopUp("Remove " + button.AssociatedTile.Entertainment.Data.Type.ToCustomString());
                            break;
                        case Interaction.Entertainment:
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
        if (tile.TileData.AvailableInfrastructures.Count > 0)
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
            income.text = "Income: " + tile.Incomes.ToCustomString();
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
    #endregion

    #region POSITIONING & LAYOUT
    private void PositionPopup(RectTransform popupRect)
    {
        Vector2 mousePosition = Input.mousePosition;

        // Determine screen quadrant
        bool isLeft = mousePosition.x >= (_screenWidth / 2f); // flip: right side means place on left
        bool isTop = mousePosition.y <= (_screenHeight / 2f); // flip: bottom half means place on top

        // Rebuild popup layout to ensure height is accurate
        LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        // Calculate vertical offset from existing popups
        float verticalOffset = 0f;
        for (int i = 0; i < _popUps.Count - 1; i++)
        {
            var rt = _popUps[i].GetComponent<RectTransform>();
            verticalOffset += rt.rect.height + _offsetBetweenSeveralPopUps;
        }

        // Calculate zoom-based dynamic offset
        float zoomLevel = CameraManager.Instance.transform.position.y;
        float normalizedZoom = Mathf.InverseLerp(CameraManager.Instance.MaxZoomLevel, CameraManager.Instance.MinZoomLevel, zoomLevel);
        float dynamicOffset = Mathf.Lerp(_minOffset, _maxOffset, normalizedZoom);

        // Determine pivot and anchor
        Vector2 anchor, pivot;
        if (isLeft && isTop)
        {
            anchor = pivot = new Vector2(0f, 1f); // Top-left
        }
        else if (!isLeft && isTop)
        {
            anchor = pivot = new Vector2(1f, 1f); // Top-right
        }
        else if (isLeft && !isTop)
        {
            anchor = pivot = new Vector2(0f, 0f); // Bottom-left
        }
        else
        {
            anchor = pivot = new Vector2(1f, 0f); // Bottom-right
        }

        // Apply anchor and pivot
        popupRect.anchorMin = anchor;
        popupRect.anchorMax = anchor;
        popupRect.pivot = pivot;

        // Convert screen position to local anchored position
        RectTransform parentRect = popupRect.parent as RectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePosition, null, out localPoint);

        // Apply dynamic and vertical offsets
        Vector2 anchoredPos = localPoint;

        if (isTop)
            anchoredPos.y -= dynamicOffset + verticalOffset;
        else
            anchoredPos.y += dynamicOffset + verticalOffset;

        if (isLeft)
            anchoredPos.x += dynamicOffset;
        else
            anchoredPos.x -= dynamicOffset;

        // Final position
        popupRect.anchoredPosition = anchoredPos;
    }


    private void ClampTextWidth(TextMeshProUGUI tmp)
    {
        tmp.ForceMeshUpdate();
        tmp.alignment = TextAlignmentOptions.Justified;
        float contentWidth = tmp.preferredWidth;
        tmp.GetComponent<LayoutElement>().preferredWidth = Mathf.Min(contentWidth, _maxAllowed);
    }

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
