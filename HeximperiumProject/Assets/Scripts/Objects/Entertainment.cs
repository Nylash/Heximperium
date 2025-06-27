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
        print(PATH_SPRITES_ENTERTAINMENT + data.name);
        _points = data.BasePoints;

        //do effect

        EntertainmentManager.Instance.UpdateScore(_points, Transaction.Gain, _tile);
    }

    public void DestroyEntertainment()
    {
        EntertainmentManager.Instance.UpdateScore(_points, Transaction.Spent);

        //undo effect
    }
}
