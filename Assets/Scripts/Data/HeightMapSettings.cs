using UnityEngine;

[CreateAssetMenu]
public class HeightMapSettings : UpdatableData
{
    public NoiseSettings noiseSettings;

    public float heightMultiplier = 10;
    public AnimationCurve heightCurve;
    public bool useFalloff;

    public float MinHeight {
        get{ return heightMultiplier * heightCurve.Evaluate(0); }
    }

    public float MaxHeight {
        get{ return heightMultiplier * heightCurve.Evaluate(1); }
    }

    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();
            
        base.OnValidate();
    }
    #endif
}
