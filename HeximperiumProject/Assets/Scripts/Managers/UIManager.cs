using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    #region CONFIGURATION
    [SerializeField] private Transform _mainCanvas;
    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI _claimText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _stoneText;
    [SerializeField] private TextMeshProUGUI _essenceText;
    [SerializeField] private TextMeshProUGUI _horseText;
    [SerializeField] private TextMeshProUGUI _pigmentText;
    [SerializeField] private TextMeshProUGUI _crystalText;
    [SerializeField] private TextMeshProUGUI _emberboneText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [Header("Phase UI")]
    [SerializeField] private TextMeshProUGUI _currentPhaseText;
    [SerializeField] private TextMeshProUGUI _confirmPhaseButtonText;
    [SerializeField] private TextMeshProUGUI _turnCounterText;
    [Header("PopUp UI")]
    [SerializeField] private GameObject _unclaimedTilePopup;
    [SerializeField] private float _durationHoverForUI = 2.0f;
    [Header("Radial menu")]
    [SerializeField] private Color _colorCantAfford;
    [Header("Units visibility UI")]
    [SerializeField] private Image _scoutsImage;
    [SerializeField] private Image _entertainersImage;
    [SerializeField] private Sprite _scoutsVisible;
    [SerializeField] private Sprite _scoutsHidden;
    [SerializeField] private Sprite _entertainersVisible;
    [SerializeField] private Sprite _entertainersHidden;
    #endregion

    #region VARIABLES
    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _screenWidth;
    private float _screenHeight;
    private GameObject _popup;

    private bool _areScoutsVisible;
    private bool _areEntertainersVisible;
    #endregion

    #region ACCESSORS
    public Color ColorCantAfford { get => _colorCantAfford;}
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnNewTurn.AddListener(UpdateTurnCounterText);

        GameManager.Instance.OnExplorationPhaseStarted.AddListener(UpdatePhaseUI);
        GameManager.Instance.OnExpansionPhaseStarted.AddListener(UpdatePhaseUI);
        GameManager.Instance.OnExploitationPhaseStarted.AddListener(UpdatePhaseUI);
        GameManager.Instance.OnEntertainementPhaseStarted.AddListener(UpdatePhaseUI);

        GameManager.Instance.OnExplorationPhaseStarted.AddListener(ForceScoutsToShow);
        GameManager.Instance.OnExplorationPhaseStarted.AddListener(ForceEntertainersToHide);

        GameManager.Instance.OnEntertainementPhaseStarted.AddListener(ForceScoutsToHide);
        GameManager.Instance.OnEntertainementPhaseStarted.AddListener(ForceEntertainersToShow);

    }

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
}

    #region RESOURCES BAR UI
    public void UpdateClaimUI(int value)
    {
        _claimText.text = value.ToString();
    }

    public void UpdateResourceUI(Resource resource, int value)
    {
        switch (resource)
        {
            case Resource.Stone:
                _stoneText.text = value.ToString();
                break;
            case Resource.Essence:
                _essenceText.text = value.ToString();
                break;
            case Resource.Horse:
                _horseText.text = value.ToString();
                break;
            case Resource.Pigment:
                _pigmentText.text = value.ToString();
                break;
            case Resource.Crystal:
                _crystalText.text = value.ToString();
                break;
            case Resource.Emberbone:
                _emberboneText.text = value.ToString();
                break;
            case Resource.Gold:
                _goldText.text = value.ToString();
                break;
        }
    }

    public void UpdateScoreUI(int value)
    {
        _scoreText.text = value.ToString();
    }
    #endregion

    #region UNITS VISIBILITY UI
    //OnClick for UI button
    public void ScoutsVisibility()
    {
        _areScoutsVisible = !_areScoutsVisible;

        _scoutsImage.sprite = _areScoutsVisible ? _scoutsVisible : _scoutsHidden;

        foreach (Scout item in ExplorationManager.Instance.Scouts)
        {
            item.ScoutVisibility(_areScoutsVisible);
        }
    }

    private void ScoutsVisibility(bool visible)
    {
        _areScoutsVisible = visible;

        _scoutsImage.sprite = _areScoutsVisible ? _scoutsVisible : _scoutsHidden;

        foreach (Scout item in ExplorationManager.Instance.Scouts)
        {
            item.ScoutVisibility(visible);
        }
    }

    private void ForceScoutsToShow()
    {
        ScoutsVisibility(true);
    }

    private void ForceScoutsToHide()
    {
        ScoutsVisibility(false);
    }

    //OnClick for UI button
    public void EntertainersVisibility()
    {
        _areEntertainersVisible = !_areEntertainersVisible;

        _entertainersImage.sprite = _areEntertainersVisible ? _entertainersVisible : _entertainersHidden;

        foreach (Entertainer item in EntertainementManager.Instance.Entertainers)
        {
            item.EntertainerVisibility(_areEntertainersVisible);
        }
    }

    private void EntertainersVisibility(bool visible)
    {
        _areEntertainersVisible = visible;

        _entertainersImage.sprite = _areEntertainersVisible ? _entertainersVisible : _entertainersHidden;

        foreach (Entertainer item in EntertainementManager.Instance.Entertainers)
        {
            item.EntertainerVisibility(visible);
        }
    }

    private void ForceEntertainersToShow()
    {
        EntertainersVisibility(true);
    }

    private void ForceEntertainersToHide()
    {
        EntertainersVisibility(false);
    }
    #endregion

    #region POPUP UI
    public void HoverUIPopupCheck(GameObject obj)
    {
        if (obj == _objectUnderMouse) 
        {
            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= _durationHoverForUI && _popup == null) 
            {
                Tile tile = obj.GetComponent<Tile>();
                if(tile != null)
                {
                    if (tile.Revealed)
                    {
                        DisplayPopUp(tile);
                    }
                }
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            _objectUnderMouse = obj;
            _hoverTimer = 0.0f;
            if(_popup)
                Destroy(_popup);
        }
    }

    private void DisplayPopUp(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Popup for no tile.");
            return;
        }

        //Display Pop up for unclaimed tile
        if (!tile.Claimed)
        {
            TileData data = tile.TileData;
            _popup = Instantiate(_unclaimedTilePopup, _mainCanvas);
            _popup.GetComponent<UI_UnclaimedTile>().Initialize(data.TileName, tile.Biome.ToString(), data.TextEffect, data.GetSpecificIncome(Resource.Gold).ToString(), data.ClaimCost.ToString());

            PositionPopup(_popup.transform, GetPopUpSize(_popup.GetComponent<RectTransform>()));

            _popup.SetActive(true);
        }
        //Add for every possibility of tile
    }

    private void PositionPopup(Transform popup, Vector2 popupSize)
    {
        // Get the current mouse position
        Vector3 mousePosition = Input.mousePosition;

        // Determine the quadrant
        bool isLeft = mousePosition.x < (_screenWidth / 2);
        bool isTop = mousePosition.y > (_screenHeight / 2);

        if (isLeft && isTop)
        {
            // Top-Left Quadrant: Snap top-left corner to cursor
            popup.position = new Vector3(mousePosition.x + popupSize.x / 2, mousePosition.y - popupSize.y / 2, 0);
        }
        else if (!isLeft && isTop)
        {
            // Top-Right Quadrant: Snap top-right corner to cursor
            popup.position = new Vector3(mousePosition.x - popupSize.x / 2, mousePosition.y - popupSize.y / 2, 0);
        }
        else if (isLeft && !isTop)
        {
            // Bottom-Left Quadrant: Snap bottom-left corner to cursor
            popup.position = new Vector3(mousePosition.x + popupSize.x / 2, mousePosition.y + popupSize.y / 2, 0);
        }
        else
        {
            // Bottom-Right Quadrant: Snap bottom-right corner to cursor
            popup.position = new Vector3(mousePosition.x - popupSize.x / 2, mousePosition.y + popupSize.y / 2, 0);
        }

    }

    private Vector2 GetPopUpSize(RectTransform rectTransform)
    {
        return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
    }
    #endregion

    #region PHASE UI
    private void UpdatePhaseUI()
    {
        switch (GameManager.Instance.CurrentPhase)
        {
            case Phase.Explore:
                _currentPhaseText.text = "Explore";
                _confirmPhaseButtonText.text = "End Phase";
                break;
            case Phase.Expand:
                _currentPhaseText.text = "Expand";
                _confirmPhaseButtonText.text = "End Phase";
                break;
            case Phase.Exploit:
                _currentPhaseText.text = "Exploit";
                _confirmPhaseButtonText.text = "End Phase";
                break;
            case Phase.Entertain:
                _currentPhaseText.text = "Entertain";
                _confirmPhaseButtonText.text = "End Turn";
                break;
        }
    }

    private void UpdateTurnCounterText(int turnCounter)
    {
        _turnCounterText.text = "Turn : " + turnCounter;
    }
    #endregion
}
