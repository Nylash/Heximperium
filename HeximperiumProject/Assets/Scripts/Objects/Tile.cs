using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    private Vector2 _coordinate;
    private TileData _tileData;
    private Biome _biome;
    private bool _revealed;
    private bool _claimed;

    public Vector2 Coordinate { get => _coordinate; set => _coordinate = value; }
}
