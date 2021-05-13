using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class TempleSettings : ScriptableObject
{
    public float templeOnChunkProbability = 0.1f;

    public float templeScale = 1f;

    public float falloffNoiseFrequency = 10f;
    [Range(0, 1)]
    public float falloffNoiseAmplitude = 0.1f;

    public GameObject[] templePrefabs;

    [Range(0, 1)]
    public float minHeight = 0.1f;

    [Range(0, 1)]
    public float maxHeight = 0.6f;
}
