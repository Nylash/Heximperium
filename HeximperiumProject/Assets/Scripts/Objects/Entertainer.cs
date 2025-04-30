using UnityEngine;

public class Entertainer : MonoBehaviour
{
    [SerializeField] private EntertainerData _entertainerData;

    private Tile _tile;
    private SpriteRenderer _renderer;

    public EntertainerData EntertainerData { get => _entertainerData; set => _entertainerData = value; }
    public Tile Tile { get => _tile; set => _tile = value; }
    public SpriteRenderer Renderer { get => _renderer; }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }
}
