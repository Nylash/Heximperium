using UnityEngine;

public class Entertainer : MonoBehaviour
{
    #region CONSTANTS
    private const int SYNERGY_BONUS = 2;
    #endregion

    #region VARIABLES
    private EntertainerData _entertainerData;
    private Tile _tile;
    private SpriteRenderer _renderer;
    private int _points;
    #endregion

    #region ACCESSORS
    public int Points { get => _points; set => _points = value; }
    public EntertainerData EntertainerData { get => _entertainerData; }
    #endregion

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
        CheckBoostingInfra();
    }

    private void CheckBoostingInfra()
    {
        if (_tile.TileData.SpecialBehaviour is EntertainersBoosting entertainersBoosting)
            entertainersBoosting.BoostingSpecificEntertainer(_tile.Entertainer);
        foreach (Tile neighbor in _tile.Neighbors)
        {
            if (neighbor.TileData.SpecialBehaviour is EntertainersBoosting NeighborEntertainersBoosting)
                NeighborEntertainersBoosting.BoostingSpecificEntertainer(_tile.Entertainer);
        }
    }

    //Check if this new entertainer get bonuses from its neighbors and so apply it to them too (synergies are shared)
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

    //Remove the bonus from its neighbors, used before destroying the entertainer
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

    public void EntertainerVisibility(bool visible)
    {
        _renderer.enabled = visible;
    }
}
