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
    public TileData TileData { get => _tileData; set => _tileData = value; }
    public bool Claimed { get => _claimed; set => _claimed = value; }
    public bool Revealed { get => _revealed; set => _revealed = value; }
    public Biome Biome { get => _biome; set => _biome = value; }

    private void Start()
    {

    }

    private void ApplyMaterial()
    {
        
        if (_tileData.name == "Water")
        {
            //Material not linked to biome
            GetComponent<Renderer>().material = Resources.Load(_tileData.name) as Material;
            return;
        }
        GetComponent<Renderer>().material = Resources.Load(_biome + "/" + _tileData.name + "_" + _biome) as Material;
    }
}
