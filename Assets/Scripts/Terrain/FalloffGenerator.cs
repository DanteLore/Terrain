using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int width, int height)
    {
        float[,] map = new float[width, height];

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                float x = i / (float)width * 2 - 1; // -1 to 1 range
                float y = j / (float)height * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    private static float Evaluate(float value)
    {
        float a = 2f;
        float b = 2.2f;

        return Mathf.Pow(value, a)/(Mathf.Pow(value, a) + Mathf.Pow(b - b*value, a));
    }
}
