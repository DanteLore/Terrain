using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class TreeSettings : ScriptableObject
{
    [Range(1, 16)]
    public int gridStep = 2;

    public float placementProbability = 0.01f;

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
