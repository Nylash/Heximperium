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
    #endregion

    #region ACCESSORS
    public EntertainmentData Data { get => _data; set => _data = value; }
    public Tile Tile { get => _tile; set => _tile = value; }
    public SpriteRenderer Renderer { get => _renderer; }
    public int Points { get => _points; }
    #endregion

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        _pointsBuffer = _points;
    }

    private void LateUpdate()
    {
        if (_pointsBuffer > _points)//We lost points during the frame
        {
            EntertainmentManager.Instance.OnScoreLost?.Invoke(_tile, _pointsBuffer - _points);
            // Reset the buffer so we don't fire again until the next FixedUpdate
            _pointsBuffer = _points;
        }
    }

    public void Initialize(Tile tile, EntertainmentData data)
    {
        _tile = tile;
        _data = data;
        _renderer.sprite = Resources.Load<Sprite>(PATH_SPRITES_ENTERTAINMENT + data.name);

        if (_data.SpecialEffect != null)
            _data.SpecialEffect.InitializeSpecialEffect(this);

        UpdatePoints(data.BasePoints, Transaction.Gain);

        gameObject.name = _data.name + " (" + (int)_tile.Coordinate.x + ";" + _tile.Coordinate.y + ")";
    }

    public void UpdatePoints(int value, Transaction transaction, bool skipVFX = false)
    {
        EntertainmentManager.Instance.UpdateScore(value, transaction, _tile, skipVFX);

        if (transaction == Transaction.Spent)
            value = -value;

        _points += value; 
    }

    public void DestroyEntertainment()
    {
        EntertainmentManager.Instance.UpdateScore(_points, Transaction.Spent);//Since we remove the entertainment with all its, no need to rollback them on special effects

        if (_data.SpecialEffect != null)
            _data.SpecialEffect.RollbackSpecialEntertainment(this);
        _tile.UniqueEntertainmentNeighborsCount_SB = 0;
        _tile.UniqueEntertainmentNeighborsCount_SE = 0;
        Destroy(gameObject);
    }

    public void EntertainmentVisibility(bool visible)
    {
        _renderer.enabled = visible;
    }

    #region SPECIAL EFFECTS
    public void ListenerOnEntertainmentModified_BoostByNeighbors(Tile tile)
    {
        if (_data.SpecialEffect is BoostByNeighbors effect)
            effect.CheckEntertainment(this, tile);
    }

    public void ListenerOnEntertainmentModified_BoostByUniqueNeighbors(Tile tile)
    {
        if (_data.SpecialEffect is BoostByUniqueNeighbors effect)
            effect.CheckEntertainment(this);
    }

    public void ListenerOnEntertainmentModified_BoostByZoneSize(Tile tile)
    {
        if (_data.SpecialEffect is BoostByZoneSize effect)
            effect.CheckEntertainment(this, tile);
    }
    #endregion
}
