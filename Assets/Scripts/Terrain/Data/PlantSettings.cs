using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class PlantSettings : ScriptableObject
{
    public int lodIndex = 1;

    [Range(1, 16)]
    public int gridStep = 2;

    [Range(0, 1)]
    public float placementThreshold = 0.2f;

    [Range(0, 2)]
    public float minScale = 0.25f;

    [Range(0, 2)]
    public float maxScale = 0.75f;

    public GameObject[] prefabs;

    [Range(0,1)]
    public float minHeight;

    [Range(0,1)]
    public float maxHeight;
}
