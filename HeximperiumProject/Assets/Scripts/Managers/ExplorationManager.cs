using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExplorationManager : Singleton<ExplorationManager>
{
    [SerializeField] private GameObject _scoutPrefab;
    [SerializeField] private GameObject _scoutCounterPrefab;

    private List<Scout> _scouts = new List<Scout>();
    private List<GameObject> _buttons = new List<GameObject>();
    private List<Vector3> _interactionPositions = new List<Vector3>();
    private int _freeScouts;

    private Scout _currentScout;
    private bool _choosingScoutDirection;
    private Tile _tileRefForScoutDirection;

    public int FreeScouts { get => _freeScouts; set => _freeScouts = value; }
    public bool ChoosingScoutDirection { get => _choosingScoutDirection;}

    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
        GameManager.Instance.event_newTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.event_tileUnselected.AddListener(TileUnselected);
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentPhase != Phase.Explore)
            return;
        if(ChoosingScoutDirection)
            _currentScout.Direction = GetAngleForScout();
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
            _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
            ScoutInteraction(tile, 0);
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
        if(ResourcesManager.Instance.CanAfford(data.Costs) || _freeScouts != 0)
        {
            if (_freeScouts != 0)
                _freeScouts--;
            else
                ResourcesManager.Instance.UpdateResource(data.Costs, Transaction.Spent);

            _currentScout = Instantiate(_scoutPrefab, tile.transform).GetComponent<Scout>();
            tile.Scouts.Add(_currentScout);
            _tileRefForScoutDirection = tile;

            //Add the scout counter to visualize if the tile has several scouts
            if (tile.ScoutCounter == null)
                tile.ScoutCounter = Instantiate(_scoutCounterPrefab, tile.transform).GetComponent<TextMeshPro>();
            tile.ScoutCounter.text = tile.Scouts.Count.ToString();

            _choosingScoutDirection = true;
        }
    }

    private void ScoutInteraction(Tile tile, int positionIndex)
    {
        GameObject buttonScout = Instantiate(GameManager.Instance.InteractionPrefab, _interactionPositions[positionIndex], Quaternion.identity);
        buttonScout.GetComponent<UI_InteractionButton>().Initialize(tile, Interaction.Scout);

        _buttons.Add(buttonScout);
    }

    public void ConfirmDirection()
    {
        _choosingScoutDirection = false;
        _currentScout.Direction = GetAngleForScout();
        _tileRefForScoutDirection = null;
        _currentScout = null;
    }

    private Direction GetAngleForScout()
    {
        Vector3 mousePosition = Input.mousePosition;
        // Convert the target object's position to screen space
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_tileRefForScoutDirection.transform.position);
        // Calculate the direction vector from the target object to the cursor in screen space
        Vector2 direction = mousePosition - screenPosition;
        // Calculate the angle between the direction vector and the upwards direction (positive Y-axis)
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        // Invert the angle to correct the direction
        angle = -angle;
        // Normalize the angle to be within 0 to 360 degrees
        angle = (angle + 360) % 360;

        // Determine the enum direction based on the angle
        return GetDirectionFromAngle(angle);
    }

    private Direction GetDirectionFromAngle(float angle)
    {
        if (angle >= 0 && angle < 60)
        {
            return Direction.TopRight;
        }
        else if (angle >= 60 && angle < 120)
        {
            return Direction.Right;
        }
        else if (angle >= 120 && angle < 180)
        {
            return Direction.BottomRight;
        }
        else if (angle >= 180 && angle < 240)
        {
            return Direction.BottomLeft;
        }
        else if (angle >= 240 && angle < 300)
        {
            return Direction.Left;
        }
        else
        {
            return Direction.TopLeft;
        }
    }
}

public enum Direction
{
    TopRight, Right, BottomRight, BottomLeft, Left, TopLeft
}
