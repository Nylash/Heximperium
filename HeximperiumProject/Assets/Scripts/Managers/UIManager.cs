using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject _prefabPopUpEntertainer;
    [SerializeField] private GameObject _prefabPopUpScout;
    [Header("Radial menu")]
    [SerializeField] private Color _colorCantAfford;
    [Header("Units visibility UI")]
    [SerializeField] private Image _scoutsImage;
    [SerializeField] private Image _entertainersImage;
    [SerializeField] private Sprite _scoutsVisible;
    [SerializeField] private Sprite _scoutsHidden;
    [SerializeField] private Sprite _entertainersVisible;
    [SerializeField] private Sprite _entertainersHidden;
    [Header("Menu")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _endMenu;
    [SerializeField] private TextMeshProUGUI _endScore;
    [Header("Tutorial")]
    [SerializeField] private GameObject _exploTuto;
    [SerializeField] private GameObject _expandTuto;
    [SerializeField] private GameObject _exploitTuto;
    [SerializeField] private GameObject _entertainTuto;
    #endregion

    #region VARIABLES
    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _screenWidth;
    private float _screenHeight;
    private List<GameObject> _popUps = new List<GameObject>();

    private bool _areScoutsVisible;
    private bool _areEntertainersVisible;

    private bool _menuOpen;
    #endregion

    #region ACCESSORS
    public Color ColorCantAfford { get => _colorCantAfford;}
    public bool MenuOpen { get => _menuOpen; }
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnNewTurn.AddListener(UpdateTurnCounterText);

        GameManager.Instance.OnExplorationPhaseStarted.AddListener(UpdatePhaseUI);
        GameManager.Instance.OnExpansionPhaseStarted.AddListener(UpdatePhaseUI);
        GameManager.Instance.OnExploitationPhaseStarted.AddListener(UpdatePhaseUI);
        GameManager.Instance.OnEntertainementPhaseStarted.AddListener(UpdatePhaseUI);

        GameManager.Instance.OnExplorationPhaseStarted.AddListener(ForceScoutsToShow);

        GameManager.Instance.OnEntertainementPhaseStarted.AddListener(ForceEntertainersToShow);

        GameManager.Instance.OnGameFinished.AddListener(GameFinished);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnNewTurn.RemoveListener(UpdateTurnCounterText);
        GameManager.Instance.OnExplorationPhaseStarted.RemoveListener(UpdatePhaseUI);
        GameManager.Instance.OnExpansionPhaseStarted.RemoveListener(UpdatePhaseUI);
        GameManager.Instance.OnExploitationPhaseStarted.RemoveListener(UpdatePhaseUI);
        GameManager.Instance.OnEntertainementPhaseStarted.RemoveListener(UpdatePhaseUI);
        GameManager.Instance.OnExplorationPhaseStarted.RemoveListener(ForceScoutsToShow);
        GameManager.Instance.OnEntertainementPhaseStarted.RemoveListener(ForceEntertainersToShow);
        GameManager.Instance.OnGameFinished.RemoveListener(GameFinished);
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
                        if (tile.Entertainer != null)
                        {
                            DisplayPopUp(tile.Entertainer);
                        }
                        if (tile.Scouts.Count > 0) 
                        {
                            foreach (Scout item in tile.Scouts)
                            {
                                DisplayPopUp(item);
                            }
                        }
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

    private void DisplayPopUp(Entertainer entertainer)
    {
        DisplayPopUp(entertainer, _prefabPopUpEntertainer);
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

        if (isLeft && isTop)
        {
            // Top-Left Quadrant: Snap top-left corner to cursor
            popup.position = new Vector3(mousePosition.x + popupSize.x / 2, mousePosition.y - popupSize.y / 2 - verticalOffset, 0);
        }
        else if (!isLeft && isTop)
        {
            // Top-Right Quadrant: Snap top-right corner to cursor
            popup.position = new Vector3(mousePosition.x - popupSize.x / 2, mousePosition.y - popupSize.y / 2 - verticalOffset, 0);
        }
        else if (isLeft && !isTop)
        {
            // Bottom-Left Quadrant: Snap bottom-left corner to cursor
            popup.position = new Vector3(mousePosition.x + popupSize.x / 2, mousePosition.y + popupSize.y / 2 + verticalOffset, 0);
        }
        else
        {
            // Bottom-Right Quadrant: Snap bottom-right corner to cursor
            popup.position = new Vector3(mousePosition.x - popupSize.x / 2, mousePosition.y + popupSize.y / 2 + verticalOffset, 0);
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
                _materialBack.SetColor("_ColorTop", _colorTopExplo);
                _materialBack.SetColor("_ColorBottom", _colorBotExplo);
                EnableRenderers(_popUpEntertainPhase.gameObject, false);
                EnableRenderers(_popUpExploPhase.gameObject, true);
                _popUpExploPhase.SetTrigger("PopUp");
                //TMP
                if (_entertainTuto)
                {
                    if (_entertainTuto.activeSelf)
                        Destroy(_entertainTuto);
                }  
                break;
            case Phase.Expand:
                _currentPhaseText.text = "Expand";
                _confirmPhaseButtonText.text = "End Phase";
                _materialBack.SetColor("_ColorTop", _colorTopExpand);
                _materialBack.SetColor("_ColorBottom", _colorBotExpand);
                EnableRenderers(_popUpExploPhase.gameObject, false);
                EnableRenderers(_popUpExpandPhase.gameObject, true);
                _popUpExpandPhase.SetTrigger("PopUp");
                //TMP
                if (_exploTuto)
                    Destroy(_exploTuto);
                if (_expandTuto)
                    _expandTuto.SetActive(true);
                break;
            case Phase.Exploit:
                _currentPhaseText.text = "Exploit";
                _confirmPhaseButtonText.text = "End Phase";
                _materialBack.SetColor("_ColorTop", _colorTopExploit);
                _materialBack.SetColor("_ColorBottom", _colorBotExploit);
                EnableRenderers(_popUpExpandPhase.gameObject, false);
                EnableRenderers(_popUpExploitPhase.gameObject, true);
                _popUpExploitPhase.SetTrigger("PopUp");
                //TMP
                if (_expandTuto)
                    Destroy(_expandTuto);
                if (_exploitTuto)
                    _exploitTuto.SetActive(true);
                break;
            case Phase.Entertain:
                _currentPhaseText.text = "Entertain";
                _confirmPhaseButtonText.text = "End Turn";
                _materialBack.SetColor("_ColorTop", _colorTopEntertain);
                _materialBack.SetColor("_ColorBottom", _colorBotEntertain);
                EnableRenderers(_popUpExploitPhase.gameObject, false);
                EnableRenderers(_popUpEntertainPhase.gameObject, true);
                _popUpEntertainPhase.SetTrigger("PopUp");
                //TMP
                if (_exploitTuto)
                    Destroy(_exploitTuto);
                if (_entertainTuto)
                    _entertainTuto.SetActive(true);
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
        _endScore.text = EntertainementManager.Instance.Score.ToString();
    }

    public void OpenCloseMenu()
    {
        _menu.SetActive(!_menu.activeSelf);
        _menuOpen = _menu.activeSelf;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfav2yqM8XQFg-BkDHh5HvbugKSOXGCSP6hiaSW58-OyttKgQ/viewform?usp=sharing&ouid=102342740940582761191");
        Application.Quit();
    }
    #endregion
}
