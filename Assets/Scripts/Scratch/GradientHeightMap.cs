using UnityEngine;
using UnityEngine.Assertions;

//TODO: rename file
public class GradientHeightMapMaker
{
    GradientHeightMapParams param;
    float maxHeight;
    Vector2[] octaveOffsets;
    AnimationCurve heightCurve;

    public GradientHeightMapMaker(GradientHeightMapParams param)
    {
        this.param = param;
        this.maxHeight = param.CalculateMaxFBMValue();
        this.octaveOffsets = param.CalculateOctaveOffsets();
        this.heightCurve = new AnimationCurve(param.heightCurve.keys); // For threading
    }

    public float[,] Make(Vector2 center)
    {
        int size = GradientHeightMapParams.size;
        float[,] heightMap = new float[size, size];

        Assert.IsTrue(param.noiseScale > 0.0f, "scale factor is not positive");

        float max = float.MinValue;
        float min = float.MaxValue;

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                float height = FBM(x, y, center);
                height = Normalize(height, maxHeight);
                height = heightCurve.Evaluate(height);
                height *= param.heightScale;
                heightMap[x, y] = height;
                if(max < height)
                max = height;
                if(min > height)
                min = height;
            }
        }

        return heightMap;
    }

    float FBM(float x, float y, Vector2 center)
    {
        float height = 0;
        float frequency = 1;
        float amplitude = 1;

        float halfSize = GradientHeightMapParams.size / 2;

        for (int i = 0; i < param.octaveCount; ++i)
        {
            float effectiveX = CalculateEffectiveX(x, halfSize, i, frequency, center);
            float effectiveY = CalculateEffectiveY(y, halfSize, i, frequency, center);

            float noise = NNoise.PrimaryNoise(effectiveX, effectiveY);
            noise = (noise * 2 - 1) * amplitude;

            height += noise;

            amplitude *= param.persistance;
            frequency *= param.lacunarity;
        }

        return height;
    }

    float CalculateEffectiveX(float x, float halfSize, int octaveIndex,
                              float frequency, Vector2 center)
    {
        return (x
                - halfSize
                + octaveOffsets[octaveIndex].x
                + param.landOffset.x
                + center.x)
            / param.noiseScale
            * frequency;
    }

    float CalculateEffectiveY(float y, float halfSize, int octaveIndex,
                                     float frequency, Vector2 center)
    {
        return (y
                - halfSize
                + octaveOffsets[octaveIndex].y
                + param.landOffset.y
                + center.y)
            / (param.noiseScale)
            * frequency;
    }

    /// unfold value and normalize with a constant
    /// (value + 1) / 2f * maxValue * constant
    /// here constant is 2f
    public static float Normalize(float height, float maxHeight)
    {
        // REVIEW: check
        // return (height + maxHeight) / (maxHeight * 2f);
        return (height + 1) / maxHeight;
    }
}
