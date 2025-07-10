using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    #region CONFIGURATION
    [SerializeField] private Transform _mainCanvas;
    [Header("Resources Bar")]
    [SerializeField] private TextMeshProUGUI _scoutsLimitText;
    [SerializeField] private TextMeshProUGUI _claimText;
    [SerializeField] private TextMeshProUGUI _townsLimitText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _srText;
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
    [Header("PopUp UI")]
    [SerializeField] private float _durationHoverForUI = 2.0f;
    [SerializeField] private float _offsetBetweenPopUps = 0.5f;
    [SerializeField] private GameObject _prefabPopUpEntertainment;
    [SerializeField] private GameObject _prefabPopUpScout;
    [SerializeField] private float _minOffset;
    [SerializeField] private float _maxOffset;
    [Header("Radial menu")]
    [SerializeField] private Color _colorCantAfford;
    [Header("Units visibility UI")]
    [SerializeField] private Image _visibilityImage;
    [SerializeField] private Image _scoutImageVisibility;
    [SerializeField] private Image _entertainmentImageVisibility;
    [SerializeField] private Sprite _visible;
    [SerializeField] private Sprite _hidden;
    [Header("Menu")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _endMenu;
    [SerializeField] private TextMeshProUGUI _endScore;
    [Header("Score")]
    [SerializeField] private GameObject _scoreUI;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [Header("TradeMenu")]
    [SerializeField] private GameObject _tradeMenu;
    #endregion

    #region VARIABLES
    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _screenWidth;
    private float _screenHeight;
    private List<GameObject> _popUps = new List<GameObject>();

    private bool _areUnitsVisible;
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
        GameManager.Instance.OnEntertainmentPhaseStarted.AddListener(UpdatePhaseUI);

        GameManager.Instance.OnEntertainmentPhaseStarted.AddListener(UpdateUIForEntertainment);

        GameManager.Instance.OnExplorationPhaseStarted.AddListener(ForceScoutsToShow);

        GameManager.Instance.OnGameFinished.AddListener(GameFinished);

        ExplorationManager.Instance.OnScoutsLimitModified.AddListener(UpdateScoutLimit);

        EntertainmentManager.Instance.OnScoreUpdated.AddListener(UpdateScoreText);
    }

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        InitializeUI();
    }

    private void UpdateUIForEntertainment()
    {
        _scoutImageVisibility.enabled = false;
        _entertainmentImageVisibility.enabled = true;

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
    private void UpdateScoreText()
    {
        _scoreText.text = EntertainmentManager.Instance.Score.ToString();
    }

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
            _tradeMenu.SetActive(true);
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

    #region POPUP UI
    public void PopUpUI(GameObject obj)
    {
        if(obj == _objectUnderMouse)
        {
            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                SpawnUIPopUp popUpComponent = obj.GetComponent<SpawnUIPopUp>();

                if (popUpComponent != null)
                {
                    GameObject popUp = popUpComponent.SpawnPopUp(_mainCanvas);

                    UI_ResourcePopUp popUpResource = popUp.GetComponent<UI_ResourcePopUp>();
                    if (popUpResource != null)
                        popUpResource.InitializePopUp();

                    _popUps.Add(popUp);
                }
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            ResetPopUps(obj);
        }
    }

    public void PopUpNonUI(GameObject obj)
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
                        DisplayPopUp(tile);
                        if (tile.Scouts.Count > 0) 
                        {
                            foreach (Scout item in tile.Scouts)
                            {
                                DisplayPopUp(item);
                            }
                        }
                        if(tile.Entertainment != null)
                            DisplayPopUp(tile.Entertainment);
                    }
                }
                else if (obj.GetComponent<InteractionButton>() is  InteractionButton button)
                {
                    DisplayPopUp(button);
                }
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            ResetPopUps(obj);
        }
    }

    private void ResetPopUps(GameObject obj)
    {
        _objectUnderMouse = obj;
        _hoverTimer = 0.0f;

        if (_popUps.Count > 0)
        {
            foreach (GameObject item in _popUps)
            {
                UI_PopUp scriptPopUp = item.GetComponent<UI_PopUp>();
                if (scriptPopUp)
                    scriptPopUp.DestroyPopUp();
                else
                    Destroy(item);
            }
            _popUps.Clear();
        }
    }

    private void DisplayPopUp<T>(T item, GameObject prefab)
    {
        GameObject popUp;

        // Spawn the pop up
        popUp = Instantiate(prefab, _mainCanvas);
        _popUps.Add(popUp);

        // Initialize the popup
        popUp.GetComponent<UI_DynamicPopUp>().InitializePopUp(item);

        // Position the pop up relatively to the mouse cursor
        PositionPopup(popUp.transform, GetPopUpSize(popUp.GetComponent<RectTransform>()));
    }

    // Overloaded methods for different types
    private void DisplayPopUp(Scout scout)
    {
        DisplayPopUp(scout, _prefabPopUpScout);
    }

    private void DisplayPopUp(Entertainment entertainment)
    {
        DisplayPopUp(entertainment, _prefabPopUpEntertainment);
    }

    private void DisplayPopUp(InteractionButton button)
    {
        DisplayPopUp(button, button.GetPopUpPrefab());
    }

    private void DisplayPopUp(Tile tile)
    {
        DisplayPopUp(tile, tile.TileData.PopUpPrefab);
    }

    private void PositionPopup(Transform popup, Vector2 popupSize)
    {
        // Get the current mouse position
        Vector3 mousePosition = Input.mousePosition;

        // Determine the quadrant
        bool isLeft = mousePosition.x < (_screenWidth / 2);
        bool isTop = mousePosition.y > (_screenHeight / 2);

        // Calculate the cumulative vertical offset based on the sizes of previous pop-ups
        float verticalOffset = 0;
        //_popUps.Count -1 to avoid counting the current pop up
        for (int i = 0; i < _popUps.Count -1; i++)
        {
            RectTransform previousPopupRectTransform = _popUps[i].GetComponent<RectTransform>();
            verticalOffset += previousPopupRectTransform.rect.height + _offsetBetweenPopUps;
        }

        // Get the current zoom level from the camera
        float zoomLevel = CameraManager.Instance.transform.position.y;

        // Calculate the dynamic offset based on the zoom level
        float normalizedZoom = Mathf.InverseLerp(CameraManager.Instance.MinZoomLevel, CameraManager.Instance.MaxZoomLevel, zoomLevel);
        float dynamicOffset = Mathf.Lerp(_minOffset, _maxOffset, normalizedZoom);

        #if UNITY_EDITOR
        dynamicOffset = 0;//Remove the offset in the editor (game screen is small so popup aren't visible)
        #endif

        if (isLeft && isTop)
        {
            // Top-Left Quadrant: Snap top-left corner to cursor with dynamic offset
            popup.position = new Vector3(
                mousePosition.x + popupSize.x / 2 + dynamicOffset,
                mousePosition.y - popupSize.y / 2 - verticalOffset - dynamicOffset,
                0);
        }
        else if (!isLeft && isTop)
        {
            // Top-Right Quadrant: Snap top-right corner to cursor with dynamic offset
            popup.position = new Vector3(
                mousePosition.x - popupSize.x / 2 - dynamicOffset,
                mousePosition.y - popupSize.y / 2 - verticalOffset - dynamicOffset,
                0);
        }
        else if (isLeft && !isTop)
        {
            // Bottom-Left Quadrant: Snap bottom-left corner to cursor with dynamic offset
            popup.position = new Vector3(
                mousePosition.x + popupSize.x / 2 + dynamicOffset,
                mousePosition.y + popupSize.y / 2 + verticalOffset + dynamicOffset,
                0);
        }
        else
        {
            // Bottom-Right Quadrant: Snap bottom-right corner to cursor with dynamic offset
            popup.position = new Vector3(
                mousePosition.x - popupSize.x / 2 - dynamicOffset,
                mousePosition.y + popupSize.y / 2 + verticalOffset + dynamicOffset,
                0);
        }
    }

    private Vector2 GetPopUpSize(RectTransform rectTransform)
    {
        return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
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
        _endMenu.SetActive(true);
        _endScore.text = EntertainmentManager.Instance.Score.ToString();
    }

    public void OpenCloseMenu()
    {
        _menu.SetActive(!_menu.activeSelf);
        GameManager.Instance.GamePaused = _menu.activeSelf;
        ResetPopUps(null);
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
}
