using System.Collections.Generic;
using UnityEngine;

public class EntertainementManager : Singleton<EntertainementManager>
{
    [SerializeField] private GameObject _entertainerPrefab;
    [SerializeField] private Transform _entertainersParent;

    private List<Entertainer> _entertainers = new List<Entertainer>();
    private List<Vector3> _interactionPositions = new List<Vector3>();
    private List<GameObject> _buttons = new List<GameObject>();
    private List<EntertainerData> _entertainerDatas = new List<EntertainerData>();
    private int _score;

    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
        GameManager.Instance.event_newTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.event_tileUnselected.AddListener(TileUnselected);

        foreach (EntertainerData item in Resources.LoadAll<EntertainerData>("Data/Units/Entertainers/"))
        {
            _entertainerDatas.Add(item);
        }
    }

    private void StartPhase(Phase phase)
    {
        if (phase != Phase.Entertain)
            return;

        ResourcesManager.Instance.UpdateResource(Resource.Stone, 5, Transaction.Gain);
        ResourcesManager.Instance.UpdateResource(Resource.Crystal, 5, Transaction.Gain);
        ResourcesManager.Instance.UpdateResource(Resource.Gold, 100, Transaction.Gain);
    }

    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Entertain)
            return;

        _interactionPositions.Clear();

        if (tile.Claimed)
        {
            if(tile.Entertainer != null)
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                DestroyInteraction(tile, 0);
                return;
            }
            else
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, _entertainerDatas.Count);
                for (int i = 0; i < _entertainerDatas.Count; i++)
                {
                    EntertainerInteraction(tile, i, _entertainerDatas[i]);
                }
            }
        }
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

            currentEntertainer.Initialize(tile, data);

            _entertainers.Add(currentEntertainer);
            tile.Entertainer = currentEntertainer;
        }
    }

    private void EntertainerInteraction(Tile tile, int positionIndex, EntertainerData data)
    {
        GameObject button = Instantiate(GameManager.Instance.InteractionPrefab, _interactionPositions[positionIndex], Quaternion.identity);
        button.GetComponent<UI_InteractionButton>().Initialize(tile, Interaction.Entertainer, null, data);

        _buttons.Add(button);
    }

    private void DestroyInteraction(Tile tile, int positionIndex)
    {
        GameObject button = Instantiate(GameManager.Instance.InteractionPrefab, _interactionPositions[positionIndex], Quaternion.identity);
        button.GetComponent<UI_InteractionButton>().Initialize(tile, Interaction.Destroy);

        _buttons.Add(button);
    }

    public void DestroyEntertainer(Tile tile)
    {
        tile.Entertainer.RemoveSynergies();
        Destroy(tile.Entertainer.gameObject);
        _entertainers.Remove(tile.Entertainer);
        tile.Entertainer = null;
    }

    private void TileUnselected()
    {
        foreach (GameObject button in _buttons)
        {
            Destroy(button);
        }
        _buttons.Clear();
    }

    public void ConfirmingPhase()
    {
        foreach (Entertainer item in _entertainers)
        {
            _score += item.Points;
            UIManager.Instance.UpdateScoreUI(_score);
        }
    }
}
