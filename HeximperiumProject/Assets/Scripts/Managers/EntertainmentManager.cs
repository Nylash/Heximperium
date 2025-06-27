using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Progress;

public class EntertainmentManager : Singleton<EntertainmentManager>
{
    #region CONFIGURATION
    [SerializeField] private List<EntertainmentData> _entertainmentsData = new List<EntertainmentData>();
    [SerializeField] private GameObject _entertainmentPrefab;
    [SerializeField] private Transform _entertainmentsParent;
    [SerializeField] private GameObject _pointsGainPrefab;
    [SerializeField] private Material _pointVFXMat;
    [SerializeField] private float _pointForOneGold;
    [SerializeField] private float _pointForOneSR;
    #endregion

    #region VARIABLES
    private List<Entertainment> _entertainments = new List<Entertainment>();
    private List<Vector3> _interactionPositions = new List<Vector3>();
    private List<GameObject> _buttons = new List<GameObject>();
    private int _score;
    #endregion

    #region ACCESSORS
    public List<Entertainment> Entertainments { get => _entertainments; }
    public int Score { get => _score; }
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent OnPhaseFinalized = new UnityEvent();
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnEntertainmentPhaseStarted.AddListener(StartPhase);
        GameManager.Instance.OnEntertainmentPhaseEnded.AddListener(ConfirmPhase);
        GameManager.Instance.OnNewTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.OnTileUnselected.AddListener(TileUnselected);
    }

    #region PHASE LOGIC
    private void StartPhase()
    {
        //Convert savings into points
        float goldIntoPoints = _pointForOneGold * ResourcesManager.Instance.GetResourceStock(Resource.Gold);
        float srIntoPoints = _pointForOneSR * ResourcesManager.Instance.GetResourceStock(Resource.SpecialResources);
        _score += Mathf.RoundToInt(goldIntoPoints) + Mathf.RoundToInt(srIntoPoints);
        ResourcesManager.Instance.SpendAllResources();

        //Earn incomes of every claimed tiles
        foreach (Tile tile in ExpansionManager.Instance.ClaimedTiles)
            ResourcesManager.Instance.UpdateResource(tile.Incomes, Transaction.Gain, tile);

        ResourcesManager.Instance.CHEAT_RESOURCES();
    }

    private void ConfirmPhase()
    {
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
            //Interaction depend on if the tile got an entertainment or not
            if(tile.Entertainment != null)
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                DestroyInteraction(tile, 0);
                return;
            }
            else
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, _entertainmentsData.Count);
                for (int i = 0; i < _entertainmentsData.Count; i++)
                {
                    EntertainmentInteraction(tile, i, _entertainmentsData[i]);
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
    private void EntertainmentInteraction(Tile tile, int positionIndex, EntertainmentData data)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Entertainment, null, data));
    }

    private void DestroyInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Destroy));
    }

    public void SpawnEntertainment(Tile tile, EntertainmentData data)
    {
        if (tile.Entertainment != null)
            return;

        if (ResourcesManager.Instance.CanAfford(data.Costs))
        {
            ResourcesManager.Instance.UpdateResource(data.Costs, Transaction.Spent);

            Entertainment currentEntertainment = Instantiate(_entertainmentPrefab,
                tile.transform.position + _entertainmentPrefab.transform.localPosition,
                _entertainmentPrefab.transform.rotation,
                _entertainmentsParent).GetComponent<Entertainment>();

            _entertainments.Add(currentEntertainment);
            tile.Entertainment = currentEntertainment;

            currentEntertainment.Initialize(tile, data);
        }
    }

    public void DestroyEntertainment(Tile tile)
    {
        tile.Entertainment.DestroyEntertainment();
        Destroy(tile.Entertainment.gameObject);
        _entertainments.Remove(tile.Entertainment);
        tile.Entertainment = null;
    }

    public void ButtonsFade(bool fade)
    {
        foreach (GameObject item in _buttons)
        {
            item.GetComponent<InteractionButton>().FadeAnimation(fade);
        }
    }
    #endregion

    public void UpdateScore(int value, Transaction transaction, Tile tile = null)
    {
        if (transaction == Transaction.Spent)
            value = -value;

        _score += value;

        //Play VFX if we gain score from a tile
        if (tile != null && transaction == Transaction.Gain)
            Utilities.PlayResourceGainVFX(tile, _pointsGainPrefab, _pointVFXMat, value);
    }
}
