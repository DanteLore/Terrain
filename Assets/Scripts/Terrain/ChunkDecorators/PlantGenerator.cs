using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlantGenerator : ChunkDecorator
{
    [Range(1, 16)]
    public int gridStep = 6;

    private Dictionary<Vector2, List<GameObject>> plants;

    void Awake()
    {
        priority = 10;
        plants = new Dictionary<Vector2, List<GameObject>>();
    }
    

    public override void OnLodChange(TerrainChunk chunk, int lod)
    {
        base.OnLodChange(chunk, lod);
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y) * 1000000 + Mathf.RoundToInt(chunk.coord.x));

        PlantSettings plantSettings = chunk.BlendedBiome(chunk.sampleCenter, rand).settings.plantSettings;

        if(lod <= plantSettings.lodIndex)
        {
            if(plants.ContainsKey(chunk.coord))
            {
                plants[chunk.coord].ForEach(f => f.SetActive(true));
            }
            else
            {
                GeneratePlants(chunk);
            }
        }
        else if(plants.ContainsKey(chunk.coord))
        {
            plants[chunk.coord].ForEach(f => f.SetActive(false));
        }
    }

    private void GeneratePlants(TerrainChunk chunk)
    {
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y) * 1000000 + Mathf.RoundToInt(chunk.coord.x));
        plants[chunk.coord] = new List<GameObject>();

        for(int y = gridStep / 2; y < chunk.MapHeight - 1; y += gridStep)
        {
            for(int x = gridStep / 2; x < chunk.MapWidth - 1; x += gridStep)
            {  
                int pX = Mathf.Clamp(x + rand.Next(gridStep) - gridStep * 2, 0, chunk.MapWidth - 1); // Don't be so regular
                int pY = Mathf.Clamp(y + rand.Next(gridStep) - gridStep * 2, 0, chunk.MapHeight - 1);

                var plant = PlacePlant(chunk, pX, pY, rand);
                if(plant != null)
                    plants[chunk.coord].Add(plant);
            }
        }
    }

    private GameObject PlacePlant(TerrainChunk chunk, int x, int y, System.Random rand)
    {
        Vector3 pos = chunk.MapToWorldPoint(x, y);

        Biome biome = chunk.BlendedBiome(pos, rand);
        PlantSettings plantSettings = biome.settings.plantSettings;
        float placementProbability = (float)rand.NextDouble();

        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        if(placementProbability <= plantSettings.placementThreshold && normHeight >= plantSettings.minHeight && normHeight <= plantSettings.maxHeight && !chunk.IsInExclusionZone(pos))
        {
            GameObject plant = Instantiate(plantSettings.prefabs[rand.Next(plantSettings.prefabs.Length)]);
            plant.transform.SetParent(chunk.meshObject.transform);
            plant.layer = LayerMask.NameToLayer("Plants");
            plant.name = "Plant on chunk " + chunk.coord + " at: " + pos;

            var randomRotation = Quaternion.Euler(5f, (float)rand.NextDouble() * 360, 5f);
            plant.transform.rotation = plant.transform.rotation * randomRotation;

            plant.transform.position = pos + new Vector3(1f - (float)rand.NextDouble() * 2f, -0.05f, 1f - (float)rand.NextDouble() * 2f);
            plant.transform.localScale = Vector3.one * Mathf.Lerp(plantSettings.minScale, plantSettings.maxScale, (float)rand.NextDouble());

            return plant;
        }
        return null;
    }
}
