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
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y * 10000000000 + chunk.coord.x));

        rocks[chunk.coord] = new List<GameObject>();

        for(int y = rockSettings.gridStep; y < chunk.MapHeight; y += rockSettings.gridStep)
        {
            for(int x = rockSettings.gridStep; x < chunk.MapWidth; x += rockSettings.gridStep)
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
        Vector3 pos = chunk.MapToWorldPoint(x, y);
        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        var possibleRocks = rockSettings.rocks.Where(t => normHeight >= t.minHeight && normHeight <= t.maxHeight).ToList();

        if(possibleRocks.Any())
        {
            var prefabs = possibleRocks.SelectMany(t => t.prefabs).ToList();

            GameObject rock = Instantiate(prefabs[rand.Next(prefabs.Count)]);
            rock.transform.SetParent(chunk.meshObject.transform);

            rock.transform.position = pos + new Vector3(0f, -0.05f, 0f);
            rock.transform.localScale = Vector3.one * Mathf.Lerp(rockSettings.rockScale * 0.5f, rockSettings.rockScale * 1.5f, (float)rand.NextDouble());

            return rock;
        }

        return null;
    }
}
