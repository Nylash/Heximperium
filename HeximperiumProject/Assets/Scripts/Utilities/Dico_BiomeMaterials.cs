using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//Class used to represent the materials available for a given biome
public class Dico_BiomeMaterials
{
    public Biome biome;
    public List<Material> materials;

    public Dico_BiomeMaterials(Biome b, List<Material> m)
    {
        biome = b;
        materials = m;
    }
}
