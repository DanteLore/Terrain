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
    public float flowerScale = 0.75f;

    public GameObject[] prefabs;

    [Range(0,1)]
    public float minHeight;

    [Range(0,1)]
    public float maxHeight;

    public int lodIndex = 0;

    public int maxClustersPerChunk = 5;

    public float minClusterRadius = 2f;

    public float maxClusterRadius = 8f;

    public float maxTiltAngle = 10f;
}
