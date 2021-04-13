using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh }

    const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;
    public float meshHeightMultiplier = 10;
    public AnimationCurve meshHeightCurve;
    public float noiseScale = 10f;
    public bool autoUpdate = true;
    public int octaves = 3;
    [Range(0, 1)]
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public int seed;
    public Vector2 offset;
    public TerrainType[] regions;
    public DrawMode drawMode;

    public void GenerateMap() 
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for(int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];

                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if(drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if(drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        else if(drawMode == DrawMode.Mesh)
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(
                noiseMap, 
                meshHeightMultiplier, 
                meshHeightCurve, 
                levelOfDetail), 
                TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize)
                );
        
    }

    void OnValidate()
    {
        if(lacunarity < 1)
            lacunarity = 1;

        if(octaves <= 0)
            octaves = 1;
    }
}

[System.Serializable]
public struct TerrainType 
{
    public float height;
    public Color color;
    public string name;
}
