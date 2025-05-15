using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EntertainementManager : Singleton<EntertainementManager>
{
    #region CONSTANTS
    private const string ENTERTAINERS_DATA_PATH = "Data/Units/Entertainers/";
    #endregion

    #region CONFIGURATION
    [SerializeField] private GameObject _entertainerPrefab;
    [SerializeField] private Transform _entertainersParent;
    [SerializeField] private GameObject _pointsGainPrefab;
    [SerializeField] private Material _pointVFXMat;
    #endregion

    #region VARIABLES
    private List<Entertainer> _entertainers = new List<Entertainer>();
    private List<Vector2> _interactionPositions = new List<Vector2>();
    private List<GameObject> _buttons = new List<GameObject>();
    private List<EntertainerData> _entertainerDatas = new List<EntertainerData>();
    private int _score;
    #endregion

    #region ACCESSORS
    public List<Entertainer> Entertainers { get => _entertainers; }
    public int Score { get => _score; }
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent OnPhaseFinalized = new UnityEvent();
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnEntertainementPhaseStarted.AddListener(StartPhase);
        GameManager.Instance.OnEntertainementPhaseEnded.AddListener(ConfirmPhase);
        GameManager.Instance.OnNewTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.OnTileUnselected.AddListener(TileUnselected);

        EntertainerData[] entertainerDataArray = Resources.LoadAll<EntertainerData>(ENTERTAINERS_DATA_PATH);
        if (entertainerDataArray.Length == 0)
        {
            Debug.LogError("No entertainer data found at path: " + ENTERTAINERS_DATA_PATH);
        }
        else
        {
            _entertainerDatas.AddRange(entertainerDataArray);
        }
    }

    public int GetPointsIncome()
    {
        int value = 0;
        foreach (Entertainer item in _entertainers)
        {
            value += item.Points;
        }
        return value;
    }

    #region PHASE LOGIC
    private void StartPhase()
    {

    }

    private void ConfirmPhase()
    {
        //Mark score for each entertainer
        foreach (Entertainer item in _entertainers)
        {
            _score += item.Points;
            UIManager.Instance.UpdateScoreUI(_score);
            Utilities.PlayResourceGainVFX(item.Tile, _pointsGainPrefab, _pointVFXMat, item.Points);
        }
        StartCoroutine(PhaseFinalized());
    }

    private IEnumerator PhaseFinalized()
    {
        // Wait for one frame
        yield return null;

        OnPhaseFinalized.Invoke();
    }
    #endregion

    #region TILE SELECTION
    //Handle the tile selection action
    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Entertain)
            return;

        _interactionPositions.Clear();

        if (tile.Claimed)
        {
            //Interaction depend on if the tile got an entertainer or not
            if(tile.Entertainer != null)
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(1);
                DestroyInteraction(tile, 0);
                return;
            }
            else
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(_entertainerDatas.Count);
                for (int i = 0; i < _entertainerDatas.Count; i++)
                {
                    EntertainerInteraction(tile, i, _entertainerDatas[i]);
                }
            }
        }
    }

    private void TileUnselected()
    {
        foreach (GameObject button in _buttons)
        {
            Destroy(button);
        }
        _buttons.Clear();
    }
    #endregion

    #region INTERACTION
    private void EntertainerInteraction(Tile tile, int positionIndex, EntertainerData data)
    {
        _buttons.Add(Utilities.CreateInteractionButton(UIManager.Instance.MainCanvas, tile, _interactionPositions[positionIndex], Interaction.Entertainer, null, data));
    }

    private void DestroyInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(UIManager.Instance.MainCanvas, tile, _interactionPositions[positionIndex], Interaction.Destroy));
    }

    public void SpawnEntertainer(Tile tile, EntertainerData data)
    {
        if (tile.Entertainer != null)
            return;

        if (ResourcesManager.Instance.CanAfford(data.Costs))
        {
            ResourcesManager.Instance.UpdateResource(data.Costs, Transaction.Spent);

            Entertainer currentEntertainer = Instantiate(_entertainerPrefab,
                tile.transform.position + _entertainerPrefab.transform.localPosition,
                _entertainerPrefab.transform.rotation,
                _entertainersParent).GetComponent<Entertainer>();

            _entertainers.Add(currentEntertainer);
            tile.Entertainer = currentEntertainer;

            currentEntertainer.Initialize(tile, data);
        }
    }

    public void DestroyEntertainer(Tile tile)
    {
        tile.Entertainer.RemoveSynergies();
        Destroy(tile.Entertainer.gameObject);
        _entertainers.Remove(tile.Entertainer);
        tile.Entertainer = null;
    }
    #endregion
}
