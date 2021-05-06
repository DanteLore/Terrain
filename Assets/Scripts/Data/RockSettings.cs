using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class RockSettings : ScriptableObject
{
    [Range(1, 16)]
    public int gridStep = 2;

    [Range(0, 1)]
    public float placementThreshold = 0.2f;

    [Range(0, 2)]
    public float rockScale = 0.75f;

    public RockInfo[] rocks;
}

[Serializable]
public struct RockInfo{
    public string name;

    public GameObject[] prefabs;

    [Range(0,1)]
    public float minHeight;

    [Range(0,1)]
    public float maxHeight;
}
