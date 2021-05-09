using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BiomeGenerator : ChunkDecorator
{
    public List<BiomeSettings> biomeSettings;    
    public float chunkBiomeCentreProbability = 0.1f;

    public MeshSettings meshSettings;

    private List<Biome> biomes;
}

public class Biome
{
    public BiomeSettings settings;

    public Vector2 biomeCoords;

    public Vector2 worldCoords2D;

    public Biome(BiomeSettings settings, Vector2 biomeCoords, Vector2 worldCoords2D)
    {
        this.settings = settings;
        this.biomeCoords = biomeCoords;
        this.worldCoords2D = worldCoords2D;
    }
}
