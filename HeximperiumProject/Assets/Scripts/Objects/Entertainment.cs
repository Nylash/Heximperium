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

    public void UpdatePoints(int value, Transaction transaction)
    {
        EntertainmentManager.Instance.UpdateScore(value, transaction, _tile);

        if (transaction == Transaction.Spent)
            value = -value;

        _points += value; 
    }

    public void DestroyEntertainment()
    {
        EntertainmentManager.Instance.UpdateScore(_points, Transaction.Spent);

        if (_data.SpecialEffect != null)
            _data.SpecialEffect.RollbackSpecialEntertainment(this);

        Destroy(gameObject);
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
