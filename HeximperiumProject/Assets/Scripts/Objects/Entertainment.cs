using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entertainment : MonoBehaviour
{
    #region CONSTANTS
    private const string PATH_SPRITES_ENTERTAINMENT = "Sprites/Entertainments/";
    #endregion

    #region VARIABLES
    private EntertainmentData _data;
    private Tile _tile;
    private SpriteRenderer _renderer;
    private int _points;
    private int _pointsBuffer;
    private Dictionary<Tile, int> _externalPointsSources = new Dictionary<Tile, int>();// Points coming from other ent/tile behaviours
    private Dictionary<Tile, int> _internalPointsSources = new Dictionary<Tile, int>();// Points coming from this ent/tile behaviours
    //Variables for special effects
    private HashSet<Tile> _uniqueNeighbors = new HashSet<Tile>();
    private HashSet<Tile> _identicalNeighbors = new HashSet<Tile>();
    // Upgrade variables
    private bool _boostedByIdenticalNeighbors;
    #endregion

    #region ACCESSORS
    public EntertainmentData Data { get => _data; set => _data = value; }
    public Tile Tile { get => _tile; set => _tile = value; }
    public SpriteRenderer Renderer { get => _renderer; }
    public int Points { get => _points; }
    public bool BoostedByIdenticalNeighbors { get => _boostedByIdenticalNeighbors; set => _boostedByIdenticalNeighbors = value; }
    public Dictionary<Tile, int> ExternalPointsSources { get => _externalPointsSources; }
    public HashSet<Tile> UniqueNeighbors { get => _uniqueNeighbors; }
    public HashSet<Tile> IdenticalNeighbors { get => _identicalNeighbors; }
    public Dictionary<Tile, int> InternalPointsSources { get => _internalPointsSources; }
    #endregion

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (EntertainmentManager.Instance.IsPredictingPoints)
            return;

        if (_pointsBuffer > _points)//We lost points during the frame
        {
            EntertainmentManager.Instance.OnScoreLost?.Invoke(_tile, _pointsBuffer - _points); 
        }
        _pointsBuffer = _points;
    }

    public void Initialize(Tile tile, EntertainmentData data)
    {
        _tile = tile;
        _data = data;
        _renderer.sprite = Resources.Load<Sprite>(PATH_SPRITES_ENTERTAINMENT + data.name);

        foreach (SpecialEffect effect in data.SpecialEffects)
            effect.InitializeSpecialEffect(this);

        UpdatePoints(data.BasePoints, Transaction.Gain);

        gameObject.name = _data.name + " (" + (int)_tile.Coordinate.x + ";" + _tile.Coordinate.y + ")";
    }

    public void UpdatePoints(int value, Transaction transaction, bool skipVFX = false, 
        Tile intSource = null, Tile extSource = null)
    {
        if (extSource && intSource)
        {
            Debug.LogError("Both external and internal source cannot be defined at the same time");
            return;
        }

        EntertainmentManager.Instance.UpdateScore(value, transaction, _tile, skipVFX);

        if (transaction == Transaction.Spent)
            value = -value;

        _points += value;

        if (UIManager.Instance.AreIncomesShown)
            _tile.ShowIncomeUI(true);

        if (extSource)
        {
            if (_externalPointsSources.ContainsKey(extSource))
            {
                _externalPointsSources[extSource] += value;
                if (_externalPointsSources[extSource] == 0)
                    _externalPointsSources.Remove(extSource);
            }
            else if (transaction == Transaction.Spent)
                Debug.LogWarning("Trying to remove points from a source that doesn't exist in the dictionary");
            else
                _externalPointsSources.Add(extSource, value);
        }
        if (intSource)
        {
            UpdateInternalSources(intSource, value, transaction);
        }
    }

    public void UpdateInternalSources(Tile intSource, int value, Transaction transaction)
    {
        if (_internalPointsSources.ContainsKey(intSource))
        {
            _internalPointsSources[intSource] += value;
            if (_internalPointsSources[intSource] == 0)
                _internalPointsSources.Remove(intSource);
        }
        else if (transaction == Transaction.Spent)
            Debug.LogWarning("Trying to remove points from a source that doesn't exist in the dictionary");
        else
            _internalPointsSources.Add(intSource, value);
    }

    public void DestroyEntertainment()
    {
        EntertainmentManager.Instance.UpdateScore(_points, Transaction.Spent);//Since we remove the entertainment with all its points, no need to rollback them on special effects

        foreach (SpecialEffect effect in _data.SpecialEffects)
            effect.RollbackSpecialEntertainment(this);
        _tile.UniqueEntertainmentNeighborsCount_SE = 0;
        Destroy(gameObject);
    }

    public void EntertainmentVisibility(bool visible)
    {
        _renderer.enabled = visible;
    }

    public int GetPointsFromEntertainmentOnly()
    {
        if (_externalPointsSources.Count == 0)
            return _points;

        int pointsFromEntOnly = _points;
        foreach (var kvp in _externalPointsSources)
        {
            pointsFromEntOnly -= kvp.Value;
        }
        return pointsFromEntOnly;
    }

    #region SPECIAL EFFECTS
    public void ListenerOnEntertainmentModified_BoostByNeighbors(Tile tile)
    {
        foreach (BoostByNeighbors effect in _data.SpecialEffects.OfType<BoostByNeighbors>())
        {
            effect.CheckEntertainment(this, tile);
        }
    }

    public void ListenerOnEntertainmentModified_BoostByUniqueNeighbors(Tile tile)
    {
        foreach (BoostByUniqueNeighbors effect in _data.SpecialEffects.OfType<BoostByUniqueNeighbors>())
        {
            effect.CheckEntertainment(this);
        }
    }

    public void ListenerOnEntertainmentModified_BoostByZoneSize(Tile tile)
    {
        foreach (BoostByZoneSize effect in _data.SpecialEffects.OfType<BoostByZoneSize>())
        {
            effect.CheckEntertainment(this, tile);
        }
    }

    public void ListenerOnEntertainmentModified_BoostIfEnoughIdenticalNeighbors(Tile tile)
    {
        foreach (BoostIfEnoughIdenticalNeighbors effect in _data.SpecialEffects.OfType<BoostIfEnoughIdenticalNeighbors>())
        {
            effect.CheckEntertainment(this);
        }
    }
    #endregion
}
