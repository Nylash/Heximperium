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
    private Dictionary<TileData, int> _pointsSpecialSource = new Dictionary<TileData, int>();
    //Variables for special effects
    private HashSet<Tile> _uniqueNeighbors = new HashSet<Tile>();
    // Upgrade variables
    private bool _boostedByIdenticalNeighbors;
    #endregion

    #region ACCESSORS
    public EntertainmentData Data { get => _data; set => _data = value; }
    public Tile Tile { get => _tile; set => _tile = value; }
    public SpriteRenderer Renderer { get => _renderer; }
    public int Points { get => _points; }
    public bool BoostedByIdenticalNeighbors { get => _boostedByIdenticalNeighbors; set => _boostedByIdenticalNeighbors = value; }
    public Dictionary<TileData, int> PointsSpecialSource { get => _pointsSpecialSource; }
    public HashSet<Tile> UniqueNeighbors { get => _uniqueNeighbors; }
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

    public void UpdatePoints(int value, Transaction transaction, bool skipVFX = false, TileData specialBevSource = null)
    {
        EntertainmentManager.Instance.UpdateScore(value, transaction, _tile, skipVFX);

        if (transaction == Transaction.Spent)
            value = -value;

        _points += value;

        if (UIManager.Instance.AreIncomesShown)
            _tile.ShowIncomeUI(true);

        if (specialBevSource)
        {
            if (_pointsSpecialSource.ContainsKey(specialBevSource))
            {
                _pointsSpecialSource[specialBevSource] += value;
                if (_pointsSpecialSource[specialBevSource] == 0)
                    _pointsSpecialSource.Remove(specialBevSource);
            }
            else if (transaction == Transaction.Spent)
                Debug.LogWarning("Trying to remove points from a source that doesn't exist in the dictionary");
            else
                _pointsSpecialSource.Add(specialBevSource, value);
        }
    }

    public void DestroyEntertainment()
    {
        EntertainmentManager.Instance.UpdateScore(_points, Transaction.Spent);//Since we remove the entertainment with all its points, no need to rollback them on special effects

        foreach (SpecialEffect effect in _data.SpecialEffects)
            effect.RollbackSpecialEntertainment(this);
        _tile.UniqueEntertainmentNeighborsCount_SB = 0;
        _tile.UniqueEntertainmentNeighborsCount_SE = 0;
        Destroy(gameObject);
    }

    public void EntertainmentVisibility(bool visible)
    {
        _renderer.enabled = visible;
    }

    public int GetPointsFromEntertainmentOnly()
    {
        if (_pointsSpecialSource.Count == 0)
            return _points;

        int pointsFromEntOnly = _points;
        foreach (var kvp in _pointsSpecialSource)
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
