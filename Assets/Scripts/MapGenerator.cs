using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth = 100;
    public int mapHeight = 100;
    public float noiseScale = 10f;
    public bool autoUpdate = true;
    public int octaves = 3;
    [Range(0, 1)]
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public int seed;
    public Vector2 offset;

    public void GenerateMap() 
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset);

        MapDisplay display = FindObjectOfType<MapDisplay>();

        display.DrawNoiseMap(noiseMap);
    }

    void OnValidate()
    {
        if(mapWidth < 1)
            mapWidth = 1;

        if(mapHeight < 1)
            mapHeight = 1;

        if(lacunarity < 1)
            lacunarity = 1;

        if(octaves <= 0)
            octaves = 1;
    }
}
