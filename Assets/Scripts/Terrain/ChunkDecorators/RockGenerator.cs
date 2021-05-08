using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RockGenerator : ChunkDecorator
{
    public RockSettings rockSettings;
    private Dictionary<Vector2, List<GameObject>> rocks;

    private void Start()
    {
        rocks = new Dictionary<Vector2, List<GameObject>>();
    }
    
    public override void OnHeightMapReady(TerrainChunk chunk)
    {
        base.OnHeightMapReady(chunk);
        
        if(!rocks.ContainsKey(chunk.coord))
        {
            GenerateRocks(chunk);
        }
    }

    private void GenerateRocks(TerrainChunk chunk)
    {
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y) * 1000000 + Mathf.RoundToInt(chunk.coord.x));

        rocks[chunk.coord] = new List<GameObject>();

        for(int y = rockSettings.gridStep / 2; y < chunk.MapHeight - 1; y += rockSettings.gridStep)
        {
            for(int x = rockSettings.gridStep / 2; x < chunk.MapWidth - 1; x += rockSettings.gridStep)
            {  
                float prob = (float)rand.NextDouble();

                if(prob <= rockSettings.placementThreshold)
                {
                    var rock = PlaceRock(chunk, x, y, rand);
                    if(rock != null)
                        rocks[chunk.coord].Add(rock);
                }
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

        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        var possibleRocks = rockSettings.rocks.Where(t => normHeight >= t.minHeight && normHeight <= t.maxHeight).ToList();

        if(possibleRocks.Any())
        {
            var prefabs = possibleRocks.SelectMany(t => t.prefabs).ToList();

            GameObject rock = Instantiate(prefabs[rand.Next(prefabs.Count)]);
            rock.transform.SetParent(chunk.meshObject.transform);

            // Rotate so it's flat on the ground and randomly around the y axis
            var randomRotation = Quaternion.Euler(0, (float)rand.NextDouble() * 360, 0);
            var layFlat = Quaternion.FromToRotation(transform.up, normal);
            rock.transform.rotation = layFlat * rock.transform.rotation * randomRotation;

            rock.transform.position = pos + new Vector3(0f, -0.05f, 0f);
            rock.transform.localScale = Vector3.one * Mathf.Lerp(rockSettings.rockScale * 0.5f, rockSettings.rockScale * 1.5f, (float)rand.NextDouble());

            return rock;
        }

        return null;
    }
}
