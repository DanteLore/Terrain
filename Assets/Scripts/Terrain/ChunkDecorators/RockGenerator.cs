using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RockGenerator : ChunkDecorator
{
    [Range(1, 16)]
    public int gridStep = 6;

    private Dictionary<Vector2, List<GameObject>> rocks;

    protected override void Awake()
    {
        base.Awake();

        priority = 10;
        rocks = new Dictionary<Vector2, List<GameObject>>();
    }

    public override void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
        base.OnChunkVisibilityChanged(chunk, visible);
        
        if(visible)
        {
            GenerateRocks(chunk);
        }
        else
        {
            rocks[chunk.coord].ForEach(ReleaseToPool);
            rocks.Remove(chunk.coord);
        }
    }

    private void GenerateRocks(TerrainChunk chunk)
    {
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y) * 1000000 + Mathf.RoundToInt(chunk.coord.x));
        rocks[chunk.coord] = new List<GameObject>();

        for(int y = gridStep / 2; y < chunk.MapHeight - 1; y += gridStep)
        {
            for(int x = gridStep / 2; x < chunk.MapWidth - 1; x += gridStep)
            {  
                var rock = PlaceRock(chunk, x, y, rand);
                if(rock != null)
                    rocks[chunk.coord].Add(rock);
            }
        }
    }

    private GameObject PlaceRock(TerrainChunk chunk, int x, int y, System.Random rand)
    {
        Vector3 p1 = chunk.MapToWorldPoint(x, y);
        Vector3 p2 = chunk.MapToWorldPoint(x + 1, y);
        Vector3 p3 = chunk.MapToWorldPoint(x, y + 1);
        Vector3 normal = SurfaceNormalFromPoints(p1, p2, p3);
        Vector3 pos = (p1 + p2 + p3) / 3; // Centre of triangle

        Biome biome = chunk.BlendedBiome(pos, rand);
        RockSettings rockSettings = biome.settings.rockSettings;
        float placementProbability = (float)rand.NextDouble();

        if(placementProbability <= rockSettings.placementThreshold && !chunk.IsInExclusionZone(pos))
        {
            float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

            var possibleRocks = rockSettings.rocks.Where(t => normHeight >= t.minHeight && normHeight <= t.maxHeight).ToList();

            if(possibleRocks.Any())
            {
                var prefabs = possibleRocks.SelectMany(t => t.prefabs).ToList();

                GameObject prefab = prefabs[rand.Next(prefabs.Count)];
                GameObject rock = InstantiateFromPool(prefab);
                rock.transform.SetParent(chunk.meshObject.transform);
                rock.layer = LayerMask.NameToLayer("Rocks");
                rock.name = prefab.name;

                // Rotate so it's flat on the ground and randomly around the y axis
                var randomRotation = Quaternion.Euler(0, (float)rand.NextDouble() * 360, 0);
                var layFlat = Quaternion.FromToRotation(transform.up, normal);
                rock.transform.rotation = layFlat * rock.transform.rotation * randomRotation;

                rock.transform.position = pos + new Vector3(0f, -0.1f, 0f);
                rock.transform.localScale = Vector3.one * Mathf.Lerp(rockSettings.rockScale * 0.5f, rockSettings.rockScale * 1.5f, (float)rand.NextDouble());

                return rock;
            }
        }
        return null;
    }
}
