using UnityEngine;

[CreateAssetMenu()]
public class NoisePreset : UpdatablePreset
{
    [Min(0.001f)]
    public float noiseScale = 0.3f;

    [Min(1)]
    public int octaves;

    [Range(0, 1)]
    public float persistance;
    [Min(1)]
    public float lacunarity;

    public int landSeed;
    public Vector2 landOffset;

    public NormalizeMode normalizeMode;
}
