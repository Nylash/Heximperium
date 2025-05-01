using UnityEngine;

public class Entertainer : MonoBehaviour
{
    private EntertainerData _entertainerData;
    private Tile _tile;
    private SpriteRenderer _renderer;
    private int _points;

    private const int SYNERGY_BONUS = 2;

    public int Points { get => _points; set => _points = value; }
    public EntertainerData EntertainerData { get => _entertainerData; }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Tile tile, EntertainerData data)
    {
        _tile = tile;
        _entertainerData = data;
        _renderer.sprite = Resources.Load<Sprite>("Units/" + data.EntertainerType);
        _points = _entertainerData.Points;

        CheckSynergies();
    }

    private void CheckSynergies()
    {
        foreach (Tile neighbor in _tile.Neighbors)
        {
            if (!neighbor.Entertainer)
                continue;
            if (_entertainerData.Synergies.Contains(neighbor.Entertainer.EntertainerData.EntertainerType))
            {
                _points += SYNERGY_BONUS;
                neighbor.Entertainer.Points += SYNERGY_BONUS;
            }
        }
    }

    public void RemoveSynergies()
    {
        foreach (Tile neighbor in _tile.Neighbors)
        {
            if (!neighbor.Entertainer)
                continue;
            if (_entertainerData.Synergies.Contains(neighbor.Entertainer.EntertainerData.EntertainerType))
            {
                neighbor.Entertainer.Points -= SYNERGY_BONUS;
            }
        }
    }
}
