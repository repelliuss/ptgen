using UnityEngine;

[CreateAssetMenu()]
public class GradientHeightMapParams : UpdatableScriptableObject
{
    //TODO: remove const and make a multiple choice values
    public const int size = 127;

    public Vector2 landOffset;
    public float heightScale;
    public AnimationCurve heightCurve;

    [Min(1)]
    public int octaveCount;

    public float noiseScale;
    public float persistance;
    public float lacunarity;
    public int seed;

    public Vector2[] CalculateOctaveOffsets()
    {
        Vector2[] octaveOffsets = new Vector2[octaveCount];
        System.Random prng = new System.Random(seed);
        float amplitude = 1;

        for (int i = 0; i < octaveCount; ++i)
        {
            //TODO: experiment with -10000, 10000 range
            octaveOffsets[i].x = prng.Next(-10000, 10000);
            octaveOffsets[i].y = prng.Next(-10000, 10000);

            amplitude *= persistance;
        }

        return octaveOffsets;
    }

    public float CalculateMaxHeight()
    {
        float maxHeight = 1;
        float amplitude = persistance;

        for(int i = 1; i < octaveCount; ++i)
        {
            maxHeight += amplitude;
            amplitude *= persistance;
        }

        return maxHeight;
    }
}
