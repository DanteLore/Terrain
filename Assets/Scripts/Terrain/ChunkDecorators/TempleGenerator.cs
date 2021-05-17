using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class TempleGenerator : ChunkDecorator
{
    [Range(1, 16)]
    public int gridStep = 6;

    private Dictionary<Vector2, List<GameObject>> temples;

    void Awake()
    {
        priority = 5;
        temples = new Dictionary<Vector2, List<GameObject>>();
    }

    public override void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
        base.OnChunkVisibilityChanged(chunk, visible);

        if(visible)
        {
            CreateTemples(chunk);
        }
        else
        {
            temples[chunk.coord].ForEach(ReleaseToPool);
            temples.Remove(chunk.coord);
        }
    }

    private void CreateTemples(TerrainChunk chunk)
    {
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y * 10000 + chunk.coord.x));
        Biome biome = chunk.NearestBiome(chunk.sampleCenter);
        TempleSettings templeSettings = biome.settings.templeSettings;
        temples.Add(chunk.coord, new List<GameObject>());

        float prob = (float)rand.NextDouble();
        float min = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, chunk.heightMap.minValue);
        float max = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, chunk.heightMap.maxValue);
        if(prob <= templeSettings.templeOnChunkProbability && min >= templeSettings.minHeight && max <= templeSettings.maxHeight)
        {
            FlattenTheLand(chunk, templeSettings);

            GameObject obj = PlaceTemple(chunk, chunk.heightMap.width / 2, chunk.heightMap.height / 2, rand, templeSettings);

            if(obj != null)
            {
                temples[chunk.coord].Add(obj);
                ReserveSpace(chunk, obj);
            }
        }
    }

    private void ReserveSpace(TerrainChunk chunk, GameObject obj)
    {
        Bounds b = new Bounds(obj.transform.position, Vector3.zero);

        foreach(var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(renderer.bounds);
        }

        chunk.AddExclusionZone(b.center, b.size * 1.2f);
    }

    private void FlattenTheLand(TerrainChunk chunk, TempleSettings templeSettings)
    {
        HeightMap map = chunk.heightMap;
        float newLevel = map.minValue + (map.maxValue - map.minValue);

        var falloff = FalloffGenerator.GenerateFalloffMap(map.width, map.height);

        float targetHeight = (map.maxValue + map.minValue) / 2;

        for(int y = 5; y < map.height - 5; y++)
        {
            for(int x = 5; x < map.width - 5; x++)
            {
                float difference = targetHeight - map.values[x, y];

                Vector3 pos = chunk.MapToWorldPoint(x, y);
                float perlinValue = Mathf.PerlinNoise(pos.x * templeSettings.falloffNoiseFrequency, pos.y * templeSettings.falloffNoiseFrequency) * 2 - 1;
                float noiseFactor = perlinValue * templeSettings.falloffNoiseAmplitude * Mathf.Clamp01(difference / targetHeight);

                map.values[x, y] += difference * (1 - falloff[x, y]) + noiseFactor;
            }
        }
    }

    private GameObject PlaceTemple(TerrainChunk chunk, int x, int y, System.Random rand, TempleSettings templeSettings)
    {
        Vector3 pos = chunk.MapToWorldPoint(x, y);
        float placementProbability = (float)rand.NextDouble();

        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        GameObject prefab = templeSettings.templePrefabs[rand.Next(templeSettings.templePrefabs.Length)];
        GameObject temple = InstantiateFromPool(prefab);
        temple.transform.SetParent(chunk.meshObject.transform);
        temple.layer = LayerMask.NameToLayer("Rocks");
        temple.name = prefab.name;

        // Rotate so it's flat on the ground and randomly around the y axis
        var randomRotation = Quaternion.Euler((float)rand.NextDouble(), (float)rand.NextDouble() * 360, (float)rand.NextDouble());
        temple.transform.rotation = temple.transform.rotation * randomRotation;

        temple.transform.position = pos + new Vector3(0f, -0.3f, 0f);
        temple.transform.localScale = Vector3.one * Mathf.Lerp(templeSettings.templeScale * 0.5f, templeSettings.templeScale * 1.5f, (float)rand.NextDouble());

        return temple;
    }
}
