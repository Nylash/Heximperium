using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scout : MonoBehaviour
{
    #region CONFIGURATION
    [SerializeField] private ScoutData _data;
    #endregion

    #region VARIABLES
    private Direction _direction;
    private Animator _animator;
    private Tile _currentTile;
    private bool _hasDoneMoving;
    private float _yOffset;

    private int _speed;
    private int _lifespan;
    private int _revealRadius;

    private List<Renderer> _renderers = new List<Renderer>();
    #endregion

    #region ACCESSORS
    public Direction Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            UpdateCursor();
            if (!ExplorationManager.Instance.ChoosingScoutDirection)
                _animator.SetTrigger("DirectionConfirmed");
        }
    }

    public int Speed { get => _speed; set => _speed = value; }
    public int Lifespan { get => _lifespan; set => _lifespan = value; }
    public int RevealRadius { get => _revealRadius; set => _revealRadius = value; }
    public Tile CurrentTile { get => _currentTile; set => _currentTile = value; }
    public bool HasDoneMoving { get => _hasDoneMoving;}
    #endregion

    private void Awake()
    {
        _renderers.Add(GetComponent<Renderer>());
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) 
            _renderers.Add(renderer);

        ExplorationManager.Instance.OnPhaseFinalized.AddListener(CheckLifeSpan);
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _speed = _data.Speed;
        _lifespan = _data.Lifespan;
        _revealRadius = _data.RevealRadius;

        _yOffset = transform.position.y;
    }

    //Coroutine to move the scout at the end of exploration phase
    public IEnumerator Move()
    {
        for (int i = 0; i < _speed; i++)
        {
            Vector3 pos = transform.localPosition;

            //Move from ancient tile to new
            _currentTile.Scouts.Remove(this);
            _currentTile.UpdateScoutCounter();
            //New tile
            _currentTile = _currentTile.Neighbors[(int)_direction];
            if (_currentTile == null)
            {
                _lifespan = 0;
                _hasDoneMoving = true;
                yield break;
            }
            _currentTile.Scouts.Add(this);
            transform.position = _currentTile.transform.position + new Vector3(0,_yOffset,0);

            if (_currentTile.TileData as HazardousTileData) 
               i++;;                

            //Reveal recursively
            if (!_currentTile.Revealed)
                _currentTile.RevealTile(false);
            RevealTilesRecursively(_currentTile, _revealRadius);

            yield return new WaitForSeconds(ExplorationManager.Instance.AwaitTimeScoutMovement);
        }
        _currentTile.UpdateScoutCounter();

        _hasDoneMoving = true;
    }

    //Check if the scout must stay alive
    private void CheckLifeSpan()
    {
        //Reset bool for next Explore phase
        _hasDoneMoving = false;

        _lifespan--;
        if (_lifespan <= 0)
        {
            ExplorationManager.Instance.Scouts.Remove(this);
            if(_currentTile != null)
            {
                _currentTile.Scouts.Remove(this);
                _currentTile.UpdateScoutCounter();
            }
            Destroy(gameObject);
        }
    }

    //Method to reveal the tiles, depending on the scout reveal radius
    private void RevealTilesRecursively(Tile currentTile, int depth)
    {
        if (depth <= 0)
        {
            return;
        }

        foreach (Tile neighbor in currentTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Revealed)
            {
                neighbor.RevealTile(false);
            }
            // Recursively reveal the neighbors of the current neighbor
            RevealTilesRecursively(neighbor, depth - 1);
        }
    }

    //Rotate the cursor sprites while spawning a scout
    private void UpdateCursor()
    {
        switch (_direction)
        {
            case Direction.TopRight:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case Direction.Right:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -60);
                break;
            case Direction.BottomRight:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -120);
                break;
            case Direction.BottomLeft:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -180);
                break;
            case Direction.Left:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -240);
                break;
            case Direction.TopLeft:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -300);
                break;
        }
    }

    public void ScoutVisibility(bool visible)
    {
        foreach (Renderer item in _renderers)
        {
            item.enabled = visible;
        }
    }
}
