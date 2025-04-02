using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    [SerializeField] private Biome _biome;
    [SerializeField] private TileData _tileData;

    private Vector2 _coordinate;
    private bool _revealed;
    private bool _claimed;

    public Vector2 Coordinate { get => _coordinate; set => _coordinate = value; }

    private void Start()
    {
        if (_tileData != null)
        {
            if(_biome == Biome.Grassland)
            {
                if(_tileData.name == "Water")
                {
                    GetComponent<Renderer>().material = Resources.Load(_tileData.name) as Material;
                    return;
                }
                GetComponent<Renderer>().material = Resources.Load(_biome + "/" + _tileData.name + "_" + _biome) as Material;
            }
        }
    }
}
