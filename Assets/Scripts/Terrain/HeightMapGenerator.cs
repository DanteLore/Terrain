using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter)
    {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                // TODO: Also apply falloff here!
                values[i,j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if(values[i, j] > maxValue)
                    maxValue = values[i, j];
                if(values[i, j] < minValue)
                    minValue = values[i, j];
            }
        }

        return new HeightMap(values, minValue, maxValue, width, height);
    }
}


public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;
    public readonly int width;
    public readonly int height;

    public HeightMap(float[,] heightMap, float minValue, float maxValue, int width, int height)
    {
        this.values = heightMap;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.width = width;
        this.height = height;
    }
}
