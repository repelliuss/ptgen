using UnityEngine;

[CreateAssetMenu()]
public class NoiseParams : UpdatableScriptableObject
{
    [Min(1)]
    public int octaveCount;

    public float noiseScale;
    public float persistance;
    public float lacunarity;
    public int seed;
}
