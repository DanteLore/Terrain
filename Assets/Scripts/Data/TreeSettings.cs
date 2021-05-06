using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class TreeSettings : ScriptableObject
{
    [Range(1, 16)]
    public int gridStep = 2;

    [Range(0, 1)]
    public float placementThreshold = 0.2f;

    public float noiseScale = 10.0f;

    [Range(0, 1)]
    public float noiseAmplitude = 0.1f;

    public TreeInfo[] trees;
}

[Serializable]
public struct TreeInfo{
    public string name;

    public GameObject[] prefabs;

    [Range(0,1)]
    public float minHeight;

    [Range(0,1)]
    public float maxHeight;
}
