using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdatableData
{
    public float uniformScale = 1f;
    public bool useFlatShading;
    public float meshHeightMultiplier = 10;
    public AnimationCurve meshHeightCurve;
    public bool useFalloff;

    public float MinHeight {
        get{ return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0); }
    }

    public float MaxHeight {
        get{ return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1); }
    }
}
