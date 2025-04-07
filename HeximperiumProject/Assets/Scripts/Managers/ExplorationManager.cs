using System.Collections.Generic;
using UnityEngine;

public class ExplorationManager : Singleton<ExplorationManager>
{
    [SerializeField] private GameObject _scoutPrefab;

    private List<Scout> _scouts = new List<Scout>();
    private List<GameObject> _buttons = new List<GameObject>();
    private List<Vector3> _interactionPositions = new List<Vector3>();
    private int _freeScouts;

    public int FreeScouts { get => _freeScouts; set => _freeScouts = value; }

    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
        GameManager.Instance.event_newTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.event_tileUnselected.AddListener(TileUnselected);
    }

    private void StartPhase(Phase phase)
    {
        if (phase != Phase.Explore)
            return;
    }

    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Explore)
            return;

        if(tile.TileData.ScoutStartingPoint)
        {
            if(tile.Scout == null)
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                ScoutInteraction(tile, 0);
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

    public void SpawnScout(Tile tile, ScoutData data)
    {
        if(tile.Scout == null)
        {
            if(ResourcesManager.Instance.CanAfford(data.Costs) || _freeScouts != 0)
            {
                if (_freeScouts != 0)
                    _freeScouts--;
                else
                    ResourcesManager.Instance.UpdateResource(data.Costs, Transaction.Spent);

                Scout scout = Instantiate(_scoutPrefab, tile.transform).GetComponent<Scout>();
                tile.Scout = scout;
            }
        }
    }

    private void ScoutInteraction(Tile tile, int positionIndex)
    {
        GameObject buttonScout = Instantiate(GameManager.Instance.InteractionPrefab, _interactionPositions[positionIndex], Quaternion.identity);
        buttonScout.GetComponent<UI_InteractionButton>().Initialize(tile, Interaction.Scout);

        _buttons.Add(buttonScout);
    }
}
