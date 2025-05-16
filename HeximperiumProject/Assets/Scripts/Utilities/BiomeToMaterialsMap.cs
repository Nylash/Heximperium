using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//Class used to represent the materials available for a given biome
public class BiomeToMaterialsMap
{
    public Biome biome;
    public List<Material> materials;

    public BiomeToMaterialsMap(Biome b, List<Material> m)
    {
        biome = b;
        materials = m;
    }
}
