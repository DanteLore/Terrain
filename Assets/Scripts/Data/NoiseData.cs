using UnityEngine;

[CreateAssetMenu]
public class NoiseData : UpdatableData
{
    public float noiseScale = 10f;   
    [Range(1, 10)]
    public int octaves = 3;
    [Range(0, 1)]
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public int seed;
    public Vector2 offset;
    public Noise.NormalizeMode normalizeMode;


    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        if(lacunarity < 1)
            lacunarity = 1;

        if(octaves <= 0)
            octaves = 1;
            
        base.OnValidate();
    }
    #endif
}
