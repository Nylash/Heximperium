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
    public int Points { get => _points; set => _points = value; }
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

        _points = data.BasePoints;

        if (_data.SpecialEffect != null)
            _data.SpecialEffect.InitializeSpecialEffect(this);

        EntertainmentManager.Instance.UpdateScore(_points, Transaction.Gain, _tile);
    }

    public void DestroyEntertainment()
    {
        EntertainmentManager.Instance.UpdateScore(_points, Transaction.Spent);

        if (_data.SpecialEffect != null)
            _data.SpecialEffect.RollbackSpecialEntertainment(this);
    }

    #region SPECIAL EFFECTS
    public void ListenerOnEntertainmentModified(Tile tile)
    {
        if (_data.SpecialEffect is BoostByNeighbors effect)
            effect.CheckEntertainment(this, tile);
    }
    #endregion
}
