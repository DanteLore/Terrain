using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class BiomeSettings : ScriptableObject
{
    public TreeSettings treeSettings;

    public FlowerSettings flowerSettings;

    public RockSettings rockSettings;

    public PlantSettings plantSettings;

    public TempleSettings templeSettings;

    public MobSettings mobSettings;
}
