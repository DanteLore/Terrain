using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BiomeGenerator : ChunkDecorator
{
    public List<BiomeSettings> biomeSettings;

    public MeshSettings meshSettings;

    public int biomeComputeGridSize = 100000;

    public int maximumChunkAssignmentRange = 10000;

    public int biomesPerGridCell = 10;

    private List<Biome> biomes;

    private HashSet<Vector2> generatedCenters;

    private HashSet<Vector2> updatedChunks;

    void Start()
    {
        biomes = new List<Biome>();
        generatedCenters = new HashSet<Vector2>();
        updatedChunks = new HashSet<Vector2>();
    }

    public override void OnHeightMapReady(TerrainChunk chunk)
    {
        base.OnHeightMapReady(chunk);

        if(!updatedChunks.Contains(chunk.coord))
        {
            updatedChunks.Add(chunk.coord);
            UpdateBiomes(chunk.sampleCenter);
            AddNearestBiomesToChunk(chunk);
        }
    }

    private void AddNearestBiomesToChunk(TerrainChunk chunk)
    {
        int threshold = maximumChunkAssignmentRange * maximumChunkAssignmentRange;

        var sorted = biomes.OrderBy(b => (b.biomeCoords - chunk.sampleCenter).sqrMagnitude);
        var closeOnes = sorted.Take(1).Union(sorted.TakeWhile(b => (b.biomeCoords - chunk.sampleCenter).sqrMagnitude < threshold));
        
        chunk.AddBiomes(closeOnes);
    }

    private void UpdateBiomes(Vector2 chunkCenter)
    {
        int centreX = Mathf.RoundToInt(chunkCenter.x / biomeComputeGridSize) * biomeComputeGridSize;
        int centreY = Mathf.RoundToInt(chunkCenter.y / biomeComputeGridSize) * biomeComputeGridSize;

        for(int y = centreY - biomeComputeGridSize; y <= centreY + biomeComputeGridSize; y += biomeComputeGridSize)
        {
            for(int x = centreX - biomeComputeGridSize; x <= centreX + biomeComputeGridSize; x += biomeComputeGridSize)
            {
                Vector2 point = new Vector2(x, y);

                if(!generatedCenters.Contains(point))
                {
                    GenerateBiomesAround(point);
                    generatedCenters.Add(point);
                }
            }
        }
    }

    private void GenerateBiomesAround(Vector2 point)
    {
        System.Random rand = new System.Random(Mathf.RoundToInt(point.y) ^ Mathf.RoundToInt(point.x));

        for(int i = 0; i < biomesPerGridCell; i++)
        {
            float x = rand.Next(biomeComputeGridSize) + point.x;
            float y = rand.Next(biomeComputeGridSize) + point.y;

            Vector2 location = new Vector2(x, y);
            BiomeSettings settings = biomeSettings[rand.Next(biomeSettings.Count)];
            Biome biome = new Biome(settings, location);

            biomes.Add(biome);
        }
    }
}

public class Biome
{
    public BiomeSettings settings;

    public Vector2 biomeCoords;

    public Biome(BiomeSettings settings, Vector2 biomeCoords)
    {
        this.settings = settings;
        this.biomeCoords = biomeCoords;
    }

    public override string ToString()
    {
        return settings.name + " at " + biomeCoords;
    }
}
