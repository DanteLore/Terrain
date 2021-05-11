using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class FlowerSettings : ScriptableObject
{
    [Range(1, 16)]
    public int gridStep = 2;

    [Range(0, 1)]
    public float placementThreshold = 0.2f;

    [Range(0, 2)]
    public float flowerScale = 1.0f;

    public GameObject[] prefabs;

    [Range(0,1)]
    public float minHeight = 0.1f;

    [Range(0,1)]
    public float maxHeight = 0.7f;

    public int lodIndex = 1;

    public int maxClustersPerChunk = 5;

    public float minClusterRadius = 2f;

    public float maxClusterRadius = 8f;

    public float maxTiltAngle = 10f;
}
