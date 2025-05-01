using UnityEngine;

public class Entertainer : MonoBehaviour
{
    private EntertainerData _entertainerData;
    private Tile _tile;
    private SpriteRenderer _renderer;
    private int _points;

    public int Points { get => _points; }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Tile tile, EntertainerData data)
    {
        _tile = tile;
        _entertainerData = data;
        _renderer.sprite = Resources.Load<Sprite>("Units/" + data.Entertainer);
        _points = _entertainerData.Points;
    }
}
