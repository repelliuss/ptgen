using UnityEngine;

[CreateAssetMenu()]
public class GradientHeightMapParams : UpdatableScriptableObject
{
    //TODO: remove const and make a multiple choice values
    public const int size = 129;

    [Min(1)]
    public float uniformScale = 1;

    public Vector2 landOffset;
    public float heightScale;
    public AnimationCurve heightCurve;
    public NoiseParams[] noises;

    public ErosionParams thermalParam;
    public ErosionParams windParam;

    public Vector2[][] CalculateOctaveOffsets()
    {
        Vector2[][] offsets = new Vector2[noises.Length][];

        for(int i = 0; i < noises.Length; ++i)
        {
            offsets[i] = CalculateOctaveOffsets(i);
        }

        return offsets;
    }

    public Vector2[] CalculateOctaveOffsets(int index)
    {
        Vector2[] octaveOffsets = new Vector2[noises[index].octaveCount];
        System.Random prng = new System.Random(noises[index].seed);
        float amplitude = 1;

        for (int i = 0; i < noises[index].octaveCount; ++i)
        {
            //TODO: experiment with -10000, 10000 range
            octaveOffsets[i].x = prng.Next(-10000, 10000);
            octaveOffsets[i].y = prng.Next(-10000, 10000);

            amplitude *= noises[index].persistance;
        }

        return octaveOffsets;
    }

    public float CalculateMaxFBMValue()
    {
        float maxHeight = 0;

        for(int i = 0; i < noises.Length; ++i)
        {
            float height = 1;
            float amplitude = noises[i].persistance;

            for(int j = 1; j < noises[i].octaveCount; ++j)
            {
                height += amplitude;
                amplitude *= noises[i].persistance;
            }

            maxHeight += height;
        }

        return maxHeight / noises.Length;
    }

    public float GetMinHeight()
    {
        AnimationCurve threadCurve = new AnimationCurve(heightCurve.keys);
        return uniformScale * heightScale *
            threadCurve.Evaluate(0);
    }

    public float GetMaxHeight()
    {
        AnimationCurve threadCurve = new AnimationCurve(heightCurve.keys);
        return uniformScale * heightScale *
            threadCurve.Evaluate(1);
    }
}
